# QWar 配置驱动功能 OpenSpec

本文档记录基于 Excel 配置的棋将、敌将、地图贴图等功能的规格与实现约定，便于后续维护与扩展。

---

## 1. 概述

- **设计原则**：棋将/敌将/地图格均衍生于同一节点类型，通过 **ID + 配置表** 决定显示与数值，实现数据与逻辑解耦。
- **配置来源**：`config/General.xlsx`（棋将）、`config/Map.xlsx`（地图布局）、`config/MapCell.xlsx`（地图格贴图路径）。
- **资源目录**：`Res/General/`（棋将立绘）、`Res/Enemy/`（敌将立绘）、`Res/Map/`（地图格贴图）。

---

## 2. 棋将（ChessHero）配置

### 2.1 场景与脚本

- **场景**：`Scenes/ChessHero.tscn`。根节点为 `Control`，子节点包含 `TextureRect`（立绘）、`ActionPopup`（移动/攻击/技能）。
- **脚本**：`Scripts/ChessHero.cs`。同一场景、同一脚本，通过运行时设置属性区分不同棋将。

### 2.2 配置表 General.xlsx

- **路径**：`res://config/General.xlsx`。
- **表头（第 1 行）**：`ID`、`攻击力`、`攻击距离`、`移动距离`、`形象配置`。
- **数据行**：从第 2 行起，每行对应一个棋将配置；`ID` 为唯一标识，在代码中通过 ID 加载对应行。
- **形象配置**：填写贴图路径。支持：
  - 完整路径：`res://Res/General/knight.png`
  - 相对路径：`Res/General/knight.png` 或 `grass.png`（相对路径会补 `res://`，形象类会补 `Res/General` 逻辑在 GeneralConfigLoader 中未强制，实际多为完整路径或 `Res/General/xxx.png`）。

### 2.3 数据与 API

- **HeroConfig**（`Scripts/HeroConfig.cs`）：`Id`、`Attack`、`AttackRange`、`MoveRange`、`PortraitPath`。
- **GeneralConfigLoader**（`Scripts/GeneralConfigLoader.cs`）：
  - `LoadHeroConfigs()`：返回 `IReadOnlyDictionary<int, HeroConfig>`，键为表格中的 ID。
  - `ApplyToHero(ChessHero hero, int configId, configs = null)`：根据 configId 从配置中读取并应用到棋将（攻击力、攻击距离、移动距离、立绘）；可选传入已加载的 configs 避免重复读表。
- **ChessHero**：
  - 属性：`Attack`、`MoveRange`、`AttackRange`、`PortraitTexture`、内部 `PortraitPath`。
  - 方法：`SetPortrait(Texture2D)`、内部 `SetPortraitPath(string)`。`_Ready` 中若已有 `PortraitTexture` 则绑定到 TextureRect，否则用 `PortraitPath` 再加载一次，确保入树后立绘正确显示。

### 2.4 使用约定

- 修改表格中某 ID 的数值或形象后，重新运行即可生效；新增棋将时在表内新加一行并赋予新 ID，在关卡/测试场景中指定该 ID 即可。
- 测试场景中通过静态数组 `HeroConfigIds`（如 `{ 1, 2, 3 }`）指定本场出战的棋将 ID，实例化后调用 `ApplyToHero(hero, configId)` 完成绑定。

---

## 3. 敌将（ChessEnemy）

- **场景**：`Scenes/ChessEnemy.tscn`。含 `TextureRect`（立绘）、`Label`（血量）。
- **脚本**：`Scripts/ChessEnemy.cs`。提供 `SetPortrait(Texture2D)`、导出 `PortraitTexture`。
- 当前测试场景中敌将贴图仍由代码写死为 `Res/Enemy/monster1/2/3.png`，未接 Excel；结构与棋将一致，可按需扩展配置表。

---

## 4. 地图棋盘（FightBoard）与地图配置

### 4.1 地图格贴图配置 MapCell.xlsx

- **路径**：`res://config/MapCell.xlsx`。
- **表头（第 1 行）**：`MAPCELLID`、`MAPCELLRES`。
- **数据行**：从第 2 行起；`MAPCELLRES` 为贴图路径，资源均在 `Res/Map/` 下。
- **路径规范化**（MapConfigLoader.NormalizeMapCellPath）：
  - 已为 `res://` 开头：原样返回。
  - 以 `Res/Map/` 开头（不区分大小写）：仅补 `res://`。
  - 其他（如 `grass.png`）：补 `res://Res/Map/`。  
  避免出现 `res://Res/Map/Res/Map/xxx.png` 的重复前缀。

### 4.2 地图布局 Map.xlsx

- **路径**：`res://config/Map.xlsx`。
- **格式**：每格一个 MAPCELLID，无分号拼接。
  - **第 1 行**：表头（不参与解析）。
  - **第 2～6 行**：对应地图第 0～4 行（共 5 行）。
  - **每行 11 列**：列 0 = MAPID（可选），列 1～10 = 该行从左到右 10 个格子的 MAPCELLID。
