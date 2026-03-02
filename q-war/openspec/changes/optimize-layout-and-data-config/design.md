# Design: 棋将包布局优化与数据配置工具链

## Context

- **当前状态**：DeckPanel 使用 HBoxContainer 横向排列棋将卡片，CustomMinimumSize 固定为 (0, 100)，卡片尺寸 80×90；当 General.xlsx 中棋将数量较多时，横向排布会超出视口或挤压，导致显示不全。配置方面，GeneralConfigLoader 与 MapConfigLoader 在运行时通过 ExcelDataReader 直接读取 xlsx，每次加载都做流式解析，列索引与表头顺序强耦合，且无构建期校验。
- **约束**：沿用 Godot 4 + C#；配置表语义（棋将 ID、攻击/移动/攻击距离、形象路径、血量；地图格 MAPCELLID/MAPCELLRES 等）保持不变；不破坏现有 TestScene、FightBoard、ChessHero 的调用方式（如 `LoadHeroConfigs()`、`ApplyToHero` 等）。
- **相关方**：DeckPanel/DeckHeroCard 负责棋将包 UI；GeneralConfigLoader/MapConfigLoader 负责配置加载；构建/资源流程需接入转换步骤。

## Goals / Non-Goals

**Goals:**

- 棋将包区域在任意合理棋将数量下均可完整展示所有卡片，支持滚动或换行/分页，避免溢出或显示不全。
- 配置在构建期转换为程序易读格式（如 JSON），运行时只读转换结果，提升加载性能并降低与 Excel 实现的耦合。
- 建立「Excel → 目标格式」的自动化工具链（可选用 Luban 或自研脚本），并融入项目构建或资源导入流程。
- 统一配置加载约定：Loader 优先读转换后数据，表结构变更时同步更新转换规则与加载代码。

**Non-Goals:**

- 不改变棋将包的业务逻辑（拖拽布阵、确认出战、每战每棋将上场一次等）；不在本变更内实现关卡级「可用棋将子集」配置。
- 不强制替换为 Luban；若自研脚本更易维护可优先自研；不改变现有 Excel 表头语义与列含义。

## Decisions

1. **棋将包布局方案**  
   - **决策**：采用「横向 ScrollContainer + 内部 HBox」或「多行换行（如 FlowContainer/自定义换行）」之一，确保所有卡片在可视或可滚动区域内完整显示；DeckPanel 设置合理最小高度（如 120）并保证在 TestScene 的 VBox 中能获得足够空间，避免被挤压。  
   - **理由**：横向滚动与多行换行均为常见模式，任选其一即可解决显示不全；具体实现时可根据 Godot 控件可用性（如 FlowContainer 在 4.x 的 support）选择。  
   - **备选**：保持单行 HBox 不变。不采纳，无法解决数量多时的显示问题。

2. **配置目标格式与存放位置**  
   - **决策**：转换输出为 JSON 文件，放在 `res://config/` 下（如 `general.json`、`map_cell.json`、`map_grid.json`），与现有「配置在 config 目录」的约定一致；运行时 Loader 优先读取同名的 .json，若不存在则回退到读 .xlsx（便于开发期未跑转换时仍可运行）。  
   - **理由**：JSON 为 Godot/C# 原生支持，无需额外运行时依赖；路径统一便于工具链与加载逻辑收敛。  
   - **备选**：二进制格式。不优先，因可读性与调试便利性不如 JSON；若后续有体量或性能需求再考虑。

3. **工具链选型：Luban 与自研脚本**  
   - **决策**：优先评估 Luban 与当前表结构（General/Map/MapCell）的匹配度；若 Luban 的 Excel 规则与输出格式能直接满足「按 ID 索引、字段一致」且易于集成到本地构建，则采用 Luban；否则采用自研脚本（如 Python + openpyxl 或 Node 脚本）读取 Excel 并输出 JSON，由项目内 Makefile/批处理或 Godot 导入脚本触发。  
   - **理由**：Luban 为成熟配置导出方案，可减少重复造轮子；若表结构或命名与 Luban 默认习惯差异大，自研脚本更易定制。  
   - **备选**：仅自研。保留灵活性，按评估结果二选一。

4. **Loader 兼容策略**  
   - **决策**：GeneralConfigLoader.LoadHeroConfigs() 与 MapConfigLoader 的加载方法内部：先尝试读取对应 .json（如 general.json），成功则解析 JSON 并返回；若文件不存在或解析失败则回退到当前 Excel 逻辑，并打日志提示「建议运行配置转换」。对外 API（返回类型、键值语义）不变。  
   - **理由**：平滑迁移，开发与 CI 未集成转换时仍可跑；上线或正式包可通过构建步骤保证 json 存在从而不再依赖 Excel。

5. **构建/资源流程集成**  
   - **决策**：在项目根或 config 目录提供脚本/命令（如 `tools/export_config.py` 或 `luban_gen.bat`），在「资源导入」或「打包前」文档中说明需先执行该步骤；若使用 Godot 的 Import 插件，可在检测到 xlsx 变更时调用导出脚本生成 json（可选）。  
   - **理由**：先保证有一条明确的手动/CI 可用的转换路径，再视需要做自动化程度提升。

## Risks / Trade-offs

- **[风险] 棋将包改为滚动/换行后，不同分辨率下高度不一致**  
  → 缓解：DeckPanel 使用合理的最小高度与 SizeFlags，保证在 TestScene 的 VBox 中与棋盘、规则说明等分配空间时不被压扁；必要时为棋将包区域设最大高度并内部滚动。

- **[风险] 同时维护 JSON 与 Excel 两套输入，逻辑分支复杂**  
  → 缓解：Loader 内「先 json 后 xlsx」单一分支，表结构变更时同时改转换规则与 Loader 的 JSON 解析字段；长期可考虑仅保留 JSON 作为源，Excel 仅作编辑用、转换必跑。

- **[trade-off] Luban 与自研脚本的取舍**  
  → 接受：先做「自研脚本 + JSON」最小方案，保证「转换 + 运行时读 JSON」闭环；若后续引入 Luban，仅替换转换步骤，Loader 仍读同一套 JSON 结构即可。

## Migration Plan

- 实现顺序建议：先做「棋将包布局优化」（ScrollContainer 或换行），验证显示与拖拽正常；再做「Excel → JSON 转换脚本」与「Loader 优先读 JSON」；最后文档化工具链与构建步骤，可选集成到 CI 或 Godot 导入。
- 回滚：布局可还原为单行 HBox；Loader 在无 json 时已回退 Excel，移除或不再运行转换脚本即可回到当前行为。

## Open Questions

- 无。若后续需要「按关卡限制可用棋将」，可在关卡配置中增加 ID 列表，布阵时对 LoadHeroConfigs 结果做过滤，与本次变更独立。
