using KFF;
using SS.Content;
using SS.Extras;
using SS.Heroes;
using SS.TerrainCreation;
using SS.Units;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Levels
{
	/// <summary>
	/// Manages level-specific stuff. Exists only on a 'Map' scene.
	/// </summary>
	public class LevelManager
	{
		/// <summary>
		/// The time stamp of when the last level was loaded (in units of time elapsed since the game's launch) (Read only).
		/// </summary>
		public static float lastLoadTime { get; private set; }

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
			Faction[] fac = new Faction[analysisData.childCount];

			for( int i = 0; i < fac.Length; i++ )
			{
				fac[i] = new Faction();
			}

			serializer.DeserializeArray<Faction>( "Factions", fac );

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
			Object.Instantiate( AssetManager.GetPrefab( AssetManager.RESOURCE_ID + "Prefabs/Map Scene/__ GAME MANAGER __" ), Vector3.zero, Quaternion.identity );
			Object.Instantiate( AssetManager.GetPrefab( AssetManager.RESOURCE_ID + "Prefabs/Map Scene/__ Game UI Canvas __" ), Vector3.zero, Quaternion.identity );
			Object.Instantiate( AssetManager.GetPrefab( AssetManager.RESOURCE_ID + "Prefabs/Map Scene/__ Camera __" ), Vector3.zero, Quaternion.Euler( CameraController.defaultRotX, CameraController.defaultRotY, CameraController.defaultRotZ ) );
			environment = Object.Instantiate( AssetManager.GetPrefab( AssetManager.RESOURCE_ID + "Prefabs/Map Scene/Environment" ), Vector3.zero, Quaternion.identity );
		}

		private static void OnLevelLoad( string path, GameObject env )
		{
			const int size = 4; // the size of the map (in chunks).

			LevelTerrainCreator.terrainParent = env.transform;
			Texture2D[,] color = new Texture2D[size, size];
			for( int i = 0; i < size; i++ )
			{
				for( int j = 0; j < size; j++ )
				{
					color[i, j] = AssetManager.GetTexture2D( AssetManager.RESOURCE_ID + "colormap/row-" + (size - j) + "-col-" + (i + 1), Katniss.Utils.TextureType.Color );
				}
			}
			Texture2D[,] height = new Texture2D[size, size];
			for( int i = 0; i < size; i++ )
			{
				for( int j = 0; j < size; j++ )
				{
					height[i, j] = AssetManager.GetTexture2D( AssetManager.RESOURCE_ID + "heightmap/row-" + (size - j) + "-col-" + (i + 1), Katniss.Utils.TextureType.Color );
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
						UnitCreator.Create( units[i], hit.point, Quaternion.identity, 0 );
					}
					x = Random.Range( 22f, 42f );
					z = Random.Range( 32f + (-2f * i), 32f + (-i) );

					if( Physics.Raycast( new Vector3( x, 50f, z ), Vector3.down, out hit, 100f, ObjectLayer.TERRAIN_MASK ) )
					{
						UnitCreator.Create( units[i], hit.point, Quaternion.identity, 1 );
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
						HeroCreator.Create( heroes[i], hit.point, Quaternion.identity, 0 );
					}
					x = Random.Range( 22f, 42f );
					z = Random.Range( 32f + (-4f * i), 32f + (-i) );

					if( Physics.Raycast( new Vector3( x, 50f, z ), Vector3.down, out hit, 100f, ObjectLayer.TERRAIN_MASK ) )
					{
						HeroCreator.Create( heroes[i], hit.point, Quaternion.identity, 1 );
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
						ExtraCreator.Create( extras[i], hit.point, Quaternion.Euler( 0f, Random.Range( -180f, 180f ), 0f ) );
					}
				}
			}

			List<ResourceDepositDefinition> deposits = DataManager.GetAllOfType<ResourceDepositDefinition>();
			for( int i = 0; i < (deposits.Count < 2 ? deposits.Count : 2); i++ )
			{
				for( int j = 0; j < 100; j++ )
				{
					float x = Random.Range( 22f, 42f );
					float z = Random.Range( 22f, 42f );

					if( Physics.Raycast( new Vector3( x, 50f, z ), Vector3.down, out RaycastHit hit, 100f, ObjectLayer.TERRAIN_MASK ) )
					{
						ResourceDepositCreator.Create( deposits[i], hit.point, Quaternion.Euler( 0f, Random.Range( -180f, 180f ), 0f ), 5 );
					}
				}
			}
		}

		private static void OnPostlevelLoad()
		{
			lastLoadTime = Time.time;
		}
	}
}