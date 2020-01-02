using KFF;
using SS.Objects.Buildings;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class TavernWorkplaceModuleDefinition : ModuleDefinition
	{
		public override bool CheckTypeDefConstraints( Type objType )
		{
			return
				objType == typeof( BuildingDefinition );
		}

		public override bool CheckModuleDefConstraints( List<Type> modTypes )
		{
			return true; // no module constraints
		}

		public override void AddModule( GameObject gameObject, Guid moduleId )
		{
			TavernWorkplaceModule module = gameObject.AddComponent<TavernWorkplaceModule>();
			module.moduleId = moduleId;
			
		}
		

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			
		}
	}
}