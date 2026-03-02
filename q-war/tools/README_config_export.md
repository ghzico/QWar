# 配置导出与 JSON 格式说明

游戏运行时优先从 `config/*.json` 加载配置；若不存在则回退到 Excel。资源更新或打包前请执行配置转换。

## 目标 JSON 结构（与 Loader 约定一致）

### general.json

棋将表，与 `General.xlsx` 对应。格式：**对象，键为 ID 字符串，值为棋将配置**。

```json
{
  "1": {
    "id": 1,
    "attack": 1,
    "attackRange": 2,
    "moveRange": 2,
    "portraitPath": "res://Res/General/knight.png",
    "ghp": 100
  },
  "2": { ... }
}
```

- `id`: int，棋将 ID  
- `attack`, `attackRange`, `moveRange`, `ghp`: int  
- `portraitPath`: string，形象资源路径（可为相对路径，Loader 会补 `res://`）

### map_cell.json

地图格贴图表，与 `MapCell.xlsx` 对应。格式：**对象，键为 MAPCELLID 字符串，值为贴图路径**。

```json
{
  "1": "res://Res/Map/grass.png",
  "2": "res://Res/Map/stone.png"
}
```

### map_grid.json

地图布局，与 `Map.xlsx` 对应。格式：**5×10 二维数组**，`grid[row][col]` 为 MAPCELLID。

```json
[
  [ 1, 1, 2, 2, 1, 1, 2, 2, 1, 1 ],
  [ 1, 2, 2, 1, 1, 2, 2, 1, 1, 2 ],
  ...
]
```

共 5 行，每行 10 个整数。

## 转换方式

### 方式一：Python 脚本（沿用当前 Excel 格式）

- 无需修改现有 Excel 表头。
- 依赖：Python 3.7+，`openpyxl`。
- 执行：`pip install -r tools/requirements.txt` 后运行  
  `python tools/export_config.py`  
  或从项目根：`python -m tools.export_config`（若 tools 为包）  
  或直接：`python tools/export_config.py`（脚本内用相对路径定位 config/）。

- 输出：在项目根下找到 `config` 目录，将生成的 json 写入 `config/general.json`、`config/map_cell.json`、`config/map_grid.json`。脚本需能解析当前 General/MapCell/Map 的列布局。

### 方式二：Luban（可选，适合扩展与校验）

- 使用 Luban 需将 Excel 改为 Luban 格式（首行 `##var`，次行 `##type`，其后数据行）。
- 将 Luban 输出目录配置为项目的 `config/`，或生成后复制到 `config/`，使文件名与字段约定与上述一致。
- 详见 `tools/luban/README.md`。

**CI/构建**：若在 CI 或打包流程中构建，可在构建步骤中执行 `python tools/export_config.py` 生成 `config/*.json`。

## 表结构与 Excel 列对应

| 表       | Excel 列（当前） | JSON 字段 / 含义 |
|----------|------------------|-------------------|
| General  | ID, 攻击力, 攻击距离, 移动距离, 形象配置, 血量 | id, attack, attackRange, moveRange, portraitPath, ghp |
| MapCell  | MAPCELLID, MAPCELLRES | 键=id，值=归一化路径 |
| Map      | 第 1～5 行，列 1～10 为 MAPCELLID | map_grid 二维数组 |
