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

		public Vector3 localPosition { get; set; }
		public Quaternion localRotation { get; set; }


		public abstract SubObject AddTo( GameObject gameObject ); // adds the SubObject to gameobject.

		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );
	}
}