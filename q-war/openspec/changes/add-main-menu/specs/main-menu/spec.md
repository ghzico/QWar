# Spec: 主界面（main-menu）

## ADDED Requirements

### Requirement: 应用启动显示主界面

应用启动后 SHALL 首先显示主界面场景（MainMenu）。主界面 SHALL 包含标题/品牌区、开始游戏按钮、设置入口、玩法说明入口；布局 SHALL 在常见分辨率下居中且可读，不溢出视口。

#### Scenario: 启动应用

- **WHEN** 用户启动应用（run/main_scene 指向主界面）
- **THEN** 首个可见界面 SHALL 为主界面，且 SHALL 显示标题/品牌、开始游戏、设置、玩法说明等入口

#### Scenario: 主界面布局适配

- **WHEN** 主界面在默认或常见窗口尺寸下显示
- **THEN** 主要元素（标题、按钮列表）SHALL 在视口内可见且布局合理，不因拉伸导致严重错位或遮挡

---

### Requirement: 开始游戏按钮样式与交互

主界面 SHALL 提供「开始游戏」按钮作为主要操作入口。该按钮 SHALL 在视觉上突出（如更大字号、更醒目颜色或背景）。SHALL 提供 hover（鼠标移入/移出）与按下（pressed）的视觉或动效反馈（如缩放、高亮、Theme 状态）；点击后 SHALL 切换至当前可玩战斗场景（本期为 TestScene）。

#### Scenario: 开始游戏按钮可见且突出

- **WHEN** 主界面显示
- **THEN** 「开始游戏」按钮 SHALL 存在且 SHALL 在视觉上易于识别为主操作（如与设置、玩法说明按钮在尺寸或样式上区分）

#### Scenario: 开始游戏 hover 与按下反馈

- **WHEN** 用户将鼠标移入「开始游戏」按钮或按下该按钮
- **THEN** 界面 SHALL 提供可感知的反馈（如颜色/缩放/Theme 状态变化），且 SHALL 不破坏布局或导致闪烁异常

#### Scenario: 点击开始游戏进入战斗

- **WHEN** 用户点击「开始游戏」按钮
- **THEN** 应用 SHALL 切换至 TestScene（或当前配置的战斗场景），且 SHALL 能正常进入布阵与战斗流程

---

### Requirement: 设置与玩法说明入口占位

主界面 SHALL 提供「设置」与「玩法说明」入口（按钮或等效控件）。本期 SHALL 允许点击上述入口；点击后 SHALL 显示占位反馈（如弹窗提示「敬请期待」或简短说明），关闭后 SHALL 回到主界面。SHALL 不要求实现完整设置页或玩法说明内容页。

#### Scenario: 设置入口可点击

- **WHEN** 用户点击主界面上的「设置」入口
- **THEN** 系统 SHALL 显示占位反馈（如弹窗），关闭后 SHALL 回到主界面，不切换场景

#### Scenario: 玩法说明入口可点击

- **WHEN** 用户点击主界面上的「玩法说明」入口
- **THEN** 系统 SHALL 显示占位反馈（如弹窗），关闭后 SHALL 回到主界面，不切换场景

---

### Requirement: 主场景配置

项目配置 SHALL 将主界面场景设为应用入口。即 `project.godot` 的 `run/main_scene` SHALL 指向主界面场景（如 `res://Scenes/MainMenu.tscn`），以便启动时首先加载主界面而非直接进入 TestScene。

#### Scenario: 主场景为 MainMenu

- **WHEN** 通过引擎或发行包启动应用
- **THEN** 首个加载并显示的场景 SHALL 为主界面场景（MainMenu），而非 TestScene
