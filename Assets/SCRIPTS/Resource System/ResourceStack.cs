using KFF;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS.ResourceSystem
{
	/// <summary>
	/// A class representing an amount of specific resource.
	/// </summary>
	public class ResourceStack : IKFFSerializable
	{
		/// <summary>
		/// The id of the resource in the stack.
		/// </summary>
		public string id { get; set; }
		/// <summary>
		/// The amount of the resource in the stack.
		/// </summary>
		public int amount { get; private set; }
		
		public ResourceStack( string id, int amount )
		{
			this.id = id;
			this.amount = amount;
		}

		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.amount = serializer.ReadInt( "Amount" );
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteInt( "", "Amount", this.amount );
		}
	}
}