# Spec: hero-hp

## ADDED Requirements

### Requirement: 棋将拥有血量且可从配置读取初始值

棋将 SHALL 拥有当前血量（HP）；初始血量 SHALL 从 `config/General.xlsx` 的 **血量（GHp）** 列读取，与现有 ID、攻击力、攻击距离、移动距离、形象配置并列。

#### Scenario: 从 General.xlsx 读取 GHp

- **WHEN** 加载棋将配置（GeneralConfigLoader）并解析 General.xlsx
- **THEN** 表头包含“血量”或“GHp”列；每行棋将配置对应一列 GHp 数值；HeroConfig 暴露该值；ApplyToHero 时将该值设为棋将的初始/最大血量

#### Scenario: 缺列或无效值时默认

- **WHEN** 某行缺少 GHp 列或值为非数字/空
- **THEN** 该棋将使用默认初始血量（如 1），避免运行时异常；文档中说明 GHp 建议必填

---

### Requirement: 棋将可受伤与死亡

棋将 SHALL 可接受伤害（TakeDamage）；当当前血量降至 0 或以下时 SHALL 视为死亡并从棋盘移除，且不再参与回合与目标选择。

#### Scenario: 棋将受到伤害

- **WHEN** 对某棋将调用 TakeDamage(amount)（例如怪物攻击造成 1 点伤害）
- **THEN** 该棋将当前血量减少 amount（不低于 0）；血量显示更新

#### Scenario: 棋将死亡

- **WHEN** 棋将当前血量变为 0
- **THEN** 该棋将从 FightBoard 移除并 QueueFree；不再参与“最近单位”计算与回合操作

---

### Requirement: 棋将血量在战斗中可见

棋将的当前血量 SHALL 在战斗界面可见（如 Label 或进度条），便于玩家判断状态。

#### Scenario: 血量显示更新

- **WHEN** 棋将初始血量被设置或受到伤害后
- **THEN** 场景中与该棋将关联的血量 UI 显示当前血量数值（或当前/最大）
