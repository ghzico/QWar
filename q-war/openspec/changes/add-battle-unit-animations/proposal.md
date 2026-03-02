# 为战斗中怪物添加动画（本期仅怪物）

## Why

当前战斗中怪物（ChessEnemy）仅使用静态立绘，移动、攻击、受击、死亡等行为没有视觉反馈。本期仅对怪物增加动画能力并完成测试，资源已就绪于 `Res/Enemy` 下的 Slime1、Slime2、Slime3 文件夹，均为 .png 精灵表。棋将动画留待后续变更。

## What Changes

- **怪物动画能力**：为怪物增加 attack、death、hurt、idle、run、walk 六种动画，在逻辑触发时播放对应动画。
- **资源与精灵表约定**：
  - 资源位置：`Res/Enemy/Slime1/`、`Res/Enemy/Slime2/`、`Res/Enemy/Slime3/`，各目录下为 .png 精灵表。
  - **文件名约定**：`怪物名序号_动作.png`，例如 `Slime1_Attack.png`、`Slime1_Death.png`、`Slime1_Hurt.png`、`Slime1_Idle.png`、`Slime1_Run.png`、`Slime1_Walk.png`（Slime2、Slime3 同理）。
  - 每种动作为一张精灵表，网格为：**Attack 4×10**，**Death 4×10**，**Hurt 4×5**，**Idle 4×6**，**Run 4×8**，**Walk 4×8**（行×列）。4 行方向依次为：向玩家面、背玩家面、向左、向右；本期**仅使用第一行（向玩家面）**表现。
- **逻辑接入**：在怪物待机时播 idle；移动时播 walk（或 run）；攻击棋将时播 attack；受到伤害时播 hurt；死亡时播 death 后移除。未提供动画或加载失败时回退到现有静态立绘，不破坏流程。
- **本期不做**：棋将动画、配置表扩展、多方向（仅用第一行）；不修改棋将相关代码的动画逻辑。

## Capabilities

### New Capabilities

- `battle-unit-animations`：本期仅覆盖**怪物**的 attack、death、hurt、idle、run、walk 动画的触发时机、资源路径（Slime1/2/3）、精灵表网格与「仅第一行」约定，以及与 FightBoard/ChessEnemy 的对接与回退策略。

### Modified Capabilities

- （无。）

## Impact

- **受影响代码**：`Scripts/ChessEnemy.cs`、`Scripts/FightBoard.cs`（怪物移动/攻击时的表现）；场景 `Scenes/ChessEnemy.tscn`（增加 AnimatedSprite2D 等）。
- **资源目录**：`Res/Enemy/Slime1/`、`Res/Enemy/Slime2/`、`Res/Enemy/Slime3/` 下的 .png 精灵表。
- **依赖**：Godot 内置 AnimatedSprite2D/精灵表；无新增第三方依赖。
