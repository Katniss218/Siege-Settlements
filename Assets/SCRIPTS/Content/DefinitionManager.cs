using KFF;
using SS.Buildings;
using SS.Extras;
using SS.Heroes;
using SS.Projectiles;
using SS.ResourceSystem;
using SS.Technologies;
using SS.Units;
using System;
using System.Text;

namespace SS.Content
{
	/// <summary>
	/// A class for managing game's currently loaded definitions.
	/// </summary>
	public static class DefinitionManager
	{
		public static readonly Encoding FILE_ENCODING = Encoding.UTF8;

		private const string DEFINITION_DIRNAME = "Definitions";

		// Object definitions

		private static UnitDefinition[] unitDefinitions = null;
		private static BuildingDefinition[] buildingDefinitions = null;
		private static ProjectileDefinition[] projectileDefinitions = null;
		private static HeroDefinition[] heroDefinitions = null;
		private static ExtraDefinition[] extraDefinitions = null;

		// "Soft" definitions.

		private static ResourceDefinition[] resourceDefinitions = null;
		private static TechnologyDefinition[] technologyDefinitions = null;


		/// <summary>
		/// Clears the cached definitions (typicaly used when level is unloaded).
		/// </summary>
		public static void Purge()
		{
			unitDefinitions = null;
			buildingDefinitions = null;
			projectileDefinitions = null;
			heroDefinitions = null;
			extraDefinitions = null;

			resourceDefinitions = null;
			technologyDefinitions = null;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
		public static void LoadUnitDefinitions( KFFSerializer serializer )
		{
			UnitDefinition[] deserialized = new UnitDefinition[serializer.Analyze( "List" ).childCount];
			for( int i = 0; i < deserialized.Length; i++ )
			{
				deserialized[i] = new UnitDefinition( "<missing>" );
			}
			serializer.DeserializeArray( "List", deserialized );

			unitDefinitions = deserialized;
		}
		
		public static void LoadBuildingDefinitions( KFFSerializer serializer )
		{
			BuildingDefinition[] deserialized = new BuildingDefinition[serializer.Analyze( "List" ).childCount];
			for( int i = 0; i < deserialized.Length; i++ )
			{
				deserialized[i] = new BuildingDefinition( "<missing>" );
			}
			serializer.DeserializeArray( "List", deserialized );

			buildingDefinitions = deserialized;
		}
		
		public static void LoadProjectileDefinitions( KFFSerializer serializer )
		{
			ProjectileDefinition[] deserialized = new ProjectileDefinition[serializer.Analyze( "List" ).childCount];
			for( int i = 0; i < deserialized.Length; i++ )
			{
				deserialized[i] = new ProjectileDefinition( "<missing>" );
			}
			serializer.DeserializeArray( "List", deserialized );

			projectileDefinitions = deserialized;
		}
		
		public static void LoadHeroDefinitions( KFFSerializer serializer )
		{
			HeroDefinition[] deserialized = new HeroDefinition[serializer.Analyze( "List" ).childCount];
			for( int i = 0; i < deserialized.Length; i++ )
			{
				deserialized[i] = new HeroDefinition( "<missing>" );
			}
			serializer.DeserializeArray( "List", deserialized );

			heroDefinitions = deserialized;
		}
		
		public static void LoadExtraDefinitions( KFFSerializer serializer )
		{
			ExtraDefinition[] deserialized = new ExtraDefinition[serializer.Analyze( "List" ).childCount];
			for( int i = 0; i < deserialized.Length; i++ )
			{
				deserialized[i] = new ExtraDefinition( "<missing>" );
			}
			serializer.DeserializeArray( "List", deserialized );

			extraDefinitions = deserialized;
		}
		
		// ///////////////////////////////////////////////

		public static void LoadResourceDefinitions( KFFSerializer serializer )
		{
			ResourceDefinition[] deserialized = new ResourceDefinition[serializer.Analyze( "List" ).childCount];
			for( int i = 0; i < deserialized.Length; i++ )
			{
				deserialized[i] = new ResourceDefinition( "<missing>" );
			}
			serializer.DeserializeArray( "List", deserialized );

			resourceDefinitions = deserialized;
		}

		public static void LoadTechnologyDefinitions( KFFSerializer serializer )
		{
			TechnologyDefinition[] deserialized = new TechnologyDefinition[serializer.Analyze( "List" ).childCount];
			for( int i = 0; i < deserialized.Length; i++ )
			{
				deserialized[i] = new TechnologyDefinition( "<missing>" );
			}
			serializer.DeserializeArray( "List", deserialized );

			technologyDefinitions = deserialized;
		}

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		// Getters.

		public static UnitDefinition GetUnit( string id )
		{
			if( unitDefinitions == null )
			{
				throw new Exception( "Definitions haven't been loaded yet." );
			}
			for( int i = 0; i < unitDefinitions.Length; i++ )
			{
				if( unitDefinitions[i].id == id )
				{
					return unitDefinitions[i];
				}
			}
			throw new Exception( "A unit with an id '" + id + "' is not registered." );
		}

		public static UnitDefinition[] GetAllUnits()
		{
			if( unitDefinitions == null )
			{
				throw new Exception( "Definitions haven't been loaded yet." );
			}
			UnitDefinition[] ret = new UnitDefinition[unitDefinitions.Length];
			for( int i = 0; i < ret.Length; i++ )
			{
				ret[i] = unitDefinitions[i];
			}
			return ret;
		}


		public static BuildingDefinition GetBuilding( string id )
		{
			if( buildingDefinitions == null )
			{
				throw new Exception( "Definitions haven't been loaded yet." );
			}
			for( int i = 0; i < buildingDefinitions.Length; i++ )
			{
				if( buildingDefinitions[i].id == id )
				{
					return buildingDefinitions[i];
				}
			}
			throw new Exception( "A building with an id '" + id + "' is not registered." );
		}

		public static BuildingDefinition[] GetAllBuildings()
		{
			if( buildingDefinitions == null )
			{
				throw new Exception( "Definitions haven't been loaded yet." );
			}
			BuildingDefinition[] ret = new BuildingDefinition[buildingDefinitions.Length];
			for( int i = 0; i < ret.Length; i++ )
			{
				ret[i] = buildingDefinitions[i];
			}
			return ret;
		}


		public static ProjectileDefinition GetProjectile( string id )
		{
			if( projectileDefinitions == null )
			{
				throw new Exception( "Definitions haven't been loaded yet." );
			}
			for( int i = 0; i < projectileDefinitions.Length; i++ )
			{
				if( projectileDefinitions[i].id == id )
				{
					return projectileDefinitions[i];
				}
			}
			throw new Exception( "A projectile with an id '" + id + "' is not registered." );
		}

		public static ProjectileDefinition[] GetAllProjectiles()
		{
			if( projectileDefinitions == null )
			{
				throw new Exception( "Definitions haven't been loaded yet." );
			}
			ProjectileDefinition[] ret = new ProjectileDefinition[projectileDefinitions.Length];
			for( int i = 0; i < ret.Length; i++ )
			{
				ret[i] = projectileDefinitions[i];
			}
			return ret;
		}


		public static HeroDefinition GetHero( string id )
		{
			if( heroDefinitions == null )
			{
				throw new Exception( "Definitions haven't been loaded yet." );
			}
			for( int i = 0; i < heroDefinitions.Length; i++ )
			{
				if( heroDefinitions[i].id == id )
				{
					return heroDefinitions[i];
				}
			}
			throw new Exception( "A hero with an id '" + id + "' is not registered." );
		}

		public static HeroDefinition[] GetAllHeroes()
		{
			if( heroDefinitions == null )
			{
				throw new Exception( "Definitions haven't been loaded yet." );
			}
			HeroDefinition[] ret = new HeroDefinition[heroDefinitions.Length];
			for( int i = 0; i < ret.Length; i++ )
			{
				ret[i] = heroDefinitions[i];
			}
			return ret;
		}


		public static ExtraDefinition GetExtra( string id )
		{
			if( extraDefinitions == null )
			{
				throw new Exception( "Definitions haven't been loaded yet." );
			}
			for( int i = 0; i < extraDefinitions.Length; i++ )
			{
				if( extraDefinitions[i].id == id )
				{
					return extraDefinitions[i];
				}
			}
			throw new Exception( "An extra with an id '" + id + "' is not registered." );
		}

		public static ExtraDefinition[] GetAllExtras()
		{
			if( extraDefinitions == null )
			{
				throw new Exception( "Definitions haven't been loaded yet." );
			}
			ExtraDefinition[] ret = new ExtraDefinition[extraDefinitions.Length];
			for( int i = 0; i < ret.Length; i++ )
			{
				ret[i] = extraDefinitions[i];
			}
			return ret;
		}

		// ///////////////////////////////////////////////

		public static ResourceDefinition GetResource( string id )
		{
			if( resourceDefinitions == null )
			{
				throw new Exception( "Definitions haven't been loaded yet." );
			}
			for( int i = 0; i < resourceDefinitions.Length; i++ )
			{
				if( resourceDefinitions[i].id == id )
				{
					return resourceDefinitions[i];
				}
			}
			throw new Exception( "A resource with an id '" + id + "' is not registered." );
		}

		public static ResourceDefinition[] GetAllResources()
		{
			if( resourceDefinitions == null )
			{
				throw new Exception( "Definitions haven't been loaded yet." );
			}
			ResourceDefinition[] ret = new ResourceDefinition[resourceDefinitions.Length];
			for( int i = 0; i < ret.Length; i++ )
			{
				ret[i] = resourceDefinitions[i];
			}
			return ret;
		}


		public static TechnologyDefinition GetTechnology( string id )
		{
			if( technologyDefinitions == null )
			{
				throw new Exception( "Definitions haven't been loaded yet." );
			}
			for( int i = 0; i < technologyDefinitions.Length; i++ )
			{
				if( technologyDefinitions[i].id == id )
				{
					return technologyDefinitions[i];
				}
			}
			throw new Exception( "A technology with an id '" + id + "' is not registered." );
		}

		public static TechnologyDefinition[] GetAllTechnologies()
		{
			if( technologyDefinitions == null )
			{
				throw new Exception( "Definitions haven't been loaded yet." );
			}
			TechnologyDefinition[] ret = new TechnologyDefinition[technologyDefinitions.Length];
			for( int i = 0; i < ret.Length; i++ )
			{
				ret[i] = technologyDefinitions[i];
			}
			return ret;
		}
	}
}