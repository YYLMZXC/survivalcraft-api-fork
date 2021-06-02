using Engine;
using GameEntitySystem;
using System;
using System.Globalization;
using TemplatesDatabase;

namespace Game
{
    public class ComponentMiner : Component, IUpdateable
    {
        public SubsystemTerrain m_subsystemTerrain;

        public SubsystemBodies m_subsystemBodies;

        public SubsystemMovingBlocks m_subsystemMovingBlocks;

        public SubsystemGameInfo m_subsystemGameInfo;

        public SubsystemTime m_subsystemTime;

        public SubsystemAudio m_subsystemAudio;

        public SubsystemSoundMaterials m_subsystemSoundMaterials;

        public SubsystemBlockBehaviors m_subsystemBlockBehaviors;

        public Random m_random = new Random();

        public static Random s_random = new Random();

        public double m_digStartTime;

        public float m_digProgress;

        public double m_lastHitTime;

        public static string fName = "ComponentMiner";

        public int m_lastDigFrameIndex;

        public float m_lastPokingPhase;

        public ComponentCreature ComponentCreature
        {
            get;
            set;
        }

        public ComponentPlayer ComponentPlayer
        {
            get;
            set;
        }

        public IInventory Inventory
        {
            get;
            set;
        }

        public int ActiveBlockValue
        {
            get
            {
                if (Inventory == null)
                {
                    return 0;
                }
                return Inventory.GetSlotValue(Inventory.ActiveSlotIndex);
            }
        }

        public float AttackPower
        {
            get;
            set;
        }

        public float PokingPhase
        {
            get;
            set;
        }

        public CellFace? DigCellFace
        {
            get;
            set;
        }

        public float DigTime
        {
            get
            {
                if (!DigCellFace.HasValue)
                {
                    return 0f;
                }
                return (float)(m_subsystemTime.GameTime - m_digStartTime);
            }
        }

        public float DigProgress
        {
            get
            {
                if (!DigCellFace.HasValue)
                {
                    return 0f;
                }
                return m_digProgress;
            }
        }

        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public void Poke(bool forceRestart)
        {
            if (forceRestart)
            {
                PokingPhase = 0.0001f;
            }
            else
            {
                PokingPhase = MathUtils.Max(0.0001f, PokingPhase);
            }
        }

        public bool Dig(TerrainRaycastResult raycastResult)
        {
            bool flag=false;
            foreach (ModLoader modEntity in ModsManager.ModLoaders) {
                flag |= modEntity.ComponentMinerDig(this,raycastResult);
            }
            return flag;
        }

        public bool Place(TerrainRaycastResult raycastResult)
        {
            if (Place(raycastResult, ActiveBlockValue))
            {
                if (Inventory != null)
                {
                    Inventory.RemoveSlotItems(Inventory.ActiveSlotIndex, 1);
                }
                return true;
            }
            return false;
        }

        public bool Place(TerrainRaycastResult raycastResult, int value)
        {
            bool flag = false;
            foreach (ModLoader modEntity in ModsManager.ModLoaders)
            {
                flag |= modEntity.ComponentMinerPlace(this, raycastResult,value);
            }
            return flag;
        }

        public bool Use(Ray3 ray)
        {
            bool flag = false;
            foreach (ModLoader modEntity in ModsManager.ModLoaders)
            {
                flag |= modEntity.ComponentMinerUse(this, ray);
            }
            return flag;
        }

        public bool Interact(TerrainRaycastResult raycastResult)
        {            
            int cellContents = m_subsystemTerrain.Terrain.GetCellContents(raycastResult.CellFace.X, raycastResult.CellFace.Y, raycastResult.CellFace.Z);
            SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(cellContents);
            for (int i = 0; i < blockBehaviors.Length; i++)
            {
                if (blockBehaviors[i].OnInteract(raycastResult, this))
                {
                    if (ComponentCreature.PlayerStats != null)
                    {
                        ComponentCreature.PlayerStats.BlocksInteracted++;
                    }
                    Poke(forceRestart: false);
                    return true;
                }
            }
            return false;
        }

        public void Hit(ComponentBody componentBody, Vector3 hitPoint, Vector3 hitDirection)
        {
            foreach (ModLoader modLoader in ModsManager.ModLoaders) {
                modLoader.ComponentMinerHit(this, componentBody,hitPoint,hitDirection);
            
            }
        }

