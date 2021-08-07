using Engine;
using Engine.Graphics;
using Engine.Serialization;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using Engine.Media;
namespace Game
{
    public class SurvivalCraftModLoader:ModLoader
    {
        public List<ComponentEatPickableBehavior> EatPickableBehaviors = new List<ComponentEatPickableBehavior>();
        public List<ComponentChaseBehavior> ChaseBehaviors = new List<ComponentChaseBehavior>();
        public List<ComponentCreatureModel> CreatureModels = new List<ComponentCreatureModel>();
        public List<ComponentHerdBehavior> HerdBehaviors = new List<ComponentHerdBehavior>();
        public List<ComponentRunAwayBehavior> RunAwayBehaviors = new List<ComponentRunAwayBehavior>();
        public List<ComponentSwimAwayBehavior> SwimAwayBehaviors = new List<ComponentSwimAwayBehavior>();
        public List<ComponentSleep> Sleeps = new List<ComponentSleep>();
        public List<ComponentVitalStats> VitalStats = new List<ComponentVitalStats>();
        public override void __ModInitialize()
        {
            ModsManager.RegisterHook("OnCameraChange", this);
            ModsManager.RegisterHook("OnPlayerDead", this);
            ModsManager.RegisterHook("OnModelRendererDrawExtra", this);
            ModsManager.RegisterHook("GetMaxInstancesCount", this);
            ModsManager.RegisterHook("PickableAdded", this);
            ModsManager.RegisterHook("PickableRemoved", this);
            ModsManager.RegisterHook("ProjectileAdded", this);
            ModsManager.RegisterHook("ProjectileRemoved", this);
            ModsManager.RegisterHook("OnEntityAdd", this);
            ModsManager.RegisterHook("OnEntityRemove", this);
            ModsManager.RegisterHook("OnBodyAttacked", this);
        }
        public override void OnCameraChange(ComponentPlayer m_componentPlayer,ComponentGui componentGui)
        {
            GameWidget gameWidget = m_componentPlayer.GameWidget;
            if (gameWidget.ActiveCamera is FppCamera)
            {
                gameWidget.ActiveCamera = gameWidget.FindCamera<TppCamera>();
                componentGui.DisplaySmallMessage(LanguageControl.Get(ComponentGui.fName, 9), Color.White, blinking: false, playNotificationSound: false);
            }
            else if (gameWidget.ActiveCamera is TppCamera)
            {
                gameWidget.ActiveCamera = gameWidget.FindCamera<OrbitCamera>();
                componentGui.DisplaySmallMessage(LanguageControl.Get(ComponentGui.fName, 10), Color.White, blinking: false, playNotificationSound: false);
            }
            else if (gameWidget.ActiveCamera is OrbitCamera)
            {
                gameWidget.ActiveCamera = gameWidget.FindCamera<FixedCamera>();
                componentGui.DisplaySmallMessage(LanguageControl.Get(ComponentGui.fName, 11), Color.White, blinking: false, playNotificationSound: false);
            }
            else
            {
                if (componentGui.m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative && gameWidget.ActiveCamera is FixedCamera)
                {
                    gameWidget.ActiveCamera = gameWidget.FindCamera<DebugCamera>();
                    componentGui.DisplaySmallMessage(LanguageControl.Get(ComponentGui.fName, 19), Color.White, blinking: false, playNotificationSound: false);
                }
                else
                {
                    gameWidget.ActiveCamera = gameWidget.FindCamera<FppCamera>();
                    componentGui.DisplaySmallMessage(LanguageControl.Get(ComponentGui.fName, 12), Color.White, blinking: false, playNotificationSound: false);
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
                if (playerData.m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Cruel)
                {
                    playerData.ComponentPlayer.ComponentGui.DisplayLargeMessage(LanguageControl.Get(PlayerData.fName, 6), string.Format(LanguageControl.Get(PlayerData.fName, 7), arg, LanguageControl.Get("GameMode", playerData.m_subsystemGameInfo.WorldSettings.GameMode.ToString())), 30f, 1.5f);
                }
                else if (playerData.m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Adventure && !playerData.m_subsystemGameInfo.WorldSettings.IsAdventureRespawnAllowed)
                {
                    playerData.ComponentPlayer.ComponentGui.DisplayLargeMessage(LanguageControl.Get(PlayerData.fName, 6), string.Format(LanguageControl.Get(PlayerData.fName, 8), arg), 30f, 1.5f);
                }
                else
                {
                    playerData.ComponentPlayer.ComponentGui.DisplayLargeMessage(LanguageControl.Get(PlayerData.fName, 6), string.Format(LanguageControl.Get(PlayerData.fName, 9), arg), 30f, 1.5f);
                }
            }
            playerData.Level = MathUtils.Max(MathUtils.Floor(playerData.Level / 2f), 1f);
        }
        public override void OnModelRendererDrawExtra(SubsystemModelsRenderer modelsRenderer, ComponentModel componentModel, Camera camera, float? alphaThreshold)
        {
            if (componentModel is ComponentHumanModel) {
                ComponentPlayer m_componentPlayer = componentModel.Entity.FindComponent<ComponentPlayer>();
                if (m_componentPlayer != null && camera.GameWidget.PlayerData != m_componentPlayer.PlayerData)
                {
                    ComponentCreature m_componentCreature = m_componentPlayer.ComponentMiner.ComponentCreature;
                    var position = Vector3.Transform(m_componentCreature.ComponentBody.Position + 1.02f * Vector3.UnitY * m_componentCreature.ComponentBody.BoxSize.Y, camera.ViewMatrix);
                    if (position.Z < 0f)
                    {
                        var color = Color.Lerp(Color.White, Color.Transparent, MathUtils.Saturate((position.Length() - 4f) / 3f));
                        if (color.A > 8)
                        {
                            var right = Vector3.TransformNormal(0.005f * Vector3.Normalize(Vector3.Cross(camera.ViewDirection, Vector3.UnitY)), camera.ViewMatrix);
                            var down = Vector3.TransformNormal(-0.005f * Vector3.UnitY, camera.ViewMatrix);
                            BitmapFont font = LabelWidget.BitmapFont;
                            modelsRenderer.PrimitivesRenderer.FontBatch(font, 1, DepthStencilState.DepthRead, RasterizerState.CullNoneScissor, BlendState.AlphaBlend, SamplerState.LinearClamp).QueueText(m_componentPlayer.PlayerData.Name, position, right, down, color, TextAnchor.HorizontalCenter | TextAnchor.Bottom);
                        }
                    }
                }
            }
        }
        public override int GetMaxInstancesCount()
        {
            return 7;
        }
        public override void PickableAdded(SubsystemPickables subsystemPickables, Pickable pickable)
        {
            foreach (ComponentEatPickableBehavior behavior in EatPickableBehaviors)
            {
                if (behavior.TryAddPickable(pickable) && behavior.m_pickable == null)
                {
                    behavior.m_pickable = pickable;
                }
            }
        }
        public override void PickableRemoved(SubsystemPickables subsystemPickables, Pickable pickable)
        {
            foreach (ComponentEatPickableBehavior behavior in EatPickableBehaviors)
            {
                behavior.m_pickables.Remove(pickable);
                if (behavior.m_pickable == pickable)
                {
                    behavior.m_pickable = null;
                }
            }
        }
        public override void ProjectileAdded(SubsystemProjectiles subsystemProjectiles, Projectile projectile)
        {
            SubsystemBombBlockBehavior subsystemBombBlockBehavior = subsystemProjectiles.Project.FindSubsystem<SubsystemBombBlockBehavior>();
            subsystemBombBlockBehavior.ScanProjectile(projectile);
        }
        public override void ProjectileRemoved(SubsystemProjectiles subsystemProjectiles, Projectile projectile)
        {
            SubsystemBombBlockBehavior subsystemBombBlockBehavior = subsystemProjectiles.Project.FindSubsystem<SubsystemBombBlockBehavior>();
            subsystemBombBlockBehavior.m_projectiles.Remove(projectile);
        }
        public override void OnEntityAdd(Entity entity)
        {
            ComponentEatPickableBehavior behavior = entity.FindComponent<ComponentEatPickableBehavior>();
            if (behavior != null) EatPickableBehaviors.Add(behavior);
            ComponentChaseBehavior behavior1 = entity.FindComponent<ComponentChaseBehavior>();
            if (behavior1 != null) ChaseBehaviors.Add(behavior1);
            ComponentCreatureModel behavior2 = entity.FindComponent<ComponentCreatureModel>();
            if (behavior2 != null) CreatureModels.Add(behavior2);
            ComponentHerdBehavior behavior3 = entity.FindComponent<ComponentHerdBehavior>();
            if (behavior3 != null) HerdBehaviors.Add(behavior3);
            ComponentRunAwayBehavior behavior4 = entity.FindComponent<ComponentRunAwayBehavior>();
            if (behavior4 != null) RunAwayBehaviors.Add(behavior4);
            ComponentSwimAwayBehavior behavior5 = entity.FindComponent<ComponentSwimAwayBehavior>();
            if (behavior5 != null) SwimAwayBehaviors.Add(behavior5);
            ComponentSleep behavior6 = entity.FindComponent<ComponentSleep>();
            if (behavior6 != null) Sleeps.Add(behavior6);
            ComponentVitalStats behavior7 = entity.FindComponent<ComponentVitalStats>();
            if (behavior7 != null) VitalStats.Add(behavior7);
        }
        public override void OnEntityRemove(Entity entity)
        {
            ComponentEatPickableBehavior behavior = entity.FindComponent<ComponentEatPickableBehavior>();
            if (behavior != null) EatPickableBehaviors.Remove(behavior);
            ComponentChaseBehavior behavior1 = entity.FindComponent<ComponentChaseBehavior>();
            if (behavior1 != null) ChaseBehaviors.Remove(behavior1);
            ComponentCreatureModel behavior2 = entity.FindComponent<ComponentCreatureModel>();
            if (behavior2 != null) CreatureModels.Remove(behavior2);
            ComponentHerdBehavior behavior3 = entity.FindComponent<ComponentHerdBehavior>();
            if (behavior3 != null) HerdBehaviors.Remove(behavior3);
            ComponentRunAwayBehavior behavior4 = entity.FindComponent<ComponentRunAwayBehavior>();
            if (behavior4 != null) RunAwayBehaviors.Remove(behavior4);
            ComponentSwimAwayBehavior behavior5 = entity.FindComponent<ComponentSwimAwayBehavior>();
            if (behavior5 != null) SwimAwayBehaviors.Remove(behavior5);
            ComponentSleep behavior6 = entity.FindComponent<ComponentSleep>();
            if (behavior6 != null) Sleeps.Remove(behavior6);
            ComponentVitalStats behavior7 = entity.FindComponent<ComponentVitalStats>();
            if (behavior7 != null) VitalStats.Remove(behavior7);
        }
        public override void OnBodyAttacked(ComponentCreature attacker, ComponentHealth ToCreatureHealth)
        {
            if (attacker == null) return;
            for (int i = 0; i < ChaseBehaviors.Count; i++)
            {
                ComponentChaseBehavior behavior = ChaseBehaviors[i];
                if (behavior.m_random.Float(0f, 1f) < behavior.m_chaseWhenAttackedProbability)
                {
                    if (behavior.m_chaseWhenAttackedProbability >= 1f)
                    {
                        behavior.Attack(attacker, 30f, 60f, isPersistent: true);
                    }
                    else
                    {
                        behavior.Attack(attacker, 7f, 7f, isPersistent: false);
                    }
                }
            }
            for (int i = 0; i < CreatureModels.Count; i++)
            {
                ComponentCreatureModel behavior = CreatureModels[i];
                if (behavior.DeathPhase == 0f && behavior.m_componentCreature.ComponentHealth.Health == 0f)
                {
                    behavior.DeathCauseOffset = attacker.ComponentBody.BoundingBox.Center() - behavior.m_componentCreature.ComponentBody.BoundingBox.Center();
                }
            }
            for (int i = 0; i < HerdBehaviors.Count; i++)
            {
                ComponentHerdBehavior behavior = HerdBehaviors[i];
                behavior.CallNearbyCreaturesHelp(attacker, 20f, 30f, isPersistent: false);
            }
            for (int i = 0; i < RunAwayBehaviors.Count; i++)
            {
                ComponentRunAwayBehavior behavior = RunAwayBehaviors[i];
                behavior.RunAwayFrom(attacker.ComponentBody);
            }
            for (int i = 0; i < SwimAwayBehaviors.Count; i++)
            {
                ComponentSwimAwayBehavior behavior = SwimAwayBehaviors[i];
                behavior.SwimAwayFrom(attacker.ComponentBody);
            }
            for (int i = 0; i < Sleeps.Count; i++)
            {
                ComponentSleep behavior = Sleeps[i];
                if (behavior.IsSleeping && behavior.m_componentPlayer.ComponentVitalStats.Sleep > 0.25f)
                {
                    behavior.WakeUp();
                }
            }
            for (int i = 0; i < VitalStats.Count; i++)
            {
                ComponentVitalStats behavior = VitalStats[i];
                behavior.m_lastAttackedTime = behavior.m_subsystemTime.GameTime;
            }

        }
    }
}
