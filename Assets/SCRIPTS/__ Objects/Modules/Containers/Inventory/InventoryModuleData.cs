using KFF;
using SS.Modules;
using SS.Modules.Inventories;

namespace SS.Levels.SaveStates
{
	public class InventoryModuleData : ModuleData
	{
		public struct SlotData : IKFFSerializable
		{
			public string resourceId { get; set; }
			public int resourceAmount { get; set; }

			public SlotData( InventoryModule.SlotGroup slotGroup )
			{
				this.resourceId = slotGroup.resourceId;
				this.resourceAmount = slotGroup.amount;
			}

			public void DeserializeKFF( KFFSerializer serializer )
			{
				this.resourceId = serializer.ReadString( "ResourceId" );
				this.resourceAmount = serializer.ReadInt( "ResourceAmount" );
			}

			public void SerializeKFF( KFFSerializer serializer )
			{
				serializer.WriteString( "", "ResourceId", this.resourceId );
				serializer.WriteInt( "", "ResourceAmount", this.resourceAmount );
			}
		}

		public const string KFF_TYPEID = "inventory";

		public SlotData[] items { get; set; }
		//public Dictionary<string, int> items { get; set; }


		public InventoryModuleData()
		{
			//this.items = new Dictionary<string, int>();
		}
		

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			var analysisData = serializer.Analyze( "Items" );
			this.items = new SlotData[analysisData.childCount];
			for( int i = 0; i < analysisData.childCount; i++ )
			{
				this.items[i] = new SlotData()
				{
					resourceId = serializer.ReadString( new Path( "Items.{0}.Id", i ) ),
					resourceAmount = serializer.ReadInt( new Path( "Items.{0}.Amount", i ) )
				};
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.SerializeArray( "", "Slots", this.items );
		}
	}
}