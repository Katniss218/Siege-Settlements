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
			this.SetTechsUnresearched();
		}

		public Faction( string name, Color color )
		{
			this.name = name;
			this.color = color;
			this.techs = new Dictionary<string, TechnologyResearchProgress>();
			this.SetTechsUnresearched();
		}

		private void SetTechsUnresearched()
		{
			List<TechnologyDefinition> technologiesLoaded = DataManager.GetAllOfType<TechnologyDefinition>();
			for( int i = 0; i < technologiesLoaded.Count; i++ )
			{
				techs.Add( technologiesLoaded[i].id, TechnologyResearchProgress.Available );
			}
		}
		
		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.name = serializer.ReadString( "Name" );
			this.color = serializer.ReadColor( "Color" );
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Name", this.name );
			serializer.WriteColor( "", "Color", this.color );
		}
	}
}