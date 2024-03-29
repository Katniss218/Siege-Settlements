﻿using KFF;
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
		/// Adds the Sub-Object to an SSObject.
		/// </summary>
		/// <param name="ssObject">The SSObject to add this Sub-object to.</param>
		public abstract SubObject AddTo( SSObject ssObject );

		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );

		public static SubObjectDefinition TypeIdToDefinition( string subObjectId )
		{
			if( subObjectId == MeshSubObjectDefinition.KFF_TYPEID )
			{
				return new MeshSubObjectDefinition();
			}
			if( subObjectId == MeshPredicatedSubObjectDefinition.KFF_TYPEID )
			{
				return new MeshPredicatedSubObjectDefinition();
			}
			if( subObjectId == ParticlesSubObjectDefinition.KFF_TYPEID )
			{
				return new ParticlesSubObjectDefinition();
			}
			if( subObjectId == LightSubObjectDefinition.KFF_TYPEID )
			{
				return new LightSubObjectDefinition();
			}
			throw new Exception( "Unknown Sub-Object Id '" + subObjectId + "'." );
		}

		public static string DefinitionToTypeId( SubObjectDefinition def )
		{
			if( def is MeshSubObjectDefinition )
			{
				return MeshSubObjectDefinition.KFF_TYPEID;
			}
			if( def is MeshPredicatedSubObjectDefinition )
			{
				return MeshPredicatedSubObjectDefinition.KFF_TYPEID;
			}
			if( def is ParticlesSubObjectDefinition )
			{
				return ParticlesSubObjectDefinition.KFF_TYPEID;
			}
			if( def is LightSubObjectDefinition )
			{
				return LightSubObjectDefinition.KFF_TYPEID;
			}
			throw new Exception( "Unknown Sub-Object type '" + def.GetType().Name + "'." );
		}
	}
}