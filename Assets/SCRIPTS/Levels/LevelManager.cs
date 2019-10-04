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
			KFFSerializer serializerL = KFFSerializer.ReadFromFile( GetLevelMainDirectory( levelIdentifier ) + System.IO.Path.DirectorySeparatorChar + "level.kff", DefinitionManager.FILE_ENCODING );
			currentLevelDisplayName = serializerL.ReadString( "DisplayName" );

			KFFSerializer serializerLSS = KFFSerializer.ReadFromFile( GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "level_save_state.kff", DefinitionManager.FILE_ENCODING );
			currentLevelSaveStateDisplayName = serializerLSS.ReadString( "DisplayName" );
			

			DefinitionManager.LoadUnitDefinitions( levelIdentifier );
			DefinitionManager.LoadBuildingDefinitions( levelIdentifier );
			DefinitionManager.LoadProjectileDefinitions( levelIdentifier );
			DefinitionManager.LoadHeroDefinitions( levelIdentifier );
			DefinitionManager.LoadExtraDefinitions( levelIdentifier );
			DefinitionManager.LoadResourceDepositDefinitions( levelIdentifier );

			DefinitionManager.LoadResourceDefinitions( levelIdentifier );
			DefinitionManager.LoadTechnologyDefinitions( levelIdentifier );
			
			InstantiateLevelPrefabs(); // game UI prefabs

			CreateTerrain(); // create "env" organizational gameobject, and load terrain from files.

			LevelDataManager.LoadFactions( levelIdentifier );
			LevelDataManager.LoadDaylightCycle( serializerL );


			LevelDataManager.LoadFactionData( levelIdentifier, levelSaveStateIdentifier );
			LevelDataManager.LoadDaylightCycleData( serializerLSS );
			LevelDataManager.LoadCameraData( serializerLSS );
			
			// apply save state

			var sUnits = GetSavedUnits( levelIdentifier, levelSaveStateIdentifier );
			var sBuildings = GetSavedBuildings( levelIdentifier, levelSaveStateIdentifier );
			var sProjectiles = GetSavedProjectiles( levelIdentifier, levelSaveStateIdentifier );
			var sHeroes = GetSavedHeroes( levelIdentifier, levelSaveStateIdentifier );
			var sExtras = GetSavedExtras( levelIdentifier, levelSaveStateIdentifier );
			var sResourceDeposits = GetSavedResourceDeposits( levelIdentifier, levelSaveStateIdentifier );

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
			GameObject[] selected = GetSelected( levelIdentifier, levelSaveStateIdentifier, out highlighted, out selectionPanelMode );

			SelectionPanel.instance.SetMode( selectionPanelMode );

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

		private static List<Tuple<UnitDefinition, UnitData>> GetSavedUnits( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_units.kff";

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );

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

		private static List<Tuple<BuildingDefinition, BuildingData>> GetSavedBuildings( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_buildings.kff";

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );

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

		private static List<Tuple<ProjectileDefinition, ProjectileData>> GetSavedProjectiles( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_projectiles.kff";

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );

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

		private static List<Tuple<HeroDefinition, HeroData>> GetSavedHeroes( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_heroes.kff";

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );

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

		private static List<Tuple<ExtraDefinition, ExtraData>> GetSavedExtras( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_extras.kff";

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );

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

		private static List<Tuple<ResourceDepositDefinition, ResourceDepositData>> GetSavedResourceDeposits( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_resource_deposits.kff";

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );

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

		private static GameObject[] GetSelected( string levelIdentifier, string levelSaveStateIdentifier, out GameObject highlight, out SelectionPanelMode selectionPanelMode )
		{
			string path = GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_selection.kff";

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );

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
			const int size = 4; // the size of the map (in chunks).

			GameObject environment = new GameObject( "Environment" );

			LevelTerrainCreator.terrainParent = environment.transform;
			Texture2D[,] color = new Texture2D[size, size];
			for( int i = 0; i < size; i++ )
			{
				for( int j = 0; j < size; j++ )
				{
					color[i, j] = AssetManager.GetTexture2D( AssetManager.EXTERN_ASSET_IDENTIFIER + "Colormap/row-" + (size - j) + "-col-" + (i + 1) + ".png", Katniss.Utils.TextureType.Color );
				}
			}
			Texture2D[,] height = new Texture2D[size, size];
			for( int i = 0; i < size; i++ )
			{
				for( int j = 0; j < size; j++ )
				{
					height[i, j] = AssetManager.GetTexture2D( AssetManager.EXTERN_ASSET_IDENTIFIER + "Heightmap/row-" + (size - j) + "-col-" + (i + 1) + ".png", Katniss.Utils.TextureType.Color );
				}
			}
			LevelTerrainCreator.SpawnMap( height, color, 6f );
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

			SaveUnits( unitData, currentLevelId, newLevelSaveStateId );
			SaveBuildings( buildingData, currentLevelId, newLevelSaveStateId );
			SaveProjectiles( projectileData, currentLevelId, newLevelSaveStateId );
			SaveHeroes( heroData, currentLevelId, newLevelSaveStateId );
			SaveExtras( extraData, currentLevelId, newLevelSaveStateId );
			SaveResourceDeposits( resourceDepositData, currentLevelId, newLevelSaveStateId );
			
			LevelDataManager.SaveFactionData( currentLevelId, newLevelSaveStateId );

			string levelSaveStateFilePath = LevelManager.GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId ) + System.IO.Path.DirectorySeparatorChar + "level_save_state.kff";
			KFFSerializer serializer = new KFFSerializer( new KFFFile( levelSaveStateFilePath ) );

			serializer.WriteString( "", "DisplayName", newLevelSaveStateDisplayName );
			LevelDataManager.SaveDaylightCycleData( serializer );
			LevelDataManager.SaveCameraData( serializer );

			serializer.WriteToFile( levelSaveStateFilePath, DefinitionManager.FILE_ENCODING );

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
			
			SaveSelection( highlighted, selection, currentLevelId, newLevelSaveStateId );
		}

		private static void SaveUnits( Tuple<string, UnitData>[] units, string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_units.kff";

			KFFSerializer serializer = new KFFSerializer( new KFFFile( path ) );

			serializer.WriteList( "", "List" );
			for( int i = 0; i < units.Length; i++ )
			{
				serializer.AppendClass( "List" );

				serializer.WriteString( new Path( "List.{0}", i ), "DefinitionId", units[i].Item1 );

				serializer.Serialize( new Path( "List.{0}", i ), "Data", units[i].Item2 );
			}

			serializer.WriteToFile( path, DefinitionManager.FILE_ENCODING );
		}

		private static void SaveBuildings( Tuple<string, BuildingData>[] buildings, string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_buildings.kff";

			KFFSerializer serializer = new KFFSerializer( new KFFFile( path ) );

			serializer.WriteList( "", "List" );
			for( int i = 0; i < buildings.Length; i++ )
			{
				serializer.AppendClass( "List" );

				serializer.WriteString( new Path( "List.{0}", i ), "DefinitionId", buildings[i].Item1 );

				serializer.Serialize( new Path( "List.{0}", i ), "Data", buildings[i].Item2 );
			}

			serializer.WriteToFile( path, DefinitionManager.FILE_ENCODING );
		}

		private static void SaveProjectiles( Tuple<string, ProjectileData>[] projectiles, string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_projectiles.kff";

			KFFSerializer serializer = new KFFSerializer( new KFFFile( path ) );

			serializer.WriteList( "", "List" );
			for( int i = 0; i < projectiles.Length; i++ )
			{
				serializer.AppendClass( "List" );

				serializer.WriteString( new Path( "List.{0}", i ), "DefinitionId", projectiles[i].Item1 );

				serializer.Serialize( new Path( "List.{0}", i ), "Data", projectiles[i].Item2 );
			}

			serializer.WriteToFile( path, DefinitionManager.FILE_ENCODING );
		}

		private static void SaveHeroes( Tuple<string, HeroData>[] heroes, string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_heroes.kff";

			KFFSerializer serializer = new KFFSerializer( new KFFFile( path ) );

			serializer.WriteList( "", "List" );
			for( int i = 0; i < heroes.Length; i++ )
			{
				serializer.AppendClass( "List" );

				serializer.WriteString( new Path( "List.{0}", i ), "DefinitionId", heroes[i].Item1 );

				serializer.Serialize( new Path( "List.{0}", i ), "Data", heroes[i].Item2 );
			}

			serializer.WriteToFile( path, DefinitionManager.FILE_ENCODING );
		}

		private static void SaveExtras( Tuple<string, ExtraData>[] extras, string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_extras.kff";

			KFFSerializer serializer = new KFFSerializer( new KFFFile( path ) );

			serializer.WriteList( "", "List" );
			for( int i = 0; i < extras.Length; i++ )
			{
				serializer.AppendClass( "List" );

				serializer.WriteString( new Path( "List.{0}", i ), "DefinitionId", extras[i].Item1 );

				serializer.Serialize( new Path( "List.{0}", i ), "Data", extras[i].Item2 );
			}

			serializer.WriteToFile( path, DefinitionManager.FILE_ENCODING );
		}

		private static void SaveResourceDeposits( Tuple<string, ResourceDepositData>[] resourceDeposits, string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_resource_deposits.kff";

			KFFSerializer serializer = new KFFSerializer( new KFFFile( path ) );

			serializer.WriteList( "", "List" );
			for( int i = 0; i < resourceDeposits.Length; i++ )
			{
				serializer.AppendClass( "List" );

				serializer.WriteString( new Path( "List.{0}", i ), "DefinitionId", resourceDeposits[i].Item1 );

				serializer.Serialize( new Path( "List.{0}", i ), "Data", resourceDeposits[i].Item2 );
			}

			serializer.WriteToFile( path, DefinitionManager.FILE_ENCODING );
		}




		private static void SaveSelection( Guid? highlighted, Guid?[] selected, string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_selection.kff";

			KFFSerializer serializer = new KFFSerializer( new KFFFile( path ) );

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
			serializer.WriteToFile( path, DefinitionManager.FILE_ENCODING );
		}
	}
}