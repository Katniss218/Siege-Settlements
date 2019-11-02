using KFF;
using System;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Contains every information to (with a definition) successfully round-trip a unit, to and from file.
	/// </summary>
	public class UnitData : IKFFSerializable
	{
		public Guid guid { get; set; }

		public Vector3 position { get; set; }
		public Quaternion rotation { get; set; }

		public int factionId { get; set; }

		public float health { get; set; }

		public MeleeModuleData meleeData { get; set; }
		public RangedModuleData rangedData { get; set; }

		public InventoryUnconstrainedData inventoryData { get; set; }

		public TAIGoalData taiGoalData { get; set; }

		// when a module save state is present on the save state, and it's not present on the definition - throw an exception.
		// not every module needs save state, but every save state needs module.

		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.guid = Guid.ParseExact( serializer.ReadString( "Guid" ), "D" );

			this.position = serializer.ReadVector3( "Position" );
			this.rotation = serializer.ReadQuaternion( "Rotation" );

			this.factionId = serializer.ReadInt( "FactionId" );
			this.health = serializer.ReadFloat( "Health" );
			
			if( serializer.Analyze( "MeleeModuleData").isSuccess )
			{
				this.meleeData = new MeleeModuleData();
				serializer.Deserialize( "MeleeModuleData", this.meleeData );
			}

			if( serializer.Analyze( "RangedModuleData" ).isSuccess )
			{
				this.rangedData = new RangedModuleData();
				serializer.Deserialize( "RangedModuleData", this.rangedData );
			}
			
			inventoryData = new InventoryUnconstrainedData();
			serializer.Deserialize( "InventoryData", inventoryData );

			this.taiGoalData = TAIGoalData.DeserializeUnknownType( serializer );
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Guid", this.guid.ToString( "D" ) );

			serializer.WriteVector3( "", "Position", this.position );
			serializer.WriteQuaternion( "", "Rotation", this.rotation );

			serializer.WriteInt( "", "FactionId", this.factionId );
			serializer.WriteFloat( "", "Health", this.health );

			if( this.meleeData != null )
			{
				serializer.Serialize( "", "MeleeModuleData", this.meleeData );
			}

			if( this.rangedData != null )
			{
				serializer.Serialize( "", "RangedModuleData", this.rangedData );
			}

			serializer.Serialize( "", "InventoryData", this.inventoryData );

			TAIGoalData.SerializeUnknownType( serializer, this.taiGoalData );
		}
	}
}