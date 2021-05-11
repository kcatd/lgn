using System.Text.RegularExpressions;
using KitsuneCore.Game;
using UnityEngine;

namespace KitsuneAPI.KitsuneEditor.Editor.Game
{
	public class KitsuneGameLevelXpData : ScriptableObject, IGameLevelXpData
	{
		[SerializeField]
		private string _gameVersion;
		public string GameVersion
		{
			get => _gameVersion;
			set => _gameVersion = value;
		}
		
		[SerializeField]
		private int _maxLevel;
		public int MaxLevel
		{
			get => _maxLevel;
			set => _maxLevel = value;
		}

		[SerializeField]
		private bool _autoScale = true;
		public bool AutoScale
		{
			get => _autoScale;
			set => _autoScale = value;
		}
		
		/// <value>
		/// For explicitly setting each level
		/// </value>
		[SerializeField]
		private string _levelXpJson;
		public string LevelXpJson
		{
			get
			{
				string json = Regex.Replace(_levelXpJson, @"\\", "");
				json = json.Replace("\n", "");
				json = json.Replace(" ", "");
				return json;
			}
			set => _levelXpJson = value;
		}

		/// <value>
		/// For auto scaling levels
		/// </value>
		[SerializeField]
		private int _startingXp = 200;
		public int StartingXp
		{
			get => _startingXp;
			set => _startingXp = value;
		}
		
		/// <value>
		/// For auto scaling levels
		/// </value>
		[SerializeField]
		private float _levelCoefficient = 0.2f;
		public float LevelCoefficient
		{
			get => _levelCoefficient;
			set => _levelCoefficient = value;
		}
	}
}