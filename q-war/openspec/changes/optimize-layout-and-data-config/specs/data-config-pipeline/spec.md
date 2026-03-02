# Spec: data-config-pipeline

## ADDED Requirements

### Requirement: 配置在构建期可转换为程序易读格式

项目 SHALL 提供将 Excel 配置表（General.xlsx、Map.xlsx、MapCell.xlsx）在构建或资源准备阶段转换为程序可高效读取的格式（如 JSON）的机制；转换结果 SHALL 写入项目指定目录（如 `res://config/` 下），供运行时加载使用。

#### Scenario: 转换产出可被运行时加载

- **WHEN** 已执行配置转换步骤且输出文件存在
- **THEN** 游戏运行时 SHALL 能优先从转换后的文件（如 general.json、map_cell.json、map_grid.json 等）加载配置，并得到与从 Excel 加载语义一致的数据（如按 ID 索引的棋将配置、地图格 ID 与贴图路径、地图网格等）

#### Scenario: 表结构变更可同步到转换与加载

- **WHEN** Excel 表头或列含义发生变更（如新增列、列顺序调整）
- **THEN** 转换规则（脚本或 Luban 等工具配置）与运行时 Loader 的解析逻辑 SHALL 可同步更新，保证转换结果与加载代码一致；文档或注释 SHALL 指明表结构与目标格式的对应关系

---

### Requirement: 运行时优先使用转换后数据并支持回退

配置加载逻辑（GeneralConfigLoader、MapConfigLoader 等）SHALL 优先尝试读取转换后的数据文件（如 JSON）；当转换结果不存在或读取失败时，SHALL 可回退到直接读取 Excel，并给出可选的提示（如日志），以便开发期未执行转换时仍可运行。

#### Scenario: 存在转换结果时使用转换结果

- **WHEN** 目标格式文件（如 general.json）存在且格式正确
- **THEN** Loader SHALL 从该文件加载配置，不依赖 Excel 与 ExcelDataReader；对外 API（如 LoadHeroConfigs() 的返回类型与键值语义）SHALL 与现有行为一致

#### Scenario: 无转换结果时回退 Excel

- **WHEN** 转换结果文件不存在或解析失败
- **THEN** Loader SHALL 回退到从原有 Excel 文件加载，保证游戏仍可运行；SHALL 可选地输出日志或提示，建议执行配置转换以使用高效格式

---

### Requirement: 工具链可融入构建或资源流程

项目 SHALL 提供可重复执行的配置转换入口（如脚本或 Luban 命令），并 SHALL 在文档或脚本内说明何时需执行（如资源导入后、打包前）；可选地，在 CI 或 Godot 导入流程中集成该步骤，确保正式构建使用转换后的数据。

#### Scenario: 转换步骤可被手动或脚本触发

- **WHEN** 开发者或 CI 需要更新配置
- **THEN** 存在明确的方式（如运行 `tools/export_config.py` 或 `luban_gen.bat`）可触发 Excel → 目标格式的转换，且输出路径与运行时 Loader 的查找路径一致

#### Scenario: 配置使用逻辑有统一约定

- **WHEN** 新增或修改配置表
- **THEN** 项目内 SHALL 有统一约定：配置数据来源为「转换后的文件」，Excel 为编辑源；表结构变更时需同时更新转换规则与 Loader 解析逻辑，保证可维护性
