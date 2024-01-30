using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using GameEntitySystem;

namespace Game;

public class SurvivalCraftModInterface(IModLoader parent) : ModInterface(parent)
{
    private static string InvalidOperationMessage =>
        "请检查代码中是否出现了以下情况之一，若有，请进行修复：\n1.注册了未经重写的 Hook \n2.在 Hook 内调用了 base 方法。";

    public override string[] AvailableHooks =>
    [
        "ModDispose",
        "ViewFogColor",
        "CalculateLighting",
        "OnMinerHit",
        "OnMinerDig",
        "OnMinerPlace",
        "SetRainAndSnowColor",
        "SetFurnitureDesignColor",
        "AttackPowerParameter",
        "ClothingProcessSlotItems",
        "OnEatPickable",
        "OnPlayerSpawned",
        "OnPlayerDead",
        "AttackBody",
        "OnSetModel",
        "OnModelAnimate",
        "ApplyArmorProtection",
        "OnLevelUpdate",
        "GuiUpdate",
        "GuiDraw",
        "UpdateInput",
        "DrawToScreen",
        "ClothingWidgetOpen",
        "OnBlockExploded",
        "OnEntityAdd",
        "OnEntityRemove",
        "InitializeCreatureTypes",
        "SpawnEntity",
        "OnDeSpawned",
        "DeadBeforeDrops",
        "TerrainChangeCell",
        "OnCreatureInjure",
        "ChangeSkyColor",
        "SetShaderParameter",
        "ModelShaderParameter",
        "SkyDrawExtra",
        "GetMaxInstancesCount",
        "OnModelRendererDrawExtra",
        "SetHitValueParticleSystem",
        "OnSaveSpawnData",
        "OnReadSpawnData",
        "OnTerrainContentsGenerated",
        "ToFreeChunks",
        "ToAllocateChunks",
        "SubsystemUpdate",
        "OnProjectLoaded",
        "OnProjectDisposed",
        "BlocksInitialized",
        "BeforeGameLoading",
        "OnLoadingStart",
        "OnLoadingFinished",
        "SaveSettings",
        "LoadSettings",
        "OnXdbLoad",
        "ProjectXmlLoad",
        "ProjectXmlSave",
        "OnCraftingRecipeDecode",
        "MatchRecipe",
        "DecodeResult",
        "DecodeIngredient",
        "OnCameraChange",
        "OnCapture",
        "MenuPlayMusic",
        "CallNearbyCreaturesHelp",
        "OnTreasureGenerate",
        "OnWidgetConstruct",
        "OnModalPanelWidgetSet",
        "GenerateChunkVertices",
        "GenerateChunkLightSources",
        "CalculateSmoothLight"
    ];

