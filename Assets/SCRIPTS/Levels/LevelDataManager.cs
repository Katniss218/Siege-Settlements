using KFF;
using SS.Content;
using SS.Diplomacy;
using SS.Technologies;
using UnityEngine;
using UnityEngine.Events;

namespace SS.Levels
{
	/// <summary>
	/// Contains data, that's unique to the level or level save state and can't be represented by any other piece of data in the level's scene.
	/// </summary>
	public static class LevelDataManager
	{
		//
		//    General
		//

		/// <summary>
		/// The number of segments of the map (Read Only).
		/// </summary>
		public static int mapSegments { get; private set; }
		
		/// <summary>
		/// The y size of the map. In world space units (Read Only).
		/// </summary>
		public static float mapHeight { get; private set; }


		//
		//    Factions
		//

		/// <summary>
		/// The ID of the player's faction in a currently loaded level.
		/// </summary>
		public const int PLAYER_FAC = 0;

		/// <summary>
		/// Contains the amount of factions registered in the currently loaded level (Read Only).
		/// </summary>
		public static int factionCount { get; private set; }

		static FactionDefinition[] _factions;
		/// <summary>
		/// Contains definitions for every faction registered in the currently loaded level (Read Only).
		/// </summary>
		public static FactionDefinition[] factions
		{
			get
			{
				return _factions;
			}
			set
			{
				// if factions are reset, reset faction data as well.
				_factionData = null;
				_factions = value;
			}
		}


		
		static FactionData[] _factionData;
		/// <summary>
		/// Contains data for every faction registered in the currently loaded level (Read Only).
		/// </summary>
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
		
		/// <summary>
		/// Contains relations between every faction registered in the currently loaded level (Read Only).
		/// </summary>
		private static RelationMap<DiplomaticRelation> diplomaticRelations;

		public class _UnityEvent_int_int_DiplomaticRelation : UnityEvent<int, int, DiplomaticRelation> { }

		public static _UnityEvent_int_int_DiplomaticRelation onRelationChanged = new _UnityEvent_int_int_DiplomaticRelation();

		public static DiplomaticRelation GetRelation( int fac1, int fac2 )
		{
			return diplomaticRelations[fac1, fac2];
		}

		public static void SetRelation( int fac1, int fac2, DiplomaticRelation rel )
		{
			diplomaticRelations[fac1, fac2] = rel;
			onRelationChanged?.Invoke( fac1, fac2, rel );
		}


		//
		//    Quests
		//


		//
		//    Dialogues
		//



		
		


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static void LoadMapData( KFFSerializer serializer )
		{
			mapSegments = serializer.ReadInt( "Map.Segments" );
			mapHeight = serializer.ReadFloat( "Map.Height" );
		}


		public static void LoadFactions( string levelIdentifier )
		{
			string path = LevelManager.GetFullDataPath( levelIdentifier, "factions.kff" );

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );


			int count = serializer.Analyze( "List" ).childCount;
			factionCount = count;

			factions = new FactionDefinition[factionCount];
			for( int i = 0; i < factions.Length; i++ )
			{
				factions[i] = new FactionDefinition();
			}

			serializer.DeserializeArray( "List", factions );
		}

		public static void LoadFactionData( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = LevelManager.GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_factions.kff";

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );


			int count = serializer.Analyze( "List" ).childCount;
			if( count != factionCount )
			{
				throw new System.Exception( "The number of faction data doesn't match the number of factions of this level - '" + levelIdentifier + ":" + levelSaveStateIdentifier + "'." );
			}

			factionData = new FactionData[factionCount];

			for( int i = 0; i < factionData.Length; i++ )
			{
				factionData[i] = new FactionData();
			}

			serializer.DeserializeArray( "List", factionData );


			int relMatCount = serializer.Analyze( "RelationMatrix" ).childCount;

			sbyte[] array = serializer.ReadSByteArray( "RelationMatrix" );

