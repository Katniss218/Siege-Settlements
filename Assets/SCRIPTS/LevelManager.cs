using KFF;
using SS.Buildings;
using SS.Projectiles;
using SS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SS.Levels
{
	public static class LevelManager
	{
		private static string path;

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


			LevelManager.path = path;
			// Load the empty level scene.
			SceneManager.LoadScene( "Map" );
		}

		public static void PostInitLoad() // called after the scene is loaded.
		{
			// FIXME @@@@@ Remove this lazy thing and replace it with other way to load this.

			// Spawn the GameObject to the scene.
			Unit.Create( DataManager.FindDefinition<UnitDefinition>( "unit.wolf" ), new Vector3( 0, 0, 0 ), 0 );

			Building.Create( DataManager.FindDefinition<BuildingDefinition>( "building.house0" ), new Vector3( 5, 0, 5 ), Quaternion.identity, 0 );

			UnitDefinition defx = DataManager.FindDefinition<UnitDefinition>( "unit.light_infantry" );
			for( int i = 0; i < 4; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, 10f ) ), 1 );
			}
			for( int i = 0; i < 4; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, 10f ) ), 0 );
			}

			defx = DataManager.FindDefinition<UnitDefinition>( "unit.heavy_infantry" );
			for( int i = 0; i < 3; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, 10f ) ), 1 );
			}
			for( int i = 0; i < 3; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, 10f ) ), 0 );
			}

			defx = DataManager.FindDefinition<UnitDefinition>( "unit.light_cavalry" );
			for( int i = 0; i < 3; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, 10f ) ), 1 );
			}
			for( int i = 0; i < 3; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, 10f ) ), 0 );
			}

			defx = DataManager.FindDefinition<UnitDefinition>( "unit.heavy_cavalry" );
			for( int i = 0; i < 2; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, 10f ) ), 1 );
			}
			for( int i = 0; i < 2; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, 10f ) ), 0 );
			}

			defx = DataManager.FindDefinition<UnitDefinition>( "unit.bowmen" );
			for( int i = 0; i < 4; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, 10f ) ), 1 );
			}
			for( int i = 0; i < 4; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, 10f ) ), 0 );
			}

			defx = DataManager.FindDefinition<UnitDefinition>( "unit.crossbowmen" );
			for( int i = 0; i < 3; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, 10f ) ), 1 );
			}
			for( int i = 0; i < 3; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, 10f ) ), 0 );
			}

			defx = DataManager.FindDefinition<UnitDefinition>( "unit.mounted_archers" );
			for( int i = 0; i < 2; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, 10f ) ), 1 );
			}
			for( int i = 0; i < 2; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, 10f ) ), 0 );
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
			ProjectileDefinition def2 = DataManager.FindDefinition<ProjectileDefinition>( "projectile.arrow" );
			Projectile.Create( def2, new Vector3( 0, 2, 0 ), new Vector3( 3, 3, 3 ), 0, 0f, null );
		}
	}
}