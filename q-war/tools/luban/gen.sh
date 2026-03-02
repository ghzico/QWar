#!/usr/bin/env bash
# 将 Excel 配置导出为 config/*.json（需先配置 Luban 路径与 Defines/Datas）
# 使用方式见 tools/luban/README.md
set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
CONFIG_DIR="$PROJECT_ROOT/config"

LUBAN_DLL="$SCRIPT_DIR/Tools/Luban.ClientServer/Luban.ClientServer.dll"
[ -f "$LUBAN_DLL" ] || LUBAN_DLL="$SCRIPT_DIR/Luban.ClientServer.dll"
if [ ! -f "$LUBAN_DLL" ]; then
  echo "Luban.ClientServer.dll 未找到。请将 dll 放到 tools/luban/Tools/Luban.ClientServer/ 或 tools/luban/"
  echo "或使用 Python 脚本: python tools/export_config.py"
  exit 1
fi

DEFINES="$SCRIPT_DIR/Defines"
DATAS="$SCRIPT_DIR/Datas"
[ -d "$DEFINES" ] || mkdir -p "$DEFINES"
[ -d "$DATAS" ] || mkdir -p "$DATAS"
if [ ! -f "$DEFINES/__root__.xml" ]; then
  echo "请先在 tools/luban/Defines 下配置 __root__.xml 和表定义，并在 Datas 下放置 Luban 格式 Excel。"
  echo "详见 tools/luban/README.md"
  exit 1
fi

dotnet "$LUBAN_DLL" -j cfg \
  --define_file "$DEFINES/__root__.xml" \
  --input_data_dir "$DATAS" \
  --output_data_dir "$CONFIG_DIR" \
  --gen_types data_json2

echo "已导出到 $CONFIG_DIR"
