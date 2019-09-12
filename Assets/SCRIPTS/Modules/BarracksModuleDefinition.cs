using KFF;
using UnityEngine;

namespace SS.Modules
{
	public class BarracksModuleDefinition : ModuleDefinition
	{
		public string[] spawnableUnits { get; set; }
		public float constructionSpeed { get; set; }
		
		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.constructionSpeed = serializer.ReadFloat( "ConstructionSpeed" );
			this.spawnableUnits = serializer.ReadStringArray( "SpawnableUnits" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteFloat( "", "ConstructionSpeed", this.constructionSpeed );
			serializer.WriteStringArray( "", "SpawnableUnits", this.spawnableUnits );
		}
	}
}