        public bool Aim(Ray3 aim, AimState state)
        {
            int num = Terrain.ExtractContents(ActiveBlockValue);
            Block block = BlocksManager.Blocks[num];
            if (block.IsAimable_(ActiveBlockValue))
            {
                if (!CanUseTool(ActiveBlockValue))
                {
                    ComponentPlayer?.ComponentGui.DisplaySmallMessage(string.Format(LanguageControl.Get(fName, 1), block.PlayerLevelRequired_(ActiveBlockValue), block.GetDisplayName(m_subsystemTerrain, ActiveBlockValue)), Color.White, blinking: true, playNotificationSound: true);
                    Poke(forceRestart: false);
                    return true;
                }
                SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(num);
                for (int i = 0; i < blockBehaviors.Length; i++)
                {
                    if (blockBehaviors[i].OnAim(aim, this, state))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public object Raycast(Ray3 ray, RaycastMode mode, bool raycastTerrain = true, bool raycastBodies = true, bool raycastMovingBlocks = true)
        {
            float reach = (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative) ? SettingsManager.CreativeReach : 5f;
            Vector3 creaturePosition = ComponentCreature.ComponentCreatureModel.EyePosition;
            Vector3 start = ray.Position;
            Vector3 direction = Vector3.Normalize(ray.Direction);
            Vector3 end = ray.Position + direction * 15f;
            Point3 startCell = Terrain.ToCell(start);
            BodyRaycastResult? bodyRaycastResult = m_subsystemBodies.Raycast(start, end, 0.35f, (ComponentBody body, float distance) => (Vector3.DistanceSquared(start + distance * direction, creaturePosition) <= reach * reach && body.Entity != base.Entity && !body.IsChildOfBody(ComponentCreature.ComponentBody) && !ComponentCreature.ComponentBody.IsChildOfBody(body) && Vector3.Dot(Vector3.Normalize(body.BoundingBox.Center() - start), direction) > 0.7f) ? true : false);
            MovingBlocksRaycastResult? movingBlocksRaycastResult = m_subsystemMovingBlocks.Raycast(start, end, extendToFillCells: true);
            TerrainRaycastResult? terrainRaycastResult = m_subsystemTerrain.Raycast(start, end, useInteractionBoxes: true, skipAirBlocks: true, delegate (int value, float distance)
            {
                if (Vector3.DistanceSquared(start + distance * direction, creaturePosition) <= reach * reach)
                {
                    Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
                    if (distance == 0f && block is CrossBlock && Vector3.Dot(direction, new Vector3(startCell) + new Vector3(0.5f) - start) < 0f)
                    {
                        return false;
                    }
                    if (mode == RaycastMode.Digging)
                    {
                        return !block.IsDiggingTransparent;
                    }
                    if (mode == RaycastMode.Interaction)
                    {
                        if (block.IsPlacementTransparent_(value))
                        {
                            return block.IsInteractive(m_subsystemTerrain, value);
                        }
                        return true;
                    }
                    if (mode == RaycastMode.Gathering)
                    {
                        return block.IsGatherable_(value);
                    }
                }
                return false;
            });
            float num = bodyRaycastResult.HasValue ? bodyRaycastResult.Value.Distance : float.PositiveInfinity;
            float num2 = movingBlocksRaycastResult.HasValue ? movingBlocksRaycastResult.Value.Distance : float.PositiveInfinity;
            float num3 = terrainRaycastResult.HasValue ? terrainRaycastResult.Value.Distance : float.PositiveInfinity;
            if (num < num2 && num < num3)
            {
                return bodyRaycastResult.Value;
            }
            if (num2 < num && num2 < num3)
            {
                return movingBlocksRaycastResult.Value;
            }
            if (num3 < num && num3 < num2)
            {
                return terrainRaycastResult.Value;
            }
            return new Ray3(start, direction);
        }

        public T? Raycast<T>(Ray3 ray, RaycastMode mode, bool raycastTerrain = true, bool raycastBodies = true, bool raycastMovingBlocks = true) where T : struct
        {
            object obj = Raycast(ray, mode, raycastTerrain, raycastBodies, raycastMovingBlocks);
            if (!(obj is T))
            {
                return null;
            }
            return (T)obj;
        }

        public void RemoveActiveTool(int removeCount)
        {
            if (Inventory != null)
            {
                Inventory.RemoveSlotItems(Inventory.ActiveSlotIndex, removeCount);
            }
        }

        public void DamageActiveTool(int damageCount)
        {
            if (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative || Inventory == null)
            {
                return;
            }
            int num = BlocksManager.DamageItem(ActiveBlockValue, damageCount);
            if (num != 0)
            {
                int slotCount = Inventory.GetSlotCount(Inventory.ActiveSlotIndex);
                Inventory.RemoveSlotItems(Inventory.ActiveSlotIndex, slotCount);
                if (Inventory.GetSlotCount(Inventory.ActiveSlotIndex) == 0)
                {
                    Inventory.AddSlotItems(Inventory.ActiveSlotIndex, num, slotCount);
                }
            }
            else
            {
                Inventory.RemoveSlotItems(Inventory.ActiveSlotIndex, 1);
            }
        }

        public static void AttackBody(ComponentBody target, ComponentCreature attacker, Vector3 hitPoint, Vector3 hitDirection, float attackPower, bool isMeleeAttack)
        {
            foreach (ModLoader modEntity in ModsManager.ModLoaders) {
                modEntity.AttackBody(target,attacker,hitPoint,hitDirection,attackPower,isMeleeAttack);
            }
        }

        public void Update(float dt)
        {
            float num = (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative) ? (0.5f / SettingsManager.CreativeDigTime) : 4f;
            m_lastPokingPhase = PokingPhase;
            if (DigCellFace.HasValue || PokingPhase > 0f)
            {
                PokingPhase += num * m_subsystemTime.GameTimeDelta;
                if (PokingPhase > 1f)
                {
                    if (DigCellFace.HasValue)
                    {
                        PokingPhase = MathUtils.Remainder(PokingPhase, 1f);
                    }
                    else
                    {
                        PokingPhase = 0f;
                    }
                }
            }
            if (DigCellFace.HasValue && Time.FrameIndex - m_lastDigFrameIndex > 1)
            {
                DigCellFace = null;
            }
        }

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
        {
            m_subsystemTerrain = base.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
            m_subsystemBodies = base.Project.FindSubsystem<SubsystemBodies>(throwOnError: true);
            m_subsystemMovingBlocks = base.Project.FindSubsystem<SubsystemMovingBlocks>(throwOnError: true);
            m_subsystemGameInfo = base.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
            m_subsystemTime = base.Project.FindSubsystem<SubsystemTime>(throwOnError: true);
            m_subsystemAudio = base.Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
            m_subsystemSoundMaterials = base.Project.FindSubsystem<SubsystemSoundMaterials>(throwOnError: true);
            m_subsystemBlockBehaviors = base.Project.FindSubsystem<SubsystemBlockBehaviors>(throwOnError: true);
            ComponentCreature = base.Entity.FindComponent<ComponentCreature>(throwOnError: true);
            ComponentPlayer = base.Entity.FindComponent<ComponentPlayer>();
            if (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative && ComponentPlayer != null)
            {
                Inventory = base.Entity.FindComponent<ComponentCreativeInventory>();
            }
            else
            {
                Inventory = base.Entity.FindComponent<ComponentInventory>();
            }
            AttackPower = valuesDictionary.GetValue<float>("AttackPower");
        }

        public static bool IsBlockPlacingAllowed(ComponentBody componentBody)
        {
            if (componentBody.StandingOnBody != null || componentBody.StandingOnValue.HasValue)
            {
                return true;
            }
            if (componentBody.ImmersionFactor > 0.01f)
            {
                return true;
            }
            if (componentBody.ParentBody != null && IsBlockPlacingAllowed(componentBody.ParentBody))
            {
                return true;
            }
            ComponentLocomotion componentLocomotion = componentBody.Entity.FindComponent<ComponentLocomotion>();
            if (componentLocomotion != null && componentLocomotion.LadderValue.HasValue)
            {
                return true;
            }
            return false;
        }

        public float CalculateDigTime(int digValue, int toolValue)
        {
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(toolValue)];
            Block block2 = BlocksManager.Blocks[Terrain.ExtractContents(digValue)];
            float digResilience = block2.GetDigResilience(digValue);
            BlockDigMethod digBlockMethod = block2.GetBlockDigMethod(digValue);
            float ShovelPower = block.GetShovelPower(toolValue);
            float QuarryPower = block.GetQuarryPower(toolValue);
            float HackPower = block.GetHackPower(toolValue);

            if (ComponentPlayer != null && m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative)
            {
                if (digResilience < float.PositiveInfinity)
                {
                    return 0f;
                }
                return float.PositiveInfinity;
            }
            if (ComponentPlayer != null && m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Adventure)
            {
                float num = 0f;
                if (digBlockMethod == BlockDigMethod.Shovel && ShovelPower >= 2f)
                {
                    num = ShovelPower;
                }
                else if (digBlockMethod == BlockDigMethod.Quarry && QuarryPower >= 2f)
                {
                    num = QuarryPower;
                }
                else if (digBlockMethod == BlockDigMethod.Hack && HackPower >= 2f)
                {
                    num = HackPower;
                }
                num *= ComponentPlayer.ComponentLevel.StrengthFactor;
                if (!(num > 0f))
                {
                    return float.PositiveInfinity;
                }
                return MathUtils.Max(digResilience / num, 0f);
            }
            float num2 = 0f;
            if (digBlockMethod == BlockDigMethod.Shovel)
            {
                num2 = ShovelPower;
            }
            else if (digBlockMethod == BlockDigMethod.Quarry)
            {
                num2 = QuarryPower;
            }
            else if (digBlockMethod == BlockDigMethod.Hack)
            {
                num2 = HackPower;
            }
            if (ComponentPlayer != null)
            {
                num2 *= ComponentPlayer.ComponentLevel.StrengthFactor;
            }
            if (!(num2 > 0f))
            {
                return float.PositiveInfinity;
            }
            return MathUtils.Max(digResilience / num2, 0f);
        }

        public bool CanUseTool(int toolValue)
        {
            if (m_subsystemGameInfo.WorldSettings.GameMode != 0 && m_subsystemGameInfo.WorldSettings.AreAdventureSurvivalMechanicsEnabled)
            {
                Block block = BlocksManager.Blocks[Terrain.ExtractContents(toolValue)];
                if (ComponentPlayer != null && ComponentPlayer.PlayerData.Level < (float)block.PlayerLevelRequired_(toolValue))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
