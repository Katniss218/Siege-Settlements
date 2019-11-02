using KFF;
using UnityEngine;

namespace SS.Modules.Inventories
{
	public class InventoryConstrainedDefinition : ModuleDefinition
	{
		public struct Slot : IKFFSerializable
		{
			public string resourceId { get; set; }
			public int capacity { get; set; }


			public void DeserializeKFF( KFFSerializer serializer )
			{
				this.resourceId = serializer.ReadString( "ResourceId" );
				this.capacity = serializer.ReadInt( "Capacity" );
			}

			public void SerializeKFF( KFFSerializer serializer )
			{
				serializer.WriteString( "", "ResourceId", this.resourceId );
				serializer.WriteInt( "", "Capacity", this.capacity );
			}
		}


		public Slot[] slots { get; set; }


		public override bool CanBeAddedTo( GameObject gameObject )
		{
			if( gameObject.GetComponent<IInventory>() != null )
			{
				return false;
			}
			return true;
		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.slots = new Slot[serializer.Analyze( "Slots" ).childCount];
			serializer.DeserializeArray( "Slots", this.slots );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.SerializeArray( "", "Slots", this.slots );
		}
	}
}