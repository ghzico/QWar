using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ExcelDataReader;
using Godot;

namespace QWar;

/// <summary>
/// 从 config/General.xlsx 或 config/general.json 加载棋将配置。
/// 优先读取 general.json（由配置导出脚本生成）；不存在或解析失败时回退到 Excel。
/// </summary>
public static class GeneralConfigLoader
{
	private const string JsonPath = "res://config/general.json";
	private const string ConfigPath = "res://config/General.xlsx";

	static GeneralConfigLoader()
	{
		// ExcelDataReader 读取 xls 等格式时需要
		System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
	}

	/// <summary>按 ID 索引的棋将配置。键为表格中的 ID 列。</summary>
	public static IReadOnlyDictionary<int, HeroConfig> LoadHeroConfigs()
	{
		if (TryLoadFromJson(out var fromJson))
			return fromJson;
		return LoadFromExcel();
	}

	private static bool TryLoadFromJson(out IReadOnlyDictionary<int, HeroConfig> result)
	{
		result = new Dictionary<int, HeroConfig>();
		if (!Godot.FileAccess.FileExists(JsonPath))
			return false;
		try
		{
			using var file = Godot.FileAccess.Open(JsonPath, Godot.FileAccess.ModeFlags.Read);
			if (file == null) return false;
			string jsonText = file.GetAsText();
			var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
			var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, JsonHeroEntry>>(jsonText, opts);
			if (dict == null) return false;
			var outDict = new Dictionary<int, HeroConfig>();
			foreach (var kv in dict)
			{
				if (!int.TryParse(kv.Key, out int id)) continue;
				var e = kv.Value;
				outDict[id] = new HeroConfig
				{
					Id = e.Id,
					Attack = e.Attack,
					AttackRange = e.AttackRange,
					MoveRange = e.MoveRange,
					PortraitPath = e.PortraitPath ?? "",
					GHp = e.GHp > 0 ? e.GHp : 1
				};
			}
			result = outDict;
			return true;
		}
		catch (Exception ex)
		{
			GD.PushWarning($"读取 general.json 失败，将回退到 Excel: {ex.Message}");
			return false;
		}
	}

	private sealed class JsonHeroEntry
	{
		public int Id { get; set; }
		public int Attack { get; set; }
		public int AttackRange { get; set; }
		public int MoveRange { get; set; }
		public string? PortraitPath { get; set; }
		public int GHp { get; set; }
	}

	private static IReadOnlyDictionary<int, HeroConfig> LoadFromExcel()
	{
		var result = new Dictionary<int, HeroConfig>();
		if (!Godot.FileAccess.FileExists(ConfigPath))
		{
			GD.PushWarning($"配置不存在: {ConfigPath}，建议运行配置转换: python tools/export_config.py");
			return result;
		}

		byte[] bytes = Godot.FileAccess.GetFileAsBytes(ConfigPath);
		using var stream = new MemoryStream(bytes);

		using (var reader = ExcelReaderFactory.CreateReader(stream))
		{
			var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
			{
				ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = false }
			});

			if (dataSet.Tables.Count == 0)
				return result;

			DataTable table = dataSet.Tables[0];
			for (int r = 1; r < table.Rows.Count; r++)
			{
				DataRow row = table.Rows[r];
				if (row.ItemArray.Length < 5)
					continue;

				try
				{
					int id = ParseInt(row[0]);
					int attack = ParseInt(row[1]);
					int attackRange = ParseInt(row[2]);
					int moveRange = ParseInt(row[3]);
					string portraitPath = (row[4]?.ToString() ?? "").Trim();
					int ghp = row.ItemArray.Length >= 6 ? ParseInt(row[5]) : 1;
					if (ghp < 1) ghp = 1;

					result[id] = new HeroConfig
					{
						Id = id,
						Attack = attack,
						AttackRange = attackRange,
						MoveRange = moveRange,
						PortraitPath = portraitPath,
						GHp = ghp
					};
				}
				catch (System.Exception ex)
				{
					GD.PushWarning($"General.xlsx 第 {r + 1} 行解析失败: {ex.Message}");
				}
			}
		}

		return result;
	}

	/// <summary>
	/// 根据 config/General.xlsx 中指定 ID 的配置，应用到棋将实例。
	/// 支持：修改表格后重新运行即可生效；表格中新增 ID 后，在代码中使用该 ID 即可正确加载。
	/// </summary>
	/// <param name="hero">棋将节点（通常为刚 Instantiate 的 ChessHero）</param>
	/// <param name="configId">表格中的 ID 列取值</param>
	/// <param name="configs">若为 null 则内部调用 LoadHeroConfigs()，也可传入已加载的配置避免重复读表</param>
	/// <returns>是否找到并应用了该 ID 的配置</returns>
	public static bool ApplyToHero(ChessHero hero, int configId, IReadOnlyDictionary<int, HeroConfig> configs = null)
	{
		configs ??= LoadHeroConfigs();
		if (!configs.TryGetValue(configId, out var config))
			return false;

		hero.Attack = config.Attack;
		hero.AttackRange = config.AttackRange;
		hero.MoveRange = config.MoveRange;
		hero.SetInitialHp(config.GHp);
		if (!string.IsNullOrWhiteSpace(config.PortraitPath))
		{
			string path = NormalizeResourcePath(config.PortraitPath);
			hero.SetPortraitPath(path); // 先存路径，ChessHero._Ready 时会用路径再绑一次，确保显示
			var tex = GD.Load<Texture2D>(path);
			if (tex != null)
				hero.SetPortrait(tex);
			else
				GD.PushWarning($"棋将 ID {configId} 形象加载失败，路径: {path}");
		}
		return true;
	}

	/// <summary>将表格中的路径转为 Godot 可用的 res:// 路径（无前缀则补 res://）</summary>
	private static string NormalizeResourcePath(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
			return path;
		path = path.Trim().Replace('\\', '/');
		return path.StartsWith("res://", StringComparison.Ordinal) ? path : "res://" + path.TrimStart('/');
	}

	private static int ParseInt(object cell)
	{
		if (cell == null || cell is System.DBNull)
			return 0;
		if (cell is int i)
			return i;
		if (cell is double d)
			return (int)d;
		return int.TryParse(cell.ToString(), out int v) ? v : 0;
	}
}
