using Engine;
using Engine.Graphics;
using Engine.Media;
using GameEntitySystem;
using System.Collections.Generic;
using System.Xml.Linq;
using TemplatesDatabase;

namespace Game
{
    public abstract class ModLoader
    {
        public ModEntity Entity;

        /// <summary>
        /// 当ModLoader类被实例化时执行
        /// </summary>
        public virtual void __ModInitialize()
        {
        }

        /// <summary>
        /// Mod被卸载时执行
        /// </summary>
        public virtual void ModDispose()
        {
        }

        /// <summary>
        /// 实体打其它实体时执行
        /// </summary>
        /// <param name="miner">发出者</param>
        /// <param name="componentBody">目标实体</param>
        /// <param name="hitPoint">打击点</param>
        /// <param name="hitDirection">打击方向</param>
        /// <param name="attackPower">攻击力</param>
        /// <param name="playerProbability">(攻击者是玩家)武器攻击的命中率</param>
        /// <param name="creatureProbability">(攻击者不是玩家)其它实体攻击命中率</param>
        /// <returns>true 移交 false 不移交 下一个Mod处理</returns>
        public virtual bool OnMinerHit(ComponentMiner miner, ComponentBody componentBody, Vector3 hitPoint, Vector3 hitDirection, ref float attackPower, ref float playerProbability, ref float creatureProbability)
        {
            return true;
        }
        /// <summary>
        /// 当人物挖掘时执行
        /// </summary>
        /// <param name="miner">发出者</param>
        /// <param name="raycastResult">挖掘目标</param>
        /// <param name="DigProgress">挖掘进度</param>
        /// <returns>true 移交 false 不移交 下一个Mod处理</returns>
        public virtual bool OnMinerDig(ComponentMiner miner, TerrainRaycastResult raycastResult, ref float DigProgress)
        {
            return true;
        }

        /// <summary>
        /// 更改击退和晕眩效果
        /// </summary>
        /// <param name="impulseFactor">击退系数，越大越远，所有Mod共享参数</param>
        /// <param name="stunTimeFactor">眩晕系数，越大越长，所有Mod共享参数</param>
        public virtual void AttackPowerParameter(ref float impulseFactor, ref float stunTimeFactor)
        {

        }

        /// <summary>
        /// 当人物吃东西时执行
        /// </summary>
        /// <param name="componentPlayer"></param>
        /// <param name="block"></param>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <param name="processCount"></param>
        /// <param name="processedValue"></param>
        /// <param name="processedCount"></param>
        /// <returns>true 不移交 false 移交到下一个mod处理</returns>
        public virtual bool ClothingProcessSlotItems(ComponentPlayer componentPlayer, Block block, int slotIndex, int value, int count)
        {
            return false;
        }

        /// <summary>
        /// 动物吃掉落物时执行
        /// </summary>
        /// <param name="eatPickableBehavior"></param>
        /// <param name="EatPickable"></param>
        /// <returns>true 移交 false 不移交下一个Mod</returns>
        public virtual bool OnEatPickable(ComponentEatPickableBehavior eatPickableBehavior, Pickable EatPickable)
        {
            return true;
        }

        /// <summary>
        /// 人物出生时执行
        /// </summary>
        /// <param name="spawnMode"></param>
        /// <param name="componentPlayer"></param>
        /// <param name="position"></param>
        public virtual void OnPlayerSpawned(PlayerData.SpawnMode spawnMode, ComponentPlayer componentPlayer, Vector3 position)
        {
        }

        /// <summary>
        /// 当人物死亡时执行
        /// </summary>
        /// <param name="playerData"></param>
        public virtual void OnPlayerDead(PlayerData playerData)
        {

        }

        /// <summary>
        /// 当Miner执行AttackBody方法时执行
        /// </summary>
        /// <param name="target">攻击目标</param>
        /// <param name="attacker">发出者</param>
        /// <param name="hitPoint">攻击位置</param>
        /// <param name="hitDirection">攻击方向</param>
        /// <param name="attackPower">Hit计算后的攻击力，所有Mod共享参数</param>
        /// <param name="isMeleeAttack">是否是近战武器伤害</param>
        /// <returns>true移交 false不移交 到下一个Mod处理</returns>
        public virtual bool AttackBody(ComponentBody target, ComponentCreature attacker, Vector3 hitPoint, Vector3 hitDirection, ref float attackPower, bool isMeleeAttack)
        {
            return true;
        }

