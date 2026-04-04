# 三场景演示地图蓝图（简历录屏版）

## 目标
- 用现有系统做一个 `4~6` 分钟可录制的完整演示流程。
- 三关分别突出：`系统完整性`、`战斗AI与技能`、`Boss设计与状态机`。
- 仅使用你项目里已存在资源：`Level_0/1/2`、敌人预制体、任务/NPC/UI/技能系统、Reaper 二阶段。

---

## 场景 1（Level_0）- 营地与任务引导

### 演示重点
- 对话、接任务、商店、锻造、存档点、基础战斗与掉落。
- 让面试官先看到“完整RPG管线”，再进入动作玩法。

### 瓦片地图结构（建议）
- `Tilemap_Ground_Collision`：主路径 + 两段短平台。
- `Tilemap_Deco_Back`：背景地砖、墙体、火把。
- `Tilemap_Deco_Front`：前景遮挡少量使用，避免遮挡角色。
- `Tilemap_Blocker`：不可达边界（左右封边和底部深渊边缘）。

### 区域分配（从左到右）
1. `营地区（安全）`
- 放 `Object_MerchantNPC`（商店+对话+任务入口）。
- 放 `Object_BlacksmithNPC`（锻造交互）。
- 放 `Object_CheckPoint`（开场存档点）。

2. `训练区（轻战斗）`
- 2~3 只 `Enemy_Skeleton`（分散摆放，不要堆叠）。
- 1 个 `Object_Chest`（奖励一件装备或药水）。
- 1 个 `Object_Buff`（演示状态/增益反馈）。

3. `出口区`
- 放 `Object_Waypoint`（Exit 到 `Level_1`）。
- 入口旁边留 1 个回头检查点，保证死亡后恢复体验稳定。

### 录屏动作脚本（45~70 秒）
- 与商人对话 -> 打开任务/商店。
- 与铁匠交互 -> 展示锻造或仓库。
- 打开/关闭背包与装备界面（2~3秒）。
- 去训练区打掉 2 波小怪 -> 拾取掉落 -> 激活 checkpoint。
- 前往出口切到 Level_1。

---

## 场景 2（Level_1）- 进阶战斗试炼

### 演示重点
- 敌人行为差异：近战、分裂、风筝、法术。
- 玩家技能组合：Dash、Sword Throw、Time Echo、Shard、Domain。

### 瓦片地图结构（建议）
- `Tilemap_Ground_Collision`：三段式推进（前场/中场/后场）。
- `Tilemap_OneWayPlatform`：2 层小平台，制造上下追击。
- `Tilemap_Deco_Back`：重复纹理+火把，减少空洞感。
- `Tilemap_Hazard(Optional)`：小范围坑洞，避免过多即死设计。

### 敌人与元素编排（建议顺序）
1. `前场（热身）`
- `Enemy_Slime_Large` x1（展示分裂链）。
- `Enemy_Skeleton` x2（近战压迫）。

2. `中场（AI展示）`
- `Enemy_ArcherElf` x1（风筝后撤逻辑）。
- `Enemy_Mage` x1（抛物线法术+后撤/施法状态）。
- 场地中间放 `Object_CheckPoint`。

3. `后场（技能高光）`
- `Enemy_Slime_Medium` x2 + `Enemy_Skeleton` x2（多目标混战）。
- 预留一个相对空旷区，专门录 `Domain Expansion + Spell Casting`。

4. `出口区`
- `Object_Waypoint`（Exit 到 `Level_2`）。

### 录屏动作脚本（90~130 秒）
- 先打 Slime，展示分裂处理。
- 进中场，拉扯 Archer/Mage，演示走位和技能连段。
- 激活 checkpoint。
- 在后场主动演示：`Dash` -> `SwordThrow` -> `TimeEcho/Shard` -> `Domain`。
- 清场后切到 Level_2。

---

## 场景 3（Level_2）- Reaper Boss 战

### 演示重点
- 你的 Boss 状态机设计：一阶段/二阶段、转阶段演出、技能压迫节奏。
- 突出你新增的 Reaper 机制（包括 spell 预测与二阶段多段传送）。

### 瓦片地图结构（建议）
- `Tilemap_Ground_Collision`：大平地 + 少量边缘落差（避免太复杂）。
- `Tilemap_ArenaBounds`：两侧封边，保证 Boss 传送在可见战区。
- `Tilemap_Deco_Back`：暗色背景 + 红色/暖色火把，强化 Boss 氛围。

### Boss 区配置建议
- 中央单独放 `Enemy_Reaper`（不混杂小怪）。
- 一阶段参数：常规追击与近战，`disableTeleportInPhase1 = true`。
- 转阶段：多次连续传送 + Echo 残影演出（你刚实现的 `PhaseShift`）。
- 二阶段：红色 tint、攻速提升、恢复传送与 spell 压迫。
- 场景入口放一个 `Object_CheckPoint`，方便反复录制。

### 录屏动作脚本（90~140 秒）
- 开场 10 秒：保持中距离，让观众先看清 Boss 动作。
- 打到半血触发转阶段：专门留镜头给多段传送演出。
- 二阶段展示“高压追击 + 传送 + spell 落点”。
- 收尾击杀，显示战斗结束与掉落。

---

## 全局摆放清单（按项目现有预制体）
- 敌人：`Enemy_Skeleton`、`Enemy_Slime_Large/Medium/Small`、`Enemy_ArcherElf`、`Enemy_Mage`、`Enemy_Reaper`。
- 交互：`Object_MerchantNPC`、`Object_BlacksmithNPC`、`Object_CheckPoint`、`Object_Chest`、`Object_Buff`、`Object_Waypoint`。
- 管理：`Camera`、`Canvas`、`___LEVEL MANAGER___`、`___AUDIO MANAGER___`（每场景都确认存在且唯一）。

---

## 录制参数建议
- 分辨率：`1920x1080`
- 帧率：`60fps`
- UI 比例：保持默认，避免遮挡战斗区域
- 每段切场景前留 `1~2 秒` 停顿，方便后期剪辑加标题

---

## 给简历/视频文案可直接用的三句总结
- 设计并实现了包含任务、对话、商店、锻造、存档的可游玩 2D ARPG 关卡流程。
- 实现多类敌人差异化 AI（近战、分裂、风筝、抛物线施法）和可组合技能系统。
- 独立实现 Boss 二阶段状态机与转阶段演出（连续传送、残影特效、参数重构）。
