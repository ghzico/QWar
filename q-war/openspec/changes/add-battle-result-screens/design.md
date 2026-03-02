# Design: 战斗成功/失败界面（battle-result-screens）

## Context

- **当前状态**：战斗在怪物全灭或我方棋将全灭后无结算界面；无「回到主菜单」「重新挑战」入口。
- **约束**：Godot 4 + C#；沿用现有 FightBoard、TestScene 与 MainMenu 场景切换方式；不改变现有战斗逻辑，仅在满足条件时叠加结果界面。
- **相关方**：FightBoard（单位移除、回合/阶段）、TestScene（战斗入口）、MainMenu（主菜单场景）。

## Goals / Non-Goals

**Goals:**

- 战斗胜利：场上无怪物时显示结果界面，含「战斗胜利！」文本、「回到主菜单」按钮、一张结果图；图片路径预留，你后续将图放在指定位置即可。
- 战斗失败：场上仍有怪物且我方已上场棋将血量均为 0 时显示结果界面，含「战斗失败...」文本、「重新挑战」「回到主菜单」按钮、一张结果图（同上，路径预留）。
- 明确结果图资源路径：胜利图与失败图各一个路径，文档与实现中写明，资源缺失时仅不显示图或显示占位，不阻塞界面。

**Non-Goals:**

- 本期不实现结算数据（如评分、奖励）、不扩展多关卡或存档；结果图由你后续放入约定路径。

## Decisions

### 1. 胜利/失败判定时机与条件

- **决策**：
  - **胜利**：棋盘上不存在任何怪物（即无 ChessEnemy）时判定胜利。在怪物被移除时（如 RemoveUnitAt 后、或 ChessEnemy 死亡 QueueFree 后）由 FightBoard 或战斗场景检测「当前是否还有怪物」，若无则触发胜利界面。
  - **失败**：棋盘上仍存在至少一个怪物，且我方已上场的所有棋将（ChessHero）均已死亡（血量为 0 或已从棋盘移除）时判定失败。在棋将死亡移除时检测：是否还有任意怪物 && 是否已无存活棋将，若两者同时满足则触发失败界面。
- **理由**：与现有 RemoveUnitAt、TakeDamage、死亡移除逻辑一致；在单位变化后做一次检测即可，无需改怪物/棋将内部逻辑。

### 2. 结果界面呈现方式

- **决策**：使用一个结果界面（如覆盖在战斗场景上的 Panel 或独立子场景），通过参数区分胜利/失败；或两个小场景 VictoryResult、DefeatResult 由战斗场景按需实例化。推荐单一面板 + 参数（显示不同文本、图片、按钮组合），减少重复节点与脚本。
- **理由**：两种界面元素相似（标题文本 + 图 + 按钮），仅文案与按钮数量不同，单一组件更易维护。

### 3. 结果图资源路径（你之后放图的位置）

- **决策**：
  - **胜利图**：`res://Res/BattleResult/victory.png`
  - **失败图**：`res://Res/BattleResult/defeat.png`
  - 即你在项目中创建目录 `Res/BattleResult/`，将胜利时显示的图命名为 `victory.png`，失败时显示的图命名为 `defeat.png` 放入即可。资源缺失时使用 TextureRect 留空或隐藏，不报错、不阻塞显示。
- **理由**：集中存放、命名清晰；与现有 Res 下资源组织方式一致。

### 4. 按钮行为

- **决策**：「回到主菜单」→ `GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn")`；「重新挑战」→ 重新加载当前战斗场景（如 `GetTree().ChangeSceneToFile("res://Scenes/TestScene.tscn")`）。结果界面显示后暂停或屏蔽棋盘操作（可选：置灰或隐藏棋盘交互），避免重复触发。
- **理由**：与主菜单变更一致；重新挑战即重进同一关卡。

### 5. 由谁检测胜负并显示界面

- **决策**：由 FightBoard 在「单位从棋盘移除」的时机后做一次检测（如提供 CheckBattleResult()，在怪物死亡移除、棋将死亡移除后调用）；或由 TestScene 订阅 FightBoard/单位事件后检测。FightBoard 持有当前所有格子单位信息，便于统计怪物数与存活棋将数，推荐在 FightBoard 内实现判定与显示（调用结果界面场景/节点）。
- **理由**：FightBoard 已有 _gridUnits，无需额外通信即可判断；结果界面作为 FightBoard 的子节点或由 FightBoard 请求 TestScene 添加均可。

## Risks / Trade-offs

- **[风险] 判定时机遗漏**  
  → 缓解：在所有会移除怪物或棋将的路径（RemoveUnitAt、死亡后 QueueFree）统一在当帧或下一帧调用一次胜负检测，避免漏判。

- **[trade-off] 结果图先留空**  
  → 接受：实现时用占位或空白 TextureRect，你后续将 victory.png / defeat.png 放入 `Res/BattleResult/` 即可生效。

## Migration Plan

1. 在 FightBoard 或战斗场景中实现「无怪物→胜利」「有怪物且无存活棋将→失败」的检测，并在单位移除后调用。
2. 新增结果界面 UI（单一面板，按胜利/失败切换文案与按钮），资源路径使用 `Res/BattleResult/victory.png`、`Res/BattleResult/defeat.png`，缺图时不显示或留空。
3. 创建 `Res/BattleResult/` 目录并在文档/README 中说明：将胜利图命名为 victory.png、失败图命名为 defeat.png 放入该目录。
4. 验证：怪物全灭弹出胜利界面并可回到主菜单；棋将全灭且仍有怪物时弹出失败界面，可重新挑战或回到主菜单。

## Open Questions

- 无。结果图路径已约定为 `Res/BattleResult/victory.png` 与 `Res/BattleResult/defeat.png`。
