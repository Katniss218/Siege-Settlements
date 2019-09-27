using KFF;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Contains every information to successfully round-trip an extra, to and from file.
	/// </summary>
	public class ResourceDepositData : IKFFSerializable
	{
		public Guid guid { get; set; }

		public Vector3 position { get; set; }
		public Quaternion rotation { get; set; }
		
		public Dictionary<string,int> resources { get; set; }


		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.guid = Guid.ParseExact( serializer.ReadString( "Guid" ), "D" );

			this.position = serializer.ReadVector3( "Position" );
			this.rotation = serializer.ReadQuaternion( "Rotation" );

			// resources
			var analysisData = serializer.Analyze( "Resources" );
			this.resources = new Dictionary<string, int>( analysisData.childCount );
			for( int i = 0; i < analysisData.childCount; i++ )
			{
				this.resources.Add( serializer.ReadString( new Path( "Resources.{0}.Id", i ) ), serializer.ReadInt( new Path( "Resources.{0}.Amount", i ) ) );
			}
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Guid", this.guid.ToString( "D" ) );

			serializer.WriteVector3( "", "Position", this.position );
			serializer.WriteQuaternion( "", "Rotation", this.rotation );

			// resources
			serializer.WriteClass( "", "Resources" );
			int i = 0;
			foreach( var kvp in this.resources )
			{
				serializer.AppendClass( "Resources" );
				serializer.WriteString( new Path( "Resources.{0}", i ), "Id", kvp.Key );
				serializer.WriteInt( new Path( "Resources.{0}", i ), "Amount", kvp.Value );
				i++;
			}
		}
	}
}