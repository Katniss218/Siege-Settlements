using KFF;
using System;
using UnityEngine;

namespace SS.Objects.SubObjects
{
	/// <summary>
	/// An abstract class that defines every sub-object.
	/// </summary>
	public abstract class SubObjectDefinition : IKFFSerializable
	{
		public Guid subObjectId { get; set; }
#warning pseudo-modules such as damageable, etc., need to target the specific subobject. They need to specify which object to crumble (if any). if a building doesn't specify constructible object, that functionality (growing with construction progress) should be removed.

		public Vector3 localPosition { get; set; }
		public Quaternion localRotation { get; set; }


		public abstract SubObject AddTo( GameObject gameObject ); // adds the SubObject to gameobject.

		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );
	}
}