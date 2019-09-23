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
using UnityEngine;

namespace SS.Levels
{
	/// <summary>
	/// Manages level-specific stuff.
	/// </summary>
	public class LevelManager
	{
		/// <summary>
		/// The time stamp of when the last level was loaded (in units of time elapsed since the game's launch) (Read only).
		/// </summary>
		public static float lastLoadTime { get; private set; }

		public static Level? currentLevel { get; private set; }


		/// <summary>
		/// Unloads the level gameobjects from the scene.
		/// </summary>
		public static void UnloadFromScene()
		{
#warning incomplete.
			if( currentLevel == null )
			{
				throw new System.Exception( "There's no level loaded. You must load a level first." );
			}
			// unload previous scene (destroy all level gameobjects)
			// destroy and unload terrain.
			// unload definitions and assets from the managers.

			currentLevel = null;
		}

		/// <summary>
		/// Loads the level into the scene. Uses the specified save state when loading.
		/// </summary>
		/// <param name="level">The level that is going to be loaded.</param>
		/// <param name="saveState">The save state associated with the level. If null, the level's default save state will be used.</param>
		public static void LoadToScene( Level level, LevelSaveState saveState )
		{
#warning incomplete.
			if( currentLevel != null )
			{
				throw new System.Exception( "There's already a level loaded. You must unload it first." );
			}

			currentLevel = level;
			// load new definitions and assets into the managers.
			// load new terrain.

			// apply save state (use default if custom not present).
			// - create level gameobjects

		}


		/// <summary>
		/// Creates a LevelSaveState struct from the objects in the currently loaded scene.
		/// </summary>
		/// <param name="saveSettings">The additional settings that define the behaviour of the method.</param>
		public static LevelSaveState CreateSaveStateFromScene( LevelSaveSettings saveSettings )
		{
#warning incomplete.
			// TODO - collect save state of every object specified below, that currently exists in the scene:
			GameObject[] gameObjects = Object.FindObjectsOfType<GameObject>();

			List<UnitData> unitSaveStates = new List<UnitData>();
			List<BuildingData> buildingSaveStates = new List<BuildingData>();
			List<ProjectileData> projectileSaveStates = new List<ProjectileData>();
			List<HeroData> heroSaveStates = new List<HeroData>();
			List<ExtraData> extraSaveStates = new List<ExtraData>();

			for( int i = 0; i < gameObjects.Length; i++ )
			{
				switch( gameObjects[i].layer )
				{
					case ObjectLayer.UNITS:

						unitSaveStates.Add( UnitCreator.GetSaveState( gameObjects[i] ) );
						break;

					case ObjectLayer.BUILDINGS:

						buildingSaveStates.Add( BuildingCreator.GetSaveState( gameObjects[i] ) );
						break;

					case ObjectLayer.PROJECTILES:

						projectileSaveStates.Add( ProjectileCreator.GetSaveState( gameObjects[i] ) );
						break;

					case ObjectLayer.HEROES:

						heroSaveStates.Add( HeroCreator.GetSaveState( gameObjects[i] ) );
						break;

					case ObjectLayer.EXTRAS:

						extraSaveStates.Add( ExtraCreator.GetSaveState( gameObjects[i] ) );
						break;
				}
			}
			// - units
			// - buildings
			// - projectiles
			// - heroes
			// - extras

			throw new System.NotImplementedException();
			// - tai goals.
			// - selected and highlighted objects - saved as indices to the unit/hero/building/etc array.
		}



		/// <summary>
		/// Loads a level, at the specified path.
		/// </summary>
		/// <param name="path">The path to the level's directory (contains a 'level.kff' file inside).</param>
		public static void Load( string path )
		{
			// Load the default Siege Settlements data & assets.
			DataManager.LoadDefaults();
			//AssetManager.LoadDefaults();

			// Load the per-level data & assets.
			KFFSerializer serializer = KFFSerializer.ReadFromFile( path + System.IO.Path.DirectorySeparatorChar + "level.kff", System.Text.Encoding.UTF8 );

			var analysisData = serializer.Analyze( "Factions" );
			FactionData[] fac = new FactionData[analysisData.childCount];

			for( int i = 0; i < fac.Length; i++ )
			{
				fac[i] = new FactionData();
			}

			serializer.DeserializeArray<FactionData>( "Factions", fac );

			FactionManager.SetFactions( fac );



			DeleteMainMenu();
			
			LoadScenePrefabs( out GameObject environment );

			OnLevelLoad( path, environment );

			OnPostlevelLoad();
		}

