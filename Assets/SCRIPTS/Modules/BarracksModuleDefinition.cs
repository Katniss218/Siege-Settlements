using KFF;
using SS.Data;
using SS.Units;
using UnityEngine;

namespace SS.Modules
{
	public class BarracksModuleDefinition : ModuleDefinition
	{
		public string[] spawnableUnits { get; set; }
		public float constructionSpeed { get; set; }

		public override void AddTo( GameObject obj )
		{
			BarracksModule barracks = obj.AddComponent<BarracksModule>();
			barracks.constructionSpeed = this.constructionSpeed;
			barracks.spawnableUnits = new UnitDefinition[this.spawnableUnits.Length];
			for( int i = 0; i < barracks.spawnableUnits.Length; i++ )
			{
				barracks.spawnableUnits[i] = DataManager.Get<UnitDefinition>( this.spawnableUnits[i] );
			}
		}

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