using KFF;
using SS.Buildings;
using SS.Content;
using SS.Extras;
using SS.Heroes;
using SS.Levels.SaveStates;
using SS.Projectiles;
using SS.TerrainCreation;
using SS.UI;
using SS.Units;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SS.Levels
{
	/// <summary>
	/// Manages level-specific stuff.
	/// </summary>
	public class LevelManager : MonoBehaviour
	{
		public const string DEFAULT_LEVEL_SAVE_STATE_IDENTIFIER = "__default__"; // filename of default save state.
		public const string DEFAULT_LEVEL_SAVE_STATE_DISPLAYNAME = ""; // display name of default save state
		
		/// <summary>
		/// The time stamp of when the last level was loaded (in units of time elapsed since the game's launch) (Read only).
		/// </summary>
		public static float lastLoadTime { get; private set; }

		/// <summary>
		/// Returns true if a level is currently loaded.
		/// </summary>
		public static bool isLevelLoaded
		{
			get
			{
				return !string.IsNullOrEmpty( currentLevelId ) && !string.IsNullOrEmpty( currentLevelSaveStateId );
			}
		}


		/// <summary>
		/// Contains the identifier of the currently loaded level. null if no level is loaded (Read Only).
		/// </summary>
		public static string currentLevelId { get; private set; }

		/// <summary>
		/// Contains the display name of the currently loaded level. null if no level is loaded (Read Only).
		/// </summary>
		public static string currentLevelDisplayName { get; private set; }


		/// <summary>
		/// Contains the identifier of the currently loaded level save state. null if no level save state is loaded (Read Only).
		/// </summary>
		public static string currentLevelSaveStateId { get; private set; }

		/// <summary>
		/// Contains the display name of the currently loaded level save state. null if no level save state is loaded (Read Only).
		/// </summary>
		public static string currentLevelSaveStateDisplayName { get; private set; }



		private static Scene? loadedLevelScene;


		const char REPLACEMENT_CHAR = '_';
		static readonly char[] INVALID_CHARS = new char[] { ' ', '/', '\\', '?', '*', ':', '<', '>', '|' };
		/// <summary>
		/// Converts displayname into valid filename (removes special path characters).
		/// </summary>
		public static string IdentifierFromDisplayName( string displayName )
		{
			StringBuilder sb = new StringBuilder();

			for( int i = 0; i < displayName.Length; i++ )
			{
				bool isValid = true;
				for( int j = 0; j < INVALID_CHARS.Length; j++ )
				{
					if( displayName[i] == INVALID_CHARS[j] )
					{
						sb.Append( REPLACEMENT_CHAR );
						isValid = false;
						break;
					}
				}

				if( isValid )
				{
					sb.Append( displayName[i] );
				}
			}

			return sb.ToString();
		}


		/// <summary>
		/// Returns the path to the 'Levels' directory (Read Only).
		/// </summary>
		public static string levelsDirectoryPath
		{
			get
			{
				return Application.streamingAssetsPath + System.IO.Path.DirectorySeparatorChar + "Levels";
			}
		}

		/// <summary>
		/// Returns path to the level's main directory ('../Levels/levelIdentifier/').
		/// </summary>
		public static string GetLevelMainDirectory( string levelIdentifier )
		{
			return levelsDirectoryPath + System.IO.Path.DirectorySeparatorChar
				+ levelIdentifier;
		}

		/// <summary>
		/// Returns path to the level save state's main directory ('../Levels/levelIdentifier/SaveStates/levelSaveStateIdentifier/').
		/// </summary>
		public static string GetLevelSaveStateMainDirectory( string levelIdentifier, string levelSaveStateIdentifier )
		{
			return GetLevelMainDirectory( levelIdentifier ) + System.IO.Path.DirectorySeparatorChar + "SaveStates" + System.IO.Path.DirectorySeparatorChar
				+ levelSaveStateIdentifier;
		}


		/// <summary>
		/// Returns full data path (in the level's 'Data' directory).
		/// </summary>
		public static string GetFullDataPath( string levelIdentifier, string dataPath )
		{
			return GetLevelMainDirectory( levelIdentifier ) + System.IO.Path.DirectorySeparatorChar + "Data" + System.IO.Path.DirectorySeparatorChar + dataPath;
		}

		/// <summary>
		/// Returns full assets path (in the level's 'Assets' directory).
		/// </summary>
		public static string GetFullAssetsPath( string levelIdentifier, string assetsPath )
		{
			return GetLevelMainDirectory( levelIdentifier ) + System.IO.Path.DirectorySeparatorChar + "Assets" + System.IO.Path.DirectorySeparatorChar + assetsPath;
		}



		//
		//

		/// <summary>
		/// Returns full level path (in the level's main directory).
		/// </summary>
		public static string GetFullLevelPath( string levelIdentifier, string path )
		{
			return GetLevelMainDirectory( levelIdentifier ) + System.IO.Path.DirectorySeparatorChar + path;
		}

		/// <summary>
		/// Returns full level save state path (in the level save state's main directory).
		/// </summary>
		public static string GetFullLevelSaveStatePath( string levelIdentifier, string levelSaveStateIdentifier, string path )
		{
			return GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + path;
		}





		/// <summary>
		/// Returns every non-default save state of the specified level.
		/// </summary>
		/// <param name="levelDirectoryPath">The path to the level's root directory.</param>
		public static string[] GetAllCustomSaveStates( string levelIdentifier )
		{
			if( string.IsNullOrEmpty( levelIdentifier ) )
			{
				throw new System.ArgumentNullException( "Level identifier can't be null or empty." );
			}

			string[] directories = System.IO.Directory.GetDirectories( GetLevelMainDirectory( levelIdentifier ) );

			List<string> directoriesWithSaveStates = new List<string>();

			for( int i = 0; i < directories.Length; i++ )
			{
				string[] files = System.IO.Directory.GetFiles( directories[i] );

				for( int j = 0; j < files.Length; j++ )
				{
					if( System.IO.Path.GetFileName( files[j] ) == "save_state.kff" )
					{
						directoriesWithSaveStates.Add( directories[i] );
						break;
					}
				}
			}

			return directoriesWithSaveStates.ToArray();
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		// UNLOADING

		/// <summary>
		/// Unloads the level gameobjects from the scene.
		/// </summary>
		public static void UnloadLevel( bool loadMenu, Action onAfterLevelUnloaded )
		{
			//#warning incomplete.
			if( !isLevelLoaded )
			{
				throw new System.Exception( "There's no level loaded. You must load a level first." );
			}

			AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync( "Level - '" + currentLevelId + ":" + currentLevelSaveStateId + "'" );

			asyncOperation.completed += ( AsyncOperation oper ) =>
			{
				UnloadLevel_AfterAsync( loadMenu );
				onAfterLevelUnloaded?.Invoke();
			};
		}

		private static void UnloadLevel_AfterAsync( bool loadMenu )
		{
			DefinitionManager.Purge();
			AssetManager.Purge();
			AssetManager.sourceLevelId = null;
			Main.onHudLockChange.RemoveAllListeners();
			MouseOverHandler.onMouseEnter.RemoveAllListeners();
			MouseOverHandler.onMouseExit.RemoveAllListeners();
			MouseOverHandler.onMouseStay.RemoveAllListeners();

			Selection.Purge();
			AudioManager.StopSounds();

			if( loadMenu )
			{
				SceneManager.UnloadSceneAsync( "Level GUI" );
				SceneManager.LoadScene( "MainMenu", LoadSceneMode.Additive );
			}

			loadedLevelScene = null;
			currentLevelId = null;
			currentLevelSaveStateId = null;
		}

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
		// LOADING

		/// <summary>
		/// Loads the level into the scene. Uses the specified save state when loading.
		/// </summary>
		/// <param name="levelIdentifier">The level that is going to be loaded.</param>
		/// <param name="levelSaveStateIdentifier">The save state associated with the level. If null, the level's default save state will be used.</param>
		public static void LoadLevel( string levelIdentifier, string levelSaveStateIdentifier, Action onAfterSceneLoaded )
		{
			if( string.IsNullOrEmpty( levelIdentifier ) )
			{
				throw new Exception( "The level identifier can't be null or empty." );
			}
			if( isLevelLoaded )
			{
				throw new Exception( "There's already a level loaded. You must unload it first." );
			}

			if( levelSaveStateIdentifier == null ) // if specified save state == null, set to default save state.
			{
				levelSaveStateIdentifier = DEFAULT_LEVEL_SAVE_STATE_IDENTIFIER;
			}
			AssetManager.sourceLevelId = levelIdentifier;

			loadedLevelScene = SceneManager.CreateScene( "Level - '" + levelIdentifier + ":" + levelSaveStateIdentifier + "'" );
			SceneManager.SetActiveScene( loadedLevelScene.Value );

			Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Game Scene/World UI Canvas" ) );

			if( !SceneManager.GetSceneByName( "Level GUI" ).isLoaded )
			{
				SceneManager.LoadScene( "Level GUI", LoadSceneMode.Additive );
			}

			if( SceneManager.GetSceneByName( "MainMenu" ).isLoaded )
			{
				AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync( "MainMenu" );
				asyncOperation.completed += ( AsyncOperation oper ) =>
				{
					LoadLevel_AfterAsync( levelIdentifier, levelSaveStateIdentifier );
					onAfterSceneLoaded?.Invoke();
				};
			}
			else
			{
				LoadLevel_AfterAsync( levelIdentifier, levelSaveStateIdentifier );
				onAfterSceneLoaded?.Invoke();
			}
		}

		private static void LoadLevel_AfterAsync( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string pathLevel = GetLevelMainDirectory( levelIdentifier ) + System.IO.Path.DirectorySeparatorChar + "level.kff";
			string pathLevelSaveState = GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "level_save_state.kff";

			KFFSerializer serializerLevel;
			KFFSerializer serializerLevelSaveState;

			try
			{
				serializerLevel = KFFSerializer.ReadFromFile( pathLevel, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open level file '" + pathLevel + "' - file doesn't exist." );
			}
			
			try
			{
				serializerLevelSaveState = KFFSerializer.ReadFromFile( pathLevelSaveState, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open level file '" + pathLevel + "' - file doesn't exist." );
			}

			currentLevelDisplayName = serializerLevel.ReadString( "DisplayName" );
			currentLevelSaveStateDisplayName = serializerLevelSaveState.ReadString( "DisplayName" );

			// Set up the paths and serializers for definition files.

			string 
				pathUnits = GetFullDataPath( levelIdentifier, "units.kff" ),
				pathBuildings = GetFullDataPath( levelIdentifier, "buildings.kff" ),
				pathProjectiles = GetFullDataPath( levelIdentifier, "projectiles.kff" ),
				pathHeroes = GetFullDataPath( levelIdentifier, "heroes.kff" ),
				pathExtras = GetFullDataPath( levelIdentifier, "extras.kff" ),
				pathResourceDeposits = GetFullDataPath( levelIdentifier, "resource_deposits.kff" ),

				pathResources = GetFullDataPath( levelIdentifier, "resources.kff" ),
				pathTechnologies = GetFullDataPath( levelIdentifier, "technologies.kff" ),
				pathFactions = GetFullDataPath( levelIdentifier, "factions.kff" );

			KFFSerializer
				serializerUnits,
				serializerBuildings,
				serializerProjectiles,
				serializerHeroes,
				serializerExtras,
				serializerResourceDeposits,

				serializerResources,
				serializerTechnologies,
				
				serializerFactions;


			// Open the relevant definition files and parse their contents.

			try
			{
				serializerUnits = KFFSerializer.ReadFromFile( pathUnits, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open file '" + pathUnits + "' or file is invalid." );
			}

			try
			{
				serializerBuildings = KFFSerializer.ReadFromFile( pathBuildings, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open file '" + pathBuildings + "' or file is invalid." );
			}

			try
			{
				serializerProjectiles = KFFSerializer.ReadFromFile( pathProjectiles, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open file '" + pathProjectiles + "' or file is invalid." );
			}

			try
			{
				serializerHeroes = KFFSerializer.ReadFromFile( pathHeroes, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open file '" + pathHeroes + "' or file is invalid." );
			}

			try
			{
				serializerExtras = KFFSerializer.ReadFromFile( pathExtras, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open file '" + pathExtras + "' or file is invalid." );
			}

			try
			{
				serializerResourceDeposits = KFFSerializer.ReadFromFile( pathResourceDeposits, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open file '" + pathResourceDeposits + "' or file is invalid." );
			}


			try
			{
				serializerResources = KFFSerializer.ReadFromFile( pathResources, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open file '" + pathResources + "' or file is invalid." );
			}

			try
			{
				serializerTechnologies = KFFSerializer.ReadFromFile( pathTechnologies, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open file '" + pathTechnologies + "' or file is invalid." );
			}


			try
			{
				serializerFactions = KFFSerializer.ReadFromFile( pathFactions, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open file '" + pathFactions + "' or file is invalid." );
			}


			// Load the definitions using serializers.

			DefinitionManager.LoadUnitDefinitions( serializerUnits );
			DefinitionManager.LoadBuildingDefinitions( serializerBuildings );
			DefinitionManager.LoadProjectileDefinitions( serializerProjectiles );
			DefinitionManager.LoadHeroDefinitions( serializerHeroes );
			DefinitionManager.LoadExtraDefinitions( serializerExtras );
			DefinitionManager.LoadResourceDepositDefinitions( serializerResourceDeposits );

			DefinitionManager.LoadResourceDefinitions( serializerResources );
			DefinitionManager.LoadTechnologyDefinitions( serializerTechnologies );



			LevelDataManager.LoadMapData( serializerLevel );

			InstantiateLevelPrefabs(); // game UI prefabs

			CreateTerrain(); // create "env" organizational gameobject, and load terrain from files.

			LevelDataManager.LoadFactions( serializerFactions );
			LevelDataManager.LoadDaylightCycle( serializerLevel );



			// Load the save state


			// Set up the paths and serializers for definition files.

			string
				pathFactionData = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ), "save_factions.kff" ),
				
				pathSavedUnits = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ), "save_units.kff" ),
				pathSavedBuildings = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ), "save_buildings.kff" ),
				pathSavedProjectiles = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ), "save_projectiles.kff" ),
				pathSavedHeroes = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ), "save_heroes.kff" ),
				pathSavedExtras = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ), "save_extras.kff" ),
				pathSavedResourceDeposits = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ), "save_resource_deposits.kff" ),
				
				pathSelection = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ), "save_selection.kff" );

			KFFSerializer
				serializerFactionData,
				
				serializerSavedUnits,
				serializerSavedBuildings,
				serializerSavedProjectiles,
				serializerSavedHeroes,
				serializerSavedExtras,
				serializerSavedResourceDeposits,
				
				serializerSelection;


			// Open the relevant definition files and parse their contents.

			try
			{
				serializerFactionData = KFFSerializer.ReadFromFile( pathFactionData, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open file '" + pathFactionData + "' or file is invalid." );
			}


			try
			{
				serializerSavedUnits = KFFSerializer.ReadFromFile( pathSavedUnits, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open file '" + pathSavedUnits + "' or file is invalid." );
			}

			try
			{
				serializerSavedBuildings = KFFSerializer.ReadFromFile( pathSavedBuildings, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open file '" + pathSavedBuildings + "' or file is invalid." );
			}

			try
			{
				serializerSavedProjectiles = KFFSerializer.ReadFromFile( pathSavedProjectiles, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open file '" + pathSavedProjectiles + "' or file is invalid." );
			}

			try
			{
				serializerSavedHeroes = KFFSerializer.ReadFromFile( pathSavedHeroes, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open file '" + pathSavedHeroes + "' or file is invalid." );
			}

			try
			{
				serializerSavedExtras = KFFSerializer.ReadFromFile( pathSavedExtras, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open file '" + pathSavedExtras + "' or file is invalid." );
			}

			try
			{
				serializerSavedResourceDeposits = KFFSerializer.ReadFromFile( pathSavedResourceDeposits, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open file '" + pathSavedResourceDeposits + "' or file is invalid." );
			}

			try
			{
				serializerSelection = KFFSerializer.ReadFromFile( pathSelection, DefinitionManager.FILE_ENCODING );
			}
			catch( Exception )
			{
				throw new Exception( "Can't open file '" + pathSelection + "' or file is invalid." );
			}
		

			// Load the necessary things using serializers.
			
			var sUnits = GetSavedUnits( serializerSavedUnits );
			var sBuildings = GetSavedBuildings( serializerSavedBuildings );
			var sProjectiles = GetSavedProjectiles( serializerSavedProjectiles );
			var sHeroes = GetSavedHeroes( serializerSavedHeroes );
			var sExtras = GetSavedExtras( serializerSavedExtras );
			var sResourceDeposits = GetSavedResourceDeposits( serializerSavedResourceDeposits );

			LevelDataManager.LoadFactionData( serializerFactionData );
			LevelDataManager.LoadDaylightCycleData( serializerLevelSaveState );
			LevelDataManager.LoadCameraData( serializerLevelSaveState );

			GameObject[] units = new GameObject[sUnits.Count];
			GameObject[] buildings = new GameObject[sBuildings.Count];
			GameObject[] projectiles = new GameObject[sProjectiles.Count];
			GameObject[] heroes = new GameObject[sHeroes.Count];
			GameObject[] extras = new GameObject[sExtras.Count];
			GameObject[] resourceDeposits = new GameObject[sResourceDeposits.Count];

			// Spawn every object on the map (no data present yet, because that might need other objects's guids).

			for( int i = 0; i < units.Length; i++ )
			{
				units[i] = UnitCreator.CreateEmpty( sUnits[i].Item2.guid, sUnits[i].Item1 );
			}
			for( int i = 0; i < buildings.Length; i++ )
			{
				buildings[i] = BuildingCreator.CreateEmpty( sBuildings[i].Item2.guid, sBuildings[i].Item1 );
			}
			for( int i = 0; i < projectiles.Length; i++ )
			{
				projectiles[i] = ProjectileCreator.CreateEmpty( sProjectiles[i].Item2.guid, sProjectiles[i].Item1 );
			}
			for( int i = 0; i < heroes.Length; i++ )
			{
				heroes[i] = HeroCreator.CreateEmpty( sHeroes[i].Item2.guid, sHeroes[i].Item1 );
			}
			for( int i = 0; i < extras.Length; i++ )
			{
				extras[i] = ExtraCreator.CreateEmpty( sExtras[i].Item2.guid, sExtras[i].Item1 );
			}
			for( int i = 0; i < resourceDeposits.Length; i++ )
			{
				resourceDeposits[i] = ResourceDepositCreator.CreateEmpty( sResourceDeposits[i].Item2.guid, sResourceDeposits[i].Item1 );
			}

			// Set the data (guids stay the same).

			for( int i = 0; i < units.Length; i++ )
			{
				UnitCreator.SetData( units[i], sUnits[i].Item2 );
			}
			for( int i = 0; i < buildings.Length; i++ )
			{
				BuildingCreator.SetData( buildings[i], sBuildings[i].Item2 );
			}
			for( int i = 0; i < projectiles.Length; i++ )
			{
				ProjectileCreator.SetData( projectiles[i], sProjectiles[i].Item2 );
			}
			for( int i = 0; i < heroes.Length; i++ )
			{
				HeroCreator.SetData( heroes[i], sHeroes[i].Item2 );
			}
			for( int i = 0; i < extras.Length; i++ )
			{
				ExtraCreator.SetData( extras[i], sExtras[i].Item2 );
			}
			for( int i = 0; i < resourceDeposits.Length; i++ )
			{
				ResourceDepositCreator.SetData( resourceDeposits[i], sResourceDeposits[i].Item2 );
			}

			SelectionPanelMode selectionPanelMode;
			GameObject highlighted;
			GameObject[] selected = GetSelected( serializerSelection, out highlighted, out selectionPanelMode );

			SelectionPanel.instance.SetMode( selectionPanelMode );

			// Set inactive.
			SelectionPanel.instance.gameObject.SetActive( false );
			ActionPanel.instance.gameObject.SetActive( false );
			// ^ This will be set active afterwards, if something gets selected.

			// Select the objects specified by save state
			if( selected != null )
			{
				for( int i = 0; i < selected.Length; i++ )
				{
					Selection.SelectAndHighlight( selected[i].GetComponent<Selectable>() );
				}
			}
			if( highlighted != null )
			{
				Selection.HighlightSelected( highlighted.GetComponent<Selectable>() );
			}


			currentLevelId = levelIdentifier;
			currentLevelSaveStateId = levelSaveStateIdentifier;

			lastLoadTime = Time.time;
		}

		private static List<Tuple<UnitDefinition, UnitData>> GetSavedUnits( KFFSerializer serializer )
		{
			List<Tuple<UnitDefinition, UnitData>> ret = new List<Tuple<UnitDefinition, UnitData>>();

			int count = serializer.Analyze( "List" ).childCount;
			for( int i = 0; i < count; i++ )
			{
				UnitData data = new UnitData();
				serializer.Deserialize( new Path( "List.{0}.Data", i ), data );

				string defId = serializer.ReadString( new Path( "List.{0}.DefinitionId", i ) );
				UnitDefinition def = DefinitionManager.GetUnit( defId );

				ret.Add( new Tuple<UnitDefinition, UnitData>( def, data ) );
			}

			return ret;
		}

		private static List<Tuple<BuildingDefinition, BuildingData>> GetSavedBuildings( KFFSerializer serializer )
		{
			List<Tuple<BuildingDefinition, BuildingData>> ret = new List<Tuple<BuildingDefinition, BuildingData>>();

			int count = serializer.Analyze( "List" ).childCount;
			for( int i = 0; i < count; i++ )
			{
				BuildingData data = new BuildingData();
				serializer.Deserialize( new Path( "List.{0}.Data", i ), data );

				string defId = serializer.ReadString( new Path( "List.{0}.DefinitionId", i ) );
				BuildingDefinition def = DefinitionManager.GetBuilding( defId );

				ret.Add( new Tuple<BuildingDefinition, BuildingData>( def, data ) );
			}

			return ret;
		}

		private static List<Tuple<ProjectileDefinition, ProjectileData>> GetSavedProjectiles( KFFSerializer serializer )
		{
			List<Tuple<ProjectileDefinition, ProjectileData>> ret = new List<Tuple<ProjectileDefinition, ProjectileData>>();

			int count = serializer.Analyze( "List" ).childCount;
			for( int i = 0; i < count; i++ )
			{
				ProjectileData data = new ProjectileData();
				serializer.Deserialize( new Path( "List.{0}.Data", i ), data );

				string defId = serializer.ReadString( new Path( "List.{0}.DefinitionId", i ) );
				ProjectileDefinition def = DefinitionManager.GetProjectile( defId );

				ret.Add( new Tuple<ProjectileDefinition, ProjectileData>( def, data ) );
			}

			return ret;
		}

		private static List<Tuple<HeroDefinition, HeroData>> GetSavedHeroes( KFFSerializer serializer )
		{
			List<Tuple<HeroDefinition, HeroData>> ret = new List<Tuple<HeroDefinition, HeroData>>();

			int count = serializer.Analyze( "List" ).childCount;
			for( int i = 0; i < count; i++ )
			{
				HeroData data = new HeroData();
				serializer.Deserialize( new Path( "List.{0}.Data", i ), data );

				string defId = serializer.ReadString( new Path( "List.{0}.DefinitionId", i ) );
				HeroDefinition def = DefinitionManager.GetHero( defId );

				ret.Add( new Tuple<HeroDefinition, HeroData>( def, data ) );
			}

			return ret;
		}

		private static List<Tuple<ExtraDefinition, ExtraData>> GetSavedExtras( KFFSerializer serializer )
		{
			List<Tuple<ExtraDefinition, ExtraData>> ret = new List<Tuple<ExtraDefinition, ExtraData>>();

			int count = serializer.Analyze( "List" ).childCount;
			for( int i = 0; i < count; i++ )
			{
				ExtraData data = new ExtraData();
				serializer.Deserialize( new Path( "List.{0}.Data", i ), data );

				string defId = serializer.ReadString( new Path( "List.{0}.DefinitionId", i ) );
				ExtraDefinition def = DefinitionManager.GetExtra( defId );

				ret.Add( new Tuple<ExtraDefinition, ExtraData>( def, data ) );
			}

			return ret;
		}

		private static List<Tuple<ResourceDepositDefinition, ResourceDepositData>> GetSavedResourceDeposits( KFFSerializer serializer )
		{
			List<Tuple<ResourceDepositDefinition, ResourceDepositData>> ret = new List<Tuple<ResourceDepositDefinition, ResourceDepositData>>();

			int count = serializer.Analyze( "List" ).childCount;
			for( int i = 0; i < count; i++ )
			{
				ResourceDepositData data = new ResourceDepositData();
				serializer.Deserialize( new Path( "List.{0}.Data", i ), data );

				string defId = serializer.ReadString( new Path( "List.{0}.DefinitionId", i ) );
				ResourceDepositDefinition def = DefinitionManager.GetResourceDeposit( defId );

				ret.Add( new Tuple<ResourceDepositDefinition, ResourceDepositData>( def, data ) );
			}

			return ret;
		}

		private static GameObject[] GetSelected( KFFSerializer serializer, out GameObject highlight, out SelectionPanelMode selectionPanelMode )
		{
			string sel = serializer.ReadString( "SelectionPanelMode" );
			if( sel == "Object" )
			{
				selectionPanelMode = SelectionPanelMode.Object;
			}
			else if( sel == "List" )
			{
				selectionPanelMode = SelectionPanelMode.List;
			}
			else
			{
				throw new Exception( "Invalid Selection Panel Mode: '" + sel + "'." );
			}
			if( serializer.Analyze( "SelectedGuids" ).isFail )
			{
				highlight = null;
				return null;
			}

			int count = serializer.Analyze( "SelectedGuids" ).childCount;

			GameObject[] ret = new GameObject[count];

			if( serializer.Analyze( "HighlightedGuid" ).isFail )
			{
				highlight = null;
			}
			else
			{
				highlight = Main.GetGameObject( Guid.ParseExact( serializer.ReadString( "HighlightedGuid" ), "D" ) );
			}


			for( int i = 0; i < count; i++ )
			{
				ret[i] = Main.GetGameObject( Guid.ParseExact( serializer.ReadString( new Path( "SelectedGuids.{0}", i ) ), "D" ) );
				
			}

			return ret;
		}



		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		private static void CreateTerrain()
		{
			//const int size = 4; // the size of the map (in chunks).

			GameObject environment = new GameObject( "Environment" );

			LevelTerrainCreator.terrainParent = environment.transform;
			Texture2D[,] color = new Texture2D[LevelDataManager.mapSegments, LevelDataManager.mapSegments];
			for( int i = 0; i < LevelDataManager.mapSegments; i++ )
			{
				for( int j = 0; j < LevelDataManager.mapSegments; j++ )
				{
					color[i, j] = AssetManager.GetTexture2D( AssetManager.EXTERN_ASSET_IDENTIFIER + "Colormap/row-" + (LevelDataManager.mapSegments - j) + "-col-" + (i + 1) + ".png", TextureType.Color );
				}
			}
			Texture2D[,] height = new Texture2D[LevelDataManager.mapSegments, LevelDataManager.mapSegments];
			for( int i = 0; i < LevelDataManager.mapSegments; i++ )
			{
				for( int j = 0; j < LevelDataManager.mapSegments; j++ )
				{
					height[i, j] = AssetManager.GetTexture2D( AssetManager.EXTERN_ASSET_IDENTIFIER + "Heightmap/row-" + (LevelDataManager.mapSegments - j) + "-col-" + (i + 1) + ".png", TextureType.Color );
				}
			}
			LevelTerrainCreator.SpawnMap( height, color, LevelDataManager.mapHeight );
			LevelTerrainCreator.UpdateNavMesh();
		}

		private static void InstantiateLevelPrefabs()
		{
			Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Game Scene/__ GAME MANAGER __" ), Vector3.zero, Quaternion.identity );
			Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Game Scene/Daylight Cycle" ), Vector3.zero, Quaternion.identity );
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		// SAVING


		/// <summary>
		/// Saves the scene to the specified level save state (level itself is the currently loaded ofc).
		/// </summary>
		/// <param name="saveSettings">The additional settings that define the behaviour of the method.</param>
		public static void SaveScene( string newLevelSaveStateDisplayName, string newLevelSaveStateId )
		{
			Unit[] units = Unit.GetAllUnits();
			Building[] buildings = Building.GetAllBuildings();
			Projectile[] projectiles = Projectile.GetAllProjectiles();
			Hero[] heroes = Hero.GetAllHeroes();
			Extra[] extras = Extra.GetAllExtras();
			ResourceDeposit[] resourceDeposits = ResourceDeposit.GetAllResourceDeposits();

			Tuple<string, UnitData>[] unitData = new Tuple<string, UnitData>[units.Length];
			Tuple<string, BuildingData> [] buildingData = new Tuple<string, BuildingData>[buildings.Length];
			Tuple<string, ProjectileData> [] projectileData = new Tuple<string, ProjectileData>[projectiles.Length];
			Tuple<string, HeroData> [] heroData = new Tuple<string, HeroData>[heroes.Length];
			Tuple<string, ExtraData> [] extraData = new Tuple<string, ExtraData>[extras.Length];
			Tuple<string, ResourceDepositData> [] resourceDepositData = new Tuple<string, ResourceDepositData>[resourceDeposits.Length];

			for( int i = 0; i < unitData.Length; i++ )
			{
				unitData[i] = new Tuple<string, UnitData>( UnitCreator.GetDefinitionId( units[i].gameObject ), UnitCreator.GetData( units[i].gameObject ) );
			}
			for( int i = 0; i < buildingData.Length; i++ )
			{
				buildingData[i] = new Tuple<string, BuildingData>( BuildingCreator.GetDefinitionId( buildings[i].gameObject ), BuildingCreator.GetData( buildings[i].gameObject ) );
			}
			for( int i = 0; i < projectileData.Length; i++ )
			{
				projectileData[i] = new Tuple<string, ProjectileData>( ProjectileCreator.GetDefinitionId( projectiles[i].gameObject ), ProjectileCreator.GetData( projectiles[i].gameObject ) );
			}
			for( int i = 0; i < heroData.Length; i++ )
			{
				heroData[i] = new Tuple<string, HeroData>( HeroCreator.GetDefinitionId( heroes[i].gameObject ), HeroCreator.GetData( heroes[i].gameObject ) );
			}
			for( int i = 0; i < extraData.Length; i++ )
			{
				extraData[i] = new Tuple<string, ExtraData>( ExtraCreator.GetDefinitionId( extras[i].gameObject ), ExtraCreator.GetData( extras[i].gameObject ) );
			}
			for( int i = 0; i < resourceDepositData.Length; i++ )
			{
				resourceDepositData[i] = new Tuple<string, ResourceDepositData>( ResourceDepositCreator.GetDefinitionId( resourceDeposits[i].gameObject ), ResourceDepositCreator.GetData( resourceDeposits[i].gameObject ) );
			}

			string path = GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId );
			if( !System.IO.Directory.Exists( path ) )
			{
				System.IO.Directory.CreateDirectory( path );
			}

			// Save the level save state file.

			string levelSaveStateFilePath = System.IO.Path.Combine( LevelManager.GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId ), "level_save_state.kff" );
			KFFSerializer serializerSaveState = new KFFSerializer( new KFFFile( levelSaveStateFilePath ) );


			string
				pathFactionData = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId ), "save_factions.kff" ),
				pathSavedUnits = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId ), "save_units.kff" ),
				pathSavedBuildings = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId ), "save_buildings.kff" ),
				pathSavedProjectiles = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId ), "save_projectiles.kff" ),
				pathSavedHeroes = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId ), "save_heroes.kff" ),
				pathSavedExtras = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId ), "save_extras.kff" ),
				pathSavedResourceDeposits = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId ), "save_resource_deposits.kff" ),
				pathSelection = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId ), "save_selection.kff" );

			KFFSerializer 
				serializerFactionData = new KFFSerializer( new KFFFile( pathFactionData ) ),
				serializerSavedUnits = new KFFSerializer( new KFFFile( pathSavedUnits ) ),
				serializerSavedBuildings = new KFFSerializer( new KFFFile( pathSavedBuildings ) ),
				serializerSavedProjectiles = new KFFSerializer( new KFFFile( pathSavedProjectiles ) ),
				serializerSavedHeroes = new KFFSerializer( new KFFFile( pathSavedHeroes ) ),
				serializerSavedExtras = new KFFSerializer( new KFFFile( pathSavedExtras ) ),
				serializerSavedResourceDeposits = new KFFSerializer( new KFFFile( pathSavedResourceDeposits ) ),
				
				serializerSelection = new KFFSerializer( new KFFFile( pathSelection ) );

			// Serialize the data into serializers.

			serializerSaveState.WriteString( "", "DisplayName", newLevelSaveStateDisplayName );
			LevelDataManager.SaveDaylightCycleData( serializerSaveState );
			LevelDataManager.SaveCameraData( serializerSaveState );

			LevelDataManager.SaveFactionData( serializerFactionData );

			SaveUnits( unitData, serializerSavedUnits );
			SaveBuildings( buildingData, serializerSavedBuildings );
			SaveProjectiles( projectileData, serializerSavedProjectiles );
			SaveHeroes( heroData, serializerSavedHeroes );
			SaveExtras( extraData, serializerSavedExtras );
			SaveResourceDeposits( resourceDepositData, serializerSavedResourceDeposits );

			Guid? highlighted = null;
			if( Selection.highlightedObject != null )
			{
				highlighted = Main.GetGuid( Selection.highlightedObject.gameObject );
			}
			Guid?[] selection = null;
			var selectedObjs = Selection.selectedObjects;
			if( selectedObjs.Length > 0 )
			{
				selection = new Guid?[selectedObjs.Length];

				for( int i = 0; i < selectedObjs.Length; i++ )
				{
					selection[i] = Main.GetGuid( selectedObjs[i].gameObject );
				}
			}

			SaveSelection( highlighted, selection, serializerSelection );


			// Write the data in serializers to the respective files.

			serializerSaveState.WriteToFile( levelSaveStateFilePath, DefinitionManager.FILE_ENCODING );

			serializerFactionData.WriteToFile( pathFactionData, DefinitionManager.FILE_ENCODING );

			serializerSavedUnits.WriteToFile( pathSavedUnits, DefinitionManager.FILE_ENCODING );
			serializerSavedBuildings.WriteToFile( pathSavedBuildings, DefinitionManager.FILE_ENCODING );
			serializerSavedProjectiles.WriteToFile( pathSavedProjectiles, DefinitionManager.FILE_ENCODING );
			serializerSavedHeroes.WriteToFile( pathSavedHeroes, DefinitionManager.FILE_ENCODING );
			serializerSavedExtras.WriteToFile( pathSavedExtras, DefinitionManager.FILE_ENCODING );
			serializerSavedResourceDeposits.WriteToFile( pathSavedResourceDeposits, DefinitionManager.FILE_ENCODING );
			
			serializerSelection.WriteToFile( pathSelection, DefinitionManager.FILE_ENCODING );
		}

		private static void SaveUnits( Tuple<string, UnitData>[] units, KFFSerializer serializer )
		{
			serializer.WriteList( "", "List" );
			for( int i = 0; i < units.Length; i++ )
			{
				serializer.AppendClass( "List" );

				serializer.WriteString( new Path( "List.{0}", i ), "DefinitionId", units[i].Item1 );

				serializer.Serialize( new Path( "List.{0}", i ), "Data", units[i].Item2 );
			}
		}

		private static void SaveBuildings( Tuple<string, BuildingData>[] buildings, KFFSerializer serializer )
		{
			serializer.WriteList( "", "List" );
			for( int i = 0; i < buildings.Length; i++ )
			{
				serializer.AppendClass( "List" );

				serializer.WriteString( new Path( "List.{0}", i ), "DefinitionId", buildings[i].Item1 );

				serializer.Serialize( new Path( "List.{0}", i ), "Data", buildings[i].Item2 );
			}
		}

		private static void SaveProjectiles( Tuple<string, ProjectileData>[] projectiles, KFFSerializer serializer )
		{
			serializer.WriteList( "", "List" );
			for( int i = 0; i < projectiles.Length; i++ )
			{
				serializer.AppendClass( "List" );

				serializer.WriteString( new Path( "List.{0}", i ), "DefinitionId", projectiles[i].Item1 );

				serializer.Serialize( new Path( "List.{0}", i ), "Data", projectiles[i].Item2 );
			}
		}

		private static void SaveHeroes( Tuple<string, HeroData>[] heroes, KFFSerializer serializer )
		{
			serializer.WriteList( "", "List" );
			for( int i = 0; i < heroes.Length; i++ )
			{
				serializer.AppendClass( "List" );

				serializer.WriteString( new Path( "List.{0}", i ), "DefinitionId", heroes[i].Item1 );

				serializer.Serialize( new Path( "List.{0}", i ), "Data", heroes[i].Item2 );
			}
		}

		private static void SaveExtras( Tuple<string, ExtraData>[] extras, KFFSerializer serializer )
		{
			serializer.WriteList( "", "List" );
			for( int i = 0; i < extras.Length; i++ )
			{
				serializer.AppendClass( "List" );

				serializer.WriteString( new Path( "List.{0}", i ), "DefinitionId", extras[i].Item1 );

				serializer.Serialize( new Path( "List.{0}", i ), "Data", extras[i].Item2 );
			}
		}

		private static void SaveResourceDeposits( Tuple<string, ResourceDepositData>[] resourceDeposits, KFFSerializer serializer )
		{
			serializer.WriteList( "", "List" );
			for( int i = 0; i < resourceDeposits.Length; i++ )
			{
				serializer.AppendClass( "List" );

				serializer.WriteString( new Path( "List.{0}", i ), "DefinitionId", resourceDeposits[i].Item1 );

				serializer.Serialize( new Path( "List.{0}", i ), "Data", resourceDeposits[i].Item2 );
			}
		}




		private static void SaveSelection( Guid? highlighted, Guid?[] selected, KFFSerializer serializer )
		{
			serializer.WriteString( "", "SelectionPanelMode", SelectionPanel.instance.mode == SelectionPanelMode.Object ? "Object" : "List" );
			if( highlighted != null )
			{
				serializer.WriteString( "", "HighlightedGuid", highlighted.Value.ToString( "D" ) );
			}
			serializer.WriteList( "", "List" );
			if( selected != null )
			{
				string[] selectedGuidStrings = new string[selected.Length];
				for( int i = 0; i < selected.Length; i++ )
				{
					selectedGuidStrings[i] = selected[i].Value.ToString( "D" );
				}
				for( int i = 0; i < selected.Length; i++ )
				{
					serializer.WriteStringArray( "", "SelectedGuids", selectedGuidStrings );
				}
			}
		}
	}
}