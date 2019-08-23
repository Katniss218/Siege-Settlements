using KFF;
using SS.Data;
using SS.Extras;
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

			serializer.Analyze( "Factions" );
			Faction[] fac = new Faction[serializer.aChildCount];

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
			List<UnitDefinition> units = DataManager.GetAllOfType<UnitDefinition>();
			for( int i = 0; i < units.Count; i++ )
			{
				for( int j = 0; j < 4; j++ )
				{
					Unit.Create( units[i], new Vector3( Random.Range( -10f, 10f ), 0, Random.Range( i, 2 * i ) ), Quaternion.identity, 0 );
					Unit.Create( units[i], new Vector3( Random.Range( -10f, 10f ), 0, Random.Range( -2*i, -i ) ), Quaternion.identity, 1 );
				}
			}
			// Spawn the GameObject to the scene.
			//Unit.Create( DataManager.Get<UnitDefinition>( "unit.wolf" ), new Vector3( 0, 0, 0 ), Quaternion.identity, 0 );

			List<ExtraDefinition> extras = DataManager.GetAllOfType<ExtraDefinition>();
			for( int i = 0; i < extras.Count; i++ )
			{
				for( int j = 0; j < 400; j++ )
				{
					Extra.Create( extras[i], new Vector3( Random.Range( -25f, 25f ), 0, Random.Range( -25f, 25f ) ), Quaternion.identity );
					Extra.Create( extras[i], new Vector3( Random.Range( -25f, 25f ), 0, Random.Range( -25f, 25f ) ), Quaternion.identity );
				}
			}

			List<ResourceDepositDefinition> deposits = DataManager.GetAllOfType<ResourceDepositDefinition>();
			for( int i = 0; i < deposits.Count; i++ )
			{
				for( int j = 0; j < 100; j++ )
				{
					float x = Random.Range( -10f, 10f );
					float z = Random.Range( -10f, 10f );

					if( Physics.Raycast( new Vector3( x, 50f, z ), Vector3.down, out RaycastHit hit, 100f ) )
					{
						ResourceDeposit.Create( deposits[i], hit.point, Quaternion.Euler( 0f, Random.Range( -180f, 180f ), 0f ), 50 );
					}
				}
			}
		}
	}
}