using KFF;
using SS.Data;
using SS.Extras;
using SS.Heroes;
using SS.TerrainCreation;
using SS.Units;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SS.Levels
{
	/// <summary>
	/// Manages level-specific stuff. Exists only on a 'Map' scene.
	/// </summary>
	public class LevelManager : MonoBehaviour
	{
		public static void Load( string path )
		{
			// Load the default Siege Settlements data & assets.
			DataManager.LoadDefaults();
			AssetsManager.LoadDefaults();

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
			
			// Load the empty level scene.
			SceneManager.LoadScene( "Map" );
		}

		public void Start()
		{
			const int size = 4; // the size of the map (in chunks).

			LevelTerrainCreator.terrainParent = this.transform;
			Texture2D[,] color = new Texture2D[size, size];
			for( int i = 0; i < size; i++ )
			{
				for( int j = 0; j < size; j++ )
				{
					color[i, j] = Resources.Load<Texture2D>( "colormap/row-" + (size - j) + "-col-" + (i + 1) );
				}
			}
			Texture2D[,] height = new Texture2D[size, size];
			for( int i = 0; i < size; i++ )
			{
				for( int j = 0; j < size; j++ )
				{
					height[i, j] = Resources.Load<Texture2D>( "heightmap/row-" + (size - j) + "-col-" + (i + 1) );
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
					float z = Random.Range( 32 + i, 32 + (2 * i) );

					if( Physics.Raycast( new Vector3( x, 50f, z ), Vector3.down, out RaycastHit hit, 100f ) )
					{
						Unit.Create( units[i], hit.point, Quaternion.identity, 0 );
					}
					x = Random.Range( 22f, 42f );
					z = Random.Range( 32 + (-2 * i), 32 + (-i) );

					if( Physics.Raycast( new Vector3( x, 50f, z ), Vector3.down, out hit, 100f ) )
					{
						Unit.Create( units[i], hit.point, Quaternion.identity, 1 );
					}
				}
			}

			List<HeroDefinition> heroes = DataManager.GetAllOfType<HeroDefinition>();
			for( int i = 0; i < heroes.Count; i++ )
			{
				for( int j = 0; j < 4; j++ )
				{
					float x = Random.Range( 22f, 42f );
					float z = Random.Range( 32 + i, 32 + (2 * i) );

					if( Physics.Raycast( new Vector3( x, 50f, z ), Vector3.down, out RaycastHit hit, 100f ) )
					{
						Hero.Create( heroes[i], hit.point, Quaternion.identity, 0 );
					}
					x = Random.Range( 22f, 42f );
					z = Random.Range( 32 + (-2 * i), 32 + (-i) );

					if( Physics.Raycast( new Vector3( x, 50f, z ), Vector3.down, out hit, 100f ) )
					{
						Hero.Create( heroes[i], hit.point, Quaternion.identity, 1 );
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

					if( Physics.Raycast( new Vector3( x, 50f, z ), Vector3.down, out RaycastHit hit, 100f ) )
					{
						Extra.Create( extras[i], hit.point, Quaternion.Euler( 0f, Random.Range( -180f, 180f ), 0f ) );
					}
				}
			}

			List<ResourceDepositDefinition> deposits = DataManager.GetAllOfType<ResourceDepositDefinition>();
			for( int i = 0; i < deposits.Count; i++ )
			{
				for( int j = 0; j < 100; j++ )
				{
					float x = Random.Range( 22f, 42f );
					float z = Random.Range( 22f, 42f );

					if( Physics.Raycast( new Vector3( x, 50f, z ), Vector3.down, out RaycastHit hit, 100f ) )
					{
						ResourceDeposit.Create( deposits[i], hit.point, Quaternion.Euler( 0f, Random.Range( -180f, 180f ), 0f ), 50 );
					}
				}
			}
		}
	}
}