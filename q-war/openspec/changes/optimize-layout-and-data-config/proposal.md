# Proposal: 棋将包布局优化与数据配置工具链

## Why

当前棋将包（DeckPanel）使用横向 HBox 排布卡片且无滚动/换行，当可选棋将较多时会出现显示不全、超出视口或挤压变形的问题，影响布阵体验。同时，游戏配置（General.xlsx、Map.xlsx、MapCell.xlsx）在运行时通过 ExcelDataReader 直接读 Excel，存在解析开销大、列顺序与表头强耦合、难以做校验与热更等问题；需要将配置在构建期转换为程序高效读取的格式（如 JSON/二进制），并建立从 Excel 编辑到引擎使用的自动化工具链（可选用 Luban 等），完善整体配置使用逻辑。

## What Changes

- **棋将包布局优化**：解决棋将包区域显示不全问题。可采取横向滚动、多行换行、或限制可见数量+翻页等方案，确保所有可选棋将均可被看到与拖拽；必要时调整 DeckPanel 与 TestScene 的布局约束（最小高度、锚点等），避免被裁剪或溢出。
- **配置转高效格式**：在构建/导出前将 Excel 配置转换为引擎可高效加载的格式（如 JSON、或 Godot Resource 可读格式），运行时仅加载转换后的数据，不再直接依赖 ExcelDataReader 读 xlsx。
- **自动化工具链**：引入或自建「Excel → 中间/目标格式」的转换流水线；可考虑使用 Luban 等成熟工具，或基于现有表结构编写导出脚本（如 Python/Node），输出到项目指定目录供 Godot 读取；在资源导入或 CI 中集成该步骤。
- **完善配置使用逻辑**：统一配置加载入口（如单一 ConfigService 或按表类型的 Loader），明确「仅读转换后数据」的约定；表结构变更时需同步更新转换规则与加载代码，保证可维护性。

## Capabilities

### New Capabilities

- `deck-layout`: 棋将包区域布局与显示优化。保证所有棋将卡片在界面内可完整展示与操作（滚动/换行/分页等），避免显示不全或溢出；必要时调整父级布局与 DeckPanel 尺寸约束。
- `data-config-pipeline`: 数据配置工具链与高效格式。Excel 配置在构建期转换为程序易读格式并写入指定目录；运行时从转换结果加载，不直接读 xlsx；工具链可选用 Luban 或自研脚本，并融入项目构建/资源流程；统一配置加载约定与入口。

### Modified Capabilities

- 无（本变更不修改既有 spec 的契约级行为；deck-system 的「从配置加载棋将」仍成立，仅数据来源从「直接读 Excel」改为「读转换后数据」；棋将包仍从同一配置数据源展示，仅布局与展示方式优化。）

## Impact

- **UI/场景**：DeckPanel、TestScene 的布局与尺寸；可能新增 ScrollContainer 或换行容器，以及 DeckPanel 的 CustomMinimumSize/锚点调整。
- **配置与构建**：新增「Excel → 目标格式」的转换步骤与产出目录（如 `config/generated/` 或 `res://config/` 下 json）；构建脚本或编辑器插件中需调用该步骤；若采用 Luban，需引入其 CLI/配置与输出路径约定。
- **代码**：GeneralConfigLoader、MapConfigLoader 等改为从转换后文件（如 JSON）加载，或保留兼容路径（先尝试读转换结果，不存在时回退 Excel）；ExcelDataReader 可能仅在开发/回退时保留；配置加载 API（如 `LoadHeroConfigs()`）的返回形态保持不变，仅数据来源变化。
- **依赖**：可能新增 Luban 或脚本运行时（Python/Node）；Godot 侧可能新增 JSON 解析（引擎已支持）或保持现有 C# 读文件方式。
