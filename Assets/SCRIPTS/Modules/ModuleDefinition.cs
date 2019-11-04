using KFF;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Modules
{
	public abstract class ModuleDefinition : IKFFSerializable
	{
		/// <summary>
		/// Use this to constrain to which objects this definition can be added (return true to allow, false to disallow).
		/// </summary>
		public abstract bool CheckTypeDefConstraints( Type objType );

		/// <summary>
		/// Use this to constrain to which objects this definition can be added (return true to allow, false to disallow).
		/// </summary>
		public abstract bool CheckModuleDefConstraints( List<Type> modTypes );

		public abstract void AddModule( GameObject gameObject, Guid moduleId, ModuleData data );

		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );
	}
}