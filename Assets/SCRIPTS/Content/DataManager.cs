﻿using KFF;
using SS.Buildings;
using SS.Extras;
using SS.Heroes;
using SS.Projectiles;
using SS.ResourceSystem;
using SS.Technologies;
using SS.Units;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SS.Content
{
	public static class DataManager
	{
		private const string KFF_TNAME_UNITS_LIST = "Units";
		private const string KFF_TNAME_BUILDINGS_LIST = "Buildings";
		private const string KFF_TNAME_HEROES_LIST = "Heroes";
		private const string KFF_TNAME_PROJECTILES_LIST = "Projectiles";
		private const string KFF_TNAME_RESOURCES_LIST = "Resources";
		private const string KFF_TNAME_EXTRAS_LIST = "Extras";
		private const string KFF_TNAME_RESOURCEDEPOSITS_LIST = "ResourceDeposits";
		private const string KFF_TNAME_TECHNOLOGIES_LIST = "Technologies";

		private static readonly Encoding FILE_ENCODING = Encoding.UTF8;

		private const string DEFINITION_DIRNAME = "Definitions";

		/// <summary>
		/// Returns the path to the "GameData" directory (Read Only).
		/// </summary>
		public static string dirPath
		{
			get
			{
				return Application.streamingAssetsPath + System.IO.Path.DirectorySeparatorChar + "Data";
			}
		}

		/// <summary>
		/// Converts relative path into a full system path.
		/// </summary>
		/// <param name="assetsPath">The path starting at GameData directory.</param>
		public static string GetFullPath( string assetsPath )
		{
			return dirPath + System.IO.Path.DirectorySeparatorChar + assetsPath;
		}

		private static List<Definition> registeredDefinitions = new List<Definition>();

		/// <summary>
		/// Registers a new Definition. Definitions must have a unique ID, even if they are of different definition type.
		/// </summary>
		/// <param name="d">The definition to register.</param>
		/// <exception cref="System.Exception">Thrown when the definition has already been registered.</exception>
		public static void RegisterDefinition( Definition d )
		{
			for( int i = 0; i < registeredDefinitions.Count; i++ )
			{
				if( registeredDefinitions[i].id == d.id )
				{
					throw new System.Exception( "The definition with id '" + d.id + "' has already been registered." );
				}
			}

			registeredDefinitions.Add( d );
		}

		/// <summary>
		/// Returns an already registered definition boxed as type T.
		/// </summary>
		/// <typeparam name="T">The type of definitino to return.</typeparam>
		/// <param name="id">The id of the definition.</param>
		/// <exception cref="System.Exception">Thrown when the definition is not registered or if the types don't match.</exception>
		public static T Get<T>( string id ) where T : Definition
		{
			for( int i = 0; i < registeredDefinitions.Count; i++ )
			{
				if( registeredDefinitions[i].id == id )
				{
					if( registeredDefinitions[i] is T )
					{
						return (T)registeredDefinitions[i];
					}
					throw new System.Exception( "The definition with id '" + id + "' is not of type '" + typeof( T ).ToString() + "'." );
				}
			}
			throw new System.Exception( "The definition with id '" + id + "' is not registered." );
		}
		
		/// <summary>
		/// Returns every registered definition of the specified type T.
		/// </summary>
		/// <typeparam name="T">The type of definitions to return.</typeparam>
		public static List<T> GetAllOfType<T>() where T : Definition
		{
			List<T> ret = new List<T>();
			for( int i = 0; i < registeredDefinitions.Count; i++ )
			{
				if( registeredDefinitions[i] is T )
				{
					ret.Add( (T)registeredDefinitions[i] );
				}
			}
			return ret;

		}

		/// <summary>
		/// Clears every registered definition.
		/// </summary>
		public static void ClearDefinitions()
		{
			registeredDefinitions.Clear();
		}
		
		private static void LoadUnitDefinitions( string path )
		{
			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, FILE_ENCODING );
			var analysisData = serializer.Analyze( KFF_TNAME_UNITS_LIST );
			UnitDefinition[] deserialized = new UnitDefinition[analysisData.childCount];

			for( int i = 0; i < deserialized.Length; i++ )
			{
				deserialized[i] = new UnitDefinition( "<missing>" );
			}
			serializer.DeserializeArray( KFF_TNAME_UNITS_LIST, deserialized );

			for( int i = 0; i < deserialized.Length; i++ )
			{
				DataManager.RegisterDefinition( deserialized[i] );
			}
		}

		private static void LoadBuildingDefinitions( string path )
		{
			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, FILE_ENCODING );
			var analysisData = serializer.Analyze( KFF_TNAME_BUILDINGS_LIST );
			BuildingDefinition[] deserialized = new BuildingDefinition[analysisData.childCount];

			for( int i = 0; i < deserialized.Length; i++ )
			{
				deserialized[i] = new BuildingDefinition( "<missing>" );
			}
			serializer.DeserializeArray( KFF_TNAME_BUILDINGS_LIST, deserialized );

			for( int i = 0; i < deserialized.Length; i++ )
			{
				DataManager.RegisterDefinition( deserialized[i] );
			}
		}

		private static void LoadHeroDefinitions( string path )
		{
			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, FILE_ENCODING );
			var analysisData = serializer.Analyze( KFF_TNAME_HEROES_LIST );
			HeroDefinition[] deserialized = new HeroDefinition[analysisData.childCount];

			for( int i = 0; i < deserialized.Length; i++ )
			{
				deserialized[i] = new HeroDefinition( "<missing>" );
			}
			serializer.DeserializeArray( KFF_TNAME_HEROES_LIST, deserialized );

			for( int i = 0; i < deserialized.Length; i++ )
			{
				DataManager.RegisterDefinition( deserialized[i] );
			}
		}

		private static void LoadProjectileDefinitions( string path )
		{
			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, FILE_ENCODING );
			var analysisData = serializer.Analyze( KFF_TNAME_PROJECTILES_LIST );
			ProjectileDefinition[] deserialized = new ProjectileDefinition[analysisData.childCount];

			for( int i = 0; i < deserialized.Length; i++ )
			{
				deserialized[i] = new ProjectileDefinition( "<missing>" );
			}
			serializer.DeserializeArray( KFF_TNAME_PROJECTILES_LIST, deserialized );

			for( int i = 0; i < deserialized.Length; i++ )
			{
				DataManager.RegisterDefinition( deserialized[i] );
			}

		}

		private static void LoadResourceDefinitions( string path )
		{
			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, FILE_ENCODING );
			var analysisData = serializer.Analyze( KFF_TNAME_RESOURCES_LIST );
			ResourceDefinition[] deserialized = new ResourceDefinition[analysisData.childCount];

			for( int i = 0; i < deserialized.Length; i++ )
			{
				deserialized[i] = new ResourceDefinition( "<missing>" );
			}
			serializer.DeserializeArray( KFF_TNAME_RESOURCES_LIST, deserialized );

			for( int i = 0; i < deserialized.Length; i++ )
			{
				DataManager.RegisterDefinition( deserialized[i] );
			}
		}

		private static void LoadExtraDefinitions( string path )
		{
			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, FILE_ENCODING );
			var analysisData = serializer.Analyze( KFF_TNAME_EXTRAS_LIST );
			ExtraDefinition[] deserialized = new ExtraDefinition[analysisData.childCount];

			for( int i = 0; i < deserialized.Length; i++ )
			{
				deserialized[i] = new ExtraDefinition( "<missing>" );
			}
			serializer.DeserializeArray( KFF_TNAME_EXTRAS_LIST, deserialized );

			for( int i = 0; i < deserialized.Length; i++ )
			{
				DataManager.RegisterDefinition( deserialized[i] );
			}
		}

		private static void LoadResourceDepositDefinitions( string path )
		{
			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, FILE_ENCODING );
			var analysisData = serializer.Analyze( KFF_TNAME_RESOURCEDEPOSITS_LIST );
			ResourceDepositDefinition[] deserialized = new ResourceDepositDefinition[analysisData.childCount];

			for( int i = 0; i < deserialized.Length; i++ )
			{
				deserialized[i] = new ResourceDepositDefinition( "<missing>" );
			}
			serializer.DeserializeArray( KFF_TNAME_RESOURCEDEPOSITS_LIST, deserialized );

			for( int i = 0; i < deserialized.Length; i++ )
			{
				DataManager.RegisterDefinition( deserialized[i] );
			}
		}

		private static void LoadTechnologyDefinitions( string path )
		{
			KFFSerializer serializer = KFFSerializer.ReadFromFile( path, FILE_ENCODING );
			var analysisData = serializer.Analyze( KFF_TNAME_TECHNOLOGIES_LIST );
			TechnologyDefinition[] deserialized = new TechnologyDefinition[analysisData.childCount];

			for( int i = 0; i < deserialized.Length; i++ )
			{
				deserialized[i] = new TechnologyDefinition( "<missing>" );
			}
			serializer.DeserializeArray( KFF_TNAME_TECHNOLOGIES_LIST, deserialized );

			for( int i = 0; i < deserialized.Length; i++ )
			{
				DataManager.RegisterDefinition( deserialized[i] );
			}

		}

		public static void LoadDefaults()
		{
			// Clear any residual definitions (if opening another level).
			DataManager.ClearDefinitions();

			string definitionsFullPath = GetFullPath( DEFINITION_DIRNAME );
			
			// Load each type of definition.
			LoadUnitDefinitions( definitionsFullPath + System.IO.Path.DirectorySeparatorChar + "Units.kff" );

			LoadBuildingDefinitions( definitionsFullPath + System.IO.Path.DirectorySeparatorChar + "Buildings.kff" );

			LoadHeroDefinitions( definitionsFullPath + System.IO.Path.DirectorySeparatorChar + "Heroes.kff" );

			LoadProjectileDefinitions( definitionsFullPath + System.IO.Path.DirectorySeparatorChar + "Projectiles.kff" );
			
			LoadResourceDefinitions( definitionsFullPath + System.IO.Path.DirectorySeparatorChar + "Resources.kff" );

			LoadExtraDefinitions( definitionsFullPath + System.IO.Path.DirectorySeparatorChar + "Extras.kff" );

			LoadResourceDepositDefinitions( definitionsFullPath + System.IO.Path.DirectorySeparatorChar + "ResourceDeposits.kff" );

			LoadTechnologyDefinitions( definitionsFullPath + System.IO.Path.DirectorySeparatorChar + "Technologies.kff" );
		}

		public static void LoadFromLevel( string pathToLevel )
		{
			// Loads the definitions from the level file, overriding any existing fields.
		}
	}
}