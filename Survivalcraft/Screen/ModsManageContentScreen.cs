using Engine;
using Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

public class ModsManageContentScreen : Screen
{
    public static string fName = "ModsManageContentScreen";

    public enum StateFilter { UninstallState, InstallState };

    public class ModItem
    {
        public ModInfo ModInfo;
        public ExternalContentEntry ExternalContentEntry;
        public Subtexture Subtexture;
        public ModItem(ModInfo ModInfo, ExternalContentEntry ExternalContentEntry, Subtexture Subtexture)
        {
            this.ModInfo = ModInfo;
            this.ExternalContentEntry = ExternalContentEntry;
            this.Subtexture = Subtexture;
        }
    }

    public ListPanelWidget m_modsContentList;

    public LabelWidget m_topBarLabel;

    public LabelWidget m_modsContentLabel;

    public LabelWidget m_filterLabel;

    public ButtonWidget m_actionButton;

    public ButtonWidget m_actionButton2;

    public ButtonWidget m_changeFilterButton;

    public ButtonWidget m_upDirectoryButton;

    public StateFilter m_filter;

    public List<ModInfo> m_installModInfo = new List<ModInfo>();

    public List<ModInfo> m_lastInstallModInfo = new List<ModInfo>();

    public int count;

    public bool androidSystem;

    public string m_path;

    public string m_lastPath;

    public string m_uninstallPath = "app:/ModsCache";

    public string m_installPath = "app:/Mods";

    public string[] m_commonPaths = new string[9]
    {
        "android:/Download",
        "android:/Android/data/com.tencent.mobileqq/Tencent/QQfile_recv",
        "android:/Android/data/com.tencent.tim/Tencent/TIMfile_recv",
        "android:tencent/TIMfile_recv",
        "android:tencent/QQfile_recv",
        "android:/Quark/Download",
        "android:/BaiduNetdisk",
        "android:/UCDownloads",
        "android:/baidu/searchbox/downloads"
    };