        /// <summary>
        /// 当模型对象进行模型设值时执行
        /// </summary>
        /// <param name="componentModel"></param>
        /// <param name="model"></param>
        public virtual void OnSetModel(ComponentModel componentModel, Model model)
        {
        }

        /// <summary>
        /// 当动物模型对象作出动画时执行
        /// Skip为是否跳过原动画代码
        /// </summary>
        public virtual void OnModelAnimate(ComponentCreatureModel componentCreatureModel, out bool Skip)
        {
            Skip = false;
        }
        /// <summary>
        /// 计算护甲免伤时执行
        /// </summary>
        /// <param name="componentClothing"></param>
        /// <param name="baseAttackPower">原始伤害</param>
        /// <param name="attackPower">计算护甲后的伤害，Mod共用数值</param>
        public virtual void ApplyArmorProtection(ComponentClothing componentClothing, float baseAttackPower, ref float attackPower)
        {
        }

        /// <summary>
        /// 等级更新时执行
        /// </summary>
        /// <param name="level"></param>
        public virtual void OnLevelUpdate(ComponentLevel level)
        {
        }

        /// <summary>
        /// Gui组件帧更新时执行
        /// </summary>
        /// <param name="componentGui"></param>
        public virtual void GuiUpdate(ComponentGui componentGui)
        {
        }

        /// <summary>
        /// Gui组件绘制时执行
        /// </summary>
        /// <param name="componentGui"></param>
        /// <param name="camera"></param>
        /// <param name="drawOrder"></param>
        public virtual void GuiDraw(ComponentGui componentGui, Camera camera, int drawOrder)
        {
        }

        /// <summary>
        /// ViewWidget绘制屏幕时执行
        /// </summary>
        public virtual void DrawToScreen(ViewWidget viewWidget, Widget.DrawContext dc)
        {
        }

        /// <summary>
        /// 衣物背包界面被打开时执行
        /// </summary>
        /// <param name="componentGui"></param>
        /// <param name="clothingWidget"></param>
        public virtual void ClothingWidgetOpen(ComponentGui componentGui, ClothingWidget clothingWidget)
        {
        }

        /// <summary>
        /// 当方块被炸掉时执行
        /// </summary>
        public virtual void OnBlockExploded(SubsystemTerrain subsystemTerrain, int x, int y, int z, int value)
        {
        }

        /// <summary>
        /// 当实体被添加时执行
        /// </summary>
        public virtual void OnEntityAdd(Entity entity)
        {
        }

        /// <summary>
        /// 当实体被移除时执行
        /// </summary>
        public virtual void OnEntityRemove(Entity entity)
        {
        }

        /// <summary>
        /// 自然生成生物列表初始化时执行
        /// </summary>
        /// <param name="spawn"></param>
        /// <param name="creatureTypes"></param>
        public virtual void InitializeCreatureTypes(SubsystemCreatureSpawn spawn, List<SubsystemCreatureSpawn.CreatureType> creatureTypes) 
        {
        }

        /// <summary>
        /// 还原消失生物数据时执行
        /// </summary>
        /// <param name="spawn"></param>
        /// <param name="entity"></param>
        /// <param name="spawnEntityData"></param>
        public virtual void OnSubsystemSpawnSpawn(Entity entity, ValuesDictionary spawnEntityData)
        {
        }

        /// <summary>
        /// 存储消失生物数据时执行
        /// </summary>
        /// <param name="spawn"></param>
        /// <param name="data"></param>
        public virtual void OnSubsystemSpawnDespawn(ComponentSpawn spawn, ValuesDictionary data)
        {


        }
        /// <summary>
        /// 设置跳跃是否可放置方块
        /// </summary>
        public virtual void JumpToPlace(out bool Pass)
        {
            Pass = false;
        }

        /// <summary>
        /// 设置着色器参数
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="camera"></param>
        public virtual void SetShaderParameter(Shader shader, Camera camera)
        {
        }

        /// <summary>
        /// 更改模型着色器参数的值
        /// </summary>
        public virtual void ModelShaderParameter(Shader shader, Camera camera, List<SubsystemModelsRenderer.ModelData> modelsData, float? alphaThreshold)
        {
        }

        /// <summary>
        /// 天空额外绘制
        /// </summary>
        public virtual void SkyDrawExtra(SubsystemSky subsystemSky, Camera camera)
        {
        }

        /// <summary>
        /// 设置生物最大组件数，多个Mod时取最大
        /// </summary>
        /// <returns></returns>
        public virtual int GetMaxInstancesCount()
        {
            return 7;
        }

