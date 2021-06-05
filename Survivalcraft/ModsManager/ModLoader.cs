using System.IO;
using System.Collections.Generic;
using Engine.Graphics;
using Engine;
using System.Text;
using SimpleJson;
using System.Reflection;
using System;
using System.Xml.Linq;
using System.Linq;
using GameEntitySystem;
namespace Game
{
    public abstract class ModLoader
    {
        public virtual void __ModInitialize() { }
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
        public virtual void AttackBody(ComponentBody target, ComponentCreature attacker, Vector3 hitPoint, Vector3 hitDirection, float attackPower, bool isMeleeAttack) { }
        public virtual float ApplyArmorProtection(ComponentClothing componentClothing, float attackPower)
        {
            return attackPower;
        }
        public virtual void InitializeCreatureTypes(SubsystemCreatureSpawn spawn, List<SubsystemCreatureSpawn.CreatureType> creatureTypes) { }
        public virtual void SpawnEntity(SubsystemSpawn spawn, Entity entity, SpawnEntityData spawnEntityData) { }
        public virtual void LoadSpawnsData(SubsystemSpawn spawn, string data, List<SpawnEntityData> creaturesData) { }
        public virtual string SaveSpawnsData(SubsystemSpawn spawn, List<SpawnEntityData> spawnsData) { return ""; }
        public virtual void PickableAdded(SubsystemPickables subsystemPickables, Pickable pickable) { }
        public virtual void PickableRemoved(SubsystemPickables subsystemPickables, Pickable pickable) { }
        public virtual void ProjectileAdded(SubsystemProjectiles subsystemProjectiles, Projectile projectile) { }
        public virtual void ProjectileRemoved(SubsystemProjectiles subsystemProjectiles, Projectile projectile) { }
        public virtual void OnClothingWidgetOpen(ComponentGui componentGui, ClothingWidget clothingWidget) { }
        public virtual void OnPlayerDead(PlayerData playerData) {


        }
        public virtual void OnCameraChange(ComponentPlayer m_componentPlayer, ComponentGui componentGui) {


        }
        public virtual void GenerateChunkContentsPass1(ITerrainContentsGenerator generator, TerrainChunk chunk)
        {

        }
        public virtual void GenerateChunkContentsPass2(ITerrainContentsGenerator generator, TerrainChunk chunk)
        {

        }
        public virtual void GenerateChunkContentsPass3(ITerrainContentsGenerator generator, TerrainChunk chunk)
        {

        }
        public virtual void GenerateChunkContentsPass4(ITerrainContentsGenerator generator, TerrainChunk chunk)
        {

        }
        public virtual void UpdateChunkSingleStep(TerrainUpdater terrainUpdater, TerrainChunk chunk, int skylightValue) {

        }
        public virtual void ComponentMinerHit(ComponentMiner miner, ComponentBody componentBody, Vector3 hitPoint, Vector3 hitDirection) { }
        public virtual void ComponentSpawn_Despawned(Entity entity, ComponentSpawn componentSpawn) { }
        public virtual void OnModelRendererDrawExtra(SubsystemModelsRenderer modelsRenderer, ComponentModel componentModel, Camera camera, float? alphaThreshold) { }
        public virtual void OnBlocksManagerInitalized() { 
        }
        public virtual void OnXdbLoad(XElement xElement) { }
        public virtual void OnScreensManagerInitalized(LoadingScreen loadingScreen) { }
        public virtual void SaveSettings(XElement xElement)
        {

        }
        public virtual void LoadSettings(XElement xElement)
        {

        }
        public virtual void ProjectLoad(XElement xElement) { }
        public virtual void ProjectSave(XElement xElement) { }
        public virtual void GuiUpdate(ComponentGui componentGui) { 
        
        }
        public virtual void OnGuiEntityAdd(ComponentGui componentGui,Entity entity)
        {

        }
        public virtual void OnGuiEntityRemove(ComponentGui componentGui, Entity entity)
        {

        }
        /// <summary>
        /// CornerVertex
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="color">点输出颜色</param>
        /// <param name="light">传入的亮度</param>
        /// <param name="face">哪个面</param>
        /// <param name="corner"></param>
        /// <param name="vertex"></param>
        public virtual bool SetupCornerVertex(float x, float y, float z,ref Color color, int light, int face, int corner, ref TerrainVertex vertex) {
            return false;
        }
        public virtual bool SetupLitCornerVertex(float x, float y, float z,ref Color color, int corner, ref TerrainVertex vertex) {
            return false;

        }
        public virtual bool SetupVertex(float x, float y, float z,ref Color color, float tx, float ty, ref TerrainVertex vertex) {
            return false;

        }
        public virtual int GetMaxInstancesCount() {
            return 7;
        }
    }
}
