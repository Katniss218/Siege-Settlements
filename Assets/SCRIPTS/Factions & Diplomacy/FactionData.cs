using KFF;
using SS.Content;
using SS.ResourceSystem;
using SS.Technologies;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Diplomacy
{
	/// <summary>
	/// Represents a faction's data that changes.
	/// </summary>
	public class FactionData : IKFFSerializable
	{
		/// <summary>
		/// The techs that are locked/researched/etc. for this specific faction.
		/// </summary>
		public Dictionary<string, TechnologyResearchProgress> techs = new Dictionary<string, TechnologyResearchProgress>();


		//
		//
		//

		static List<T> __GetEqualOrAboveThreshold<T>( Dictionary<T, int> container, int threshold )
		{
			// returns every key for which the value is at least equal the specified threshold.
			List<T> resources = new List<T>();
			foreach( var kvp in container )
			{
				if( kvp.Value >= threshold )
				{
					resources.Add( kvp.Key );
				}
			}
			return resources;
		}

		/// <summary>
		/// All resources in inventories belonging to this faction (stored & not stored).
		/// </summary>
		public Dictionary<string, int> resourcesAvailableCache { get; internal set; }

		/// <summary>
		/// Only stored resources belonging to this faction (inside inventories marked as storage).
		/// </summary>
		public Dictionary<string, int> resourcesStoredCache { get; internal set; }
		

		public int populationCache { get; internal set; }
		public int maxPopulationCache { get; internal set; }

		/// <summary>
		/// Returns a list of resources belonging to this faction, that this faction has at least the specified amount.
		/// </summary>
		/// <param name="threshold">The amount of resource must be at least equal to this amount.</param>
		public List<string> GetResourcesAvailable( int threshold = 0 )
		{
			return __GetEqualOrAboveThreshold( this.resourcesAvailableCache, threshold );
		}

		/// <summary>
		/// Returns a list of resources in storage of this faction, that this faction has at least the specified amount.
		/// </summary>
		/// <param name="threshold">The amount of resource must be at least equal to this amount.</param>
		public List<string> GetResourcesStored( int threshold = 0 )
		{
			return __GetEqualOrAboveThreshold( this.resourcesStoredCache, threshold );
		}

		//
		//
		//

		/// <summary>
		/// Creates a new, blank faction.
		/// </summary>
		public FactionData()
		{
			this.techs = new Dictionary<string, TechnologyResearchProgress>();
			this.LoadRegisteredTechnologies( TechnologyResearchProgress.Available );
			this.resourcesAvailableCache = new Dictionary<string, int>();
			this.resourcesStoredCache = new Dictionary<string, int>();
			this.LoadRegisteredResources();
		}
		

		//
		//
		//

		
		private void LoadRegisteredResources()
		{
			// Loads the registered resources to the faction's cache.
			ResourceDefinition[] resourcesLoaded = DefinitionManager.GetAllResources();
			for( int i = 0; i < resourcesLoaded.Length; i++ )
			{
				resourcesAvailableCache.Add( resourcesLoaded[i].id, 0 );
				resourcesStoredCache.Add( resourcesLoaded[i].id, 0 );
			}
		}

		private void LoadRegisteredTechnologies( TechnologyResearchProgress defaultState = TechnologyResearchProgress.Available )
		{
			// Loads the registered technologies to the faction's cache. Can specify a default state.
			TechnologyDefinition[] technologiesLoaded = DefinitionManager.GetAllTechnologies();
			for( int i = 0; i < technologiesLoaded.Length; i++ )
			{
				techs.Add( technologiesLoaded[i].id, defaultState );
			}
		}

		/// <summary>
		/// Returns a state of the technology with the specified id.
		/// </summary>
		public TechnologyResearchProgress GetTech( string id )
		{
			if( this.techs.TryGetValue( id, out TechnologyResearchProgress ret ) )
			{
				return ret;
			}
			throw new System.Exception( "Unknown technology '" + id + "'." );
		}

		/// <summary>
		/// Returns the state of every technology currently registered.
		/// </summary>
		public Dictionary<string, TechnologyResearchProgress> GetAllTechs()
		{
			return new Dictionary<string, TechnologyResearchProgress>( techs );
		}


		//
		//
		//

		
		public void DeserializeKFF( KFFSerializer serializer )
		{
			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "Techs" );
			if( analysisData.isFail )
			{
				throw new System.Exception( "The level file was missing per-faction technology entries." );
			}
			this.techs = new Dictionary<string, TechnologyResearchProgress>( analysisData.childCount );
			this.LoadRegisteredTechnologies( TechnologyResearchProgress.Available );
			for( int i = 0; i < analysisData.childCount; i++ )
			{
				this.techs[serializer.ReadString( new Path( "Techs.{0}.Id", i ) )] = (TechnologyResearchProgress)serializer.ReadSByte( new Path( "Techs.{0}.Progress", i ) );
			}
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteList( "", "Techs" );
			int i = 0;
			foreach( var kvp in this.techs )
			{
				if( kvp.Value != TechnologyResearchProgress.Available )
				{
					serializer.AppendClass( "Techs" );
					serializer.WriteString( new Path( "Techs.{0}", i), "Id", kvp.Key );
					serializer.WriteSByte( new Path( "Techs.{0}", i), "Progress", (sbyte)kvp.Value );
					i++;
				}
			}
		}
	}
}