        /// <summary>
        /// 绘制额外模型数据的方法，如人物头顶的名字
        /// </summary>
        /// <param name="modelsRenderer"></param>
        /// <param name="componentModel"></param>
        /// <param name="camera"></param>
        /// <param name="alphaThreshold"></param>
        public virtual void OnModelRendererDrawExtra(SubsystemModelsRenderer modelsRenderer, ComponentModel componentModel, Camera camera, float? alphaThreshold)
        {
        }

        /// <summary>
        /// 设定伤害粒子参数
        /// </summary>
        /// <param name="hitValueParticleSystem">粒子</param>
        /// <param name="Hit">true 命中 false 未命中</param>
        public virtual void SetHitValueParticleSystem(HitValueParticleSystem hitValueParticleSystem, bool Hit)
        {
        }

        /// <summary>
        /// 当区块加载时，区块的生物数据被读取时执行
        /// </summary>
        /// <param name="spawn"></param>
        /// <param name="data"></param>
        /// <param name="creaturesData"></param>
        public virtual void LoadSpawnsData(SubsystemSpawn spawn, List<ValuesDictionary> creaturesData) 
        { 
        }

        /// <summary>
        /// 当区块卸载时，区块的生物数据被保存时执行
        /// </summary>
        /// <param name="spawn"></param>
        /// <param name="spawnsData"></param>
        /// <returns></returns>
        public virtual void SaveSpawnsData(SubsystemSpawn spawn, List<ValuesDictionary> spawnsData) 
        { 
        }

        /// <summary>
        /// 区块地形生成时
        /// 注意此方法运行在子线程中
        /// </summary>
        /// <param name="chunk"></param>
        public virtual void OnTerrainContentsGenerated(TerrainChunk chunk)
        {
        }

        /// <summary>
        /// 更改方块颜色，只对使用DrawCubeBlock和DrawFlatBlock的方块生效
        /// </summary>
        /// <param name="oldColor">原来的颜色</param>
        /// <param name="oldTopColor">原来的顶部颜色</param>
        /// <param name="value">方块的值</param>
        /// <returns></returns>
        public virtual void ChangeBlockColor(Color oldColor, Color oldTopColor, int value, DrawBlockEnvironmentData environmentData, out Color color, out Color topColor)
        {
            color = oldColor;
            topColor = oldTopColor;
        }

        /// <summary>
        /// 更改已放置方块颜色，只对使用GenerateCubeVertices(单一颜色绘制)和GenerateCrossfaceVertices的方块生效
        /// </summary>
        /// <param name="oldColor">原来的颜色</param>
        /// <param name="value">方块的值</param>
        /// <returns></returns>
        public virtual void ChangePlacedBlockColor(Color oldColor, int value, int x, int y, int z, out Color color)
        {
            color = oldColor;
        }

        /// <summary>
        /// 更改方块碎屑颜色，只对使用BlockDebrisParticleSystem的方块生效
        /// </summary>
        /// <param name="oldColor">原来的颜色</param>
        /// <param name="value">方块的值</param>
        public virtual void ChangeBlockDebrisParticleColor(Color oldColor, int value, int x, int y, int z, SubsystemTerrain subsystemTerrain, out Color color)
        {
            color = oldColor;
        }

        /// <summary>
        /// 子系统帧更新时执行
        /// </summary>
        public virtual void SubsystemUpdate(float dt)
        {
        }

        /// <summary>
        /// 当Project被加载时执行
        /// </summary>
        /// <param name="project"></param>
        public virtual void OnProjectLoaded(Project project)
        {
        }

        /// <summary>
        /// 当Project被释放时执行
        /// </summary>
        public virtual void OnProjectDisposed()
        {
        }

        /// <summary>
        /// 方块初始化完成时执行
        /// </summary>
        public virtual void BlocksInitalized()
        {
        }

        /// <summary>
        /// 存档开始加载前执行
        /// </summary>
        public virtual object BeforeGameLoading(PlayScreen playScreen, object item)
        {
            return item;
        }
        /// <summary>
        /// 加载任务开始时执行
        /// 在BlocksManager初始化之前
        /// </summary>
        public virtual void OnLoadingStart(List<System.Action> actions) { 
        
        }

        /// <summary>
        /// 加载任务结束时执行
        /// 在BlocksMAnager初始化之后
        /// </summary>
        /// <param name="loadingActions"></param>
        public virtual void OnLoadingFinished(List<System.Action> actions) 
        { 
        }

