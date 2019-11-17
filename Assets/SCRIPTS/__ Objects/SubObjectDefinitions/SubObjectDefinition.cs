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
		/// <summary>
		/// The Identifier of this Sub-Object.
		/// </summary>
		public Guid subObjectId { get; set; }

		/// <summary>
		/// The position relative to the base object.
		/// </summary>
		public Vector3 localPosition { get; set; }

		/// <summary>
		/// The rotation relative to the base object.
		/// </summary>
		public Quaternion localRotation { get; set; }


		/// <summary>
		/// Adds the Sub-Object to a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject to add this Sub-object to.</param>
		public abstract SubObject AddTo( GameObject gameObject );

		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );
	}
}