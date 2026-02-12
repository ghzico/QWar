using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ExcelDataReader;
using Godot;

namespace QWar;

/// <summary>
/// 从 config/MapCell.xlsx 与 config/Map.xlsx 加载地图格子贴图配置。
/// MapCell.xlsx 表头：MAPCELLID, MAPCELLRES（MAPCELLRES 为 Res/Map 下资源路径或相对路径）。
/// Map.xlsx 表头一行；数据 5 行对应 5 行地图，MAPQUEUEx 即第 x 行：每行 10 列为从左到右的 MAPCELLID（列顺序：MAPID, 第0列…第9列 或 MAPID,MAPQUEUE0 的 10 个格子）。
/// </summary>
public static class MapConfigLoader
{
	private const string MapCellPath = "res://config/MapCell.xlsx";
	private const string MapPath = "res://config/Map.xlsx";
	private const string MapResPrefix = "res://Res/Map/";

	static MapConfigLoader()
	{
		// ExcelDataReader 内部会用到 CodePage 1252 等，必须在首次读 Excel 前注册
		System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
	}

	/// <summary>MAPCELLID -> 可用于 GD.Load 的完整资源路径（已归一化到 res://，相对路径会补 Res/Map）</summary>
	public static IReadOnlyDictionary<int, string> LoadMapCellResources()
	{
		var result = new Dictionary<int, string>();
		if (!Godot.FileAccess.FileExists(MapCellPath))
		{
			GD.PushWarning($"配置不存在: {MapCellPath}");
			return result;
		}

		byte[] bytes = Godot.FileAccess.GetFileAsBytes(MapCellPath);
		using var stream = new MemoryStream(bytes);
		using (var reader = ExcelReaderFactory.CreateReader(stream))
		{
			var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
			{
				ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = false }
			});
			if (dataSet.Tables.Count == 0) return result;
			DataTable table = dataSet.Tables[0];
			for (int r = 1; r < table.Rows.Count; r++)
			{
				DataRow row = table.Rows[r];
				if (row.ItemArray.Length < 2) continue;
				try
				{
					int id = ParseInt(row[0]);
					string res = (row[1]?.ToString() ?? "").Trim();
					if (string.IsNullOrEmpty(res)) continue;
					result[id] = NormalizeMapCellPath(res);
				}
				catch (Exception ex)
				{
					GD.PushWarning($"MapCell.xlsx 第 {r + 1} 行解析失败: {ex.Message}");
				}
			}
		}
		return result;
	}

	/// <summary>将 MAPCELLRES 转为 res:// 路径；相对路径视为 Res/Map 下，避免重复拼接 Res/Map。</summary>
	private static string NormalizeMapCellPath(string path)
	{
		if (string.IsNullOrWhiteSpace(path)) return path;
		path = path.Trim().Replace('\\', '/').TrimStart('/');
		if (string.IsNullOrEmpty(path)) return MapResPrefix.TrimEnd('/');
		// 已是完整 res:// 路径，直接返回
		if (path.StartsWith("res://", StringComparison.Ordinal))
			return path;
		// 已包含 Res/Map，只补 res://
		if (path.StartsWith("Res/Map/", StringComparison.OrdinalIgnoreCase))
			return "res://" + path;
		// 仅文件名或子路径，补 res://Res/Map/
		return MapResPrefix + path;
	}

	/// <summary>
	/// 从 Map.xlsx 读取 5×10 的 MAPCELLID 网格。
	/// 约定：第 0 行为表头；第 1～5 行为地图第 0～4 行。每行 11 列：列0=MAPID，列1～10=该行从左到右 10 格的 MAPCELLID（每格一个数，无分号）。
	/// </summary>
	public static int[,] LoadMapGrid(int mapId = 1)
	{
		var grid = new int[5, 10];
		if (!Godot.FileAccess.FileExists(MapPath))
		{
			GD.PushWarning($"配置不存在: {MapPath}");
			return grid;
		}

		byte[] bytes = Godot.FileAccess.GetFileAsBytes(MapPath);
		using var stream = new MemoryStream(bytes);
		using (var reader = ExcelReaderFactory.CreateReader(stream))
		{
			var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
			{
				ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = false }
			});
			if (dataSet.Tables.Count == 0) return grid;
			DataTable table = dataSet.Tables[0];
			// 第 0 行表头，第 1～5 行 = 地图第 0～4 行；每行 11 列：列0=MAPID（可选），列1～10=该行从左到右 10 个 MAPCELLID
			for (int r = 0; r < 5 && r + 1 < table.Rows.Count; r++)
			{
				DataRow row = table.Rows[r + 1];
				if (row.ItemArray.Length < 11) continue;
				for (int c = 0; c < 10; c++)
					grid[r, c] = ParseInt(row[c + 1]);
			}
		}
		return grid;
	}

	private static int ParseInt(object cell)
	{
		if (cell == null || cell is DBNull) return 0;
		if (cell is int i) return i;
		if (cell is double d) return (int)d;
		return int.TryParse(cell.ToString(), out int v) ? v : 0;
	}
}