			diplomaticRelations = new RelationMap<DiplomaticRelation>( RelationMap<DiplomaticRelation>.GetSize( relMatCount ), DiplomaticRelation.Neutral );
			onRelationChanged = new _UnityEvent_int_int_DiplomaticRelation();
			for( int i = 0; i < diplomaticRelations.MatrixLength; i++ )
			{
				diplomaticRelations[i] = (DiplomaticRelation)array[i];
			}
		}

		public static void LoadDaylightCycle( KFFSerializer serializer )
		{
			int dayLength = serializer.ReadInt( "DaylightCycle.DayLength" );
			int nightLength = serializer.ReadInt( "DaylightCycle.NightLength" );

			float sunIntensity = serializer.ReadFloat( "DaylightCycle.SunIntensity" );
			float moonIntensity = serializer.ReadFloat( "DaylightCycle.MoonIntensity" );

			float sunElevationAngle = serializer.ReadFloat( "DaylightCycle.SunElevationAngle" );
			float moonElevationAngle = serializer.ReadFloat( "DaylightCycle.MoonElevationAngle" );


			DaylightCycleController daylightCycle = Object.FindObjectOfType<DaylightCycleController>();
			if( daylightCycle == null )
			{
				throw new System.Exception( "Couldn't find DaylightCycleController object." );
			}
			daylightCycle.dayLength = dayLength;
			daylightCycle.nightLength = nightLength;

			daylightCycle.sunIntensity = sunIntensity;
			daylightCycle.moonIntensity = moonIntensity;

			daylightCycle.sunElevationAngle = sunElevationAngle;
			daylightCycle.moonElevationAngle = moonElevationAngle;
		}

		public static void LoadDaylightCycleData( KFFSerializer serializer )
		{
			float time = serializer.ReadFloat( "DaylightCycleData.Time" );
			
			if( DaylightCycleController.instance == null )
			{
				throw new System.Exception( "Couldn't find DaylightCycleController object." );
			}
			DaylightCycleController.instance.time = time;
		}

		public static void LoadCameraData( KFFSerializer serializer )
		{
			Vector3 pos = serializer.ReadVector3( "CameraData.Position" );
			Quaternion rot = serializer.ReadQuaternion( "CameraData.Rotation" );

			int zoomSize = serializer.ReadInt( "CameraData.ZoomSize" );

			
			if( CameraController.instance == null )
			{
				throw new System.Exception( "Couldn't find CameraController object." );
			}

			CameraController.instance.transform.SetPositionAndRotation( pos, rot );
			CameraController.instance.size = zoomSize;
		}

		//
		//
		//

		public static void SaveFactionData( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = LevelManager.GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_factions.kff";

			KFFSerializer serializer = new KFFSerializer( new KFFFile( path ) );

			serializer.SerializeArray( "", "List", factionData );

			
			sbyte[] array = new sbyte[diplomaticRelations.MatrixLength];
			
			for( int i = 0; i < array.Length; i++ )
			{
				array[i] = (sbyte)diplomaticRelations[i];
			}

			serializer.WriteSByteArray( "", "RelationMatrix", array );

			serializer.WriteToFile( path, DefinitionManager.FILE_ENCODING );
		}

		public static void SaveDaylightCycleData( KFFSerializer serializer )
		{			
			if( DaylightCycleController.instance == null )
			{
				throw new System.Exception( "Couldn't find DaylightCycleController object." );
			}

			serializer.WriteClass( "", "DaylightCycleData" );
			serializer.WriteFloat( "DaylightCycleData", "Time", DaylightCycleController.instance.time );
		}

		public static void SaveCameraData( KFFSerializer serializer )
		{
			if( CameraController.instance == null )
			{
				throw new System.Exception( "Couldn't find CameraController object." );
			}

			serializer.WriteClass( "", "CameraData" );
			serializer.WriteVector3( "CameraData", "Position", CameraController.instance.transform.position );
			serializer.WriteQuaternion( "CameraData", "Rotation", CameraController.instance.transform.rotation );

			serializer.WriteInt( "CameraData", "ZoomSize", CameraController.instance.size );
		}
	}
}