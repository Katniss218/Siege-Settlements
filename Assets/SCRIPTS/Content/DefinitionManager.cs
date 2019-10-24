﻿using KFF;
using SS.Buildings;
using SS.Extras;
using SS.Heroes;
using SS.Levels;
using SS.Projectiles;
using SS.ResourceSystem;
using SS.Technologies;
using SS.Units;
using System.Text;
using UnityEngine;

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
		private static ResourceDepositDefinition[] resourceDepositDefinitions = null;

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
			resourceDepositDefinitions = null;

			resourceDefinitions = null;
			technologyDefinitions = null;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		// Loading from CURRENT level. Replaces the old defs.
		
		public static void LoadUnitDefinitions( string levelIdentifier )
		{
			string path = LevelManager.GetFullDataPath( levelIdentifier, "units.kff" );

			if( !System.IO.File.Exists( path ) )
			{
				throw new System.Exception( "Can't open file '" + path + "' - file doesn't exist." );
			}
			try
			{

				KFFSerializer serializer = KFFSerializer.ReadFromFile( path, FILE_ENCODING );

				UnitDefinition[] deserialized = new UnitDefinition[serializer.Analyze( "List" ).childCount];
				for( int i = 0; i < deserialized.Length; i++ )
				{
					deserialized[i] = new UnitDefinition( "<missing>" );
				}
				serializer.DeserializeArray( "List", deserialized );

				unitDefinitions = deserialized;
			}
			catch( System.Exception )
			{
				throw new System.Exception( "Can't load units from file '" + path + "'." );
			}
		}


		public static void LoadBuildingDefinitions( string levelIdentifier )
		{
			string path = LevelManager.GetFullDataPath( levelIdentifier, "buildings.kff" );

			if( !System.IO.File.Exists( path ) )
			{
				throw new System.Exception( "Can't open file '" + path + "' - file doesn't exist." );
			}
			try
			{
				KFFSerializer serializer = KFFSerializer.ReadFromFile( path, FILE_ENCODING );
				
				BuildingDefinition[] deserialized = new BuildingDefinition[serializer.Analyze( "List" ).childCount];
				for( int i = 0; i < deserialized.Length; i++ )
				{
					deserialized[i] = new BuildingDefinition( "<missing>" );
				}
				serializer.DeserializeArray( "List", deserialized );

				buildingDefinitions = deserialized;
			}
			catch( System.Exception )
			{
				throw new System.Exception( "Can't load buildings from file '" + path + "'." );
			}
		}


		public static void LoadProjectileDefinitions( string levelIdentifier )
		{
			string path = LevelManager.GetFullDataPath( levelIdentifier, "projectiles.kff" );

			if( !System.IO.File.Exists( path ) )
			{
				throw new System.Exception( "Can't open file '" + path + "' - file doesn't exist." );
			}
			try
			{
				KFFSerializer serializer = KFFSerializer.ReadFromFile( path, FILE_ENCODING );
				
				ProjectileDefinition[] deserialized = new ProjectileDefinition[serializer.Analyze( "List" ).childCount];
				for( int i = 0; i < deserialized.Length; i++ )
				{
					deserialized[i] = new ProjectileDefinition( "<missing>" );
				}
				serializer.DeserializeArray( "List", deserialized );

				projectileDefinitions = deserialized;
			}
			catch( System.Exception )
			{
				throw new System.Exception( "Can't load projectiles from file '" + path + "'." );
			}
		}


		public static void LoadHeroDefinitions( string levelIdentifier )
		{
			string path = LevelManager.GetFullDataPath( levelIdentifier, "heroes.kff" );

			if( !System.IO.File.Exists( path ) )
			{
				throw new System.Exception( "Can't open file '" + path + "' - file doesn't exist." );
			}
			try
			{
				KFFSerializer serializer = KFFSerializer.ReadFromFile( path, FILE_ENCODING );
				
				HeroDefinition[] deserialized = new HeroDefinition[serializer.Analyze( "List" ).childCount];
				for( int i = 0; i < deserialized.Length; i++ )
				{
					deserialized[i] = new HeroDefinition( "<missing>" );
				}
				serializer.DeserializeArray( "List", deserialized );

				heroDefinitions = deserialized;
			}
			catch( System.Exception )
			{
				throw new System.Exception( "Can't load heroes from file '" + path + "'." );
			}
		}


		public static void LoadExtraDefinitions( string levelIdentifier )
		{
			string path = LevelManager.GetFullDataPath( levelIdentifier, "extras.kff" );

			if( !System.IO.File.Exists( path ) )
			{
				throw new System.Exception( "Can't open file '" + path + "' - file doesn't exist." );
			}
			try
			{
				KFFSerializer serializer = KFFSerializer.ReadFromFile( path, FILE_ENCODING );
				
				ExtraDefinition[] deserialized = new ExtraDefinition[serializer.Analyze( "List" ).childCount];
				for( int i = 0; i < deserialized.Length; i++ )
				{
					deserialized[i] = new ExtraDefinition( "<missing>" );
				}
				serializer.DeserializeArray( "List", deserialized );

				extraDefinitions = deserialized;
			}
			catch( System.Exception )
			{
				throw new System.Exception( "Can't load extras from file '" + path + "'." );
			}
		}

		public static void LoadResourceDepositDefinitions( string levelIdentifier )
		{
			string path = LevelManager.GetFullDataPath( levelIdentifier, "resource_deposits.kff" );

			if( !System.IO.File.Exists( path ) )
			{
				throw new System.Exception( "Can't open file '" + path + "' - file doesn't exist." );
			}
			try
			{
				KFFSerializer serializer = KFFSerializer.ReadFromFile( path, FILE_ENCODING );
			
				ResourceDepositDefinition[] deserialized = new ResourceDepositDefinition[serializer.Analyze( "List" ).childCount];
				for( int i = 0; i < deserialized.Length; i++ )
				{
					deserialized[i] = new ResourceDepositDefinition( "<missing>" );
				}
				serializer.DeserializeArray( "List", deserialized );

				resourceDepositDefinitions = deserialized;
			}
			catch( System.Exception )
			{
				throw new System.Exception( "Can't load resource deposits from file '" + path + "'." );
			}
		}

		// ///////////////////////////////////////////////

		public static void LoadResourceDefinitions( string levelIdentifier )
		{
			string path = LevelManager.GetFullDataPath( levelIdentifier, "resources.kff" );

			if( !System.IO.File.Exists( path ) )
			{
				throw new System.Exception( "Can't open file '" + path + "' - file doesn't exist." );
			}
			try
			{
				KFFSerializer serializer = KFFSerializer.ReadFromFile( path, FILE_ENCODING );
				
				ResourceDefinition[] deserialized = new ResourceDefinition[serializer.Analyze( "List" ).childCount];
				for( int i = 0; i < deserialized.Length; i++ )
				{
					deserialized[i] = new ResourceDefinition( "<missing>" );
				}
				serializer.DeserializeArray( "List", deserialized );

				resourceDefinitions = deserialized;
			}
			catch( System.Exception )
			{
				throw new System.Exception( "Can't load resources from file '" + path + "'." );
			}
		}


		public static void LoadTechnologyDefinitions( string levelIdentifier )
		{
			string path = LevelManager.GetFullDataPath( levelIdentifier, "technologies.kff" );

			if( !System.IO.File.Exists( path ) )
			{
				throw new System.Exception( "Can't open file '" + path + "' - file doesn't exist." );
			}
			try
			{
				KFFSerializer serializer = KFFSerializer.ReadFromFile( path, FILE_ENCODING );
				
				TechnologyDefinition[] deserialized = new TechnologyDefinition[serializer.Analyze( "List" ).childCount];
				for( int i = 0; i < deserialized.Length; i++ )
				{
					deserialized[i] = new TechnologyDefinition( "<missing>" );
				}
				serializer.DeserializeArray( "List", deserialized );

				technologyDefinitions = deserialized;
			}
			catch( System.Exception )
			{
				throw new System.Exception( "Can't load technologies from file '" + path + "'." );
			}
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		// Getters.

		public static UnitDefinition GetUnit( string id )
		{
			for( int i = 0; i < unitDefinitions.Length; i++ )
			{
				if( unitDefinitions[i].id == id )
				{
					return unitDefinitions[i];
				}
			}
			throw new System.Exception( "A unit with an id '" + id + "' is not registered." );
		}
		public static UnitDefinition[] GetAllUnits()
		{
			UnitDefinition[] ret = new UnitDefinition[unitDefinitions.Length];
			for( int i = 0; i < ret.Length; i++ )
			{
				ret[i] = unitDefinitions[i];
			}
			return ret;
		}


		public static BuildingDefinition GetBuilding( string id )
		{
			for( int i = 0; i < buildingDefinitions.Length; i++ )
			{
				if( buildingDefinitions[i].id == id )
				{
					return buildingDefinitions[i];
				}
			}
			throw new System.Exception( "A building with an id '" + id + "' is not registered." );
		}

		public static BuildingDefinition[] GetAllBuildings()
		{
			BuildingDefinition[] ret = new BuildingDefinition[buildingDefinitions.Length];
			for( int i = 0; i < ret.Length; i++ )
			{
				ret[i] = buildingDefinitions[i];
			}
			return ret;
		}


		public static ProjectileDefinition GetProjectile( string id )
		{
			for( int i = 0; i < projectileDefinitions.Length; i++ )
			{
				if( projectileDefinitions[i].id == id )
				{
					return projectileDefinitions[i];
				}
			}
			throw new System.Exception( "A projectile with an id '" + id + "' is not registered." );
		}

		public static ProjectileDefinition[] GetAllProjectiles()
		{
			ProjectileDefinition[] ret = new ProjectileDefinition[projectileDefinitions.Length];
			for( int i = 0; i < ret.Length; i++ )
			{
				ret[i] = projectileDefinitions[i];
			}
			return ret;
		}


		public static HeroDefinition GetHero( string id )
		{
			for( int i = 0; i < heroDefinitions.Length; i++ )
			{
				if( heroDefinitions[i].id == id )
				{
					return heroDefinitions[i];
				}
			}
			throw new System.Exception( "A hero with an id '" + id + "' is not registered." );
		}

		public static HeroDefinition[] GetAllHeroes()
		{
			HeroDefinition[] ret = new HeroDefinition[heroDefinitions.Length];
			for( int i = 0; i < ret.Length; i++ )
			{
				ret[i] = heroDefinitions[i];
			}
			return ret;
		}


		public static ExtraDefinition GetExtra( string id )
		{
			for( int i = 0; i < extraDefinitions.Length; i++ )
			{
				if( extraDefinitions[i].id == id )
				{
					return extraDefinitions[i];
				}
			}
			throw new System.Exception( "An extra with an id '" + id + "' is not registered." );
		}

		public static ExtraDefinition[] GetAllExtras()
		{
			ExtraDefinition[] ret = new ExtraDefinition[extraDefinitions.Length];
			for( int i = 0; i < ret.Length; i++ )
			{
				ret[i] = extraDefinitions[i];
			}
			return ret;
		}


		public static ResourceDepositDefinition GetResourceDeposit( string id )
		{
			for( int i = 0; i < resourceDepositDefinitions.Length; i++ )
			{
				if( resourceDepositDefinitions[i].id == id )
				{
					return resourceDepositDefinitions[i];
				}
			}
			throw new System.Exception( "A resource deposit with an id '" + id + "' is not registered." );
		}

		public static ResourceDepositDefinition[] GetAllResourceDeposits()
		{
			ResourceDepositDefinition[] ret = new ResourceDepositDefinition[extraDefinitions.Length];
			for( int i = 0; i < ret.Length; i++ )
			{
				ret[i] = resourceDepositDefinitions[i];
			}
			return ret;
		}

		// ///////////////////////////////////////////////

		public static ResourceDefinition GetResource( string id )
		{
			for( int i = 0; i < resourceDefinitions.Length; i++ )
			{
				if( resourceDefinitions[i].id == id )
				{
					return resourceDefinitions[i];
				}
			}
			throw new System.Exception( "A resource with an id '" + id + "' is not registered." );
		}

		public static ResourceDefinition[] GetAllResources()
		{
			ResourceDefinition[] ret = new ResourceDefinition[resourceDefinitions.Length];
			for( int i = 0; i < ret.Length; i++ )
			{
				ret[i] = resourceDefinitions[i];
			}
			return ret;
		}


		public static TechnologyDefinition GetTechnology( string id )
		{
			for( int i = 0; i < technologyDefinitions.Length; i++ )
			{
				if( technologyDefinitions[i].id == id )
				{
					return technologyDefinitions[i];
				}
			}
			throw new System.Exception( "A technology with an id '" + id + "' is not registered." );
		}

		public static TechnologyDefinition[] GetAllTechnologies()
		{
			TechnologyDefinition[] ret = new TechnologyDefinition[technologyDefinitions.Length];
			for( int i = 0; i < ret.Length; i++ )
			{
				ret[i] = technologyDefinitions[i];
			}
			return ret;
		}
	}
}