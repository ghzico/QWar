# Spec: 怪物动画（battle-unit-animations，本期仅怪物）

## ADDED Requirements

### Requirement: 怪物动画资源位置与精灵表网格

系统 SHALL 从 `Res/Enemy/Slime1/`、`Res/Enemy/Slime2/`、`Res/Enemy/Slime3/` 加载怪物动画资源，均为 .png 精灵表。**文件名约定** SHALL 为 `怪物名序号_动作.png`，例如 `Slime1_Attack.png`、`Slime1_Death.png`、`Slime1_Hurt.png`、`Slime1_Idle.png`、`Slime1_Run.png`、`Slime1_Walk.png`。每种动作的网格（行×列）SHALL 为：Attack 4×10，Death 4×10，Hurt 4×5，Idle 4×6，Run 4×8，Walk 4×8。4 行方向依次为：向玩家面、背玩家面、向左、向右。本期 SHALL 仅使用**第一行（向玩家面，行索引 0）**完成表现。未提供某动作或加载失败时 SHALL 回退到静态立绘或跳过该动作动画，不阻塞逻辑。

#### Scenario: Slime1 目录存在且含 attack 精灵表

- **WHEN** 怪物使用 Slime1 动画集且 `Res/Enemy/Slime1/Slime1_Attack.png` 存在
- **THEN** 该怪物攻击时 SHALL 播放该精灵表第一行（4×10 中的第 0 行）帧序列，否则 SHALL 不播放攻击动画

#### Scenario: 某动作资源缺失

- **WHEN** 某 Slime 目录下缺少 SlimeN_Death.png 或 SlimeN_Hurt.png 等约定文件名
- **THEN** 该动作触发时 SHALL 不播放动画或使用立绘，并 SHALL 正常执行扣血/移除与 QueueFree，不卡住

---

### Requirement: 怪物待机播放 idle

系统 SHALL 在怪物无其他行为时循环播放 idle 动画（使用对应精灵表第一行，4×6 帧）。无 idle 资源时 SHALL 保持现有静态立绘显示。

#### Scenario: 怪物回合开始前

- **WHEN** 怪物在格子上且未在执行移动或攻击
- **THEN** 该怪物 SHALL 播放 idle 动画（若存在），否则 SHALL 显示静态立绘

---

### Requirement: 怪物移动时播放 walk（或 run）

系统 SHALL 在怪物从一格移动到另一格时播放 walk 动画（精灵表 4×8，仅第一行）；实现可选用 run（4×8）替代或与 walk 二选一。移动 SHALL 可通过 Tween 或动画驱动位置变化，动画或移动结束后 SHALL 完成网格更新。无 walk/run 资源时 SHALL 直接更新位置，与当前无动画行为一致。

#### Scenario: 怪物 AI 移动一格

- **WHEN** 怪物方回合中怪物向最近棋将移动一格
- **THEN** 系统 SHALL 播放该怪物的 walk（或 run）动画（若存在），并 SHALL 在表现结束后完成格子更新，不阻塞后续怪物或回合切换

---

### Requirement: 怪物攻击时播放 attack

系统 SHALL 在怪物对棋将执行攻击时播放 attack 动画（精灵表 4×10，仅第一行）。动画可在造成伤害前或后播放；播完后 SHALL 对棋将造成 1 点伤害，逻辑与现有 RunMonsterAi 一致。无 attack 资源时 SHALL 不播放，逻辑照常结算。

#### Scenario: 怪物攻击邻格棋将

- **WHEN** 怪物方回合中怪物对邻格棋将执行攻击
- **THEN** 系统 SHALL 播放该怪物的 attack 动画（若存在），并 SHALL 对棋将造成 1 点伤害

---

### Requirement: 怪物受击时播放 hurt

系统 SHALL 在怪物受到伤害（ChessEnemy.TakeDamage）时播放 Hurt 动画（精灵表 4×5，仅第一行）。扣血与血条更新 SHALL 与现有逻辑一致。若该次受击导致死亡，SHALL 先完成 hurt 表现再进入死亡流程。无 Hurt 资源时 SHALL 仅扣血与更新显示，不阻塞。

#### Scenario: 棋将攻击怪物

- **WHEN** 棋将普攻或技能对怪物造成伤害并调用 ChessEnemy.TakeDamage
- **THEN** 系统 SHALL 播放该怪物的 hurt 动画（若存在），并 SHALL 扣减血量、更新显示，若未死亡则恢复 idle

---

### Requirement: 怪物死亡时播放 death 再移除

系统 SHALL 在怪物死亡（血量归零）时播放 death 动画（精灵表 4×10，仅第一行）；动画结束后 SHALL 从棋盘移除并 QueueFree。若未配置或加载失败，SHALL 在短延时或立即执行移除与 QueueFree，不无限等待。SHALL 支持最大等待时间或可配置跳过，避免缺资源时卡住。

#### Scenario: 怪物死亡有 death 资源

- **WHEN** 怪物血量归零且该怪物动画集存在 death 精灵表
- **THEN** 系统 SHALL 播放 death 动画（第一行），动画结束或超时后 SHALL 从棋盘移除并 QueueFree

#### Scenario: 怪物死亡无 death 资源

- **WHEN** 怪物血量归零且无 death 资源或加载失败
- **THEN** 系统 SHALL 不等待或仅等待极短时间后执行移除与 QueueFree，保证战斗流程继续

---

### Requirement: 怪物与 Slime 动画集绑定及回退

系统 SHALL 支持为每个怪物实例指定使用的动画集（Slime1、Slime2 或 Slime3）；可通过场景导出、放置时参数或简单映射（如 monster1→Slime1）实现。加载某 Slime 目录失败或目录不存在时，该怪物 SHALL 仅显示现有静态立绘，所有动作 SHALL 仅做逻辑结算、无动画表现，与当前无动画行为一致。

#### Scenario: 指定 Slime2 动画集

- **WHEN** 某怪物实例指定使用 Slime2 且 `Res/Enemy/Slime2/` 存在且有效
- **THEN** 该怪物的 idle、walk、attack、hurt、death 等 SHALL 使用 Slime2 目录下对应 .png 的第一行动画

#### Scenario: Slime 目录不存在

- **WHEN** 指定使用的 Slime 目录不存在或无法加载
- **THEN** 该怪物 SHALL 仅显示静态立绘，移动/攻击/受击/死亡 SHALL 仅做逻辑结算、无动画表现
