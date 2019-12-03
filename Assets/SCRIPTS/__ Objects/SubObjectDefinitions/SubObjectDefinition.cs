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
#warning Sub-Objects are just purely graphical objects.
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

		public static SubObjectDefinition TypeIdToDefinition( string subObjectId )
		{
			if( subObjectId == MeshSubObjectDefinition.KFF_TYPEID )
			{
				return new MeshSubObjectDefinition();
			}
			if( subObjectId == ParticlesSubObjectDefinition.KFF_TYPEID )
			{
				return new ParticlesSubObjectDefinition();
			}
			throw new Exception( "Unknown Sub-Object Id '" + subObjectId + "'." );
		}

		public static string DefinitionToTypeId( SubObjectDefinition def )
		{
			if( def is MeshSubObjectDefinition )
			{
				return MeshSubObjectDefinition.KFF_TYPEID;
			}
			if( def is ParticlesSubObjectDefinition )
			{
				return ParticlesSubObjectDefinition.KFF_TYPEID;
			}
			throw new Exception( "Unknown Sub-Object type '" + def.GetType().Name + "'." );
		}
	}
}