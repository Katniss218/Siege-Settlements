using KFF;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Modules.Inventories
{
	public class InventoryUnconstrainedDefinition : IKFFSerializable
	{
		
		public int slotCount { get; set; }
		public int slotCapacity { get; set; }


		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.slotCount = serializer.ReadInt( "SlotCount" );
			this.slotCapacity = serializer.ReadInt( "SlotCapacity" );
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteInt( "", "SlotCount", this.slotCount );
			serializer.WriteInt( "", "SlotCapacity", this.slotCapacity );
		}
	}
}