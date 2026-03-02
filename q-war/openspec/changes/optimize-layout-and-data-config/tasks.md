# Tasks: 棋将包布局优化与数据配置工具链

## 1. 棋将包布局优化

- [x] 1.1 为 DeckPanel 内棋将卡片列表增加横向滚动或换行：使用 ScrollContainer + HBox，或可换行的容器（如 FlowContainer/自定义），使所有卡片在可视或可滚动区域内完整显示
- [x] 1.2 设置 DeckPanel 的 CustomMinimumSize 或锚点，保证在 TestScene 的 MainVBox 中获得稳定高度（如不小于 120），避免被挤压导致显示不全
- [x] 1.3 验证规则说明、棋盘与棋将包在默认窗口下均可见且无重叠；棋将包位于战斗场面下方且所有卡片可被看到与拖拽

## 2. 配置转换脚本与输出格式

- [x] 2.1 确定目标 JSON 结构：general（按 ID 的棋将列表）、map_cell（MAPCELLID → 贴图路径）、map_grid（5×10 的 MAPCELLID 网格），与现有 Loader 所需字段一致
- [x] 2.2 实现 Excel → JSON 转换脚本（Python 或 Node）：读取 config/General.xlsx、Map.xlsx、MapCell.xlsx，输出到 res://config/ 下对应 .json 文件（如 general.json、map_cell.json、map_grid.json）
- [x] 2.3 若评估后采用 Luban：配置 Luban 的 Excel 规则与输出路径，使输出格式与 2.1 约定一致；否则以自研脚本为准完成 2.2

## 3. Loader 优先读 JSON 并回退 Excel

- [x] 3.1 GeneralConfigLoader：优先尝试读取 config/general.json（或约定路径），解析为 IReadOnlyDictionary<int, HeroConfig>；若文件不存在或解析失败则回退到当前 Excel 逻辑，并可选打日志提示运行配置转换
- [x] 3.2 MapConfigLoader：优先读取 map_cell.json、map_grid.json（或约定文件名），成功则用 JSON 构建地图格资源与网格；失败则回退到现有 Excel 读取逻辑
- [x] 3.3 对外 API（LoadHeroConfigs、LoadMapCellResources、LoadMapGrid 等）的返回类型与语义保持不变，仅数据来源在「有 json 时」改为 JSON

## 4. 工具链与文档

- [x] 4.1 在项目内提供可执行入口（如 tools/export_config.py 或 export_config.bat），文档中说明「资源更新或打包前需执行配置转换」
- [x] 4.2 在 OPENSPEC.md 或本变更下补充：配置使用约定（Excel 为编辑源、运行时读转换后 JSON）、表结构与 JSON 字段对应关系、转换步骤与构建集成说明
- [x] 4.3 可选：在 CI 或 Godot 资源导入流程中集成配置转换步骤，确保正式构建使用转换后数据

## 5. 验证与回归

- [x] 5.1 验证：有 json 时游戏从 JSON 加载配置，无 json 时仍可从 Excel 正常进入游戏；棋将包在 8 个以上棋将时通过滚动/换行可访问全部
- [x] 5.2 验证：修改 Excel 后执行转换脚本，运行游戏确认数值与贴图与表格一致；FightBoard、棋将包、布阵与战斗流程无回归
