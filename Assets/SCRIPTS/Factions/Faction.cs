using KFF;
using SS.Data;
using SS.Technologies;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	public class Faction : IKFFSerializable
	{
		public string name { get; private set; }
		public Color color { get; private set; }

		public Dictionary<string, TechnologyResearchProgress> techs { get; private set; }

		public Faction()
		{
			this.name = "<missing>";
			this.color = Color.black;
			this.techs = new Dictionary<string, TechnologyResearchProgress>();
			this.LoadRegisteredTechnologies( TechnologyResearchProgress.Available );
		}

		public Faction( string name, Color color )
		{
			this.name = name;
			this.color = color;
			this.techs = new Dictionary<string, TechnologyResearchProgress>();
			this.LoadRegisteredTechnologies( TechnologyResearchProgress.Available );
		}

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