using KFF;
using System;
using UnityEngine;

namespace SS.Objects.SubObjects
{
	public class LightSubObjectDefinition : SubObjectDefinition
	{
		public const string KFF_TYPEID = "LIGHT";

		public Color color { get; set; }

		float __minIntensity;
		public float minIntensity
		{
			get
			{
				return this.__minIntensity;
			}
			set
			{
				if( value <= 0 )
				{
					throw new Exception( "Min intensity must be greater than 0." );
				}
				this.__minIntensity = value;
			}
		}
		float __maxIntensity;
		public float maxIntensity
		{
			get
			{
				return this.__maxIntensity;
			}
			set
			{
				if( value <= 0 )
				{
					throw new Exception( "Max intensity must be greater than 0." );
				}
				this.__maxIntensity = value;
			}
		}

		float __range;
		public float range
		{
			get
			{
				return this.__range;
			}
			set
			{
				if( value <= 0 )
				{
					throw new Exception( "Range must be greater than 0." );
				}
				this.__range = value;
			}
		}

		public override SubObject AddTo( GameObject gameObject )
		{
			GameObject child = new GameObject( "Sub-Object [" + KFF_TYPEID + "] '" + this.subObjectId.ToString( "D" ) + "'" );
			child.transform.SetParent( gameObject.transform );

			child.transform.localPosition = this.localPosition;
			child.transform.localRotation = this.localRotation;

			
			Light light = child.AddComponent<Light>();
			light.type = LightType.Point;
			
			LightSubObject subObject = child.AddComponent<LightSubObject>();
			subObject.subObjectId = this.subObjectId;
			subObject.minIntensity = this.minIntensity;
			subObject.maxIntensity = this.maxIntensity;
			subObject.flickerSpeed = 8.0f;
			subObject.color = this.color;
			subObject.range = this.range;

			return subObject;
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			try
			{
				this.subObjectId = serializer.ReadGuid( "SubObjectId" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'SubObjectId' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.localPosition = serializer.ReadVector3( "LocalPosition" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'LocalPosition' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.localRotation = Quaternion.Euler( serializer.ReadVector3( "LocalRotationEuler" ) );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'LocalRotationEuler' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.color = serializer.ReadColor( "Color" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Color' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.minIntensity = serializer.ReadFloat( "MinIntensity" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'MinIntensity' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.maxIntensity = serializer.ReadFloat( "MaxIntensity" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'MaxIntensity' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.range = serializer.ReadFloat( "Range" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Range' (" + serializer.file.fileName + ")." );
			}
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