using KFF;
using SS.Levels.SaveStates;

namespace SS.Modules
{
	public class BarracksModuleDefinition : ModuleDefinition
	{
		public string[] trainableUnits { get; set; }
		public float trainSpeed { get; set; }

		/// <summary>
		/// Returns the default save state of this definition.
		/// </summary>
		public BarracksModuleSaveState defaultSaveState
		{
			get
			{
				throw new System.Exception();
			}
		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
#warning TODO -- change the tag name.
			this.trainSpeed = serializer.ReadFloat( "ConstructionSpeed" );
			this.trainableUnits = serializer.ReadStringArray( "SpawnableUnits" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteFloat( "", "ConstructionSpeed", this.trainSpeed );
			serializer.WriteStringArray( "", "SpawnableUnits", this.trainableUnits );
		}
	}
}