    public ModsManageContentScreen()
    {
        androidSystem = ModsManager.IsAndroid;
        if (androidSystem)
        {
            m_uninstallPath = m_uninstallPath.Replace("app:", "android:/SurvivalCraft2.2");
            m_installPath = m_installPath.Replace("app:", "android:/SurvivalCraft2.2");
        }
        XElement node = ContentManager.Get<XElement>("Screens/ModsManageContentScreen");
        LoadContents(this, node);
        m_modsContentList = Children.Find<ListPanelWidget>("ModsContentList");
        m_topBarLabel = Children.Find<LabelWidget>("TopBar.Label");
        m_modsContentLabel = Children.Find<LabelWidget>("ModsContentLabel");
        m_filterLabel = Children.Find<LabelWidget>("Filter");
        m_actionButton = Children.Find<ButtonWidget>("ActionButton");
        m_actionButton2 = Children.Find<ButtonWidget>("ActionButton2");
        m_changeFilterButton = Children.Find<ButtonWidget>("ChangeFilter");
        m_upDirectoryButton = Children.Find<ButtonWidget>("UpDirectory");
        m_topBarLabel.Text = LanguageControl.Get(fName, 1);
        m_filterLabel.Text = LanguageControl.Get(fName, 36);
        m_modsContentList.ItemWidgetFactory = delegate (object item)
        {
            ModItem modItem = (ModItem)item;
            XElement node2 = ContentManager.Get<XElement>("Widgets/ExternalContentItem");
            ContainerWidget containerWidget = (ContainerWidget)Widget.LoadWidget(this, node2, null);
            string title = Storage.GetFileName(modItem.ExternalContentEntry.Path);
            string details = LanguageControl.Get(fName, 2);
            if (modItem.ExternalContentEntry.Type == ExternalContentType.Mod)
            {
                title = modItem.ModInfo.Name;
                details = string.Format(LanguageControl.Get(fName, 3), modItem.ModInfo.Version, modItem.ModInfo.Author, MathUtils.Round(modItem.ExternalContentEntry.Size / 1000));
            }
            containerWidget.Children.Find<LabelWidget>("ExternalContentItem.Text").Text = title;
            containerWidget.Children.Find<LabelWidget>("ExternalContentItem.Details").Text = details;
            RectangleWidget iconWidget = containerWidget.Children.Find<RectangleWidget>("ExternalContentItem.Icon");
            iconWidget.Subtexture = modItem.Subtexture;
            iconWidget.Size = new Vector2(50, 50);
            iconWidget.Margin = new Vector2(10, 10);
            return containerWidget;
        };
        m_modsContentList.ItemClicked += delegate (object item)
        {
            if (item != null && m_modsContentList.SelectedItem == item)
            {
                ModItem modItem = (ModItem)item;
                if (modItem.ExternalContentEntry.Type == ExternalContentType.Directory && modItem.ExternalContentEntry.Path != m_installPath)
                {
                    try
                    {
                        SetPath(modItem.ExternalContentEntry.Path);
                        UpdateList();
                    }
                    catch
                    {
                        DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 4), LanguageControl.Get(fName, 5) + "\n" + modItem.ExternalContentEntry.Path, LanguageControl.Get("Usual", "ok"), null, null));
                    }
                }
                else if (modItem.ExternalContentEntry.Type == ExternalContentType.Mod && m_filter == StateFilter.UninstallState)
                {
                    string modName = Storage.GetFileName(modItem.ExternalContentEntry.Path);
                    string modDescription = LanguageControl.Get(fName, 6) + modItem.ModInfo.Description + "\n" + LanguageControl.Get(fName, 7) + modItem.ModInfo.PackageName + "，" + LanguageControl.Get(fName, 8);
                    DialogsManager.ShowDialog(null, new MessageDialog(modName, modDescription, LanguageControl.Get(fName, 9), LanguageControl.Get(fName, 10), delegate (MessageDialogButton result)
                    {
                        if (result == MessageDialogButton.Button1)
                        {
                            Storage.DeleteFile(modItem.ExternalContentEntry.Path);
                            UpdateList();
                        }
                    }));
                }
            }
        };
    }

    public override void Enter(object[] parameters)
    {
        if (!Storage.DirectoryExists(m_uninstallPath)) Storage.CreateDirectory(m_uninstallPath);
        SetPath(m_installPath);
        m_filter = StateFilter.InstallState;
        UpdateList();
        SetPath(m_uninstallPath);
        m_filter = StateFilter.UninstallState;
        UpdateList();
        foreach (ModInfo modInfo in m_installModInfo)
        {
            m_lastInstallModInfo.Add(modInfo);
        }
        if (m_modsContentList.Items.Count == 0)
        {
            List<string> commonPathList = new List<string>();
            foreach (string commonPath in m_commonPaths)
            {
                if ((androidSystem && commonPath.StartsWith("android:")) || (!androidSystem && !commonPath.StartsWith("android:")))
                {
                    commonPathList.Add(commonPath);
                }
            }
            string explanation = LanguageControl.Get(fName, 11);
            for (int i = 0; i < commonPathList.Count; i++)
            {
                explanation += "\n" + (i + 1) + ". " + commonPathList[i];
            }
            explanation += "\n\n" + LanguageControl.Get(fName, 12);
            if (commonPathList.Count == 0)
            {
                explanation = LanguageControl.Get(fName, 13);
            }
            DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 14), explanation, LanguageControl.Get(fName, 15), null, null));
        }
    }

    public override void Leave()
    {
        m_installModInfo.Clear();
        m_lastInstallModInfo.Clear();
    }

    public override void Update()
    {
        if (m_filter == StateFilter.InstallState)
        {
            m_upDirectoryButton.IsVisible = false;
            m_actionButton2.IsVisible = false;
        }
        else
        {
            m_upDirectoryButton.IsVisible = true;
            m_upDirectoryButton.IsEnabled = true;
            m_actionButton2.IsVisible = true;
            m_actionButton2.IsEnabled = true;
            m_actionButton2.Text = (m_path == m_uninstallPath) ? LanguageControl.Get(fName, 16) : LanguageControl.Get(fName, 17);
        }
        ModItem modItem = null;
        if (m_modsContentList.SelectedIndex.HasValue)
        {
            modItem = (m_modsContentList.Items[m_modsContentList.SelectedIndex.Value] as ModItem);
        }
        if (modItem != null && modItem.ExternalContentEntry.Type == ExternalContentType.Mod)
        {
            m_actionButton.Text = (m_filter == StateFilter.InstallState) ? LanguageControl.Get(fName, 18) : LanguageControl.Get(fName, 19);
            m_actionButton.IsEnabled = true;
        }
        else if (modItem != null && modItem.ExternalContentEntry.Type == ExternalContentType.Directory)
        {
            m_actionButton.Text = LanguageControl.Get(fName, 20);
            m_actionButton.IsEnabled = (modItem.ExternalContentEntry.Path != "android:/Android");
        }
        else
        {
            m_actionButton.Text = LanguageControl.Get(fName, 21);
            m_actionButton.IsEnabled = false;
        }

        if (m_actionButton.IsClicked)
        {
            if (modItem != null && modItem.ExternalContentEntry.Type == ExternalContentType.Mod)
            {
                string fileName = Storage.GetFileName(modItem.ExternalContentEntry.Path);
                string installPathName = Storage.CombinePaths(m_installPath, fileName);
                string uninstallPathName = modItem.ExternalContentEntry.Path;
                if (m_filter == StateFilter.InstallState)
                {
                    try
                    {
                        Storage.DeleteFile(installPathName);
                        UpdateList();
                        DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 22), fileName, LanguageControl.Get("Usual", "ok"), null, null));
                    }
                    catch (Exception e)
                    {
                        Log.Warning(e.Message);
                    }
                }
                else
                {
                    bool onlyPackage = true;
                    foreach (ModInfo modInfo in m_installModInfo)
                    {
                        if (modInfo.PackageName == modItem.ModInfo.PackageName)
                        {
                            onlyPackage = false;
                        }
                    }
                    if (!Storage.FileExists(installPathName) && onlyPackage)
                    {
                        Storage.CopyFile(uninstallPathName, installPathName);
                        m_installModInfo.Add(modItem.ModInfo);
                        DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 23), fileName, LanguageControl.Get("Usual", "ok"), null, null));
                    }
                    else
                    {
                        DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 24), fileName + LanguageControl.Get(fName, 25), LanguageControl.Get("Usual", "ok"), null, null));
                    }
                }
            }
            else if (modItem != null && modItem.ExternalContentEntry.Type == ExternalContentType.Directory)
            {
                BusyDialog busyDialog = new BusyDialog(LanguageControl.Get(fName, 26), LanguageControl.Get(fName, 27));
                DialogsManager.ShowDialog(null, busyDialog);
                count = 0;
                int successCount = ScanModFile(modItem.ExternalContentEntry.Path);
                DialogsManager.HideDialog(busyDialog);
                DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 28), string.Format(LanguageControl.Get(fName, 29), successCount), LanguageControl.Get(fName, 30), LanguageControl.Get(fName, 31), delegate (MessageDialogButton result)
                {
                    if (result == MessageDialogButton.Button1)
                    {
                        SetPath(m_uninstallPath);
                        UpdateList();
                    }
                }));
            }
        }
        if (m_actionButton2.IsClicked)
        {
            if (m_path == m_uninstallPath)
            {
                BusyDialog busyDialog = new BusyDialog(LanguageControl.Get(fName, 26), LanguageControl.Get(fName, 32));
                DialogsManager.ShowDialog(null, busyDialog);
                count = 0;
                int allCount = FastScanModFile();
                DialogsManager.HideDialog(busyDialog);
                if (allCount == 0)
                {
                    DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 4), LanguageControl.Get(fName, 33), LanguageControl.Get(fName, 34), LanguageControl.Get(fName, 10), delegate (MessageDialogButton result)
                    {
                        if (result == MessageDialogButton.Button1)
                        {
                            string url = "https://m.schub.top/com/mods/viewlist";
                            WebBrowserManager.LaunchBrowser(url);
                        }
                    }));
                }
                else
                {
                    DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 28), string.Format(LanguageControl.Get(fName, 35), allCount), LanguageControl.Get(fName, 30), null, delegate (MessageDialogButton result)
                    {
                        SetPath(m_uninstallPath);
                        UpdateList();
                    }));
                }
            }
            else
            {
                SetPath(m_uninstallPath);
                UpdateList();
            }
        }
        if (m_changeFilterButton.IsClicked)
        {
            if(m_filter == StateFilter.UninstallState)
            {
                m_filter = StateFilter.InstallState;
                SetPath(m_installPath);
            }
            else
            {
                m_filter = StateFilter.UninstallState;
                SetPath(m_uninstallPath);
            }
            UpdateList();
        }
        if (m_upDirectoryButton.IsClicked)
        {
            string directory = Storage.GetDirectoryName(m_path);
            if (m_path != "android:" && m_path != "app:")
            {
                if (directory.StartsWith("system:") && !directory.Contains("/")) directory = directory + "/";
                SetPath(directory);
                UpdateList();
            }
            if (m_path == "app:")
            {
                string systemPath = Storage.GetSystemPath(m_path);
                systemPath = systemPath.Replace("\\", "/");
                int index = systemPath.LastIndexOf('/');
                directory = "system:" + systemPath.Substring(0, index);
                SetPath(directory);
                UpdateList();
            }
        }
        if (base.Input.Back || base.Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
        {
            if (InstallModChange())
            {
                DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 4), LanguageControl.Get(fName, 38), LanguageControl.Get(fName, 39), LanguageControl.Get(fName, 31), delegate (MessageDialogButton result)
                {
                    if (result == MessageDialogButton.Button1)
                    {
                        ModsManager.Reboot();
                    }
                    if (result == MessageDialogButton.Button2)
                    {
                        ScreensManager.SwitchScreen("Content");
                    }
                }));
            }
            else
            {
                ScreensManager.SwitchScreen("Content");
            }
        }
    }

    public void UpdateList()
    {
        m_modsContentList.ClearItems();
        m_modsContentLabel.Text = LanguageControl.Get(fName, 40) + SetPathText(m_path);
        m_filterLabel.Text = (m_filter == StateFilter.UninstallState) ? LanguageControl.Get(fName, 36) : LanguageControl.Get(fName, 37);
        if (m_filter == StateFilter.InstallState) m_installModInfo.Clear();
        IEnumerable<string> fileNameList = Storage.ListFileNames(m_path);
        foreach (string fileName in fileNameList)
        {
            string extension = Storage.GetExtension(fileName);
            if (!string.IsNullOrEmpty(extension) && extension.ToLower() == ".scmod")
            {
                string pathName = Storage.CombinePaths(m_path, fileName);
                Stream stream = Storage.OpenFile(pathName, OpenFileMode.Read);
                try
                {
                    ModEntity modEntity = new ModEntity(ZipArchive.Open(stream, true));
                    if (modEntity.modInfo == null || string.IsNullOrEmpty(modEntity.modInfo.PackageName) || !modEntity.modInfo.ApiVersion.Contains("1.4")) continue;
                    ExternalContentEntry externalContentEntry = new ExternalContentEntry
                    {
                        Type = ExternalContentType.Mod,
                        Path = pathName,
                        Size = Storage.GetFileSize(pathName),
                        Time = Storage.GetFileLastWriteTime(pathName)
                    };
                    Subtexture subtexture = ExternalContentManager.GetEntryTypeIcon(ExternalContentType.Mod);
                    if (modEntity.Icon != null)
                    {
                        subtexture = new Subtexture(modEntity.Icon, Vector2.Zero, Vector2.One);
                    }
                    if (m_filter == StateFilter.InstallState)
                    {
                        m_installModInfo.Add(modEntity.modInfo);
                    }
                    m_modsContentList.AddItem(new ModItem(modEntity.modInfo, externalContentEntry, subtexture));
                }
                catch
                {
                    throw new InvalidOperationException("Mod file acquisition failed");
                }
                finally
                {
                    if (stream != null) stream.Close();
                }
            }
        }
        IEnumerable<string> directoryNameList = Storage.ListDirectoryNames(m_path);
        foreach (string directoryName in directoryNameList)
        {
            string directory = Storage.CombinePaths(m_path, directoryName);
            ModInfo modInfo = null;
            Subtexture subtexture = ExternalContentManager.GetEntryTypeIcon(ExternalContentType.Directory);
            ExternalContentEntry externalContentEntry = new ExternalContentEntry
            {
                Type = ExternalContentType.Directory,
                Path = directory,
                Size = 0,
                Time = Storage.GetFileLastWriteTime(directory)
            };
            m_modsContentList.AddItem(new ModItem(modInfo, externalContentEntry, subtexture));
        }
    }

    public int ScanModFile(string path)
    {
        foreach (string fileName in Storage.ListFileNames(path))
        {
            if (path == m_uninstallPath) continue;
            string extension = Storage.GetExtension(fileName);
            if (!string.IsNullOrEmpty(extension) && extension.ToLower() == ".scmod")
            {
                string pathName = Storage.CombinePaths(path, fileName);
                Stream stream = null;
                try
                {
                    ModEntity modEntity = null;
                    try
                    {
                        stream = Storage.OpenFile(pathName, OpenFileMode.Read);
                        modEntity = new ModEntity(ZipArchive.Open(stream, true));
                    }
                    catch
                    {
                    }
                    if (stream == null || modEntity == null) continue;
                    if (modEntity.modInfo == null || string.IsNullOrEmpty(modEntity.modInfo.PackageName) || !modEntity.modInfo.ApiVersion.Contains("1.4")) continue;
                    string uninstallPathName = Storage.CombinePaths(m_uninstallPath, fileName);
                    if (!Storage.FileExists(uninstallPathName))
                    {
                        Storage.CopyFile(pathName, uninstallPathName);
                        count++;
                    }
                }
                catch
                {
                    Log.Error("Mod file scan failed");
                    throw new InvalidOperationException("Mod file scan failed");
                }
                finally
                {
                    if (stream != null) stream.Close();
                }
            }
        }
        foreach (string directory in Storage.ListDirectoryNames(path))
        {
            ScanModFile(Storage.CombinePaths(path, directory));
        }
        return count;
    }

    public int FastScanModFile()
    {
        int allCount = 0;
        foreach (string commonPath in m_commonPaths)
        {
            if ((androidSystem && commonPath.StartsWith("android:")) || (!androidSystem && !commonPath.StartsWith("android:")))
            {
                try
                {
                    if (Storage.DirectoryExists(commonPath))
                    {
                        count = 0;
                        int sucesssCount = ScanModFile(commonPath);
                        allCount += sucesssCount;
                    }
                }
                catch
                {
                    DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 4), string.Format(LanguageControl.Get(fName, 41), commonPath), LanguageControl.Get(fName, 15), null, null));
                }
            }
        }
        return allCount;
    }

    public bool InstallModChange()
    {
        bool IsChange = false;
        if (m_installModInfo.Count != m_lastInstallModInfo.Count)
        {
            return true;
        }
        foreach (ModInfo modInfo in m_installModInfo)
        {
            if (!m_lastInstallModInfo.Contains(modInfo))
            {
                IsChange = true;
                break;
            }
        }
        return IsChange;
    }

    public void SetPath(string path)
    {
        path = path.Replace("\\", "/");
        if (path != m_path)
        {
            m_lastPath = m_path;
            m_path = path;
        }
    }

    public string SetPathText(string path)
    {
        string newText = Storage.GetSystemPath(path);
        string[] arPath = path.Split('/');
        if (arPath.Length > 5)
        {
            newText = ".../" + arPath[arPath.Length - 3] + "/" + arPath[arPath.Length - 2] + "/" + arPath[arPath.Length - 1];
        }
        return newText;
    }
}
