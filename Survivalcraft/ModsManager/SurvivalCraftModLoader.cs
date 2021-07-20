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
        public override void __ModInitialize()
        {
            ModsManager.RegisterHook("ComponentMinerDig",this);
            ModsManager.RegisterHook("ComponentMinerHit", this);
            ModsManager.RegisterHook("AttackBody", this);
            ModsManager.RegisterHook("ApplyArmorProtection", this);
            ModsManager.RegisterHook("SpawnEntity", this);
            ModsManager.RegisterHook("OnCameraChange", this);
            ModsManager.RegisterHook("OnPlayerDead", this);
            ModsManager.RegisterHook("OnModelRendererDrawExtra", this);
            ModsManager.RegisterHook("GuiUpdate", this);
            ModsManager.RegisterHook("OnGuiEntityAdd", this);
            ModsManager.RegisterHook("OnGuiEntityRemove", this);
            ModsManager.RegisterHook("OnLevelUpdate", this);
            ModsManager.RegisterHook("GetMaxInstancesCount", this);
        }
        public override bool ComponentMinerDig(ComponentMiner miner, TerrainRaycastResult raycastResult)
        {
            bool result = false;
            miner.m_lastDigFrameIndex = Time.FrameIndex;
            CellFace cellFace = raycastResult.CellFace;
            int cellValue = miner.m_subsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
            int num = Terrain.ExtractContents(cellValue);
            Block block = BlocksManager.Blocks[num];
            int activeBlockValue = miner.ActiveBlockValue;
            int num2 = Terrain.ExtractContents(activeBlockValue);
            Block block2 = BlocksManager.Blocks[num2];
            if (!miner.DigCellFace.HasValue || miner.DigCellFace.Value.X != cellFace.X || miner.DigCellFace.Value.Y != cellFace.Y || miner.DigCellFace.Value.Z != cellFace.Z)
            {
                miner.m_digStartTime = miner.m_subsystemTime.GameTime;
                miner.DigCellFace = cellFace;
            }
            float num3 = miner.CalculateDigTime(cellValue, activeBlockValue);
            miner.m_digProgress = ((num3 > 0f) ? MathUtils.Saturate((float)(miner.m_subsystemTime.GameTime - miner.m_digStartTime) / num3) : 1f);
            if (!miner.CanUseTool(activeBlockValue))
            {
                miner.m_digProgress = 0f;
                if (miner.m_subsystemTime.PeriodicGameTimeEvent(5.0, miner.m_digStartTime + 1.0))
                {
                    miner.ComponentPlayer?.ComponentGui.DisplaySmallMessage(string.Format(LanguageControl.Get(ComponentMiner.fName, 1), block2.PlayerLevelRequired_(miner.ActiveBlockValue), block2.GetDisplayName(miner.m_subsystemTerrain, activeBlockValue)), Color.White, blinking: true, playNotificationSound: true);
                }
            }
            bool flag = miner.ComponentPlayer != null && !miner.ComponentPlayer.ComponentInput.IsControlledByTouch && miner.m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative;
            if (flag || (miner.m_lastPokingPhase <= 0.5f && miner.PokingPhase > 0.5f))
            {
                if (miner.m_digProgress >= 1f)
                {
                    miner.DigCellFace = null;
                    if (flag)
                    {
                        miner.Poke(forceRestart: true);
                    }
                    BlockPlacementData digValue = block.GetDigValue(miner.m_subsystemTerrain, miner, cellValue, activeBlockValue, raycastResult);
                    miner.m_subsystemTerrain.DestroyCell(block2.GetToolLevel(activeBlockValue), digValue.CellFace.X, digValue.CellFace.Y, digValue.CellFace.Z, digValue.Value, noDrop: false, noParticleSystem: false);
                    miner.m_subsystemSoundMaterials.PlayImpactSound(cellValue, new Vector3(cellFace.X, cellFace.Y, cellFace.Z), 2f);
                    miner.DamageActiveTool(1);
                    if (miner.ComponentCreature.PlayerStats != null)
                    {
                        miner.ComponentCreature.PlayerStats.BlocksDug++;
                    }
                    result = true;
                }
                else
                {
                    miner.m_subsystemSoundMaterials.PlayImpactSound(cellValue, new Vector3(cellFace.X, cellFace.Y, cellFace.Z), 1f);
                    BlockDebrisParticleSystem particleSystem = block.CreateDebrisParticleSystem(miner.m_subsystemTerrain, raycastResult.HitPoint(0.1f), cellValue, 0.35f);
                    miner.Project.FindSubsystem<SubsystemParticles>(throwOnError: true).AddParticleSystem(particleSystem);
                }
            }
            return result;
        }
        public override void ComponentMinerHit(ComponentMiner miner, ComponentBody componentBody, Vector3 hitPoint, Vector3 hitDirection,HitValueParticleSystem hitValueParticleSystem,ref float AttackPower)
        {
            if (!(miner.m_subsystemTime.GameTime - miner.m_lastHitTime > 0.6600000262260437))
            {
                return;
            }
            float num2 = 0f;
            miner.m_lastHitTime = miner.m_subsystemTime.GameTime;
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(miner.ActiveBlockValue)];
            if (!miner.CanUseTool(miner.ActiveBlockValue))
            {
                miner.ComponentPlayer?.ComponentGui.DisplaySmallMessage(string.Format(LanguageControl.Get(ComponentMiner.fName, 1), block.PlayerLevelRequired_(miner.ActiveBlockValue), block.GetDisplayName(miner.m_subsystemTerrain, miner.ActiveBlockValue)), Color.White, blinking: true, playNotificationSound: true);
                miner.Poke(forceRestart: false);
                return;
            }
            if (miner.ActiveBlockValue != 0)
            {
                AttackPower = block.GetMeleePower(miner.ActiveBlockValue) * miner.AttackPower * miner.m_random.Float(0.8f, 1.2f);
                num2 = block.GetMeleeHitProbability(miner.ActiveBlockValue);
            }else
            {
                AttackPower = miner.AttackPower * miner.m_random.Float(0.8f, 1.2f);
                num2 = 0.66f;
            }
            bool flag;
            if (miner.ComponentPlayer != null)
            {
                miner.m_subsystemAudio.PlaySound("Audio/Swoosh", 1f, miner.m_random.Float(-0.2f, 0.2f), componentBody.Position, 3f, autoDelay: false);
                flag = miner.m_random.Bool(num2);
                AttackPower *= miner.ComponentPlayer.ComponentLevel.StrengthFactor;
            }
            else
            {
                flag = true;
            }
            if (flag)
            {
                ComponentMiner.AttackBody(componentBody, miner.ComponentCreature, hitPoint, hitDirection, AttackPower, isMeleeAttack: true);
                miner.DamageActiveTool(1);
            }
            else if (miner.ComponentCreature is ComponentPlayer)
            {
                miner.Project.FindSubsystem<SubsystemParticles>(throwOnError: true).AddParticleSystem(hitValueParticleSystem);
            }
            if (miner.ComponentCreature.PlayerStats != null)
            {
                miner.ComponentCreature.PlayerStats.MeleeAttacks++;
                if (flag)
                {
                    miner.ComponentCreature.PlayerStats.MeleeHits++;
                }
            }
        }
        public override void AttackBody(ComponentBody target, ComponentCreature attacker, Vector3 hitPoint, Vector3 hitDirection, float attackPower, bool isMeleeAttack,HitValueParticleSystem hitValueParticleSystem)
        {
            if (attacker != null && attacker is ComponentPlayer && target.Entity.FindComponent<ComponentPlayer>() != null && !target.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true).WorldSettings.IsFriendlyFireEnabled)
            {
                attacker.Entity.FindComponent<ComponentGui>(throwOnError: true).DisplaySmallMessage(LanguageControl.Get(ComponentClothing.fName, 3), Color.White, blinking: true, playNotificationSound: true);
                return;
            }
            if (attackPower > 0f)
            {
                ComponentClothing componentClothing = target.Entity.FindComponent<ComponentClothing>();
                if (componentClothing != null)
                {
                    attackPower = componentClothing.ApplyArmorProtection(attackPower);
                }
                ComponentLevel componentLevel = target.Entity.FindComponent<ComponentLevel>();
                if (componentLevel != null)
                {
                    attackPower /= componentLevel.ResilienceFactor;
                }
                ComponentHealth componentHealth = target.Entity.FindComponent<ComponentHealth>();
                if (componentHealth != null)
                {
                    float num = attackPower / componentHealth.AttackResilience;
                    string cause;
                    if (attacker != null)
                    {
                        string str = attacker.KillVerbs[ComponentMiner.s_random.Int(0, attacker.KillVerbs.Count - 1)];
                        string attackerName = attacker.DisplayName;
                        cause = string.Format(LanguageControl.Get(ComponentClothing.fName, 4), attackerName, LanguageControl.Get(ComponentClothing.fName, str));
                    }
                    else
                    {
                        switch (ComponentMiner.s_random.Int(0, 5))
                        {
                            case 0:
                                cause = LanguageControl.Get(ComponentClothing.fName, 5);
                                break;
                            case 1:
                                cause = LanguageControl.Get(ComponentClothing.fName, 6);
                                break;
                            case 2:
                                cause = LanguageControl.Get(ComponentClothing.fName, 7);
                                break;
                            case 3:
                                cause = LanguageControl.Get(ComponentClothing.fName, 8);
                                break;
                            case 4:
                                cause = LanguageControl.Get(ComponentClothing.fName, 9);
                                break;
                            default:
                                cause = LanguageControl.Get(ComponentClothing.fName, 10);
                                break;
                        }
                    }
                    float health = componentHealth.Health;
                    componentHealth.Injure(num, attacker, ignoreInvulnerability: false, cause);
                    if (num > 0f)
                    {
                        target.Project.FindSubsystem<SubsystemAudio>(throwOnError: true).PlayRandomSound("Audio/Impacts/Body", 1f, ComponentMiner.s_random.Float(-0.3f, 0.3f), target.Position, 4f, autoDelay: false);
                        float num2 = (health - componentHealth.Health) * componentHealth.AttackResilience;
                        if (attacker is ComponentPlayer && num2 > 0f)
                        {
                            string text2 = (0f - num2).ToString("0", CultureInfo.InvariantCulture);
                            hitValueParticleSystem.Particles[0].Text = text2;
                            target.Project.FindSubsystem<SubsystemParticles>(throwOnError: true).AddParticleSystem(hitValueParticleSystem);
                        }
                    }
                }
                ComponentDamage componentDamage = target.Entity.FindComponent<ComponentDamage>();
                if (componentDamage != null)
                {
                    float num3 = attackPower / componentDamage.AttackResilience;
                    componentDamage.Damage(num3);
                    if (num3 > 0f)
                    {
                        target.Project.FindSubsystem<SubsystemAudio>(throwOnError: true).PlayRandomSound(componentDamage.DamageSoundName, 1f, ComponentMiner.s_random.Float(-0.3f, 0.3f), target.Position, 4f, autoDelay: false);
                    }
                }
            }
            float num4 = 0f;
            float x = 0f;
            if (isMeleeAttack && attacker != null)
            {
                float num5 = (attackPower >= 2f) ? 1.25f : 1f;
                float num6 = MathUtils.Pow(attacker.ComponentBody.Mass / target.Mass, 0.5f);
                float x2 = num5 * num6;
                num4 = 5.5f * MathUtils.Saturate(x2);
                x = 0.25f * MathUtils.Saturate(x2);
            }
            else if (attackPower > 0f)
            {
                num4 = 2f;
                x = 0.2f;
            }
            if (num4 > 0f)
            {
                target.ApplyImpulse(num4 * Vector3.Normalize(hitDirection + ComponentMiner.s_random.Vector3(0.1f) + 0.2f * Vector3.UnitY));
                ComponentLocomotion componentLocomotion = target.Entity.FindComponent<ComponentLocomotion>();
                if (componentLocomotion != null)
                {
                    componentLocomotion.StunTime = MathUtils.Max(componentLocomotion.StunTime, x);
                }
            }
        }
        public override float ApplyArmorProtection(ComponentClothing componentClothing,ref float attackPower)
        {
            float num = componentClothing.m_random.Float(0f, 1f);
            ClothingSlot slot = (num < 0.1f) ? ClothingSlot.Feet : ((num < 0.3f) ? ClothingSlot.Legs : ((num < 0.9f) ? ClothingSlot.Torso : ClothingSlot.Head));
            float num2 = ((ClothingBlock)BlocksManager.Blocks[203]).Durability + 1;
            var list = new List<int>(componentClothing.GetClothes(slot));
            for (int i = 0; i < list.Count; i++)
            {
                int value = list[i];
                ClothingData clothingData = ClothingBlock.GetClothingData(Terrain.ExtractData(value));
                float x = (num2 - BlocksManager.Blocks[203].GetDamage(value)) / num2 * clothingData.Sturdiness;
                float num3 = MathUtils.Min(attackPower * MathUtils.Saturate(clothingData.ArmorProtection), x);
                if (num3 > 0f)
                {
                    attackPower -= num3;
                    if (componentClothing.m_subsystemGameInfo.WorldSettings.GameMode != 0)
                    {
                        float x2 = num3 / clothingData.Sturdiness * num2 + 0.001f;
                        int damageCount = (int)(MathUtils.Floor(x2) + (componentClothing.m_random.Bool(MathUtils.Remainder(x2, 1f)) ? 1 : 0));
                        list[i] = BlocksManager.DamageItem(value, damageCount);
                    }
                    if (!string.IsNullOrEmpty(clothingData.ImpactSoundsFolder))
                    {
                        componentClothing.m_subsystemAudio.PlayRandomSound(clothingData.ImpactSoundsFolder, 1f, componentClothing.m_random.Float(-0.3f, 0.3f), componentClothing.m_componentBody.Position, 4f, 0.15f);
                    }
                }
            }
            int num4 = 0;
            while (num4 < list.Count)
            {
                if (Terrain.ExtractContents(list[num4]) != 203)
                {
                    list.RemoveAt(num4);
                    componentClothing.m_subsystemParticles.AddParticleSystem(new BlockDebrisParticleSystem(componentClothing.m_subsystemTerrain, componentClothing.m_componentBody.Position + componentClothing.m_componentBody.BoxSize / 2f, 1f, 1f, Color.White, 0));
                }
                else
                {
                    num4++;
                }
            }
            componentClothing.SetClothes(slot, list);
            return MathUtils.Max(attackPower, 0f);
        }
        public override void SpawnEntity(SubsystemSpawn spawn, Entity entity, SpawnEntityData data)
        {
            entity.FindComponent<ComponentBody>(throwOnError: true).Position = data.Position;
            entity.FindComponent<ComponentBody>(throwOnError: true).Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, spawn.m_random.Float(0f, (float)Math.PI * 2f));
            ComponentCreature componentCreature = entity.FindComponent<ComponentCreature>();
            if (componentCreature != null)
            {
                componentCreature.ConstantSpawn = data.ConstantSpawn;
            }
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
                            BitmapFont font = ContentManager.Get<BitmapFont>("Fonts/Pericles");
                            modelsRenderer.PrimitivesRenderer.FontBatch(font, 1, DepthStencilState.DepthRead, RasterizerState.CullNoneScissor, BlendState.AlphaBlend, SamplerState.LinearClamp).QueueText(m_componentPlayer.PlayerData.Name, position, right, down, color, TextAnchor.HorizontalCenter | TextAnchor.Bottom);
                        }
                    }
                }
            }
        }
        public override void GuiUpdate(ComponentGui componentGui)
        {
            componentGui.HandleInput();
            componentGui.UpdateWidgets();
        }
        public override void OnGuiEntityAdd(ComponentGui componentGui, Entity entity)
        {
            componentGui.ShortInventoryWidget.AssignComponents(componentGui.m_componentPlayer.ComponentMiner.Inventory);
        }
        public override void OnGuiEntityRemove(ComponentGui componentGui, Entity entity)
        {
            componentGui.ShortInventoryWidget.AssignComponents(null);
            componentGui.m_message = null;
        }
        public override void OnLevelUpdate(ComponentLevel level)
        {
            level.StrengthFactor = level.CalculateStrengthFactor(null);
            level.SpeedFactor = level.CalculateSpeedFactor(null);
            level.HungerFactor = level.CalculateHungerFactor(null);
            level.ResilienceFactor = level.CalculateResilienceFactor(null);
        }
        public override int GetMaxInstancesCount()
        {
            return 7;
        }
    }
}
