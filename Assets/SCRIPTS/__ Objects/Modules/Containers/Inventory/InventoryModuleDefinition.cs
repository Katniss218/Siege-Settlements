using KFF;
using SS.Objects.Buildings;
using SS.Objects.Extras;
using SS.Objects.Heroes;
using SS.Levels.SaveStates;
using SS.Objects.Units;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Modules.Inventories
{
	public class InventoryModuleDefinition : ModuleDefinition
	{
		public const string KFF_TYPEID = "inventory";

		public struct SlotDefinition : IKFFSerializable
		{
			public string slotId { get; set; }
			public int slotCapacity { get; set; }


			public void DeserializeKFF( KFFSerializer serializer )
			{
				this.slotId = serializer.ReadString( "SlotId" );
				this.slotCapacity = serializer.ReadInt( "SlotCapacity" );
			}

			public void SerializeKFF( KFFSerializer serializer )
			{
				serializer.WriteString( "", "SlotId", this.slotId );
				serializer.WriteInt( "", "SlotCapacity", this.slotCapacity );
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


		public override ModuleData GetIdentityData()
		{
			return new InventoryModuleData();
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.slots = new SlotDefinition[serializer.Analyze( "Slots" ).childCount];
			serializer.DeserializeArray( "Slots", this.slots );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.SerializeArray( "", "Slots", this.slots );
		}

		public override void AddModule( GameObject gameObject, Guid moduleId, ModuleData data )
		{
			InventoryModule module = gameObject.AddComponent<InventoryModule>();
			module.moduleId = moduleId;
			module.SetDefData( this, data );
		}
	}
}