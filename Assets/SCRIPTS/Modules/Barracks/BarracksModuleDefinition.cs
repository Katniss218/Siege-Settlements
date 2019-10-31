using KFF;
using SS.Levels.SaveStates;
using UnityEngine;

namespace SS.Modules
{
	public class BarracksModuleDefinition : ModuleDefinition
	{
		public string[] trainableUnits { get; set; }
		public float trainSpeed { get; set; }

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
			this.trainSpeed = serializer.ReadFloat( "TrainSpeed" );
			this.trainableUnits = serializer.ReadStringArray( "TrainableUnits" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteFloat( "", "TrainSpeed", this.trainSpeed );
			serializer.WriteStringArray( "", "TrainableUnits", this.trainableUnits );
		}
	}
}