using System;
using System.Collections.Generic;
using Engine;
using Engine.Content;
using System.Threading;
using System.Threading.Tasks;

namespace Game
{
    public class ModsInitScreen:Screen
    {
        public static LabelWidget LogLabel = new LabelWidget() {FontScale=0.7f, Color = Color.Red, HorizontalAlignment = WidgetAlignment.Near, VerticalAlignment = WidgetAlignment.Far, Margin = new Vector2(200, 40) };
        public CanvasWidget Canvas = new CanvasWidget() { Size=new Vector2(float.PositiveInfinity)};
        public LabelWidget Title = new LabelWidget() {HorizontalAlignment=WidgetAlignment.Center,VerticalAlignment=WidgetAlignment.Center,Color=Color.Red,Text="Mod加载器v1.0" };
        public RectangleWidget RectangleWidget = new RectangleWidget() { FillColor=Color.White};
        public List<Action> m_loadActions = new List<Action>();
        public ModsInitScreen(){
            Canvas.Children.Add(Title);
            Canvas.Children.Add(RectangleWidget);
            Canvas.Children.Add(LogLabel);
            Children.Add(Canvas);
            LogLabel.Text = "Loading";
            try {
                ContentManager.Initialize();
                ContentManager.Get("Shaders/AlphaTested");
            }
            catch (Exception e){ 
            
            }
            Task.Run(() =>
            {
                ContentManager.Initialize();
                //检查所有文件
                ReadOnlyList<ContentInfo> infos = ContentManager.List();
                foreach (ContentInfo item in infos)
                {
                    try
                    {
                        ContentInfo localContentInfo = item;
                        SetLog("加载:" + item.Name);
                        ContentManager.Get(localContentInfo.Name);
                    }
                    catch (Exception e)
                    {
                        SetLog("加载:" + item.Name + "出错\n" + e.Message);
                        return;
                    }
                }
                SetLog("开始加载Mod");
                ModsManager.Initialize();
                LanguageControl.init(ModsManager.modSettings.languageType);
                List<FileEntry> list = ModsManager.GetEntries(".pak");
                foreach (FileEntry fileEntry in list)
                {
                    ContentManager.AddPak(fileEntry.Stream);
                }
                SetLog("初始化设置");
                SettingsManager.Initialize();
                SetLog("初始化性能分析器");
                AnalyticsManager.Initialize();
                SetLog("初始化VR");
                VersionsManager.Initialize();
                SetLog("初始化外部储存内容管理");
                ExternalContentManager.Initialize();
                SetLog("初始化Motd");
                MotdManager.Update();
            });
        }
        public static void SetLog(string text) {
            LogLabel.Text = text;
        
        }

        public void AddLoadAction(Action action)
        {
            m_loadActions.Add(action);
        }
        public override void Enter(object[] parameters)
        {
        }
        public override void Leave()
        {
        }
        public override void Update()
        {
            if (m_loadActions.Count > 0) {
                m_loadActions[0].Invoke();
                m_loadActions.RemoveAt(0);
            }
        }

    }
}
