# 2D ARPG 作品集 Demo

一个基于 Unity 制作的 2D 横版动作 RPG Demo，重点展示战斗手感、系统联动、Boss 演出，以及在教程基础上的二次开发能力。

这个项目并不是完全从零搭建的商业项目，而是在教程提供的基础 ARPG 框架上，继续扩展成一个更完整、更适合演示和写进简历的小型作品。  
我主要负责和补充的部分集中在任务、对话、NPC 好感度、商店与锻造交互、存档持久化、敌人与 Boss 机制设计等方向。

## 近期更新

- 修复了 checkpoint 复活逻辑异常，避免错误激活全部存档点
- 优化了 checkpoint 休息与场景刷新流程，提升演示稳定性
- 修复了回营地传送门与返回原关卡传送门的生成问题
- 修复了部分打包版中死亡 UI 按钮失效的问题
- 优化了 Boss 战音乐切换，支持旧音乐淡出与新音乐缓慢淡入
- 补充并接入了跳跃、落地、冲刺、反击成功等音效
- 调整了部分镜头与演出表现，优化 Boss 聚焦和转阶段体验
- 修复了超大字体资源上传限制问题，已使用 Git LFS 管理大文件

## 项目概述

- 引擎版本：`Unity 2022.3.62f3c1`
- 项目类型：`2D 横版动作 RPG`
- 项目定位：`简历 / 作品集 Demo`
- 核心展示场景：
  - `Level_0`：营地、NPC 交互、任务引导
  - `Level_1`：常规战斗、技能组合、敌人 AI 展示
  - `Level_2`：Reaper Boss 战与收尾演出

## 我在教程基础上的主要扩展

- 实现了数据驱动的对话系统，支持基于任务状态、NPC 好感度、玩家金币等条件切换对话分支
- 实现了 NPC 好感度系统，并把好感度显示整合进对话 UI
- 实现了任务链系统，后续任务必须完成前置任务后才能接取
- 重构了 Blacksmith 的交互接口，使其和 Merchant 一样支持选择式对话，并可进入仓库、合成、任务界面
- 为 Reaper Boss 设计并实现了入场镜头、Boss 战音乐触发、二阶段转场、黑屏演出、多段传送背刺等表现流程
- 重做了 Mage 的施法逻辑，使其能以火 / 冰 / 雷三种元素进行整轮统一属性施法，并附带元素效果
- 补全了存档系统对任务、好感度、checkpoint、宝箱开启状态等内容的持久化支持

## 核心系统

### 1. 对话与 NPC 交互

项目中的对话使用 `ScriptableObject` 驱动，每条对话都可以独立配置：

- 文本内容
- 选项分支
- 对话行为
- 任务条件
- 好感度条件
- 金币条件

对话不仅用于显示文本，还可以直接作为功能入口，触发：

- 商店界面
- 任务界面
- 合成界面
- 仓库界面
- 任务奖励结算

相关脚本：

- `Assets/Script/Data/Dialogue Data/DialogueLineSO.cs`
- `Assets/Script/Object/Object_NPC.cs`
- `Assets/Script/Object/Object_Merchant.cs`
- `Assets/Script/Object/Object_Blacksmith.cs`
- `Assets/Script/UI/UI_Dialogue.cs`

### 2. 任务链系统

任务支持 `击杀`、`对话`、`交付` 三种类型，每个任务都可以配置：

- 任务目标 id
- 所需进度数量
- 前置任务
- 奖励类型
- 奖励物品
- 好感度奖励

我在此基础上实现了：

- 前置任务校验
- 指定 NPC 处接取任务
- 指定 NPC 处领取奖励
- 任务进度与完成状态存档

相关脚本：

- `Assets/Script/Data/Quest Data/QuestDataSO.cs`
- `Assets/Script/Player/Player_QuestManager.cs`

### 3. NPC 好感度系统

每个 NPC 都有独立的好感度数值，并且会影响对话可见性和奖励反馈。

当前好感度系统支持：

- 根据不同 NPC 保存独立好感度
- 对话选项直接增减好感度
- 任务奖励结算时增加指定 NPC 好感度
- 在对话 UI 中实时显示好感度值与进度条

相关脚本：

- `Assets/Script/Player/Player_NpcAffinityManager.cs`
- `Assets/Script/UI/UI_Dialogue.cs`

### 4. 商店 / 锻造 / 仓库统一交互

Merchant 和 Blacksmith 都被接入到统一的 NPC 交互框架中，不再是单纯靠场景按钮或硬编码打开界面。

其中：

- Merchant 支持商店与任务入口
- Blacksmith 支持仓库、合成、任务入口
- 两者都支持选择式对话和条件分支

这部分的重点不只是“能打开 UI”，而是让对话、任务、物品系统和 NPC 行为真正串起来。

### 5. 数值、元素与战斗系统

项目使用统一的 `EntityStats` 结构管理玩家与敌人的战斗属性，支持：

- 物理伤害
- 暴击率 / 暴击伤害
- 护甲减伤
- 闪避
- 火 / 冰 / 雷元素伤害
- 火 / 冰 / 雷元素抗性

伤害计算通过 `DamageScaleData` 与 `ElementalEffectData` 进行组合，便于后续给不同敌人和技能复用。

相关脚本：

- `Assets/Script/Entity/EntityStats.cs`
- `Assets/Script/Data/Attack Data/AttackData.cs`
- `Assets/Script/Data/Attack Data/DamageScaleData.cs`
- `Assets/Script/Data/Attack Data/ElementalEffectData.cs`

