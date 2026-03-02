# Spec: 战斗成功/失败界面（battle-result-screens）

## ADDED Requirements

### Requirement: 战斗胜利界面内容与触发

当场上所有怪物被消灭（棋盘上不存在任何怪物）时，系统 SHALL 显示战斗胜利界面。该界面 SHALL 包含：「战斗胜利！」文本提示、「回到主菜单」按钮、以及一张结果图片。结果图 SHALL 从资源路径 `res://Res/BattleResult/victory.png` 加载；若该路径下无资源，SHALL 不显示图片或显示占位，且不阻塞界面与按钮操作。

#### Scenario: 怪物全灭时显示胜利界面

- **WHEN** 战斗中最后一个怪物被消灭并从棋盘移除
- **THEN** 系统 SHALL 显示战斗胜利界面，且 SHALL 显示「战斗胜利！」文本与「回到主菜单」按钮

#### Scenario: 胜利界面回到主菜单

- **WHEN** 用户在战斗胜利界面点击「回到主菜单」
- **THEN** 系统 SHALL 切换至主菜单场景（MainMenu），与从主界面进入战斗前一致

#### Scenario: 胜利图资源路径与占位

- **WHEN** 实现完成且你已将胜利图放入约定位置
- **THEN** 胜利图 SHALL 从 `Res/BattleResult/victory.png` 加载并显示；你只需在该目录下放置文件 `victory.png` 即可。若未放置，界面 SHALL 仍可正常显示文本与按钮，不报错

---

### Requirement: 战斗失败界面内容与触发

当场上仍存在至少一个怪物，且我方已上场的所有棋将血量均为 0（或已从棋盘移除）时，系统 SHALL 显示战斗失败界面。该界面 SHALL 包含：「战斗失败...」文本提示、「重新挑战」按钮、「回到主菜单」按钮、以及一张结果图片。结果图 SHALL 从资源路径 `res://Res/BattleResult/defeat.png` 加载；若该路径下无资源，SHALL 不显示图片或显示占位，且不阻塞界面与按钮操作。

#### Scenario: 我方全灭且仍有怪物时显示失败界面

- **WHEN** 我方最后一个上场棋将因血量归零被移除，且此时场上仍存在至少一个怪物
- **THEN** 系统 SHALL 显示战斗失败界面，且 SHALL 显示「战斗失败...」文本、「重新挑战」与「回到主菜单」按钮

#### Scenario: 失败界面回到主菜单

- **WHEN** 用户在战斗失败界面点击「回到主菜单」
- **THEN** 系统 SHALL 切换至主菜单场景（MainMenu）

#### Scenario: 失败界面重新挑战

- **WHEN** 用户在战斗失败界面点击「重新挑战」
- **THEN** 系统 SHALL 重新加载当前战斗场景（如 TestScene），使玩家可再次进行同一局战斗

#### Scenario: 失败图资源路径与占位

- **WHEN** 实现完成且你已将失败图放入约定位置
- **THEN** 失败图 SHALL 从 `Res/BattleResult/defeat.png` 加载并显示；你只需在 `Res/BattleResult/` 目录下放置文件 `defeat.png` 即可。若未放置，界面 SHALL 仍可正常显示文本与按钮，不报错

---

### Requirement: 结果图资源路径约定

系统 SHALL 使用以下固定路径加载战斗结果图片，便于你后续添加资源：
- 战斗胜利图：`res://Res/BattleResult/victory.png`
- 战斗失败图：`res://Res/BattleResult/defeat.png`
你只需在项目中创建目录 `Res/BattleResult/`，并将对应图片命名为 `victory.png`、`defeat.png` 放入该目录即可；实现 SHALL 在资源缺失时优雅降级（不显示图或占位），不崩溃、不阻塞。

#### Scenario: 结果图目录与文件名

- **WHEN** 你准备添加结果图
- **THEN** 将胜利时显示的图放在 `Res/BattleResult/victory.png`，失败时显示的图放在 `Res/BattleResult/defeat.png`；无需修改代码即可生效
