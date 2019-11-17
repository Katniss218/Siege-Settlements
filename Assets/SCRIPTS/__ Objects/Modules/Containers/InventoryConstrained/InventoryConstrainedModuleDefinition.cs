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
	public class InventoryConstrainedModuleDefinition : ModuleDefinition
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
				modTypes.Contains( typeof( InventoryConstrainedModuleDefinition ) ) ||
				modTypes.Contains( typeof( InventoryUnconstrainedModuleDefinition ) ) ||
				modTypes.Contains( typeof( ResourceDepositModuleDefinition ) ));
		}


		public override ModuleData GetIdentityData()
		{
			return new InventoryConstrainedModuleData();
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

		public override void AddModule( GameObject gameObject, Guid moduleId, ModuleData data )
		{
			InventoryConstrainedModule module = gameObject.AddComponent<InventoryConstrainedModule>();
			module.moduleId = moduleId;
			module.SetDefData( this, data );
		}
	}
}