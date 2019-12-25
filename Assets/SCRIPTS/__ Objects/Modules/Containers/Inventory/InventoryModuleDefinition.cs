using KFF;
using SS.Objects.Buildings;
using SS.Objects.Extras;
using SS.Objects.Heroes;
using SS.Levels.SaveStates;
using SS.Objects.Units;
using System;
using System.Collections.Generic;
using UnityEngine;
using SS.Content;

namespace SS.Objects.Modules
{
	public class InventoryModuleDefinition : ModuleDefinition
	{
		public struct SlotDefinition : IKFFSerializable
		{
			public string slotId { get; set; }
			public int capacity { get; set; }
			

			public void DeserializeKFF( KFFSerializer serializer )
			{
				this.slotId = serializer.ReadString( "SlotId" );
				this.capacity = serializer.ReadInt( "Capacity" );
			}

			public void SerializeKFF( KFFSerializer serializer )
			{
				serializer.WriteString( "", "SlotId", this.slotId );
				serializer.WriteInt( "", "Capacity", this.capacity );
			}
		}


		public SlotDefinition[] slots { get; set; }


		public override bool CheckTypeDefConstraints( Type objType )
		{
			return
				objType == typeof( UnitDefinition ) ||
				objType == typeof( BuildingDefinition ) ||
				objType == typeof( HeroDefinition ) ||
				objType == typeof( ExtraDefinition );
		}

		public override bool CheckModuleDefConstraints( List<Type> modTypes )
		{
			return !(
				modTypes.Contains( typeof( InventoryModuleDefinition ) ) ||
				modTypes.Contains( typeof( ResourceDepositModuleDefinition ) ));
		}

		public override void AddModule( GameObject gameObject, Guid moduleId )
		{
			InventoryModule module = gameObject.AddComponent<InventoryModule>();
			module.moduleId = moduleId;
			module.icon = this.icon;
#warning some sort of method for setting the slots(?)
			module.slotGroups = new InventoryModule.SlotGroup[this.slots.Length];
			for( int i = 0; i < module.slotCount; i++ )
			{
				module.slotGroups[i] = new InventoryModule.SlotGroup( this.slots[i].slotId, this.slots[i].capacity );
			}
		}
		

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "Slots" );
			if( analysisData.isSuccess )
			{
				this.slots = new SlotDefinition[analysisData.childCount];
				try
				{
					serializer.DeserializeArray( "Slots", this.slots );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'Slots' (" + serializer.file.fileName + ")." );
				}
			}
			else
			{
				throw new Exception( "Missing or invalid value of 'Slots' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.icon = serializer.ReadSpriteFromAssets( "Icon" );
			}
			catch( KFFException )
			{
				throw new Exception( "Missing 'Icon' (" + serializer.file.fileName + ")." );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.SerializeArray( "", "Slots", this.slots );
			serializer.WriteString( "", "Icon", (string)this.icon );
		}
	}
}