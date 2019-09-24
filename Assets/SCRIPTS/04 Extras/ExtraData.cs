using KFF;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Contains every information to successfully round-trip an extra, to and from file.
	/// </summary>
	public class ExtraData : IKFFSerializable
	{
		public Vector3 position { get; set; }
		public Quaternion rotation { get; set; }

#warning incomplete - lacks modules (deposits).

		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.position = serializer.ReadVector3( "Position" );
			this.rotation = serializer.ReadQuaternion( "Rotation" );
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteVector3( "", "Position", this.position );
			serializer.WriteQuaternion( "", "Rotation", this.rotation );
		}
	}
}