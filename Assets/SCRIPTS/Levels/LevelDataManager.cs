using KFF;
using SS.Diplomacy;
using SS.Technologies;
using SS.TerrainCreation;
using System;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

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
	
		public static float mapSize { get { return mapSegments * TerrainMeshCreator.SEGMENT_SIZE; } }

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
				// and on tech state changed, since the factions don't match.
				_factionData = null;
				_factions = value;
				onTechStateChanged = new UnityEvent_int_string_TechnologyResearchProgress();
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
					throw new Exception( "Faction definitions array must have the same length as the faction data array." );
				}
				_factionData = value;
			}
		}
		
		public class UnityEvent_int_string_TechnologyResearchProgress : UnityEvent<int, string, TechnologyResearchProgress> { }

		public static UnityEvent_int_string_TechnologyResearchProgress onTechStateChanged = new UnityEvent_int_string_TechnologyResearchProgress();

		public static void SetTech( int factionId, string id, TechnologyResearchProgress progress )
		{
			if( factionData[factionId].techs.TryGetValue( id, out TechnologyResearchProgress curr ) )
			{
				if( curr == progress )
				{
					return;
				}
				else
				{
					factionData[factionId].techs[id] = progress;
					onTechStateChanged?.Invoke( factionId, id, progress );
				}
			}
			else
			{
				throw new Exception( "Unknown technology '" + id + "'." );
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


		public static void LoadFactions( KFFSerializer serializer )
		{
			int count = serializer.Analyze( "List" ).childCount;
			factionCount = count;

			factions = new FactionDefinition[factionCount];
			for( int i = 0; i < factions.Length; i++ )
			{
				factions[i] = new FactionDefinition();
			}

			serializer.DeserializeArray( "List", factions );
		}

		public static void LoadFactionData( KFFSerializer serializer )
		{
			int count = serializer.Analyze( "List" ).childCount;
			if( count != factionCount )
			{
				throw new Exception( "The number of faction data doesn't match the number of factions of the level." );
			}

			factionData = new FactionData[factionCount];

			for( int i = 0; i < factionData.Length; i++ )
			{
				factionData[i] = new FactionData();
			}

			serializer.DeserializeArray( "List", factionData );


			int relMatrixLength = serializer.Analyze( "RelationMatrix" ).childCount;

			sbyte[] array = serializer.ReadSByteArray( "RelationMatrix" );

			int supposedMatrixLength = RelationMap<DiplomaticRelation>.GetMatrixLength( factionCount );
			if( relMatrixLength != supposedMatrixLength )
			{
				throw new Exception( "The number of entries in the faction relation matrix doesn't match the number of factions of the level. Supposed to be " + supposedMatrixLength + "." );
			}

			diplomaticRelations = new RelationMap<DiplomaticRelation>( factionCount, DiplomaticRelation.Neutral );
			onRelationChanged = new _UnityEvent_int_int_DiplomaticRelation();
			for( int i = 0; i < diplomaticRelations.MatrixLength; i++ )
			{
				diplomaticRelations[i] = (DiplomaticRelation)array[i];
			}
		}

		public static void LoadDaylightCycle( KFFSerializer serializer )
		{
			DaylightCycleController daylightCycle = Object.FindObjectOfType<DaylightCycleController>();
			if( daylightCycle == null )
			{
				throw new System.Exception( "Couldn't find DaylightCycleController object." );
			}

			try
			{
				daylightCycle.dayLength = serializer.ReadInt( "DaylightCycle.DayLength" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DaylightCycle.DayLength' (" + serializer.file.fileName + ")." );
			}

			try
			{
				daylightCycle.nightLength = serializer.ReadInt( "DaylightCycle.NightLength" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DaylightCycle.NightLength' (" + serializer.file.fileName + ")." );
			}

			try
			{
				daylightCycle.sunIntensity = serializer.ReadFloat( "DaylightCycle.SunIntensity" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DaylightCycle.SunIntensity' (" + serializer.file.fileName + ")." );
			}

			try
			{
				daylightCycle.moonIntensity = serializer.ReadFloat( "DaylightCycle.MoonIntensity" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DaylightCycle.MoonIntensity' (" + serializer.file.fileName + ")." );
			}

			try
			{
				daylightCycle.sunElevationAngle = serializer.ReadFloat( "DaylightCycle.SunElevationAngle" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DaylightCycle.SunElevationAngle' (" + serializer.file.fileName + ")." );
			}

			try
			{
				daylightCycle.moonElevationAngle = serializer.ReadFloat( "DaylightCycle.MoonElevationAngle" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DaylightCycle.MoonElevationAngle' (" + serializer.file.fileName + ")." );
			}
		}

		public static void LoadDaylightCycleData( KFFSerializer serializer )
		{
			if( DaylightCycleController.instance == null )
			{
				throw new Exception( "Couldn't find DaylightCycleController object." );
			}

			try
			{
				DaylightCycleController.instance.time = serializer.ReadFloat( "DaylightCycleData.Time" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DaylightCycleData.Time' (" + serializer.file.fileName + ")." );
			}
		}

		public static void LoadCameraData( KFFSerializer serializer )
		{
			
			
			if( CameraController.instance == null )
			{
				throw new Exception( "Couldn't find CameraController object." );
			}

			try
			{
				CameraController.instance.transform.position = serializer.ReadVector3( "CameraData.Position" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'CameraData.Position' (" + serializer.file.fileName + ")." );
			}

			try
			{
				CameraController.instance.transform.rotation = serializer.ReadQuaternion( "CameraData.Rotation" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'CameraData.Rotation' (" + serializer.file.fileName + ")." );
			}

			try
			{
				CameraController.instance.size = serializer.ReadInt( "CameraData.ZoomSize" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'CameraData.ZoomSize' (" + serializer.file.fileName + ")." );
			}
		}

		public static void LoadTimeData( KFFSerializer serializer )
		{
			GameTimeCounter gameTimeCounter = Object.FindObjectOfType<GameTimeCounter>();

			if( gameTimeCounter == null )
			{
				throw new Exception( "Couldn't find GameTimeCounter object." );
			}

			try
			{
				gameTimeCounter.gameTimeOffset = serializer.ReadFloat( "GameTimeElapsed" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'GameTimeElapsed' (" + serializer.file.fileName + ")." );
			}
		}

		//
		//
		//

		public static void SaveFactionData( KFFSerializer serializer )
		{
			serializer.SerializeArray( "", "List", factionData );
			
			sbyte[] array = new sbyte[diplomaticRelations.MatrixLength];
			
			for( int i = 0; i < array.Length; i++ )
			{
				array[i] = (sbyte)diplomaticRelations[i];
			}

			serializer.WriteSByteArray( "", "RelationMatrix", array );
		}

		public static void SaveDaylightCycleData( KFFSerializer serializer )
		{			
			if( DaylightCycleController.instance == null )
			{
				throw new Exception( "Couldn't find DaylightCycleController object." );
			}

			serializer.WriteClass( "", "DaylightCycleData" );
			serializer.WriteFloat( "DaylightCycleData", "Time", DaylightCycleController.instance.time );
		}

		public static void SaveCameraData( KFFSerializer serializer )
		{
			if( CameraController.instance == null )
			{
				throw new Exception( "Couldn't find CameraController object." );
			}

			serializer.WriteClass( "", "CameraData" );
			serializer.WriteVector3( "CameraData", "Position", CameraController.instance.transform.position );
			serializer.WriteQuaternion( "CameraData", "Rotation", CameraController.instance.transform.rotation );

			serializer.WriteInt( "CameraData", "ZoomSize", CameraController.instance.size );
		}


		public static void SaveTimeData( KFFSerializer serializer )
		{
			GameTimeCounter gameTimeCounter = Object.FindObjectOfType<GameTimeCounter>();

			if( gameTimeCounter == null )
			{
				throw new Exception( "Couldn't find GameTimeCounter object." );
			}

			serializer.WriteFloat( "", "GameTimeElapsed", gameTimeCounter.GetElapsed() );
		}
	}
}