using KFF;
using UnityEngine;

namespace SS.Modules
{
	public class ResearchModuleDefinition : ModuleDefinition
	{
		public float researchSpeed { get; set; }


		public override bool CanBeAddedTo( GameObject gameObject )
		{
			if( ((ObjectLayer.UNITS_MASK | ObjectLayer.BUILDINGS_MASK) & (1 << gameObject.layer)) != (1 << gameObject.layer) )
			{
				return false;
			}
			if( gameObject.GetComponent<BarracksModule>() != null ) // barracks && workplace barracks
			{
				return false;
			}
			if( gameObject.GetComponent<ResearchModule>() != null ) // barracks && workplace barracks
			{
				return false;
			}
			/*if( gameObject.GetComponent<ConstructorModule>() != null ) // allows construction of buildings, when selected.
			{
				return false;
			}*/
#warning Module class should have static method for adding modules to gameObjects. That method should check if it can be added or not.
			return true;
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.researchSpeed = serializer.ReadFloat( "ResearchSpeed" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteFloat( "", "ResearchSpeed", this.researchSpeed );
		}
	}
}