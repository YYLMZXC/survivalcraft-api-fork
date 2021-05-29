using System.IO;
using Game;
using System.Collections.Generic;
using Engine.Graphics;
using Engine;
using System.Text;
using SimpleJson;
using System.Reflection;
using System;
using System.Xml.Linq;
using System.Linq;
using Game;
using GameEntitySystem;
namespace Game
{
    public abstract class ModLoader
    {

        public virtual void __ModInitialize()
        {


        }
        public virtual bool ComponentMinerDig(ComponentMiner miner, TerrainRaycastResult raycastResult)
        {

            return false;
        }
        public virtual bool ComponentMinerUse(ComponentMiner miner, Ray3 ray)
        {

            return false;
        }
        public virtual bool ComponentMinerPlace(ComponentMiner miner, TerrainRaycastResult raycastResult, int value)
        {
            return false;
        }
        public virtual void AttackBody(ComponentBody target, ComponentCreature attacker, Vector3 hitPoint, Vector3 hitDirection, float attackPower, bool isMeleeAttack)
        {


        }
        public virtual float ApplyArmorProtection(ComponentClothing componentClothing, float attackPower)
        {
            return attackPower;
        }
        public virtual void InitializeCreatureTypes(SubsystemCreatureSpawn spawn, List<SubsystemCreatureSpawn.CreatureType> creatureTypes)
        {


        }
        public virtual void SpawnEntity(SubsystemSpawn spawn, Entity entity, SpawnEntityData spawnEntityData) { }
        public virtual void LoadSpawnsData(SubsystemSpawn spawn, string data, List<SpawnEntityData> creaturesData) { }
        public virtual string SaveSpawnsData(SubsystemSpawn spawn, List<SpawnEntityData> spawnsData) { return ""; }
        public virtual void PickableAdded(SubsystemPickables subsystemPickables, Pickable pickable) { }
        public virtual void PickableRemoved(SubsystemPickables subsystemPickables, Pickable pickable) { }
        public virtual void ProjectileAdded(SubsystemProjectiles subsystemProjectiles, Projectile projectile) { }
        public virtual void ProjectileRemoved(SubsystemProjectiles subsystemProjectiles, Projectile projectile) { }
        public virtual void OnClothingWidgetOpen(ComponentGui componentGui,ClothingWidget clothingWidget) { 
        
        
        }
    }
}
