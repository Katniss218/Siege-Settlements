using KFF;
using UnityEngine;

namespace SS
{
	public class AreaOfInfluence : IKFFSerializable
	{
		public Vector3 center { get; set; }
		public float radius { get; set; }

		public AreaOfInfluence()
		{
			this.center = Vector3.zero;
			this.radius = 0.0f;
		}

		public AreaOfInfluence( Vector3 center, float radius )
		{
			this.center = center;
			this.radius = radius;
		}


		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.center = serializer.ReadVector3( "Center" );
#warning when parsing parhs, make sure to display in the error that it's a path, now it only tells KFFParser expected to blahblahblah.
			this.radius = serializer.ReadFloat( "Radius" );
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteVector3( "", "Center", this.center );
			serializer.WriteFloat( "", "Radius", this.radius );
		}
	}
}
