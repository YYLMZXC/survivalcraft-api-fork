using Engine;
using Engine.Graphics;
using Engine.Media;
using GameEntitySystem;
using Jint;

namespace Game;

public class InterfaceImplementForSurvivalCraft(IModLoader parent) : SurvivalCraftModInterface(parent)
{
    protected internal override void _InterfaceInitialized()
    {
        RegisterHook("OnCameraChange");
        RegisterHook("OnPlayerDead");
        RegisterHook("OnModelRendererDrawExtra");
        RegisterHook("GetMaxInstancesCount");
    }

    public override void OnCameraChange(ComponentPlayer componentPlayer, ComponentGui componentGui)
    {
        GameWidget gameWidget = componentPlayer.GameWidget;
        switch (gameWidget.ActiveCamera)
        {
            case FppCamera:
                gameWidget.ActiveCamera = gameWidget.FindCamera<TppCamera>();
                componentGui.DisplaySmallMessage(LanguageControl.Get(ComponentGui.fName, 9), Color.White,
                    blinking: false, playNotificationSound: false);
                break;
            case TppCamera:
                gameWidget.ActiveCamera = gameWidget.FindCamera<OrbitCamera>();
                componentGui.DisplaySmallMessage(LanguageControl.Get(ComponentGui.fName, 10), Color.White,
                    blinking: false, playNotificationSound: false);
                break;
            case OrbitCamera:
                gameWidget.ActiveCamera = gameWidget.FindCamera<FixedCamera>();
                componentGui.DisplaySmallMessage(LanguageControl.Get(ComponentGui.fName, 11), Color.White,
                    blinking: false, playNotificationSound: false);
                break;
            default:
            {
                if (componentGui.m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative &&
                    gameWidget.ActiveCamera is FixedCamera)
                {
                    gameWidget.ActiveCamera = gameWidget.FindCamera<DebugCamera>();
                    componentGui.DisplaySmallMessage(LanguageControl.Get(ComponentGui.fName, 19), Color.White,
                        blinking: false, playNotificationSound: false);
                    break;
                }

                gameWidget.ActiveCamera = gameWidget.FindCamera<FppCamera>();
                componentGui.DisplaySmallMessage(LanguageControl.Get(ComponentGui.fName, 12), Color.White,
                    blinking: false, playNotificationSound: false);

                break;
            }
        }
    }
    
    public override void OnPlayerDead(PlayerData playerData)
    {
        playerData.GameWidget.ActiveCamera = playerData.GameWidget.FindCamera<DeathCamera>();
        if (playerData.ComponentPlayer != null)
        {
            string text = playerData.ComponentPlayer.ComponentHealth.CauseOfDeath;
            if (string.IsNullOrEmpty(text))
            {
                text = LanguageControl.Get(PlayerData.fName, 12);
            }

            string arg = string.Format(LanguageControl.Get(PlayerData.fName, 13), text);
            switch (playerData.m_subsystemGameInfo.WorldSettings.GameMode)
            {
                case GameMode.Cruel:
                    playerData.ComponentPlayer.ComponentGui.DisplayLargeMessage(
                        LanguageControl.Get(PlayerData.fName, 6),
                        string.Format(LanguageControl.Get(PlayerData.fName, 7), arg,
                            LanguageControl.Get("GameMode",
                                playerData.m_subsystemGameInfo.WorldSettings.GameMode.ToString())), 30f, 1.5f);
                    break;
                case GameMode.Adventure when
                    !playerData.m_subsystemGameInfo.WorldSettings.IsAdventureRespawnAllowed:
                    playerData.ComponentPlayer.ComponentGui.DisplayLargeMessage(
                        LanguageControl.Get(PlayerData.fName, 6),
                        string.Format(LanguageControl.Get(PlayerData.fName, 8), arg), 30f, 1.5f);
                    break;
                default:
                    playerData.ComponentPlayer.ComponentGui.DisplayLargeMessage(
                        LanguageControl.Get(PlayerData.fName, 6),
                        string.Format(LanguageControl.Get(PlayerData.fName, 9), arg), 30f, 1.5f);
                    break;
            }
        }

        playerData.Level = MathUtils.Max(MathUtils.Floor(playerData.Level / 2f), 1f);
    }

    public override void OnModelRendererDrawExtra(SubsystemModelsRenderer modelsRenderer,
        SubsystemModelsRenderer.ModelData modelData, Camera camera, float? alphaThreshold)
    {
        ComponentModel componentModel = modelData.ComponentModel;
        if (componentModel is not ComponentHumanModel) return;

        ComponentPlayer componentPlayer = componentModel.Entity.FindComponent<ComponentPlayer>();
        if (componentPlayer == null || camera.GameWidget.PlayerData == componentPlayer.PlayerData) return;

        ComponentCreature componentCreature = componentPlayer.ComponentMiner.ComponentCreature;
        var position =
            Vector3.Transform(
                componentCreature.ComponentBody.Position +
                (1.02f * Vector3.UnitY * componentCreature.ComponentBody.BoxSize.Y), camera.ViewMatrix);
        if (!(position.Z < 0f)) return;

        var color = Color.Lerp(Color.White, Color.Transparent,
            MathUtils.Saturate((position.Length() - 4f) / 3f));
        if (color.A <= 8) return;


        var right = Vector3.TransformNormal(
            0.005f * Vector3.Normalize(Vector3.Cross(camera.ViewDirection, Vector3.UnitY)),
            camera.ViewMatrix);
        var down = Vector3.TransformNormal(-0.005f * Vector3.UnitY, camera.ViewMatrix);
        BitmapFont font = LabelWidget.BitmapFont;

        modelsRenderer.PrimitivesRenderer
            .FontBatch(font, 1, DepthStencilState.DepthRead, RasterizerState.CullNoneScissor,
                BlendState.AlphaBlend, SamplerState.LinearClamp)
            .QueueText(componentPlayer.PlayerData.Name, position, right, down, color,
                TextAnchor.HorizontalCenter | TextAnchor.Bottom);
    }

    public override int GetMaxInstancesCount()
    {
        return 7;
    }
}