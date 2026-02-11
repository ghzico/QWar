using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ExcelDataReader;
using Godot;

namespace QWar;

/// <summary>
/// 从 config/General.xlsx 加载棋将配置。
/// 表头（第一行）：ID, 攻击力, 攻击距离, 移动距离, 形象配置
/// </summary>
public static class GeneralConfigLoader
{
	private const string ConfigPath = "res://config/General.xlsx";

	static GeneralConfigLoader()
	{
		// ExcelDataReader 读取 xls 等格式时需要
		System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
	}

	/// <summary>按 ID 索引的棋将配置。键为表格中的 ID 列。</summary>
	public static IReadOnlyDictionary<int, HeroConfig> LoadHeroConfigs()
	{
		var result = new Dictionary<int, HeroConfig>();

		if (!Godot.FileAccess.FileExists(ConfigPath))
		{
			GD.PushWarning($"配置不存在: {ConfigPath}");
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
			// 第 0 行为表头，从第 1 行开始为数据
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

					result[id] = new HeroConfig
					{
						Id = id,
						Attack = attack,
						AttackRange = attackRange,
						MoveRange = moveRange,
						PortraitPath = portraitPath
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
