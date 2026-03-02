@echo off
REM 将 Excel 配置导出为 config/*.json（需先配置 Luban 路径与 Defines/Datas）
REM 1. 从 https://github.com/focus-creative-games/luban_examples 获取 Luban.ClientServer
REM 2. 将 Luban.ClientServer.dll 所在目录设为 LUBAN_TOOLS，或放在本目录的 Tools 子目录下
set SCRIPT_DIR=%~dp0
set PROJECT_ROOT=%SCRIPT_DIR%..\..
set CONFIG_DIR=%PROJECT_ROOT%\config

set LUBAN_DLL=%SCRIPT_DIR%Tools\Luban.ClientServer\Luban.ClientServer.dll
if not exist "%LUBAN_DLL%" set LUBAN_DLL=%SCRIPT_DIR%Luban.ClientServer.dll
if not exist "%LUBAN_DLL%" (
    echo Luban.ClientServer.dll 未找到。请将 dll 放到 tools\luban\Tools\Luban.ClientServer\ 或 tools\luban\
    echo 或使用 Python 脚本: python tools/export_config.py
    exit /b 1
)

REM 若使用 Luban，需在 Defines 中配置 __root__.xml 和表定义，Datas 中放置 Luban 格式的 Excel
set DEFINES=%SCRIPT_DIR%Defines
set DATAS=%SCRIPT_DIR%Datas
if not exist "%DEFINES%" mkdir "%DEFINES%"
if not exist "%DATAS%" mkdir "%DATAS%"
if not exist "%DEFINES%\__root__.xml" (
    echo 请先在 tools\luban\Defines 下配置 __root__.xml 和表定义，并在 Datas 下放置 Luban 格式 Excel。
    echo 详见 tools\luban\README.md
    exit /b 1
)

dotnet "%LUBAN_DLL%" -j cfg ^
  --define_file "%DEFINES%\__root__.xml" ^
  --input_data_dir "%DATAS%" ^
  --output_data_dir "%CONFIG_DIR%" ^
  --gen_types data_json2

if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%
echo 已导出到 %CONFIG_DIR%
exit /b 0
