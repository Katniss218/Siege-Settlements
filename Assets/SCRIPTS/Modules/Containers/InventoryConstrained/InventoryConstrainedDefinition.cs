using KFF;
using SS.Buildings;
using SS.Extras;
using SS.Heroes;
using SS.Units;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Modules.Inventories
{
	public class InventoryConstrainedDefinition : ModuleDefinition
	{
		public const string KFF_TYPEID = "inventory_constrained";

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
				modTypes.Contains( typeof( InventoryConstrainedDefinition ) ) ||
				modTypes.Contains( typeof( InventoryUnconstrainedDefinition ) ) ||
				modTypes.Contains( typeof( ResourceDepositModuleDefinition ) ));
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

		public override void AddModule( GameObject gameObject, ModuleData data )
		{
			InventoryConstrained module = gameObject.AddComponent<InventoryConstrained>();
			module.SetDefData( this, data );
		}
	}
}