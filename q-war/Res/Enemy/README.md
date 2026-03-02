# 敌将 / 怪物资源

## 立绘（静态）

根目录下可放置单张立绘，如 `monster1.png`、`monster2.png`，供无动画或回退时使用。

## 无需单独导入动画场景

怪物动画由代码在运行时从 PNG 精灵表加载并生成 SpriteFrames，**不需要**在 Godot 编辑器里做「导入动画」或建动画场景；只要把约定命名的 PNG 放在约定路径下即可。

## 怪物动画（Slime 精灵表）

动画资源放在子目录 **Slime1**、**Slime2**、**Slime3** 下，每个目录对应一种怪物外观。

### 路径

怪物动画资源统一放在以下目录内：

- `Res/Enemy/Slime1/`
- `Res/Enemy/Slime2/`
- `Res/Enemy/Slime3/`

每个目录下放置该怪物对应的六种动作 PNG（见下方文件名约定）。

### 文件名约定

**`怪物名序号_动作.png`**，例如：

- `Slime1_Attack.png`
- `Slime1_Death.png`
- `Slime1_Hurt.png`
- `Slime1_Idle.png`
- `Slime1_Run.png`
- `Slime1_Walk.png`

Slime2、Slime3 同理（如 `Slime2_Attack.png`）。

### 精灵表网格（行×列）

| 动作 | 网格   | 说明     |
|------|--------|----------|
| Attack | 4×10 | 4 行 × 10 列 |
| Death  | 4×10 | 4 行 × 10 列 |
| Hurt   | 4×5  | 4 行 × 5 列  |
| Idle   | 4×6  | 4 行 × 6 列  |
| Run    | 4×8  | 4 行 × 8 列  |
| Walk   | 4×8  | 4 行 × 8 列  |

### 行方向

4 行依次表示：

- 第 0 行：向玩家面（面向镜头）
- 第 1 行：背对玩家
- 第 2 行：向左
- 第 3 行：向右

当前实现**仅使用第一行（第 0 行，向玩家面）**表现所有动作。
