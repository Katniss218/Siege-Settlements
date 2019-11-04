using KFF;
using SS.Content;
using SS.Modules;
using System;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Contains every information to (with a definition) successfully round-trip a unit, to and from file.
	/// </summary>
	public class UnitData : ObjectData
	{
		public Guid guid { get; set; }

		public Vector3 position { get; set; }
		public Quaternion rotation { get; set; }

		public int factionId { get; set; }

		public float health { get; set; }

		//public ModuleData meleeData { get; set; }
		//public ModuleData rangedData { get; set; }

		//public InventoryUnconstrainedData inventoryData { get; set; }

		public TAIGoalData taiGoalData { get; set; }

		// when a module save state is present on the save state, and it's not present on the definition - throw an exception.
		// not every module needs save state, but every save state needs module.

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.guid = Guid.ParseExact( serializer.ReadString( "Guid" ), "D" );

			this.position = serializer.ReadVector3( "Position" );
			this.rotation = serializer.ReadQuaternion( "Rotation" );

			this.factionId = serializer.ReadInt( "FactionId" );
			this.health = serializer.ReadFloat( "Health" );
			
			//inventoryData = new InventoryUnconstrainedData();
			//serializer.Deserialize( "InventoryData", inventoryData );

			this.taiGoalData = TAIGoalData.DeserializeUnknownType( serializer );

			this.DeserializeModulesKFF( serializer );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Guid", this.guid.ToString( "D" ) );

			serializer.WriteVector3( "", "Position", this.position );
			serializer.WriteQuaternion( "", "Rotation", this.rotation );

			serializer.WriteInt( "", "FactionId", this.factionId );
			serializer.WriteFloat( "", "Health", this.health );
			
			//serializer.Serialize( "", "InventoryData", this.inventoryData );

			TAIGoalData.SerializeUnknownType( serializer, this.taiGoalData );

			this.SerializeModulesKFF( serializer );
		}
	}
}