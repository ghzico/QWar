# Tasks: 主界面与开始游戏入口

## 1. 主界面场景与脚本

- [x] 1.1 新建 `Scenes/MainMenu.tscn`，根节点为 Control，设置锚点全屏铺满；内部使用 MarginContainer + VBoxContainer（或等效）实现居中垂直布局
- [x] 1.2 新建 `Scripts/MainMenu.cs`，挂载到 MainMenu 场景根节点；在 _Ready 中获取并持有「开始游戏」「设置」「玩法说明」按钮的引用

## 2. 主界面布局与入口

- [x] 2.1 在主界面顶部添加标题/品牌区（Label 或 TextureRect），如「QWar」或项目名；保证在常见分辨率下可见且居中
- [x] 2.2 在标题下方添加三个按钮：「开始游戏」「设置」「玩法说明」，垂直排列，间距与对齐一致；布局随窗口缩放时保持合理（Size Flags/锚点）

## 3. 开始游戏按钮表现与逻辑

- [x] 3.1 为「开始游戏」按钮设置突出样式（Theme 或自定义：更大字号、醒目颜色/背景），使其在视觉上明显区别于「设置」「玩法说明」
- [x] 3.2 为「开始游戏」提供 hover 与 pressed 反馈（Theme 的 hover/pressed 状态，或 mouse_entered/exited + 简单 Tween/缩放），确保可感知且不破坏布局
- [x] 3.3 在 MainMenu.cs 中连接「开始游戏」的 pressed 信号，点击后调用 `GetTree().ChangeSceneToFile("res://Scenes/TestScene.tscn")`（或等价 API）切换至 TestScene

## 4. 设置与玩法说明占位

- [x] 4.1 在 MainMenu.cs 中连接「设置」「玩法说明」的 pressed 信号；点击后弹出 AcceptDialog（或等效弹窗），内容为「敬请期待」或简短占位文案，关闭后回到主界面

## 5. 主场景配置与验证

- [x] 5.1 将 project.godot 的 `run/main_scene` 改为 `res://Scenes/MainMenu.tscn`
- [ ] 5.2 启动游戏验证：在编辑器中运行项目，确认首先显示主界面，点击「开始游戏」能进入 TestScene 并正常布阵与战斗；点击「设置」「玩法说明」显示占位弹窗并关闭后仍停留在主界面
