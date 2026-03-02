# Luban 配置导出（可选）

使用 [Luban](https://github.com/focus-creative-games/luban) 可将 Excel 配置表导出为 JSON，便于校验、扩展与多端一致。  
**与当前项目集成**：需保证导出 JSON 的字段与 `tools/README_config_export.md` 中约定一致，以便运行时 Loader 直接使用。

## 准备

1. 安装 [.NET SDK 6.0+](https://dotnet.microsoft.com/download/dotnet/6.0)。
2. 获取 Luban 工具：
   - 克隆 [luban_examples](https://github.com/focus-creative-games/luban_examples)，使用其中 `Tools/Luban.ClientServer/Luban.ClientServer.dll`；
   - 或将 Luban 发布包中的 `Luban.ClientServer.dll` 放到本目录或 `tools/luban/Tools/` 下。
3. Excel 需为 Luban 格式：首行 `##var`（字段名），次行 `##type`（类型），其后为数据行。  
   若沿用现有中文表头，可在 Luban 的 Excel 中使用相同含义的英文字段名（如 id, attack, attackRange, moveRange, portraitPath, ghp），并配置导出为与 `general.json` 相同的键值结构（按 id 为 key）。

## 导出与输出路径

- 将 Luban 的 **数据输出目录** 配置为项目的 `config/`，或生成后复制到 `config/`。
- 生成文件名需与约定一致：`general.json`、`map_cell.json`、`map_grid.json`。
- 使用 `data_json2` 等按主键导出的方式，可得到「id -> 配置项」的 JSON，与现有 Loader 兼容。

## 运行

在已配置好 Luban 与 Excel 的前提下，执行：

- Windows: `gen.bat`
- Mac/Linux: `chmod +x gen.sh && ./gen.sh`

脚本内需将 `--define_file`、`--input_data_dir`、`--output_data_dir` 指向本项目的 Defines 与 config 路径。  
具体参数可参考 [Luban 文档](https://www.datable.cn/docs/beginner/quickstart) 与 luban_examples 中的 gen 脚本。

## 与 Python 脚本二选一

- **Python 脚本**（`tools/export_config.py`）：直接读取当前格式的 Excel，无需改表，输出到 `config/*.json`。
- **Luban**：适合需要强校验、多表扩展、多端共用的项目；需将 Excel 改为 Luban 格式并维护 Defines。

两者只要生成符合 `README_config_export.md` 的 JSON，运行时行为一致。
