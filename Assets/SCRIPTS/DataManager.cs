using KFF;
using SS.Buildings;
using SS.DataStructures;
using SS.Extras;
using SS.Projectiles;
using SS.Units;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SS
{
	public static class DataManager
	{
		/// <summary>
		/// Returns the path to the "GameData" directory (Read Only).
		/// </summary>
		public static string dirPath
		{
			get
			{
				return Application.streamingAssetsPath + System.IO.Path.DirectorySeparatorChar + "GameData";
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
		
		static List<Definition> registeredDefinitions = new List<Definition>();

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
		public static T FindDefinition<T>( string id ) where T : Definition
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

		public static void ClearDefinitions()
		{
			registeredDefinitions.Clear();
		}

		private static void LoadUnitDefinition( string path )
		{

		}

		private static void LoadBuildingDefinition( string path )
		{

		}

		private static void LoadProjectileDefinition( string path )
		{

		}

		private static void LoadResourceDefinition( string path )
		{

		}

		private static void LoadExtraDefinition( string path )
		{

		}

		private static void LoadResourceDepositDefinition( string path )
		{

		}

		public static void LoadDefaults()
		{
			DataManager.ClearDefinitions();

			string definitionsPath = "Definitions";
			string definitionsFullPath = GetFullPath( definitionsPath );


			KFFSerializer serializer = KFFSerializer.ReadFromFile( definitionsFullPath + System.IO.Path.DirectorySeparatorChar + "Units.kff", Encoding.UTF8 );
			serializer.Analyze( "Units" );
			UnitDefinition[] deserialized = new UnitDefinition[serializer.aChildCount];

			for( int i = 0; i < deserialized.Length; i++ )
			{
				deserialized[i] = new UnitDefinition( "unset" );
			}
			serializer.DeserializeArray( "Units", deserialized );

			for( int i = 0; i < deserialized.Length; i++ )
			{
				DataManager.RegisterDefinition( deserialized[i] );
			}



			serializer = KFFSerializer.ReadFromFile( definitionsFullPath + System.IO.Path.DirectorySeparatorChar + "Buildings.kff", Encoding.UTF8 );
			serializer.Analyze( "Buildings" );
			BuildingDefinition[] deserializedB = new BuildingDefinition[serializer.aChildCount];

			for( int i = 0; i < deserializedB.Length; i++ )
			{
				deserializedB[i] = new BuildingDefinition( "unset" );
			}
			serializer.DeserializeArray( "Buildings", deserializedB );

			for( int i = 0; i < deserializedB.Length; i++ )
			{
				DataManager.RegisterDefinition( deserializedB[i] );
			}



			serializer = KFFSerializer.ReadFromFile( definitionsFullPath + System.IO.Path.DirectorySeparatorChar + "Projectiles.kff", Encoding.UTF8 );
			serializer.Analyze( "Projectiles" );
			ProjectileDefinition[] deserializedP = new ProjectileDefinition[serializer.aChildCount];

			for( int i = 0; i < deserializedP.Length; i++ )
			{
				deserializedP[i] = new ProjectileDefinition( "unset" );
			}
			serializer.DeserializeArray( "Projectiles", deserializedP );

			for( int i = 0; i < deserializedP.Length; i++ )
			{
				DataManager.RegisterDefinition( deserializedP[i] );
			}

			serializer = KFFSerializer.ReadFromFile( definitionsFullPath + System.IO.Path.DirectorySeparatorChar + "Extras.kff", Encoding.UTF8 );
			serializer.Analyze( "Extras" );
			ExtraDefinition[] deserializedE = new ExtraDefinition[serializer.aChildCount];

			for( int i = 0; i < deserializedE.Length; i++ )
			{
				deserializedE[i] = new ExtraDefinition( "unset" );
			}
			serializer.DeserializeArray( "Extras", deserializedE );

			for( int i = 0; i < deserializedE.Length; i++ )
			{
				DataManager.RegisterDefinition( deserializedE[i] );
			}



			serializer = KFFSerializer.ReadFromFile( definitionsFullPath + System.IO.Path.DirectorySeparatorChar + "Resources.kff", Encoding.UTF8 );
			serializer.Analyze( "Resources" );
			ResourceDefinition[] deserializedR = new ResourceDefinition[serializer.aChildCount];

			for( int i = 0; i < deserializedR.Length; i++ )
			{
				deserializedR[i] = new ResourceDefinition( "unset" );
			}
			serializer.DeserializeArray( "Resources", deserializedR );

			for( int i = 0; i < deserializedR.Length; i++ )
			{
				DataManager.RegisterDefinition( deserializedR[i] );
			}


			serializer = KFFSerializer.ReadFromFile( definitionsFullPath + System.IO.Path.DirectorySeparatorChar + "ResourceDeposits.kff", Encoding.UTF8 );
			serializer.Analyze( "ResourceDeposits" );
			ResourceDepositDefinition[] deserializedRD = new ResourceDepositDefinition[serializer.aChildCount];

			for( int i = 0; i < deserializedRD.Length; i++ )
			{
				deserializedRD[i] = new ResourceDepositDefinition( "unset" );
			}
			serializer.DeserializeArray( "ResourceDeposits", deserializedRD );

			for( int i = 0; i < deserializedRD.Length; i++ )
			{
				DataManager.RegisterDefinition( deserializedRD[i] );
			}
		}

		public static void LoadFromLevel( string pathToLevel )
		{

		}
	}
}