using KFF;
using SS.Objects.Modules;
using System;

namespace SS.Levels.SaveStates
{
	public class InventoryModuleData : SSModuleData
	{
		public struct SlotData : IKFFSerializable
		{
			public string id { get; set; }
			public int amount { get; set; }


			public SlotData( InventoryModule.Slot slotGroup )
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
		

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "Slots" );
			if( analysisData.isSuccess )
			{
				this.items = new SlotData[analysisData.childCount];
				try
				{
					serializer.DeserializeArray( "Slots", this.items );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'Slots' (" + serializer.file.fileName + ")." );
				}
			}
			else
			{
				throw new Exception( "Missing 'Slots' (" + serializer.file.fileName + ")." );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.SerializeArray( "", "Slots", this.items );
		}
	}
}