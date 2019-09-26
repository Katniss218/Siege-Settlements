using KFF;
using SS.Buildings;
using SS.Content;
using SS.Extras;
using SS.Heroes;
using SS.Levels.SaveStates;
using SS.Projectiles;
using SS.TerrainCreation;
using SS.Units;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SS.Levels
{
	/// <summary>
	/// Manages level-specific stuff.
	/// </summary>
	public class LevelManager
	{
		public const string DEFAULT_LEVEL_SAVE_STATE_IDENTIFIER = "__default__"; // filename of default save state.
		public const string DEFAULT_LEVEL_SAVE_STATE_DISPLAYNAME = ""; // display name of default save state
		/// <summary>
		/// The time stamp of when the last level was loaded (in units of time elapsed since the game's launch) (Read only).
		/// </summary>
		public static float lastLoadTime { get; private set; }
		
		public static bool isLevelLoaded
		{
			get
			{
				return !string.IsNullOrEmpty( currentLevelId ) && !string.IsNullOrEmpty( currentLevelSaveStateId );
			}
		}
		
		public static string currentLevelId { get; private set; }
		public static string currentLevelDisplayName { get; private set; }

		public static string currentLevelSaveStateId { get; private set; }
		public static string currentLevelSaveStateDisplayName { get; private set; }

		
		const char REPLACEMENT_CHAR = '_';
		static readonly char[] INVALID_CHARS = new char[] { '/', '\\', '?', '*', ':', '<', '>', '|' };
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
		/// Returns the path to the "GameData" directory (Read Only).
		/// </summary>
		public static string levelDirectoryPath
		{
			get
			{
				return Application.streamingAssetsPath + System.IO.Path.DirectorySeparatorChar + "Levels";
			}
		}

		public static string GetLevelPath( string levelIdentifier )
		{
			return Application.streamingAssetsPath + System.IO.Path.DirectorySeparatorChar + "Levels" + System.IO.Path.DirectorySeparatorChar
				+ levelIdentifier;
		}

		public static string GetFullDataPath( string levelIdentifier, string dataPath )
		{
			return Application.streamingAssetsPath + System.IO.Path.DirectorySeparatorChar + "Levels" + System.IO.Path.DirectorySeparatorChar
				+ levelIdentifier + System.IO.Path.DirectorySeparatorChar + dataPath;
		}

		public static string GetFullAssetsPath( string levelIdentifier, string assetsPath )
		{
			return Application.streamingAssetsPath + System.IO.Path.DirectorySeparatorChar + "Levels" + System.IO.Path.DirectorySeparatorChar
				+ levelIdentifier + System.IO.Path.DirectorySeparatorChar + assetsPath;
		}

		public static string GetLevelSaveStatePath( string levelIdentifier, string levelSaveStateIdentifier )
		{
			return GetLevelPath( levelIdentifier ) + System.IO.Path.DirectorySeparatorChar + "SaveStates" + System.IO.Path.DirectorySeparatorChar
				+ levelSaveStateIdentifier;
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

			string[] directories = System.IO.Directory.GetDirectories( GetLevelPath( levelIdentifier ) );

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
		public static void UnloadLevel()
		{
//#warning incomplete.
			if( !isLevelLoaded )
			{
				throw new System.Exception( "There's no level loaded. You must load a level first." );
			}
//#warning unload previous scene (destroy all level gameobjects)
//#warning destroy and unload terrain.
//#warning unload definitions and assets from the managers.

			UnityEngine.SceneManagement.SceneManager.LoadScene( "MainMenu" );
			
			currentLevelId = null;
			currentLevelSaveStateId = null;
		}


		private static void DeleteMainMenuUI()
		{
			// Find every gameObject in the scene.
			GameObject[] gameObjects = Object.FindObjectsOfType<GameObject>();

			// If it's the main menu object, destroy it.
			for( int i = 0; i < gameObjects.Length; i++ )
			{
				if( gameObjects[i].CompareTag( "Menu" ) )
				{
					Object.Destroy( gameObjects[i] );
				}
			}
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
		public static void LoadLevel( string levelIdentifier, string levelSaveStateIdentifier )
		{
			if( string.IsNullOrEmpty( levelIdentifier ) )
			{
				throw new System.Exception( "The level identifier can't be null or empty." );
			}
			if( isLevelLoaded )
			{
				throw new System.Exception( "There's already a level loaded. You must unload it first." );
			}

			if( levelSaveStateIdentifier == null ) // if specified save state == null, set to default save state.
			{
				levelSaveStateIdentifier = DEFAULT_LEVEL_SAVE_STATE_IDENTIFIER;
			}

			DefinitionManager.LoadUnitDefinitions();
			DefinitionManager.LoadBuildingDefinitions();
			DefinitionManager.LoadProjectileDefinitions();
			DefinitionManager.LoadHeroDefinitions();
			DefinitionManager.LoadExtraDefinitions();

			DefinitionManager.LoadResourceDefinitions();
			DefinitionManager.LoadTechnologyDefinitions();


			DeleteMainMenuUI(); // remove main menu ui canvas (all ui objects)

			InstantiateGameUI(); // game UI prefabs

			CreateTerrain(); // create "env" organizational gameobject, and load terrain from files.

			Main.cameraPivot.position = new Vector3( 32, 0, 32 );

			// apply save state

			InstantiateUnits( currentLevelId, currentLevelSaveStateId ); // load units from current save state.
			InstantiateBuildings( currentLevelId, currentLevelSaveStateId );
			InstantiateProjectiles( currentLevelId, currentLevelSaveStateId );
			InstantiateHeroes( currentLevelId, currentLevelSaveStateId );
			InstantiateExtras( currentLevelId, currentLevelSaveStateId );

#error incomplete. assign definitions to every object first, then assign data to every one of them (this way guids will be assigned and referencing should persist).

			lastLoadTime = Time.time;
			currentLevelId = levelIdentifier;
			currentLevelSaveStateId = levelSaveStateIdentifier;
		}

		private static void LoadLevelKFF( string levelIdentifier )
		{
			// Loads data from level.kff file.

			string levelKffPath = GetLevelPath( currentLevelId ) + System.IO.Path.DirectorySeparatorChar + "level.kff";
			KFFSerializer serializer = KFFSerializer.ReadFromFile( levelKffPath, DefinitionManager.FILE_ENCODING );

			
			FactionDefinition[] fac = new FactionDefinition[serializer.Analyze( "Factions" ).childCount];

			for( int i = 0; i < fac.Length; i++ )
			{
				fac[i] = new FactionDefinition();
			}

			serializer.DeserializeArray( "Factions", fac );

			LevelDataManager.factions = fac;
		}

		private static void LoadLevelSaveStateKFF( string levelSaveStateIdentifier )
		{
			// Loads data from save_state.kff file.

			string levelKffPath = GetLevelSaveStatePath( currentLevelId, currentLevelSaveStateId ) + System.IO.Path.DirectorySeparatorChar + "save_state.kff";
			KFFSerializer serializer = KFFSerializer.ReadFromFile( levelKffPath, DefinitionManager.FILE_ENCODING );


			FactionData[] fac = new FactionData[serializer.Analyze( "Factions" ).childCount];

			for( int i = 0; i < fac.Length; i++ )
			{
				fac[i] = new FactionData();
			}

			serializer.DeserializeArray( "Factions", fac );

			LevelDataManager.factionData = fac;
		}

		private static void InstantiateUnits( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStatePath( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_units.kff";
			
			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );
			
			int unitCount = serializer.Analyze( "List" ).childCount;
			for( int i = 0; i < unitCount; i++ )
			{
				UnitData data = new UnitData();
				serializer.Deserialize( new Path( "List.{0}.Data", i ), data );

				string defId = serializer.ReadString( new Path( "List.{0}.DefinitionId", i ) );
				UnitDefinition def = DefinitionManager.GetUnit( defId );

				UnitCreator.Create( def, data );
			}
		}

		private static void InstantiateBuildings( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStatePath( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_buildings.kff";

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );

			int unitCount = serializer.Analyze( "List" ).childCount;
			for( int i = 0; i < unitCount; i++ )
			{
				BuildingData data = new BuildingData();
				serializer.Deserialize( new Path( "List.{0}.Data", i ), data );

				string defId = serializer.ReadString( new Path( "List.{0}.DefinitionId", i ) );
				BuildingDefinition def = DefinitionManager.GetBuilding( defId );

				BuildingCreator.Create( def, data );
			}
		}

		private static void InstantiateProjectiles( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStatePath( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_projectiles.kff";

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );

			int unitCount = serializer.Analyze( "List" ).childCount;
			for( int i = 0; i < unitCount; i++ )
			{
				ProjectileData data = new ProjectileData();
				serializer.Deserialize( new Path( "List.{0}.Data", i ), data );

				string defId = serializer.ReadString( new Path( "List.{0}.DefinitionId", i ) );
				ProjectileDefinition def = DefinitionManager.GetProjectile( defId );

				ProjectileCreator.Create( def, data );
			}
		}

		private static void InstantiateHeroes( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStatePath( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_heroes.kff";

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );

			int unitCount = serializer.Analyze( "List" ).childCount;
			for( int i = 0; i < unitCount; i++ )
			{
				HeroData data = new HeroData();
				serializer.Deserialize( new Path( "List.{0}.Data", i ), data );

				string defId = serializer.ReadString( new Path( "List.{0}.DefinitionId", i ) );
				HeroDefinition def = DefinitionManager.GetHero( defId );

				HeroCreator.Create( def, data );
			}
		}

		private static void InstantiateExtras( string levelIdentifier, string levelSaveStateIdentifier )
		{
			string path = GetLevelSaveStatePath( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "save_extras.kff";

			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, DefinitionManager.FILE_ENCODING );

			int unitCount = serializer.Analyze( "List" ).childCount;
			for( int i = 0; i < unitCount; i++ )
			{
				ExtraData data = new ExtraData();
				serializer.Deserialize( new Path( "List.{0}.Data", i ), data );

				string defId = serializer.ReadString( new Path( "List.{0}.DefinitionId", i ) );
				ExtraDefinition def = DefinitionManager.GetExtra( defId );

				ExtraCreator.Create( def, data );
			}
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
					color[i, j] = AssetManager.GetTexture2D( AssetManager.BUILTIN_ASSET_IDENTIFIER + "colormap/row-" + (size - j) + "-col-" + (i + 1), Katniss.Utils.TextureType.Color );
				}
			}
			Texture2D[,] height = new Texture2D[size, size];
			for( int i = 0; i < size; i++ )
			{
				for( int j = 0; j < size; j++ )
				{
					height[i, j] = AssetManager.GetTexture2D( AssetManager.BUILTIN_ASSET_IDENTIFIER + "heightmap/row-" + (size - j) + "-col-" + (i + 1), Katniss.Utils.TextureType.Color );
				}
			}
			LevelTerrainCreator.SpawnMap( height, color, 6f );
			LevelTerrainCreator.UpdateNavMesh();
		}

		private static void InstantiateGameUI()
		{
			Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Map Scene/__ GAME MANAGER __" ), Vector3.zero, Quaternion.identity );
			Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Map Scene/__ Game UI Canvas __" ), Vector3.zero, Quaternion.identity );
			Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Map Scene/__ Camera __" ), Vector3.zero, Quaternion.Euler( CameraController.defaultRotX, CameraController.defaultRotY, CameraController.defaultRotZ ) );
			Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Map Scene/Daylight Cycle" ), Vector3.zero, Quaternion.identity );
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		// SAVING


		/// <summary>
		/// Saves the scene to the specified level save state (level itself is the currently loaded ofc).
		/// </summary>
		/// <param name="saveSettings">The additional settings that define the behaviour of the method.</param>
		public static void SaveStateFromScene( string newLevelSaveStateId )
		{
			GameObject[] units = Unit.GetAllUnits();
			GameObject[] buildings = Building.GetAllBuildings();
			GameObject[] projectiles = Projectile.GetAllProjectiles();
			GameObject[] heroes = Hero.GetAllHeroes();
			GameObject[] extras = Extra.GetAllExtras();

			UnitData[] unitData = new UnitData[units.Length];
			BuildingData[] buildingData = new BuildingData[buildings.Length];
			ProjectileData[] projectileData = new ProjectileData[projectiles.Length];
			HeroData[] heroData = new HeroData[heroes.Length];
			ExtraData[] extraData = new ExtraData[extras.Length];

			for( int i = 0; i < unitData.Length; i++ )
			{
				unitData[i] = UnitCreator.GetData( units[i] );
			}
			for( int i = 0; i < buildingData.Length; i++ )
			{
				buildingData[i] = BuildingCreator.GetData( buildings[i] );
			}
			for( int i = 0; i < projectileData.Length; i++ )
			{
				projectileData[i] = ProjectileCreator.GetData( projectiles[i] );
			}
			for( int i = 0; i < heroData.Length; i++ )
			{
				heroData[i] = HeroCreator.GetData( heroes[i] );
			}
			for( int i = 0; i < extraData.Length; i++ )
			{
				extraData[i] = ExtraCreator.GetData( extras[i] );
			}

#error incomplete.
			throw new System.NotImplementedException();
#error TAI goals have to be saved with objects.

			// we need to get the index of the object in the array.
			// onenable having it add to list would make it easier, as we can just loop over that list to make our save state list.
			// then the index of data would correspond to index of object itself.
			// the objects referencing modules that can appear on more than one object can break this and it doesn't work.
			// still doesn't solve having multiples of the same interface on an object.
			
#error Inventory needs to be a module? and it needs to be serialized.


			// - selected and highlighted objects - saved as indices to the unit/hero/building/etc array.
		}
	}
}