        /// <summary>
        /// 游戏设置数据保存时执行
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void SaveSettings(XElement xElement)
        {
        }

        /// <summary>
        /// 游戏设置数据加载时执行
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void LoadSettings(XElement xElement)
        {
        }

        /// <summary>
        /// Xdb文件加载时执行
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void OnXdbLoad(XElement xElement)
        {
        }

        /// <summary>
        /// Project.xml加载时执行
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void ProjectXmlLoad(XElement xElement) 
        { 
        }

        /// <summary>
        /// Project.xml保存时执行
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void ProjectXmlSave(XElement xElement) 
        { 
        }

        /// <summary>
        /// CraftingRecipts.xml解析
        /// </summary>
        /// <param name="element">配方的Xelement</param>
        /// <return>true 是 false 否交给下一个Mod处理</return>
        public virtual bool OnCraftingRecipeDecode(List<CraftingRecipe> m_recipes, XElement element)
        {
            return true;
        }

        /// <summary>
        /// 配方匹配时执行
        /// </summary>
        /// <param name="requiredIngredients"></param>
        /// <param name="actualIngredient"></param>
        /// <returns>是否匹配成功，不成功交由下一个Mod处理</returns>
        public virtual bool MatchRecipe(string[] requiredIngredients, string[] actualIngredients)
        {
            if (actualIngredients.Length > 9) return true;
            string[] array = new string[9];
            for (int i = 0; i < 2; i++)
            {
                for (int j = -3; j <= 3; j++)
                {
                    for (int k = -3; k <= 3; k++)
                    {
                        bool flip = (i != 0) ? true : false;
                        if (!CraftingRecipesManager.TransformRecipe(array, requiredIngredients, k, j, flip))
                        {
                            continue;
                        }
                        bool flag = true;
                        for (int l = 0; l < 9; l++)
                        {
                            if (l == actualIngredients.Length || !CraftingRecipesManager.CompareIngredients(array[l], actualIngredients[l]))
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 获得解码结果时执行
        /// </summary>
        /// <param name="result">结果字符串</param>
        /// <param name="Decoded">true 是 false 否 不成功交由下一个Mod处理</param>
        /// <returns></returns>
        public virtual int DecodeResult(string result, out bool deal)
        {
            deal = false;
            return 0;
        }

        /// <summary>
        /// 解码配方
        /// </summary>
        /// <param name="ingredient"></param>
        /// <param name="craftingId"></param>
        /// <param name="data"></param>
        /// <returns>true是 false 否交给下一个mod处理</returns>
        public virtual bool DecodeIngredient(string ingredient, out string craftingId, out int? data)
        {
            craftingId = "";
            data = null;
            return true;
        }

        /// <summary>
        /// 改变相机模式时执行
        /// </summary>
        /// <param name="m_componentPlayer"></param>
        /// <param name="componentGui"></param>
        public virtual void OnCameraChange(ComponentPlayer m_componentPlayer, ComponentGui componentGui)
        {
        }

        /// <summary>
        /// 屏幕截图时执行
        /// </summary>
        public virtual void OnCapture()
        {
        }

        /// <summary>
        /// 摇人行为
        /// </summary>
        /// <param name="herdBehavior"></param>
        /// <param name="target"></param>
        /// <param name="maxRange"></param>
        /// <param name="maxChaseTime"></param>
        /// <param name="isPersistent"></param>
        public virtual void CallNearbyCreaturesHelp(ComponentHerdBehavior herdBehavior, ComponentCreature target, float maxRange, float maxChaseTime, bool isPersistent) 
        { 
        }

        /// <summary>
        /// 挖掘触发宝物生成时，BlockValue和Count也可能是上一个Mod的结果
        /// </summary>
        /// <param name="BlockValue">宝物的方块值</param>
        /// <param name="Count">宝物数量</param>
        /// <param name="IsGenerate">true 是 false 否 交给下一个mod处理</param>
        public virtual void OnTreasureGenerate(SubsystemTerrain subsystemTerrain, int x, int y, int z, int neighborX, int neighborY, int neighborZ, ref int BlockValue, ref int Count)
        {
        }
        /// <summary>
        /// 当实体掉落到岩浆时
        /// </summary>
        /// <param name="componentHealth"></param>
        /// <returns>true 是 false 否 交给下一个mod处理</returns>
        public virtual bool OnBodyInMagmaBlock(ComponentHealth componentHealth, float dt)
        {
            return true;
        }
    }
}
