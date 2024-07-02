# SurvivalCraft-API

## 介绍

生存战争插件版源码，项目重构版

SCAPI翻新了引擎，性能大幅度提升，建议换到1.6x
同时⭐SCAPI1.7也计划发布⭐，采用了全新加载器

推荐新开发者直接从1.6开始，少走弯路
就10分钟，直接进入API1.6新时代 ！
走的是直接穿越的康庄大道 ！！！
过尽千帆仍有梦，眉眼清扬是少年 ！
API的兄弟们， 让喀秋莎火箭——模组无序加载机制, 米格15战斗机——模组自动依赖检索，水陆两用坦克——1.5-1.6兼容层，护送大家远航 !
这么多年同舟共济，辛苦了！ 咱吃饱喝足，拿好开天斧（开发工具包）乘着SCAPI制作出优秀的模组，再上征途 !
莫把未来视虚幻，踏实迈步始为真。
夺得天机真造化，一往无前写华章！！
愿各位模组开发者踏上SCAPI的台阶，向着高处前进！！！

## 下载

进入 [发行版](https://gitee.com/THPRC/survivalcraft-api/releases/latest)
* Android系统下载后缀为`.apk`的安装包，安装后即可运行
* Windows系统下载后缀为`.7z`的压缩包，推荐使用 [7-Zip](https://www.7-zip.org/download.html) 进行解压，运行解压后的`.exe`文件

> Windows系统请勿直接运行压缩包中的文件，务必解压后运行

## 构建说明

1. 首先使用 Git 克隆此仓库
```bat
git clone https://gitee.com/THPRC/survivalcraft-api.git
```
> 还没有Git？[官网下载](https://git-scm.com/downloads)

2. 进入此仓库
```bat
cd survivalcraft-api
```

3. 更新子模块
```bat
git submodule update --init
```

4. 使用[Visual Studio](https://visualstudio.microsoft.com/)打开`survivalcraft-api`目录中的`SurvivalCraft.sln`，点击`生成`-`生成解决方案`，如果报错未安装相应功能，请按提示完成安装
> 不想安装Android负载？在`解决方案资源管理器`-`解决方案'SurvivalCraft'`-`安卓端`右键，点击`卸载解决方案文件夹中的项目`