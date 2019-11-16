﻿using KFF;
using SS.Buildings;
using SS.Extras;
using SS.Heroes;
using SS.Levels.SaveStates;
using SS.Units;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Modules.Inventories
{
	public class InventoryUnconstrainedModuleDefinition : ModuleDefinition
	{
		public const string KFF_TYPEID = "inventory_unconstrained";

		public int slotCount { get; set; }
		public int slotCapacity { get; set; }


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
			return new InventoryUnconstrainedModuleData();
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

		public override void AddModule( GameObject gameObject, Guid moduleId, ModuleData data )
		{
			InventoryUnconstrainedModule module = gameObject.AddComponent<InventoryUnconstrainedModule>();
			module.moduleId = moduleId;
			module.SetDefData( this, data );
		}
	}
}