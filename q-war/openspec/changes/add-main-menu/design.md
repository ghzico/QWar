# Design: 主界面（MainMenu）与开始游戏入口

## Context

- **当前状态**：应用主场景为 `Scenes/TestScene.tscn`，直接进入布阵与战斗；无主菜单、无「开始游戏」等入口。
- **约束**：Godot 4 + C#；沿用现有 Control/UI 体系；不改变 TestScene 与 FightBoard 的现有逻辑，仅增加入口层。
- **相关方**：project.godot（main_scene）、新主界面场景与脚本；TestScene 作为「开始游戏」的目标场景。

## Goals / Non-Goals

**Goals:**

- 新增主界面场景（MainMenu），作为应用启动后的首个界面；包含标题/品牌区、开始游戏、设置、玩法说明等入口。
- 本期重点：主界面整体布局与表现、「开始游戏」按钮的样式与交互反馈（hover/按下），以及点击后正确切换至 TestScene（当前可玩内容）。
- 设置、玩法说明在主界面上有入口；本期可为占位实现（如弹窗「敬请期待」），不实现完整设置页与玩法说明页。
- 将 `run/main_scene` 改为主界面场景；从主界面「开始游戏」再进入 TestScene。

**Non-Goals:**

- 本期不实现完整设置页（音量、画质等）、不实现完整玩法说明页；不改变 TestScene/FightBoard 内部逻辑。

## Decisions

### 1. 主界面场景与节点结构

- **决策**：新建 `Scenes/MainMenu.tscn`，根节点为 Control（或 CanvasLayer），全屏铺满。内部采用 VBoxContainer 或 MarginContainer + 居中布局：上方为标题/品牌（Label 或 TextureRect），中间为按钮列表（开始游戏、设置、玩法说明），底部可留空或放版本信息。使用 Godot 内置 Theme 或简单样式区分主按钮与次要按钮。
- **理由**：与现有 TestScene 的 Control 体系一致；VBox 便于扩展与适配；无需额外 UI 框架。

### 2. 开始游戏按钮表现与交互

- **决策**：「开始游戏」使用 Button 节点，作为主 CTA；通过 Theme 或自定义样式设置字体大小、颜色、背景，使其在视觉上突出。提供 hover（mouse_entered/exited）与按下（pressed）的反馈：如 hover 时缩放或高亮、pressed 时短暂缩放或颜色变化；使用 Tween 或 Theme 的 hover/pressed 状态即可。点击后调用 `GetTree().ChangeSceneToFile("res://Scenes/TestScene.tscn")`（或等价 API）切换至 TestScene。
- **理由**：ChangeSceneToFile 为 Godot 标准方式，替换当前场景为战斗场景；按钮反馈提升可玩单元的第一印象。

### 3. 设置与玩法说明入口

- **决策**：主界面上提供「设置」「玩法说明」两个按钮；本期点击后弹出简单 AcceptDialog 或自定义弹窗，内容为「敬请期待」或一句说明，关闭后回到主界面。不实现具体设置项与玩法说明内容页。
- **理由**：满足「一般独立游戏有的项目」的占位，后续可替换为真实页面。

### 4. 主场景切换与资源路径

- **决策**：在 project.godot 中将 `run/main_scene` 改为 `res://Scenes/MainMenu.tscn`。MainMenu 脚本中「开始游戏」写死目标场景路径为 `res://Scenes/TestScene.tscn`；后续若战斗入口改为其他场景，仅改该常量或配置即可。
- **理由**：最小改动、明确入口；避免运行时分支过多。

### 5. 脚本与场景绑定

- **决策**：新建 `Scripts/MainMenu.cs`，挂载到 MainMenu 场景根节点；在 _Ready 中获取「开始游戏」「设置」「玩法说明」按钮引用，连接 pressed 信号；开始游戏 → ChangeSceneToFile(TestScene)；设置/玩法说明 → 显示占位弹窗。
- **理由**：逻辑集中、易测；与现有 C# 脚本风格一致。

## Risks / Trade-offs

- **[风险] 不同分辨率下主界面拉伸或错位**  
  → 缓解：使用锚点或 Container 的 Size Flags 使布局居中并随窗口缩放；主按钮使用相对或最小尺寸，避免过小/过大。

- **[trade-off] 设置/玩法说明仅为占位**  
  → 接受：本期仅保证入口存在与可点击，内容留待后续变更。

## Migration Plan

1. 新建 `Scenes/MainMenu.tscn` 与 `Scripts/MainMenu.cs`，实现布局与三个按钮及开始游戏/占位逻辑。
2. 在编辑器中或通过脚本验证：从 MainMenu 点击「开始游戏」能正确进入 TestScene 并正常布阵与战斗。
3. 将 project.godot 的 `run/main_scene` 改为 `res://Scenes/MainMenu.tscn`。
4. 若有自动化测试或启动检查，确认启动后首先显示 MainMenu。

## Open Questions

- 无。主界面与开始游戏目标场景路径已明确；设置/玩法说明占位策略已定。