		private static void DeleteMainMenu()
		{
			// Find every gameObject in the scene.
			GameObject[] gos = Object.FindObjectsOfType<GameObject>();
			// If it's the main menu object, destroy it.
			for( int i = 0; i < gos.Length; i++ )
			{
				if( gos[i].CompareTag( "Menu" ) )
				{
					Object.Destroy( gos[i] );
				}
			}
		}

		private static void LoadScenePrefabs( out GameObject environment )
		{
			Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Map Scene/__ GAME MANAGER __" ), Vector3.zero, Quaternion.identity );
			Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Map Scene/__ Game UI Canvas __" ), Vector3.zero, Quaternion.identity );
			Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Map Scene/__ Camera __" ), Vector3.zero, Quaternion.Euler( CameraController.defaultRotX, CameraController.defaultRotY, CameraController.defaultRotZ ) );
			environment = Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Map Scene/Environment" ), Vector3.zero, Quaternion.identity );
		}

		private static void OnLevelLoad( string path, GameObject env )
		{
#warning - change funcitonality.
			const int size = 4; // the size of the map (in chunks).

			LevelTerrainCreator.terrainParent = env.transform;
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

			Main.cameraPivot.position = new Vector3( 32, 0, 32 );
			
			List<UnitDefinition> units = DataManager.GetAllOfType<UnitDefinition>();
			for( int i = 0; i < units.Count; i++ )
			{
				for( int j = 0; j < 4; j++ )
				{
					float x = Random.Range( 22f, 42f );
					float z = Random.Range( 32f + i, 32f + (2f * i) );

					if( Physics.Raycast( new Vector3( x, 50f, z ), Vector3.down, out RaycastHit hit, 100f, ObjectLayer.TERRAIN_MASK ) )
					{
						UnitData data = new UnitData();
						data.position = hit.point;
						data.rotation = Quaternion.identity;
						data.factionId = 0;
						data.health = units[i].healthMax;
						UnitCreator.Create( units[i], data );
					}

					x = Random.Range( 22f, 42f );
					z = Random.Range( 32f + (-2f * i), 32f + (-i) );

					if( Physics.Raycast( new Vector3( x, 50f, z ), Vector3.down, out hit, 100f, ObjectLayer.TERRAIN_MASK ) )
					{
						UnitData data = new UnitData();
						data.position = hit.point;
						data.rotation = Quaternion.identity;
						data.factionId = 1;
						data.health = units[i].healthMax;
						UnitCreator.Create( units[i], data );
					}
				}
			}

			List<HeroDefinition> heroes = DataManager.GetAllOfType<HeroDefinition>();
			for( int i = 0; i < heroes.Count; i++ )
			{
				for( int j = 0; j < 2; j++ )
				{
					float x = Random.Range( 22f, 42f );
					float z = Random.Range( 32f + i, 32f + (4f * i) );

					if( Physics.Raycast( new Vector3( x, 50f, z ), Vector3.down, out RaycastHit hit, 100f, ObjectLayer.TERRAIN_MASK ) )
					{
						HeroData data = new HeroData();
						data.position = hit.point;
						data.rotation = Quaternion.identity;
						data.factionId = 0;
						data.health = units[i].healthMax;
						HeroCreator.Create( heroes[i], data );
					}
					x = Random.Range( 22f, 42f );
					z = Random.Range( 32f + (-4f * i), 32f + (-i) );

					if( Physics.Raycast( new Vector3( x, 50f, z ), Vector3.down, out hit, 100f, ObjectLayer.TERRAIN_MASK ) )
					{
						HeroData data = new HeroData();
						data.position = hit.point;
						data.rotation = Quaternion.identity;
						data.factionId = 1;
						data.health = units[i].healthMax;
						HeroCreator.Create( heroes[i], data );
					}
				}
			}


			List<ExtraDefinition> extras = DataManager.GetAllOfType<ExtraDefinition>();
			for( int i = 0; i < extras.Count; i++ )
			{
				for( int j = 0; j < 400; j++ )
				{
					float x = Random.Range( 12f, 52f );
					float z = Random.Range( 12f, 52f );

					if( Physics.Raycast( new Vector3( x, 50f, z ), Vector3.down, out RaycastHit hit, 100f, ObjectLayer.TERRAIN_MASK ) )
					{
						ExtraData data = new ExtraData();
						data.position = hit.point;
						data.rotation = Quaternion.Euler( 0f, Random.Range( -180f, 180f ), 0f );
						ExtraCreator.Create( extras[i], data );
					}
				}
			}
		}

		private static void OnPostlevelLoad()
		{
#warning - change funcitonality.
			lastLoadTime = Time.time;
		}
	}
}