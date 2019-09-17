using KFF;
using SS.Content;
using SS.Technologies;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	/// <summary>
	/// Represents a faction.
	/// </summary>
	public class Faction : IKFFSerializable
	{
		/// <summary>
		/// The diplomatic name of the faction.
		/// </summary>
		public string name { get; private set; }
		/// <summary>
		/// The team color of the faction.
		/// </summary>
		public Color color { get; private set; }

		/// <summary>
		/// The techs that are locked/researched/etc. for this specific faction.
		/// </summary>
		public Dictionary<string, TechnologyResearchProgress> techs { get; private set; }

		/// <summary>
		/// Creates a new, blank faction.
		/// </summary>
		public Faction()
		{
			this.name = "<missing>";
			this.color = Color.black;
			this.techs = new Dictionary<string, TechnologyResearchProgress>();
			this.LoadRegisteredTechnologies( TechnologyResearchProgress.Available );
		}

		/// <summary>
		/// Creates a new faction with a diplomatic name, and a teamcolor.
		/// </summary>
		/// <param name="name">The diplomatic name to assign to this faction.</param>
		/// <param name="color">The color to assign to this faction.</param>
		public Faction( string name, Color color )
		{
			this.name = name;
			this.color = color;
			this.techs = new Dictionary<string, TechnologyResearchProgress>();
			this.LoadRegisteredTechnologies( TechnologyResearchProgress.Available );
		}

		// Loads the registered technologies to the faction's cache.
		private void LoadRegisteredTechnologies( TechnologyResearchProgress defaultState )
		{
			List<TechnologyDefinition> technologiesLoaded = DataManager.GetAllOfType<TechnologyDefinition>();
			for( int i = 0; i < technologiesLoaded.Count; i++ )
			{
				techs.Add( technologiesLoaded[i].id, defaultState );
			}
		}
		
		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.name = serializer.ReadString( "Name" );
			this.color = serializer.ReadColor( "Color" );

			var analysisData = serializer.Analyze( "Techs" );
			if( analysisData.isFail )
			{
				throw new System.Exception( "The level file was missing per-faction technology entries." );
			}
			this.techs = new Dictionary<string, TechnologyResearchProgress>( analysisData.childCount );
			this.LoadRegisteredTechnologies( TechnologyResearchProgress.Available );
			for( int i = 0; i < analysisData.childCount; i++ )
			{
				this.techs[serializer.ReadString( "Techs." + i.ToString() + ".Key" )] = (TechnologyResearchProgress)serializer.ReadSByte( "Techs." + i.ToString() + ".Value" );
			}
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Name", this.name );
			serializer.WriteColor( "", "Color", this.color );

			serializer.WriteList( "", "Techs" );
			int i = 0;
			foreach( var value in this.techs )
			{
				if( value.Value != TechnologyResearchProgress.Available )
				{
					serializer.AppendClass( "Techs" );
					serializer.WriteString( "Techs." + i, "Key", value.Key );
					serializer.WriteSByte( "Techs." + i, "Value", (sbyte)value.Value );
				}
				i++;
			}
		}
	}
}