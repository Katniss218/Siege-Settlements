using KFF;
using SS.Content;
using SS.Technologies;
using System.Collections.Generic;

namespace SS.Diplomacy
{
	/// <summary>
	/// Represents a faction's data that changes.
	/// </summary>
	public class FactionData : IKFFSerializable
	{
#warning Ugly code - The 'techs' field is here but access to it is from LevelDataManager.
		/// <summary>
		/// The techs that are locked/researched/etc. for this specific faction.
		/// </summary>
		public Dictionary<string, TechnologyResearchProgress> techs = new Dictionary<string, TechnologyResearchProgress>();

		/// <summary>
		/// Creates a new, blank faction.
		/// </summary>
		public FactionData()
		{
			this.techs = new Dictionary<string, TechnologyResearchProgress>();
			this.LoadRegisteredTechnologies( TechnologyResearchProgress.Available );
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
			var analysisData = serializer.Analyze( "Techs" );
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
			foreach( var value in this.techs )
			{
				if( value.Value != TechnologyResearchProgress.Available )
				{
					serializer.AppendClass( "Techs" );
					serializer.WriteString( new Path( "Techs.{0}", i), "Id", value.Key );
					serializer.WriteSByte( new Path( "Techs.{0}", i), "Progress", (sbyte)value.Value );
				}
				i++;
			}
		}
	}
}