- **解析**：MapConfigLoader.LoadMapGrid() 读取前 5 行数据，填充 5×10 的 `int[,]`，`grid[row, col]` 即该格使用的 MAPCELLID。

### 4.3 FightBoard 构建与点击

- **BuildCells()**：
  - 调用 `MapConfigLoader.LoadMapCellResources()` 与 `LoadMapGrid()`；若抛异常则捕获并打警告，仍继续用默认 ColorRect 绘制 5×10 格，保证棋盘和点击层可用。
  - 对每格：根据 `grid[r,c]` 取 MAPCELLID → 在 MapCell 配置中取路径 → `GD.Load<Texture2D>(path)`，成功则添加 TextureRect，失败则添加 ColorRect（棋盘格样式）。所有格子统一 `MouseFilter = Ignore`，不拦截点击。
- **点击层**：透明 Control，`Position = Vector2.Zero`，尺寸与棋盘一致，`MouseFilter = Stop`，`ZIndex = 10`，最后 AddChild 以保证在最上层；接收点击后根据坐标换算 (row, col)，再通过 `GetUnitAt(row, col)` 判断是否点击到棋将并弹出操作或进入选目标。

---

## 5. 编码与依赖

- **Excel 编码**：ExcelDataReader 会使用 Code Page 1252 等，需在首次读 Excel 前注册 `System.Text.Encoding.CodePagesEncodingProvider`。
  - 在 **MapConfigLoader** 与 **GeneralConfigLoader** 的静态构造函数中均调用 `Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)`，保证无论先加载地图还是先加载棋将配置都不会报编码错误。
- **NuGet**：`ExcelDataReader`、`ExcelDataReader.DataSet`、`System.Text.Encoding.CodePages`（见 `QWar.csproj`）。

---

## 6. 文件与资源结构速查

| 用途           | 路径/位置 |
|----------------|-----------|
| 棋将配置       | `config/General.xlsx` |
| 地图布局       | `config/Map.xlsx`（5 行×11 列，每格一 MAPCELLID） |
| 地图格贴图配置 | `config/MapCell.xlsx`（MAPCELLID → MAPCELLRES） |
| 棋将立绘       | `Res/General/`（如 knight.png, Witch.png, SwordKnight.png） |
| 敌将立绘       | `Res/Enemy/`（如 monster1/2/3.png） |
| 地图格贴图     | `Res/Map/`（如 grass.png, stone.png） |
| 棋将场景/脚本  | `Scenes/ChessHero.tscn`，`Scripts/ChessHero.cs` |
| 敌将场景/脚本  | `Scenes/ChessEnemy.tscn`，`Scripts/ChessEnemy.cs` |
| 棋盘场景/脚本  | `Scenes/FightBoard.tscn`，`Scripts/FightBoard.cs` |
| 配置加载       | `Scripts/GeneralConfigLoader.cs`，`Scripts/MapConfigLoader.cs`，`Scripts/HeroConfig.cs` |
| 测试入口       | `Scripts/TestScene.cs`（HeroConfigIds + ApplyToHero；敌将暂写死贴图） |

---

## 7. 本次对话主要变更摘要

1. **棋将/敌将立绘**：ColorRect 改为 TextureRect；支持导出 `PortraitTexture` 与运行时 `SetPortrait`；TestScene 中为三个棋将指定 General 下 knight/Witch/SwordKnight，敌将指定 Enemy 下 1/2/3。
2. **棋将配置表**：从 General.xlsx 按 ID 加载攻击力、攻击距离、移动距离、形象路径；引入 HeroConfig、GeneralConfigLoader、ApplyToHero、SetPortraitPath；形象路径规范化与 _Ready 兜底加载，修复“形象不显示”；TestScene 改为通过 HeroConfigIds 与 ApplyToHero 按 ID 绑定。
3. **地图配置**：MapCell.xlsx 提供 MAPCELLID→MAPCELLRES；Map.xlsx 从“分号分隔”改为“每格一个 MAPCELLID”的 5 行×11 列格式；MapConfigLoader 路径规范化避免 Res/Map 重复前缀；FightBoard BuildCells 用贴图绘制格子，失败时回退 ColorRect，并 try-catch 保证点击层与棋盘始终可用。
4. **编码**：MapConfigLoader 与 GeneralConfigLoader 静态构造函数中注册 CodePagesEncodingProvider，消除 Excel 读取时的 encoding 1252 错误。
5. **点击与层级**：所有地图格 MouseFilter = Ignore；点击层 ZIndex = 10、最后添加，保证棋将可被选中与操作。

以上约定与实现均已在当前代码与资源结构中落地，后续扩展新棋将、新地图或新 MAPCELL 类型时，只需在对应 Excel 中增行/增列并在代码中引用对应 ID 即可。
