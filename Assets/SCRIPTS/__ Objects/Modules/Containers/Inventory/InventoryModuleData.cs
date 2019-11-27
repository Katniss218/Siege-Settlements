using KFF;
using SS.Objects.Modules;

namespace SS.Levels.SaveStates
{
	public class InventoryModuleData : ModuleData
	{
		public struct SlotData : IKFFSerializable
		{
			public string id { get; set; }
			public int amount { get; set; }


			public SlotData( InventoryModule.SlotGroup slotGroup )
			{
				this.id = slotGroup.id;
				this.amount = slotGroup.amount;
			}


			public void DeserializeKFF( KFFSerializer serializer )
			{
				this.id = serializer.ReadString( "Id" );
				this.amount = serializer.ReadInt( "Amount" );
			}

			public void SerializeKFF( KFFSerializer serializer )
			{
				serializer.WriteString( "", "Id", this.id );
				serializer.WriteInt( "", "Amount", this.amount );
			}
		}
		
		public SlotData[] items { get; set; }


		public InventoryModuleData()
		{

		}
		

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.items = new SlotData[serializer.Analyze( "Slots" ).childCount];
			serializer.DeserializeArray( "Slots", this.items );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.SerializeArray( "", "Slots", this.items );
		}
	}
}