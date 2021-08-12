﻿using Engine;
using Engine.Graphics;
using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public class ManageContentScreen : Screen
{
    public class ListItem
    {
        public ExternalContentType Type;

        public bool IsBuiltIn;

        public string Name;

        public string DisplayName;

        public DateTime CreationTime;

        public int UseCount;
        public bool IsClick;
        public Texture2D Texture;

        public ModEntity ModEntity;
    }
    public static string fName = "ManageContentScreen";

    public ListPanelWidget m_contentList;

    public ButtonWidget m_deleteButton;

    public ButtonWidget m_uploadButton;

    public LabelWidget m_filterLabel;

    public ButtonWidget m_changeFilterButton;

    public BlocksTexturesCache m_blocksTexturesCache = new BlocksTexturesCache();

    public CharacterSkinsCache m_characterSkinsCache = new CharacterSkinsCache();

    public bool changeed = false;

    public ExternalContentType m_filter;

    public ManageContentScreen()
    {
        XElement node = ContentManager.Get<XElement>("Screens/ManageContentScreen");
        LoadContents(this, node);
        m_contentList = Children.Find<ListPanelWidget>("ContentList");
        m_deleteButton = Children.Find<ButtonWidget>("DeleteButton");
        m_uploadButton = Children.Find<ButtonWidget>("UploadButton");
        m_changeFilterButton = Children.Find<ButtonWidget>("ChangeFilter");
        m_filterLabel = Children.Find<LabelWidget>("Filter");
        m_contentList.ItemWidgetFactory = delegate (object obj)
        {
            var listItem = (ListItem)obj;
            ContainerWidget containerWidget;
            switch (listItem.Type) {
                case ExternalContentType.BlocksTexture: {
                        XElement node2 = ContentManager.Get<XElement>("Widgets/BlocksTextureItem");
                        containerWidget = (ContainerWidget)LoadWidget(this, node2, null);
                        RectangleWidget rectangleWidget = containerWidget.Children.Find<RectangleWidget>("BlocksTextureItem.Icon");
                        LabelWidget labelWidget = containerWidget.Children.Find<LabelWidget>("BlocksTextureItem.Text");
                        LabelWidget labelWidget2 = containerWidget.Children.Find<LabelWidget>("BlocksTextureItem.Details");
                        Texture2D texture = m_blocksTexturesCache.GetTexture(listItem.Name);
                        BlocksTexturesManager.GetCreationDate(listItem.Name);
                        rectangleWidget.Subtexture = new Subtexture(texture, Vector2.Zero, Vector2.One);
                        labelWidget.Text = listItem.DisplayName;
                        labelWidget2.Text = string.Format(LanguageControl.Get(fName, 1), texture.Width, texture.Height);
                        if (!listItem.IsBuiltIn)
                        {
                            labelWidget2.Text += $" | {listItem.CreationTime.ToLocalTime():dd MMM yyyy HH:mm}";
                            if (listItem.UseCount > 0)
                            {
                                labelWidget2.Text += string.Format(LanguageControl.Get(fName, 2), listItem.UseCount);
                            }
                        }

                        break;
                    }
                case ExternalContentType.FurniturePack: {
                        XElement node3 = ContentManager.Get<XElement>("Widgets/FurniturePackItem");
                        containerWidget = (ContainerWidget)LoadWidget(this, node3, null);
                        LabelWidget labelWidget3 = containerWidget.Children.Find<LabelWidget>("FurniturePackItem.Text");
                        LabelWidget labelWidget4 = containerWidget.Children.Find<LabelWidget>("FurniturePackItem.Details");
                        labelWidget3.Text = listItem.DisplayName;
                        try
                        {
                            List<FurnitureDesign> designs = FurniturePacksManager.LoadFurniturePack(null, listItem.Name);
                            labelWidget4.Text = string.Format(LanguageControl.Get(fName, 3), FurnitureDesign.ListChains(designs).Count);
                            if (string.IsNullOrEmpty(listItem.Name))
                            {
                                return containerWidget;
                            }
                            labelWidget4.Text += $" | {listItem.CreationTime.ToLocalTime():dd MMM yyyy HH:mm}";
                            return containerWidget;
                        }
                        catch (Exception ex)
                        {
                            labelWidget4.Text = labelWidget4.Text + LanguageControl.Error + ex.Message;
                            return containerWidget;
                        }
                        break;
                    }
                case ExternalContentType.CharacterSkin: {
                        XElement node4 = ContentManager.Get<XElement>("Widgets/CharacterSkinItem");
                        containerWidget = (ContainerWidget)LoadWidget(this, node4, null);
                        PlayerModelWidget playerModelWidget = containerWidget.Children.Find<PlayerModelWidget>("CharacterSkinItem.Model");
                        LabelWidget labelWidget5 = containerWidget.Children.Find<LabelWidget>("CharacterSkinItem.Text");
                        LabelWidget labelWidget6 = containerWidget.Children.Find<LabelWidget>("CharacterSkinItem.Details");
                        Texture2D texture2 = m_characterSkinsCache.GetTexture(listItem.Name);
                        playerModelWidget.PlayerClass = PlayerClass.Male;
                        playerModelWidget.CharacterSkinTexture = texture2;
                        labelWidget5.Text = listItem.DisplayName;
                        labelWidget6.Text = string.Format(LanguageControl.Get(fName, 4), texture2.Width, texture2.Height);
                        if (!listItem.IsBuiltIn)
                        {
                            labelWidget6.Text += $" | {listItem.CreationTime.ToLocalTime():dd MMM yyyy HH:mm}";
                            if (listItem.UseCount > 0)
                            {
                                labelWidget6.Text += string.Format(LanguageControl.Get(fName, 2), listItem.UseCount);
                            }
                        }

                        break;
                    }
                case ExternalContentType.Mod: {
                        XElement node2 = ContentManager.Get<XElement>("Widgets/BlocksTextureItem");
                        containerWidget = (ContainerWidget)LoadWidget(this, node2, null);
                        RectangleWidget rectangleWidget = containerWidget.Children.Find<RectangleWidget>("BlocksTextureItem.Icon");
                        LabelWidget labelWidget = containerWidget.Children.Find<LabelWidget>("BlocksTextureItem.Text");
                        LabelWidget labelWidget2 = containerWidget.Children.Find<LabelWidget>("BlocksTextureItem.Details");
                        rectangleWidget.Subtexture = listItem.Texture==null ? ContentManager.Get<Subtexture>("Textures/Atlas/WorldIcon"):new Subtexture(listItem.Texture,Vector2.Zero,Vector2.One);
                        rectangleWidget.TextureLinearFilter = true;
                        labelWidget.Text = listItem.DisplayName;
                        labelWidget2.Text = listItem.Name;
                        if (!listItem.IsBuiltIn)
                        {
                            labelWidget2.Text += $" | {listItem.CreationTime.ToLocalTime():dd MMM yyyy HH:mm}";
                            if (listItem.UseCount > 0)
                            {
                                labelWidget2.Text += string.Format(LanguageControl.Get(fName, 2), listItem.UseCount);
                            }
                        }
                        break;
                    }
                default: throw new InvalidOperationException(LanguageControl.Get(fName, 10));

            }
            return containerWidget;
        };
        m_contentList.ItemClicked += (obj) =>{
            var listItem = (ListItem)obj;
            if (listItem.Type == ExternalContentType.Mod && listItem.IsClick)
            {
                var messageDialog = new MessageDialog(listItem.ModEntity.modInfo.Name, listItem.ModEntity.modInfo.Description, LanguageControl.Ok, LanguageControl.Cancel, (btn) =>{
                    DialogsManager.HideAllDialogs();
                    listItem.IsClick = false;

                });
                DialogsManager.ShowDialog(this,messageDialog);
            }
            else {
                listItem.IsClick = true;
            }
        };
    }

    public override void Enter(object[] parameters)
    {
        UpdateList();
    }

    public override void Leave()
    {
        m_blocksTexturesCache.Clear();
        m_characterSkinsCache.Clear();
    }

    public override void Update()
    {
        var selectedItem = (ListItem)m_contentList.SelectedItem;
        if (selectedItem != null) {
            m_deleteButton.IsEnabled = !selectedItem.IsBuiltIn;
            m_uploadButton.IsEnabled = !selectedItem.IsBuiltIn;
            if (selectedItem.Type == ExternalContentType.Mod)
            {
                m_deleteButton.Text = ModsManager.DisabledMods.Contains(selectedItem.ModEntity.modInfo) ? LanguageControl.Enable : LanguageControl.Disable;
                m_deleteButton.IsEnabled = !(selectedItem.ModEntity is SurvivalCrafModEntity || selectedItem.ModEntity is FastDebugModEntity);
            }
            else
            {
                m_deleteButton.Text = LanguageControl.Delete;
            }

        }
        m_filterLabel.Text = GetFilterDisplayName(m_filter);
        if (m_deleteButton.IsClicked)
        {
            string smallMessage = (selectedItem.UseCount <= 0) ? string.Format(LanguageControl.Get(fName, 5), selectedItem.DisplayName) : string.Format(LanguageControl.Get(fName, 6), selectedItem.DisplayName, selectedItem.UseCount);
            if (selectedItem.Type == ExternalContentType.Mod) {
                smallMessage = (ModsManager.DisabledMods.Contains(selectedItem.ModEntity.modInfo) ? LanguageControl.Enable : LanguageControl.Disable) + $"[{selectedItem.ModEntity.modInfo.Name}]?";
            }
            DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 9), smallMessage, LanguageControl.Yes, LanguageControl.No, delegate (MessageDialogButton button)
            {
                if (button == MessageDialogButton.Button1)
                {
                    if (selectedItem.Type == ExternalContentType.Mod)
                    {
                        changeed = true;
                        if (ModsManager.DisabledMods.Contains(selectedItem.ModEntity.modInfo))
                        {
                            selectedItem.ModEntity.Dispose();
                            ModsManager.DisabledMods.Remove(selectedItem.ModEntity.modInfo);
                            ModsManager.ModList.Add(selectedItem.ModEntity);
                        }
                        else {
                            ModsManager.DisabledMods.Add(selectedItem.ModEntity.modInfo);
                            ModsManager.ModList.Remove(selectedItem.ModEntity);
                        }
                    }
                    else {
                        ExternalContentManager.DeleteExternalContent(selectedItem.Type, selectedItem.Name);
                    }
                    UpdateList();
                }
            }));
        }
        if (m_uploadButton.IsClicked)
        {
            ExternalContentManager.ShowUploadUi(selectedItem.Type, selectedItem.Name);
        }
        if (m_changeFilterButton.IsClicked)
        {
            var list = new List<ExternalContentType>();
            list.Add(ExternalContentType.Unknown);
            list.Add(ExternalContentType.BlocksTexture);
            list.Add(ExternalContentType.CharacterSkin);
            list.Add(ExternalContentType.FurniturePack);
            DialogsManager.ShowDialog(null, new ListSelectionDialog(LanguageControl.Get(fName, 7), list, 60f, (object item) => GetFilterDisplayName((ExternalContentType)item), delegate (object item)
            {
                if ((ExternalContentType)item != m_filter)
                {
                    m_filter = (ExternalContentType)item;
                    UpdateList();
                }
            }));
        }
        if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
        {
            if (changeed) {
                DialogsManager.ShowDialog(this,new MessageDialog(LanguageControl.Warning, LanguageControl.Get(GetType().Name,11), LanguageControl.Yes, LanguageControl.No, (btn)=>{
                    DialogsManager.HideAllDialogs();
                    if (btn == MessageDialogButton.Button1)
                    {
                        SettingsManager.SaveSettings();
                        SettingsManager.LoadSettings();
                        ScreensManager.SwitchScreen("Loading");
                    }
                    else {
                        ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
                    }
                }));
            }else ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
        }
    }

    public void UpdateList()
    {
        WorldsManager.UpdateWorldsList();
        var list = new List<ListItem>();
        if (m_filter == ExternalContentType.BlocksTexture || m_filter == ExternalContentType.Unknown)
        {
            BlocksTexturesManager.UpdateBlocksTexturesList();
            foreach (string name2 in BlocksTexturesManager.BlockTexturesNames)
            {
                list.Add(new ListItem
                {
                    Name = name2,
                    IsBuiltIn = BlocksTexturesManager.IsBuiltIn(name2),
                    Type = ExternalContentType.BlocksTexture,
                    DisplayName = BlocksTexturesManager.GetDisplayName(name2),
                    CreationTime = BlocksTexturesManager.GetCreationDate(name2),
                    UseCount = WorldsManager.WorldInfos.Count((WorldInfo wi) => wi.WorldSettings.BlocksTextureName == name2)
                });
            }
        }
        if (m_filter == ExternalContentType.CharacterSkin || m_filter == ExternalContentType.Unknown)
        {
            CharacterSkinsManager.UpdateCharacterSkinsList();
            foreach (string name in CharacterSkinsManager.CharacterSkinsNames)
            {
                list.Add(new ListItem
                {
                    Name = name,
                    IsBuiltIn = CharacterSkinsManager.IsBuiltIn(name),
                    Type = ExternalContentType.CharacterSkin,
                    DisplayName = CharacterSkinsManager.GetDisplayName(name),
                    CreationTime = CharacterSkinsManager.GetCreationDate(name),
                    UseCount = WorldsManager.WorldInfos.Count((WorldInfo wi) => wi.PlayerInfos.Any((PlayerInfo pi) => pi.CharacterSkinName == name))
                });
            }
        }
        if (m_filter == ExternalContentType.FurniturePack || m_filter == ExternalContentType.Unknown)
        {
            FurniturePacksManager.UpdateFurniturePacksList();
            foreach (string furniturePackName in FurniturePacksManager.FurniturePackNames)
            {
                list.Add(new ListItem
                {
                    Name = furniturePackName,
                    IsBuiltIn = false,
                    Type = ExternalContentType.FurniturePack,
                    DisplayName = FurniturePacksManager.GetDisplayName(furniturePackName),
                    CreationTime = FurniturePacksManager.GetCreationDate(furniturePackName)
                });
            }
        }
        if (m_filter == ExternalContentType.Mod || m_filter == ExternalContentType.Unknown)
        {
            foreach (ModEntity modEntity in ModsManager.ModList)
            {
                string dis = string.Empty;
                if (ModsManager.DisabledMods.Contains(modEntity.modInfo)) dis = "[已禁用]";
                string author = string.IsNullOrEmpty(modEntity.modInfo.Author) ? "无" : modEntity.modInfo.Author;
                list.Add(new ListItem
                {
                    Name = $"[模组]{modEntity.modInfo.Description}<{author}>",
                    IsBuiltIn = false,
                    Type = ExternalContentType.Mod,
                    DisplayName = $"{dis}{modEntity.modInfo.Name} 版本:{modEntity.modInfo.Version}",
                    CreationTime = DateTime.Now,
                    Texture=modEntity.Icon,
                    ModEntity=modEntity
                });
            }
        }
        list.Sort(delegate (ListItem o1, ListItem o2)
        {
            if (o1.IsBuiltIn && !o2.IsBuiltIn)
            {
                return -1;
            }
            if (o2.IsBuiltIn && !o1.IsBuiltIn)
            {
                return 1;
            }
            if (string.IsNullOrEmpty(o1.Name) && !string.IsNullOrEmpty(o2.Name))
            {
                return -1;
            }
            return (!string.IsNullOrEmpty(o1.Name) && string.IsNullOrEmpty(o2.Name)) ? 1 : string.Compare(o1.DisplayName, o2.DisplayName);
        });
        m_contentList.ClearItems();
        foreach (ListItem item in list)
        {
            m_contentList.AddItem(item);
        }
    }

    public static string GetFilterDisplayName(ExternalContentType filter)
    {
        if (filter == ExternalContentType.Unknown)
        {
            return LanguageControl.Get(fName, 8);
        }
        return ExternalContentManager.GetEntryTypeDescription(filter);
    }
}
