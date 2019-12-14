using KFF;
using UnityEngine;

namespace SS.Objects.SubObjects
{
	public class LightSubObjectDefinition : SubObjectDefinition
	{
		public const string KFF_TYPEID = "LIGHT";

		public Color color { get; set; }

		public float minIntensity { get; set; }
		public float maxIntensity { get; set; }

		public float range { get; set; }

#warning TODO! - values negative

		public override SubObject AddTo( GameObject gameObject )
		{
			/*if( this.shape == null )
			{
				throw new Exception( "Shape can't be null" );
			}*/

			GameObject child = new GameObject( "Sub-Object [" + KFF_TYPEID + "] '" + this.subObjectId.ToString( "D" ) + "'" );
			child.transform.SetParent( gameObject.transform );

			child.transform.localPosition = this.localPosition;
			child.transform.localRotation = this.localRotation;




			Light light = child.AddComponent<Light>();

			light.type = LightType.Point;
			light.color = this.color;
			light.range = this.range;

			LightFlickerer flickerer = child.AddComponent<LightFlickerer>();
			flickerer.light = light;
			flickerer.minIntensity = this.minIntensity;
			flickerer.maxIntensity = this.maxIntensity;
			flickerer.speedMultiplier = 8.0f;


			SubObject subObject = child.AddComponent<SubObject>();
			subObject.subObjectId = this.subObjectId;

			return subObject;
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.subObjectId = serializer.ReadGuid( "SubObjectId" );

			this.localPosition = serializer.ReadVector3( "LocalPosition" );
			this.localRotation = Quaternion.Euler( serializer.ReadVector3( "LocalRotationEuler" ) );

			this.color = serializer.ReadColor( "Color" );
			this.minIntensity = serializer.ReadFloat( "MinIntensity" );
			this.maxIntensity = serializer.ReadFloat( "MaxIntensity" );
			this.range = serializer.ReadFloat( "Range" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteGuid( "", "SubObjectId", this.subObjectId );

			serializer.WriteColor( "", "Color", this.color );

			serializer.WriteFloat( "", "MinIntensity", this.minIntensity );
			serializer.WriteFloat( "", "MaxIntensity", this.maxIntensity );
			serializer.WriteFloat( "", "Range", this.range );
		}
	}
}