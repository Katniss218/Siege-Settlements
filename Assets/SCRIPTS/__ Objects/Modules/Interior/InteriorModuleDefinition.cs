using KFF;
using SS.Objects.Buildings;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class InteriorModuleDefinition : ModuleDefinition
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
			InteriorModule interior = gameObject.AddComponent<InteriorModule>();

			interior.slots = new InteriorModule.Slot[1];
			interior.slots[0] = new InteriorModule.Slot() { localPos = new Vector3( 0.0f, 0.5f, 0.0f ), localRot = Quaternion.identity };

			interior.entrancePosition = new Vector3( 0.0f, 0.0f, 0.5f );
			interior.entranceRotation = Quaternion.identity;
			interior.moduleId = moduleId;
		}
		

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			
		}
	}
}