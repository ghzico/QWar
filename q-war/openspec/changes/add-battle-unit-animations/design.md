# Design: 战斗中怪物动画（Slime1/2/3，仅第一行）

## Context

- **当前状态**：怪物（ChessEnemy）使用静态立绘（TextureRect），无动画。怪物移动与攻击逻辑已在 FightBoard.RunMonsterAi、ChessEnemy.TakeDamage 中实现。
- **约束**：Godot 4 + C#；资源限定为 `Res/Enemy` 下 Slime1、Slime2、Slime3 文件夹中的 .png 精灵表；本期仅使用精灵表第一行（向玩家面）；未提供动画时回退到静态立绘，不阻塞逻辑。
- **相关方**：FightBoard（MoveUnit、RunMonsterAi）、ChessEnemy（TakeDamage、死亡移除）。

## Goals / Non-Goals

**Goals:**

- 为怪物在 idle、walk/run、attack、hurt、death 时播放对应动画；资源路径为 `Res/Enemy/Slime1|Slime2|Slime3/`，精灵表网格与「仅第一行」见下。
- 明确精灵表网格：attack 4×10，death 4×10，Hurt 4×5，idle 4×6，run 4×8，walk 4×8；4 行对应方向为向玩家面、背玩家面、左、右；本期仅用第一行。
- 移动用 walk（或 run），攻击时播 attack，受击时播 hurt，死亡时播 death 后移除；缺资源时回退到现有立绘与逻辑。

**Non-Goals:**

- 本期不实现棋将动画、不扩展 General 配置、不实现多方向（第 2/3/4 行）；不改棋将相关表现逻辑。

## Decisions

### 1. 动画资源位置与精灵表规范

- **决策**：
  - 怪物动画资源仅来自 `Res/Enemy/Slime1/`、`Res/Enemy/Slime2/`、`Res/Enemy/Slime3/`。**文件名约定**：`怪物名序号_动作.png`，例如 `Slime1_Attack.png`、`Slime1_Death.png`、`Slime1_Hurt.png`、`Slime1_Idle.png`、`Slime1_Run.png`、`Slime1_Walk.png`（Slime2、Slime3 同理）。
  - 精灵表网格（行×列）：**Attack 4×10**，**Death 4×10**，**Hurt 4×5**，**Idle 4×6**，**Run 4×8**，**Walk 4×8**。4 行含义：第 0 行 = 向玩家面，第 1 行 = 背玩家面，第 2 行 = 向左，第 3 行 = 向右。**本期仅使用第 0 行（向玩家面）** 的帧序列。
- **理由**：与现有 Res/Enemy 约定一致；明确网格便于 AnimatedSprite2D 或逐帧切图；仅第一行可先跑通流程，后续再扩展方向。

### 2. 怪物与 Slime 的对应关系

- **决策**：通过场景实例或放置时的参数指定怪物使用 Slime1、Slime2 或 Slime3 的动画集（如导出属性或 FightBoard 放置敌将时传入文件夹名）。若未指定或路径不存在，则回退到当前静态立绘逻辑。
- **理由**：三个文件夹对应三种外观，需在运行时选定其一；本期可不接配置表，由场景/放置逻辑写死或简单映射（如 monster1→Slime1）。

### 3. 场景中的使用方式

- **决策**：在 ChessEnemy 场景中增加 **AnimatedSprite2D**，与现有 TextureRect 并存。有动画资源时用 AnimatedSprite2D 播放对应精灵表第一行帧；无则隐藏 AnimatedSprite2D、仅显示 TextureRect。idle 为默认循环；walk/run 在移动时播放；attack/hurt/death 播完一次后根据逻辑切回 idle 或移除。
- **理由**：Godot 原生支持；与现有 Control+TextureRect 可并存，回退简单。

### 4. 动作与逻辑的映射

- **决策**：
  - **idle**：怪物待机、无行为时循环播放。
  - **walk**：怪物 AI 移动一格时播放（走格用 walk；若需区分可后续用 run，本期可统一用 walk）。
  - **attack**：怪物对棋将造成伤害时，在调用 hero.TakeDamage 前或后播放 attack 动画，播完再继续。
  - **hurt**：ChessEnemy.TakeDamage 被调用时播放 hurt；若该次导致死亡，播完 hurt 后进入 death 流程。
  - **death**：血量归零时播放 death 动画，结束后再 RemoveUnitAt + QueueFree；无 death 资源时短延时或立即移除。
- **理由**：与现有 RunMonsterAi、TakeDamage、移除逻辑一一对应；先实现再调时长与节奏。

### 5. 回退策略

- **决策**：某动作缺少对应 .png 或加载失败时，该动作不播动画；整个 Slime 目录不存在或无效时，该怪物仅显示现有 TextureRect，行为与当前无动画一致。不因缺资源而卡住战斗或移除逻辑。
- **理由**：保证关卡在缺资源时仍可运行；便于先用 Slime1 验证，再挂 Slime2/3。

## Risks / Trade-offs

- **[风险] 精灵表行列与引擎默认假设不一致**  
  → 缓解：在文档与代码中明确「行×列」与第一行索引（0），使用 AnimatedSprite2D 的 hframes/vframes 或等价方式按网格切帧。

- **[trade-off] 本期仅第一行，移动时朝向不随格子变化**  
  → 接受：表现上均「面向玩家」，后续可再按格子方向选行。

## Migration Plan

- 确保 `Res/Enemy/Slime1/`、`Slime2/`、`Slime3/` 下存在约定命名的 .png 精灵表；现有根目录立绘可保留供回退。
- 代码先接怪物动画触发与回退，再在测试场景中挂 Slime1/2/3 验证 attack、death、hurt、idle、run、walk。

## Open Questions

- 无。文件名已约定为 `怪物名序号_动作.png`（如 Slime1_Attack.png）。
