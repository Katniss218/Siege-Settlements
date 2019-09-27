using KFF;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Modules.Inventories
{
	public class InventoryConstrainedSaveState : IKFFSerializable
	{
		public int[] slotResourceAmounts { get; set; }


		public void DeserializeKFF( KFFSerializer serializer )
		{
			serializer.WriteIntArray( "", "SlotResourceAmounts", this.slotResourceAmounts );
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			this.slotResourceAmounts = serializer.ReadIntArray( "SlotResourceAmounts" );
		}
	}
}