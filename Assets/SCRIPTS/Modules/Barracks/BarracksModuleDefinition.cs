using KFF;

namespace SS.Modules
{
	public class BarracksModuleDefinition : ModuleDefinition
	{
		public string[] trainableUnits { get; set; }
		public float trainSpeed { get; set; }
		

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