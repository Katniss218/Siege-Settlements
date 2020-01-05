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
		
		/// <summary>
		/// All resources in inventories belonging to this faction (stored & not stored).
		/// </summary>
		public Dictionary<string, int> resourcesAvailableCache { get; internal set; }

		/// <summary>
		/// Only stored resources belonging to this faction (inside inventories marked as storage).
		/// </summary>
		public Dictionary<string, int> resourcesStoredCache { get; internal set; }
		


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
		


		private void LoadRegisteredResources()
		{
			ResourceDefinition[] resourcesLoaded = DefinitionManager.GetAllResources();
			for( int i = 0; i < resourcesLoaded.Length; i++ )
			{
				resourcesAvailableCache.Add( resourcesLoaded[i].id, 0 );
				resourcesStoredCache.Add( resourcesLoaded[i].id, 0 );
			}
		}



		public TechnologyResearchProgress GetTech( string id )
		{
			if( this.techs.TryGetValue( id, out TechnologyResearchProgress ret ) )
			{
				return ret;
			}
			throw new System.Exception( "Unknown technology '" + id + "'." );
		}

		public Dictionary<string, TechnologyResearchProgress> GetAllTechs()
		{
			return new Dictionary<string, TechnologyResearchProgress>( techs );
		}

		// Loads the registered technologies to the faction's cache.
		private void LoadRegisteredTechnologies( TechnologyResearchProgress defaultState )
		{
			TechnologyDefinition[] technologiesLoaded = DefinitionManager.GetAllTechnologies();
			for( int i = 0; i < technologiesLoaded.Length; i++ )
			{
				techs.Add( technologiesLoaded[i].id, defaultState );
			}
		}
		
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