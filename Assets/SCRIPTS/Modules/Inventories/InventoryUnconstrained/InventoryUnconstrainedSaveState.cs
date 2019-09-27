using KFF;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Modules.Inventories
{
	public class InventoryUnconstrainedSaveState : IKFFSerializable
	{
		public string[] slotResourceIds { get; set; }
		public int[] slotResourceAmounts { get; set; }


		public void DeserializeKFF( KFFSerializer serializer )
		{
			serializer.WriteStringArray( "", "SlotResourceIds", this.slotResourceIds );
			serializer.WriteIntArray( "", "SlotResourceAmounts", this.slotResourceAmounts );
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			this.slotResourceIds = serializer.ReadStringArray( "SlotResourceIds" );
			this.slotResourceAmounts = serializer.ReadIntArray( "SlotResourceAmounts" );
		}
	}
}