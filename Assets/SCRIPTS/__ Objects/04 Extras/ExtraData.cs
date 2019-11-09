using KFF;
using SS.Content;
using System;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Contains every information to successfully round-trip an extra, to and from file.
	/// </summary>
	public class ExtraData : ObjectData
	{
		public Guid guid { get; set; }

		public Vector3 position { get; set; }
		public Quaternion rotation { get; set; }

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.guid = Guid.ParseExact( serializer.ReadString( "Guid" ), "D" );
			
			this.position = serializer.ReadVector3( "Position" );
			this.rotation = serializer.ReadQuaternion( "Rotation" );
			this.DeserializeModulesKFF( serializer );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Guid", this.guid.ToString( "D" ) );

			serializer.WriteVector3( "", "Position", this.position );
			serializer.WriteQuaternion( "", "Rotation", this.rotation );
			this.SerializeModulesKFF( serializer );
		}
	}
}