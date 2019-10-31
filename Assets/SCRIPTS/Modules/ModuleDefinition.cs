using KFF;
using UnityEngine;

namespace SS.Modules
{
	public abstract class ModuleDefinition : IKFFSerializable
	{
		/// <summary>
		/// Checks if a module can be added to an object.
		/// </summary>
		public abstract bool CanBeAddedTo( GameObject gameObject );
		
		/*
		public abstract ObjectType objectTypeConstraint { get; }
		
		public bool CanBeAddedTo( ObjectType objType )
		{
			int objTypeInt = (int)objType;
			int objectTypeConstraintInt = (int)objectTypeConstraint;

			if( (objTypeInt & objectTypeConstraintInt) == objTypeInt )
			{
				return true;
			}
			return false;
		}
		*/
		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );
	}
}