    /// <summary>
    /// Mod被卸载时执行
    /// </summary>
    public virtual void ModDispose()
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 视图雾颜色调整
    /// </summary>
    /// <param name="viewUnderWaterDepth">大于0则表示在水下</param>
    /// <param name="viewUnderMagmaDepth">大于0则表示在岩浆中</param>
    /// <param name="viewFogColor">视图雾颜色</param>
    public virtual void ViewFogColor(float viewUnderWaterDepth, float viewUnderMagmaDepth, ref Color viewFogColor)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 方块亮度
    /// （黑暗区域亮度）
    /// </summary>
    /// <param name="brightness">亮度值</param>
    public virtual void CalculateLighting(ref float brightness)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <param name="hitPoint"></param>
    /// <param name="hitDirection"></param>
    /// <param name="attackPower">伤害值</param>
    /// <param name="playerProbability">玩家命中率</param>
    /// <param name="creatureProbability">生物命中率</param>
    /// <param name="isHitSucceed"></param>
    /// <param name="miner">执行攻击操作的 Component</param>
    /// <param name="componentBody">收到攻击的 Component</param>
    public virtual void OnMinerHit(ComponentMiner miner, ComponentBody componentBody, Vector3 hitPoint,
        Vector3 hitDirection, ref float attackPower, ref float playerProbability, ref float creatureProbability,
        out bool isHitSucceed)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当人物挖掘时执行
    /// </summary>
    /// <param name="miner"></param>
    /// <param name="raycastResult"></param>
    /// <param name="digProgress"></param>
    /// <param name="isDigSucceed"></param>
    /// <returns></returns>
    public virtual void OnMinerDig(ComponentMiner miner, TerrainRaycastResult raycastResult, ref float digProgress,
        out bool isDigSucceed)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当人物放置时执行
    /// </summary>
    /// <param name="miner"/>
    /// <param name="raycastResult"/>
    /// <param name="x"/>
    /// <param name="y"/>
    /// <param name="z"/>
    /// <param name="value"/>
    /// <param name="placed">为 true 则不执行原放置操作</param>
    /// <returns></returns>
    public virtual void OnMinerPlace(ComponentMiner miner, TerrainRaycastResult raycastResult, int x, int y, int z,
        int value, out bool placed)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 设置雨和雪的颜色
    /// </summary>
    /// <param name="rainColor"></param>
    /// <param name="snowColor"></param>
    /// <returns></returns>
    public virtual bool SetRainAndSnowColor(ref Color rainColor, ref Color snowColor)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 设置家具的颜色
    /// </summary>
    public virtual void SetFurnitureDesignColor(FurnitureDesign design, Block block, int value, ref int faceTextureSlot,
        ref Color furnitureColor)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 更改击退和晕眩效果
    /// </summary>
    /// <param name="target">目标</param>
    /// <param name="attacker">攻击者</param>
    /// <param name="hitPoint">伤害位置</param>
    /// <param name="hitDirection"></param>
    /// <param name="impulseFactor">击退效果</param>
    /// <param name="stunTimeFactor">眩晕时间</param>
    /// <param name="recalculate">是否重写眩晕？</param>
    public virtual void AttackPowerParameter(ComponentBody target, ComponentCreature attacker, Vector3 hitPoint,
        Vector3 hitDirection, ref float impulseFactor, ref float stunTimeFactor, ref bool recalculate)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当人物吃东西时执行
    /// </summary>
    /// <param name="componentPlayer"></param>
    /// <param name="block"></param>
    /// <param name="slotIndex"></param>
    /// <param name="value"></param>
    /// <param name="count"></param>
    /// <returns>是否移交给下一个mod处理，true ： 不移交，false ： 移交</returns>
    public virtual bool ClothingProcessSlotItems(ComponentPlayer componentPlayer, Block block, int slotIndex, int value,
        int count)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 动物吃掉落物时执行
    /// </summary>
    public virtual void OnEatPickable(ComponentEatPickableBehavior eatPickableBehavior, Pickable pickable,
        out bool isDealSucceed)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 人物出生时执行
    /// </summary>
    public virtual bool OnPlayerSpawned(PlayerData.SpawnMode spawnMode, ComponentPlayer componentPlayer,
        Vector3 position)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当人物死亡时执行
    /// </summary>
    /// <param name="playerData"></param>
    public virtual void OnPlayerDead(PlayerData playerData)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当Miner执行AttackBody方法时执行
    /// </summary>
    /// <param name="target"></param>
    /// <param name="attacker"></param>
    /// <param name="hitPoint"></param>
    /// <param name="hitDirection"></param>
    /// <param name="attackPower"></param>
    /// <param name="isMeleeAttack"></param>
    /// <returns>false移交到下一个Mod处理,true不移交</returns>
    public virtual bool AttackBody(ComponentBody target, ComponentCreature attacker, Vector3 hitPoint,
        Vector3 hitDirection, ref float attackPower, bool isMeleeAttack)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当模型对象进行模型设值时执行
    /// </summary>
    public virtual void OnSetModel(ComponentModel componentModel, Model model, out bool isSetSucceed)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当动物模型对象作出动画时执行
    /// skipVanilla 为是否跳过原动画代码
    /// </summary>
    public virtual void OnModelAnimate(ComponentCreatureModel componentCreatureModel, out bool skipVanilla)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 计算护甲免伤时执行
    /// </summary>
    /// <param name="componentClothing"></param>
    /// <param name="attackPower">未计算免伤前的伤害</param>
    /// <param name="applied"></param>
    /// <returns>免伤后的伤害，当多个 mod 都有免伤计算时，取最小值</returns>
    public virtual float ApplyArmorProtection(ComponentClothing componentClothing, float attackPower, out bool applied)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 等级更新时执行
    /// </summary>
    /// <param name="level"></param>
    public virtual void OnLevelUpdate(ComponentLevel level)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// Gui 组件更新时执行
    /// </summary>
    /// <param name="componentGui"></param>
    public virtual void GuiUpdate(ComponentGui componentGui)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// Gui 组件绘制时执行
    /// </summary>
    /// <param name="componentGui"></param>
    /// <param name="camera"></param>
    /// <param name="drawOrder"></param>
    public virtual void GuiDraw(ComponentGui componentGui, Camera camera, int drawOrder)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 更新输入时执行
    /// </summary>
    /// <param name="componentInput"></param>
    /// <param name="widgetInput"></param>
    public virtual void UpdateInput(ComponentInput componentInput, WidgetInput widgetInput)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// ViewWidget绘制屏幕时执行
    /// </summary>
    public virtual void DrawToScreen(ViewWidget viewWidget, Widget.DrawContext dc)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 衣物背包界面被打开时执行
    /// </summary>
    /// <param name="componentGui"></param>
    /// <param name="clothingWidget"></param>
    public virtual void ClothingWidgetOpen(ComponentGui componentGui, ClothingWidget clothingWidget)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当方块被炸掉时执行
    /// </summary>
    public virtual void OnBlockExploded(SubsystemTerrain subsystemTerrain, int x, int y, int z, int value)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当实体被添加时执行
    /// </summary>
    public virtual void OnEntityAdd(Entity entity)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当实体被移除时执行
    /// </summary>
    public virtual void OnEntityRemove(Entity entity)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 自然生成生物列表初始化时执行
    /// </summary>
    /// <param name="spawn"></param>
    /// <param name="creatureTypes"></param>
    public virtual void InitializeCreatureTypes(SubsystemCreatureSpawn spawn,
        List<SubsystemCreatureSpawn.CreatureType> creatureTypes)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 生物出生时执行
    /// </summary>
    /// <param name="spawn"></param>
    /// <param name="entity"></param>
    /// <param name="spawnEntityData"></param>
    /// <param name="spawned"></param>
    public virtual void SpawnEntity(SubsystemSpawn spawn, Entity entity, SpawnEntityData spawnEntityData,
        out bool spawned)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当生物消失时执行
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="componentSpawn"></param>
    public virtual void OnDeSpawned(Entity entity, ComponentSpawn componentSpawn)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 死亡前瞬间执行，skipVanilla 为 true 则跳过死亡后执行掉落等的代码
    /// </summary>
    public virtual void DeadBeforeDrops(ComponentHealth componentHealth, out bool skipVanilla)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 重定义方块更改方法，skipVanilla 为 true 则不执行原版的ChangeCell代码
    /// </summary>
    public virtual void TerrainChangeCell(SubsystemTerrain subsystemTerrain, int x, int y, int z, int value,
        out bool skipVanilla)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 重定义生物受伤方法，skipVanilla 为 true 则不执行原 Injure 代码
    /// </summary>
    public virtual void OnCreatureInjure(ComponentHealth componentHealth, float amount, ComponentCreature attacker,
        bool ignoreInvulnerability, string cause, out bool skipVanilla)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 更改天空颜色
    /// </summary>
    public virtual Color ChangeSkyColor(Color oldColor, Vector3 direction, float timeOfDay,
        float precipitationIntensity, int temperature)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 设置着色器参数
    /// </summary>
    /// <param name="shader"></param>
    /// <param name="camera"></param>
    public virtual void SetShaderParameter(Shader shader, Camera camera)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 更改模型着色器参数的值
    /// </summary>
    public virtual void ModelShaderParameter(Shader shader, Camera camera,
        List<SubsystemModelsRenderer.ModelData> modelsData, float? alphaThreshold)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 天空额外绘制
    /// </summary>
    public virtual void SkyDrawExtra(SubsystemSky subsystemSky, Camera camera)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 设置生物最大组件数，多个 mod 时取最大
    /// </summary>
    /// <returns></returns>
    public virtual int GetMaxInstancesCount()
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 绘制额外模型数据的方法，如人物头顶的名字
    /// </summary>
    /// <param name="modelsRenderer"></param>
    /// <param name="modelData">正在绘制的模型</param>
    /// <param name="camera"></param>
    /// <param name="alphaThreshold"></param>
    public virtual void OnModelRendererDrawExtra(SubsystemModelsRenderer modelsRenderer,
        SubsystemModelsRenderer.ModelData modelData, Camera camera, float? alphaThreshold)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 设定伤害粒子参数
    /// </summary>
    /// <param name="hitValueParticleSystem">粒子</param>
    /// <param name="hit">true 命中 false 未命中</param>
    public virtual void SetHitValueParticleSystem(HitValueParticleSystem hitValueParticleSystem, bool hit)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当储存生物数据时
    /// </summary>
    /// <param name="spawn"></param>
    /// <param name="spawnEntityData"></param>
    public virtual void OnSaveSpawnData(ComponentSpawn spawn, SpawnEntityData spawnEntityData)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当读取生物数据时
    /// </summary>
    public virtual void OnReadSpawnData(Entity entity, SpawnEntityData spawnEntityData)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 区块地形生成时
    /// 注意此方法运行在子线程中
    /// </summary>
    /// <param name="chunk"></param>
    public virtual void OnTerrainContentsGenerated(TerrainChunk chunk)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当区块即将被释放时
    /// KeepWorking 为 true 时该区块会继续保持运作，不被释放
    /// </summary>
    public virtual void ToFreeChunks(TerrainUpdater terrainUpdater, TerrainChunk chunk, out bool keepWorking)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 加载指定区块,如有区块数变动返回 true，否则返回 false
    /// </summary>
    public virtual bool ToAllocateChunks(TerrainUpdater terrainUpdater, TerrainUpdater.UpdateLocation[] locations)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 子系统帧更新时执行
    /// </summary>
    public virtual void SubsystemUpdate(float dt)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当 Project 被加载时执行
    /// </summary>
    /// <param name="project"></param>
    public virtual void OnProjectLoaded(Project project)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当 Project 被释放时执行
    /// </summary>
    public virtual void OnProjectDisposed()
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 方块初始化完成时执行
    /// </summary>
    public virtual void BlocksInitialized()
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 存档开始加载前执行
    /// </summary>
    public virtual object BeforeGameLoading(PlayScreen playScreen, object item)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 加载任务开始时执行
    /// 在 BlocksManager 初始化之前
    /// </summary>
    public virtual void OnLoadingStart(List<Action> actions)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 加载任务结束时执行
    /// 在 BlocksManager 初始化之后
    /// </summary>
    /// <param name="actions"></param>
    public virtual void OnLoadingFinished(List<Action> actions)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 游戏设置数据保存时执行
    /// </summary>
    /// <param name="xElement"></param>
    public virtual void SaveSettings(XElement xElement)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 游戏设置数据加载时执行
    /// </summary>
    /// <param name="xElement"></param>
    public virtual void LoadSettings(XElement xElement)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// Xdb 文件加载时执行
    /// </summary>
    /// <param name="xElement"></param>
    public virtual void OnXdbLoad(XElement xElement)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// Project.xml 加载时执行
    /// </summary>
    /// <param name="xElement"></param>
    public virtual void ProjectXmlLoad(XElement xElement)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// Project.xml 保存时执行
    /// </summary>
    /// <param name="xElement"></param>
    public virtual void ProjectXmlSave(XElement xElement)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 配方解码时执行
    /// </summary>
    /// <param name="recipes"></param>
    /// <param name="element">配方的 XElement</param>
    /// <param name="decoded">是否解码成功，不成功交由下一个 mod 处理</param>
    public virtual void OnCraftingRecipeDecode(List<CraftingRecipe> recipes, XElement element, out bool decoded)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 比较两个配方是否相等时执行
    /// </summary>
    /// <param name="requiredIngredients"></param>
    /// <param name="actualIngredient"></param>
    /// <param name="matched">是否匹配成功，不成功交由下一个Mod处理</param>
    public virtual bool MatchRecipe(string[] requiredIngredients, string[] actualIngredient, out bool matched)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 获得解码结果时执行
    /// </summary>
    /// <param name="result">结果字符串</param>
    /// <param name="decoded">是否解码成功，不成功交由下一个Mod处理</param>
    /// <returns></returns>
    public virtual int DecodeResult(string result, out bool decoded)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 解码配方
    /// </summary>
    /// <param name="ingredient"></param>
    /// <param name="craftingId"></param>
    /// <param name="data"></param>
    /// <param name="decoded">是否解码成功，不成功交由下一个Mod处理</param>
    public virtual void DecodeIngredient(string ingredient, out string craftingId, out int? data, out bool decoded)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 改变相机模式时执行
    /// </summary>
    /// <param name="componentPlayer"></param>
    /// <param name="componentGui"></param>
    public virtual void OnCameraChange(ComponentPlayer componentPlayer, ComponentGui componentGui)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 屏幕截图时执行
    /// </summary>
    public virtual void OnCapture()
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 更改主页背景音乐
    /// </summary>
    public virtual void MenuPlayMusic(out string contentMusicPath)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 摇人行为
    /// </summary>
    /// <param name="herdBehavior"></param>
    /// <param name="target"></param>
    /// <param name="maxRange"></param>
    /// <param name="maxChaseTime"></param>
    /// <param name="isPersistent"></param>
    public virtual void CallNearbyCreaturesHelp(ComponentHerdBehavior herdBehavior, ComponentCreature target,
        float maxRange, float maxChaseTime, bool isPersistent)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 挖掘触发宝物生成时，注意这里能获取到上个Mod生成宝物的情况
    /// </summary>
    /// <param name="neighborX"></param>
    /// <param name="neighborY"></param>
    /// <param name="neighborZ"></param>
    /// <param name="blockValue">宝物的方块值</param>
    /// <param name="count">宝物数量</param>
    /// <param name="isGenerate">是否继续让其它Mod处理</param>
    /// <param name="subsystemTerrain"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public virtual void OnTreasureGenerate(SubsystemTerrain subsystemTerrain, int x, int y, int z, int neighborX,
        int neighborY, int neighborZ, ref int blockValue, ref int count, out bool isGenerate)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当界面被创建时
    /// </summary>
    /// <param name="widget"></param>
    public virtual void OnWidgetConstruct(ref Widget widget)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 当 ModalPanelWidget 被设置时执行
    /// </summary>
    public virtual void OnModalPanelWidgetSet(ComponentGui gui, Widget old, Widget @new)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 生成地形顶点时使用
    /// </summary>
    public virtual void GenerateChunkVertices(TerrainChunk chunk, bool even)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 生成光源数据
    /// </summary>
    /// <param name="lightSources">光源</param>
    /// <param name="chunk">区块</param>
    public virtual void GenerateChunkLightSources(DynamicArray<TerrainUpdater.LightSource> lightSources,
        TerrainChunk chunk)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }

    /// <summary>
    /// 计算动物模型光照
    /// </summary>
    /// <param name="subsystemTerrain"></param>
    /// <param name="p">动物位置</param>
    /// <param name="num">原版计算出来的强度</param>
    public virtual void CalculateSmoothLight(SubsystemTerrain subsystemTerrain, Vector3 p, ref float num)
    {
        throw new InvalidOperationException(InvalidOperationMessage);
    }
}