### 6. Reaper Boss 战演出

Reaper 是这个 Demo 的收尾 Boss，我在原有敌人状态机基础上，额外补充了更偏“作品展示”的演出逻辑：

- 进入 Boss 区域前的 Cinemachine 镜头聚焦
- Boss 战音乐与普通 battle state 解耦，避免战斗中断时音乐被打断
- Phase 2 转场黑屏演出
- 残影效果与多段传送
- 传送到玩家身后进行连续打击
- 二阶段参数增强与更高压制力

相关脚本：

- `Assets/Script/Enemy/ReaperBossIntroTrigger.cs`
- `Assets/Script/Enemy/Enemy_Reaper.cs`
- `Assets/Script/Enemy/EnemyReaperState/Enemy_ReaperPhaseShiftState.cs`

### 7. 存档与持久化

当前存档系统已经支持保存：

- 背包与装备
- 技能升级
- checkpoint 状态
- 已开启宝箱
- 当前任务与已完成任务
- NPC 好感度
- 上次所在场景与角色位置

这让整个项目从“单场景玩法演示”变成了一个更完整的可游玩 Demo。

相关脚本：

- `Assets/Script/SaveSystem/SaveManager.cs`
- `Assets/Script/SaveSystem/GameData.cs`
- `Assets/Script/Object/Object_Chest.cs`

## 我处理过的代表性技术问题

### 1. Domain Expansion 在顶到障碍物时卡技能状态

问题本质是：角色上升过程中如果被天花板提前打断，技能状态没有正确收尾，角色会卡在技能状态里。

我的处理方式：

- 上升前先向上做射线检测
- 根据碰撞结果计算真实可上升距离
- 如果上升速度异常中断，则提前进入悬空施法阶段
- 技能结束后统一恢复重力和受击状态

相关脚本：

- `Assets/Script/Player/PlayerState/PlayerDomainExpansionState.cs`

### 2. checkpoint 休息后宝箱被错误刷新

问题本质是：宝箱开启状态只存在于场景运行时，没有进入存档层。

我的处理方式：

- 为每个宝箱生成稳定的保存 id
- 将开启状态写入 `GameData`
- 读档时恢复开箱动画状态，但不重复掉落物品

相关脚本：

- `Assets/Script/Object/Object_Chest.cs`

### 3. 商人任务对话被普通对话覆盖

问题本质是：通用对话优先级高于任务条件对话，导致满足任务条件时仍然走了默认分支。

我的处理方式：

- 调整 NPC 对话解析顺序
- 先判断带任务条件的优先对话
- 再回退到普通优先级或默认对话

相关脚本：

- `Assets/Script/Object/Object_NPC.cs`

### 4. Reaper Boss 音乐在战斗中被中断

问题本质是：Boss 战音乐原本绑定在 `battleState` 的进入 / 退出逻辑上，而 Boss 状态机会频繁切换。

我的处理方式：

- 将 Boss 音乐启动从 `ReaperBattleState` 移到 `ReaperBossIntroTrigger`
- 让音乐和战斗状态机解耦
- 保证整个 Boss 演示段的音频体验稳定

相关脚本：

- `Assets/Script/Enemy/ReaperBossIntroTrigger.cs`
- `Assets/Script/Enemy/EnemyReaperState/Enemy_ReaperBattleState.cs`

## 默认操作键位

- `A / D` 或方向键：移动
- `Space`：跳跃
- `鼠标左键`：普通攻击 / 确认投剑
- `鼠标右键`：进入投剑瞄准 / 松开取消
- `Q`：反击
- `Left Shift`：冲刺
- `E`：法术技能
- `R`：Domain Expansion
- `1 / 2`：使用快捷物品
- `F`：交互 / 推进对话
- `C`：角色 / 背包界面
- `L`：技能树界面
- `Esc`：选项或关闭当前 UI

## 运行方式

1. 使用 Unity Hub 打开项目根目录：

   `F:\unity\RPG`

2. Unity 版本：

   `2022.3.62f3c1`

3. 推荐打开的场景：

- `Assets/Scenes/MainMenu.unity`
- `Assets/Scenes/Level_0.unity`
- `Assets/Scenes/Level_1.unity`
- `Assets/Scenes/Level_2.unity`

如果用于演示视频，推荐流程为：

`Level_0 -> Level_1 -> Level_2`

## 项目目录结构

```text
Assets/
├─ Scenes/                场景文件
├─ Script/
│  ├─ Player/             玩家控制、状态机、技能、任务与好感度管理
│  ├─ Enemy/              敌人 AI、Mage、Reaper Boss 逻辑
│  ├─ Object/             Merchant、Blacksmith、Checkpoint、Chest、Waypoint 等场景交互物
│  ├─ UI/                 Dialogue、Quest、Inventory、Merchant、Craft、Storage 等 UI
│  ├─ SaveSystem/         存档与数据持久化
│  ├─ Data/               对话、任务、物品、攻击相关 ScriptableObject
│  └─ StatSystem/         数值系统与默认属性配置
├─ Prefab/                预制体资源
└─ Data/                  游戏数据资源
```

## 说明

- 这是一个偏作品集展示的 Demo，而不是完整商业化项目
- 项目的重点在于系统整合、玩法补完、演出表现与技术表达
- 如果你是招聘方或面试官，最值得看的内容是：
  - `Level_0` 的任务 / 对话 / NPC 交互
  - `Level_2` 的 Reaper Boss 演出与战斗设计
