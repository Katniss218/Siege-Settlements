﻿using KFF;
using SS.Buildings;
using SS.Units;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Modules
{
	public class ResearchModuleDefinition : ModuleDefinition
	{
		public const string KFF_TYPEID = "research";

		public float researchSpeed { get; set; }


		public override bool CheckTypeDefConstraints( Type objType )
		{
			return
				objType == typeof( UnitDefinition ) ||
				objType == typeof( BuildingDefinition );
		}

		public override bool CheckModuleDefConstraints( List<Type> modTypes )
		{
			return !(
				modTypes.Contains( typeof( ResearchModuleDefinition ) ) ||
				modTypes.Contains( typeof( BarracksModuleDefinition ) ));
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.researchSpeed = serializer.ReadFloat( "ResearchSpeed" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteFloat( "", "ResearchSpeed", this.researchSpeed );
		}

		public override void AddModule( GameObject gameObject, Guid moduleId, ModuleData data )
		{
			ResearchModule module = gameObject.AddComponent<ResearchModule>();
			module.moduleId = moduleId;
			module.SetDefData( this, data );
		}
	}
}