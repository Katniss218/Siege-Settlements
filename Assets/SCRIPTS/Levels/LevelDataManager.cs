using SS.Levels.SaveStates;

namespace SS.Levels
{
	/// <summary>
	/// Represents the level file. Contains data, that's unique to the level, and doesn't change throuought the gameplay.
	/// </summary>
	public static class LevelDataManager
	{
		//
		//    General
		//


		//
		//    Factions
		//

		public const int PLAYER_FAC = 0;

		static FactionDefinition[] _factions;
		public static FactionDefinition[] factions
		{
			get
			{
				return _factions;
			}
			set
			{
				// if factions are reset, reset faction data to null (expecting factino data to be set afterwards too).
				_factionData = null;
				_factions = value;
			}
		}

		static FactionData[] _factionData;
		public static FactionData[] factionData
		{
			get
			{
				return _factionData;
			}
			set
			{
				if( value != null && value.Length != _factions.Length )
				{
					throw new System.Exception( "Faction definitions array must have the same length as the faction data array." );
				}
				_factionData = value;
			}
		}
		
		//
		//    Quests
		//


		//
		//    Dialogues
		//
		
			


		//
		//    Specialized Settings, used as extra parameters when creating level save states.
		//

		/// <summary>
		/// Contains extra information about what to save with save states. Not everything always needs to be saved (e.g. selection).
		/// </summary>


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
	}
}