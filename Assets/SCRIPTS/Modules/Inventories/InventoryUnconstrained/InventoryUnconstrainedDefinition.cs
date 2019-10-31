using KFF;
using UnityEngine;

namespace SS.Modules.Inventories
{
	public class InventoryUnconstrainedDefinition : ModuleDefinition
	{
		public int slotCount { get; set; }
		public int slotCapacity { get; set; }


		public override bool CanBeAddedTo( GameObject gameObject )
		{
#warning TODO!
			return true;
		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.slotCount = serializer.ReadInt( "SlotCount" );
			this.slotCapacity = serializer.ReadInt( "SlotCapacity" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteInt( "", "SlotCount", this.slotCount );
			serializer.WriteInt( "", "SlotCapacity", this.slotCapacity );
		}
	}
}