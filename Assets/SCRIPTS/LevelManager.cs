using KFF;
using SS.Buildings;
using SS.Extras;
using SS.Projectiles;
using SS.Units;
using System.Collections;
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
			// Spawn the GameObject to the scene.
			Unit.Create( DataManager.FindDefinition<UnitDefinition>( "unit.wolf" ), new Vector3( 0, 0, 0 ), Quaternion.identity, 0 );
			
			ExtraDefinition defe = DataManager.FindDefinition<ExtraDefinition>( "extra.grass" );
			for( int i = 0; i < 600; i++ )
			{
				Extra.Create( defe, new Vector3( UnityEngine.Random.Range( -25f, 25f ), 0, UnityEngine.Random.Range( -25f, 25f ) ), Quaternion.identity );
			}

			UnitDefinition defx = DataManager.FindDefinition<UnitDefinition>( "unit.civilian" );
			for( int i = 0; i < 6; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -12f, -7f ) ), Quaternion.identity, 1 );
			}
			for( int i = 0; i < 6; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( 7f, 12f ) ), Quaternion.identity, 0 );
			}

			defx = DataManager.FindDefinition<UnitDefinition>( "unit.civilian_employed" );
			for( int i = 0; i < 4; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -12f, -7f ) ), Quaternion.identity, 1 );
			}
			for( int i = 0; i < 4; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( 7f, 12f ) ), Quaternion.identity, 0 );
			}

			defx = DataManager.FindDefinition<UnitDefinition>( "unit.light_infantry" );
			for( int i = 0; i < 4; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -7f, 0f ) ), Quaternion.identity, 1 );
			}
			for( int i = 0; i < 4; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( 0f, 7f ) ), Quaternion.identity, 0 );
			}

			defx = DataManager.FindDefinition<UnitDefinition>( "unit.heavy_infantry" );
			for( int i = 0; i < 3; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -7f, 0f ) ), Quaternion.identity, 1 );
			}
			for( int i = 0; i < 3; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( 0f, 7f ) ), Quaternion.identity, 0 );
			}

			defx = DataManager.FindDefinition<UnitDefinition>( "unit.light_cavalry" );
			for( int i = 0; i < 3; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -7f, 0f ) ), Quaternion.identity, 1 );
			}
			for( int i = 0; i < 3; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( 0f, 7f ) ), Quaternion.identity, 0 );
			}

			defx = DataManager.FindDefinition<UnitDefinition>( "unit.heavy_cavalry" );
			for( int i = 0; i < 2; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -7f, 0f ) ), Quaternion.identity, 1 );
			}
			for( int i = 0; i < 2; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( 0f, 7f ) ), Quaternion.identity, 0 );
			}

			defx = DataManager.FindDefinition<UnitDefinition>( "unit.bowmen" );
			for( int i = 0; i < 4; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, -3f ) ), Quaternion.identity, 1 );
			}
			for( int i = 0; i < 4; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( 3f, 10f ) ), Quaternion.identity, 0 );
			}

			defx = DataManager.FindDefinition<UnitDefinition>( "unit.crossbowmen" );
			for( int i = 0; i < 3; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, -3f ) ), Quaternion.identity, 1 );
			}
			for( int i = 0; i < 3; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( 3f, 10f ) ), Quaternion.identity, 0 );
			}

			defx = DataManager.FindDefinition<UnitDefinition>( "unit.mounted_archers" );
			for( int i = 0; i < 2; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, -3f ) ), Quaternion.identity, 1 );
			}
			for( int i = 0; i < 2; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( 3f, 10f ) ), Quaternion.identity, 0 );
			}

			ResourceDepositDefinition def = DataManager.FindDefinition<ResourceDepositDefinition>( "resource_deposit.tree" );
			for( int i = 0; i < 75; i++ )
			{
				float x = UnityEngine.Random.Range( -10f, 10f );
				float z = UnityEngine.Random.Range( -10f, 10f );

				if( Physics.Raycast( new Vector3( x, 50, z ), Vector3.down, out RaycastHit hit, 100 ) )
				{
					ResourceDeposit.Create( def, hit.point, Quaternion.Euler( 0f, UnityEngine.Random.Range( -180f, 180f ), 0f ) );
				}
			}
			def = DataManager.FindDefinition<ResourceDepositDefinition>( "resource_deposit.pine" );
			for( int i = 0; i < 50; i++ )
			{
				float x = UnityEngine.Random.Range( -10f, 10f );
				float z = UnityEngine.Random.Range( -10f, 10f );

				if( Physics.Raycast( new Vector3( x, 50, z ), Vector3.down, out RaycastHit hit, 100 ) )
				{
					ResourceDeposit.Create( def, hit.point, Quaternion.Euler( 0f, UnityEngine.Random.Range( -180f, 180f ), 0f ) );
				}
			}
		}
	}
}