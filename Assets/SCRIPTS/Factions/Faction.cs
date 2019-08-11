using KFF;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	public struct Faction : IKFFSerializable
	{
		public string name { get; private set; }
		public Color color { get; private set; }

		public Faction( string name, Color color )
		{
			this.name = name;
			this.color = color;
		}

		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.name = serializer.ReadString( "Name" );
			this.color = serializer.ReadColor( "Color" );
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Name", this.name );
			serializer.WriteColor( "", "Color", this.color );
		}
	}
}