using KFF;
using SS.Content;
using System;
using UnityEngine;

namespace SS.Objects.SubObjects
{
	public class ParticlesSubObjectDefinition : SubObjectDefinition
	{
		public abstract class Shape
		{
		}

		public class SphereShape : Shape
		{
			public float radius;
		}

		public class ConeShape : Shape
		{
			public float radius;
			public float angle;
		}

		public class BoxShape : Shape
		{
			public Vector3 size;
		}

		public const string KFF_TYPEID = "PARTICLES";
		
		public bool isWorldSpace { get; set; }

		public float lifetimeMin { get; set; }
		public float lifetimeMax { get; set; }
		public float emissionRateTime { get; set; }
		public float startSizeMin { get; set; }
		public float startSizeMax { get; set; }
		public float startSpeedMin { get; set; }
		public float startSpeedMax { get; set; }

		public Shape shape { get; set; }

		public Tuple<float, Vector3>[] velocityOverLifetime { get; set; }
		public Tuple<float, Color>[] colorOverLifetime { get; set; }
		public Tuple<float, float>[] sizeOverLifetime { get; set; }

		public AddressableAsset<Texture2D> particleTexture { get; set; }
		public Color? emissionColor { get; set; }


		public override SubObject AddTo( GameObject gameObject )
		{
			if( this.shape == null )
			{
				throw new Exception( "Shape can't be null" );
			}

			GameObject child = new GameObject( "Sub-Object [" + KFF_TYPEID + "] '" + this.subObjectId.ToString( "D" ) + "'" );
			child.transform.SetParent( gameObject.transform );

			child.transform.localPosition = this.localPosition;
			child.transform.localRotation = this.localRotation;

			


			ParticleSystem particleSystem = child.AddComponent<ParticleSystem>();
			
			ParticlesSubObject subObject = child.AddComponent<ParticlesSubObject>();
			subObject.subObjectId = this.subObjectId;
			subObject.SetShape( this.shape );
			subObject.SetMaterial( this.particleTexture, this.emissionColor );
			subObject.SetSimulationSpace( this.isWorldSpace ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local );
			subObject.SetLifetime( this.lifetimeMin, this.lifetimeMax );
			subObject.SetSize( this.startSizeMin, this.startSizeMax );
			subObject.SetSpeed( this.startSpeedMin, this.startSpeedMax );
			subObject.SetEmission( this.emissionRateTime );

			if( this.velocityOverLifetime != null )
			{
				subObject.SetVelocityOverLifetime( this.velocityOverLifetime );
			}

			if( this.colorOverLifetime != null )
			{
				subObject.SetColorOverLifetime( this.colorOverLifetime );
			}

			if( this.sizeOverLifetime != null )
			{
				subObject.SetSizeOverLifetime( this.sizeOverLifetime );
			}
			return subObject;
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.subObjectId = serializer.ReadGuid( "SubObjectId" );

			this.localPosition = serializer.ReadVector3( "LocalPosition" );
			this.localRotation = Quaternion.Euler( serializer.ReadVector3( "LocalRotationEuler" ) );

			this.isWorldSpace = serializer.ReadBool( "IsWorldSpace" );

			this.lifetimeMin = serializer.ReadFloat( "LifetimeMin" );
			this.lifetimeMax = serializer.ReadFloat( "LifetimeMax" );
			this.emissionRateTime = serializer.ReadFloat( "EmissionRateTime" );
			this.startSizeMin = serializer.ReadFloat( "StartSizeMin" );
			this.startSizeMax = serializer.ReadFloat( "StartSizeMax" );
			this.startSpeedMin = serializer.ReadFloat( "StartSpeedMin" );
			this.startSpeedMax = serializer.ReadFloat( "StartSpeedMax" );

			string strShape = serializer.ReadString( "Shape.Type" );
			if( strShape == "box" )
			{
				BoxShape shape = new BoxShape();
				shape.size = serializer.ReadVector3( "Shape.Size" );
				this.shape = shape;
			}
			else if( strShape == "sphere" )
			{
				SphereShape shape = new SphereShape();
				shape.radius = serializer.ReadFloat( "Shape.Radius" );
				this.shape = shape;
			}
			else if( strShape == "cone" )
			{
				ConeShape shape = new ConeShape();
				shape.radius = serializer.ReadFloat( "Shape.Radius" );
				shape.angle = serializer.ReadFloat( "Shape.Angle" );
				this.shape = shape;
			}
			else
			{
				throw new Exception( "Invalid shape '" + strShape + "'." );
			}

			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "VelocityOverLifetimeKeys" );
			if( analysisData.isSuccess )
			{
				this.velocityOverLifetime = new Tuple<float, Vector3>[analysisData.childCount];
				for( int i = 0; i < this.velocityOverLifetime.Length; i++ )
				{
					float time = serializer.ReadFloat( new Path( "VelocityOverLifetimeKeys.{0}.Time", i ) );
					Vector3 value = serializer.ReadVector3( new Path( "VelocityOverLifetimeKeys.{0}.Value", i ) );
					this.velocityOverLifetime[i] = new Tuple<float, Vector3>( time, value );
				}
			}

			analysisData = serializer.Analyze( "ColorOverLifetimeKeys" );
			if( analysisData.isSuccess )
			{
				this.colorOverLifetime = new Tuple<float, Color>[analysisData.childCount];
				for( int i = 0; i < this.colorOverLifetime.Length; i++ )
				{
					float time = serializer.ReadFloat( new Path( "ColorOverLifetimeKeys.{0}.Time", i ) );
					Color value = serializer.ReadColor( new Path( "ColorOverLifetimeKeys.{0}.Value", i ) );
					this.colorOverLifetime[i] = new Tuple<float, Color>( time, value );
				}
			}

			analysisData = serializer.Analyze( "SizeOverLifetimeKeys" );
			if( analysisData.isSuccess )
			{
				this.sizeOverLifetime = new Tuple<float, float>[analysisData.childCount];
				for( int i = 0; i < this.sizeOverLifetime.Length; i++ )
				{
					float time = serializer.ReadFloat( new Path( "SizeOverLifetimeKeys.{0}.Time", i ) );
					float value = serializer.ReadFloat( new Path( "SizeOverLifetimeKeys.{0}.Value", i ) );
					this.sizeOverLifetime[i] = new Tuple<float, float>( time, value );
				}
			}

			this.particleTexture = serializer.ReadTexture2DFromAssets( "ParticleTexture" );
			if( serializer.Analyze("EmissionColor").isSuccess )
			{
				this.emissionColor = serializer.ReadColor( "EmissionColor" );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteGuid( "", "SubObjectId", this.subObjectId );

			serializer.WriteBool( "", "IsWorldSpace", this.isWorldSpace );

			serializer.WriteFloat( "", "LifetimeMin", this.lifetimeMin );
			serializer.WriteFloat( "", "LifetimeMax", this.lifetimeMax );
			serializer.WriteFloat( "", "EmissionRateTime", this.emissionRateTime );
			serializer.WriteFloat( "", "StartSizeMin", this.startSizeMin );
			serializer.WriteFloat( "", "StartSizeMax", this.startSizeMax );
			serializer.WriteFloat( "", "StartSpeedMin", this.startSpeedMin );
			serializer.WriteFloat( "", "StartSpeedMax", this.startSpeedMax );

			serializer.WriteClass( "", "Shape" );
			if( this.shape is BoxShape )
			{
				serializer.WriteString( "Shape", "Type", "box" );
				serializer.WriteVector3( "Shape", "Size", ((BoxShape)this.shape).size );
			}
			else if( this.shape is SphereShape )
			{
				serializer.WriteString( "Shape", "Type", "sphere" );
				serializer.WriteFloat( "Shape", "Radius", ((SphereShape)this.shape).radius );
			}
			else if( this.shape is ConeShape )
			{
				serializer.WriteString( "Shape", "Type", "cone" );
				serializer.WriteFloat( "Shape", "Radius", ((ConeShape)this.shape).radius );
				serializer.WriteFloat( "Shape", "Angle", ((ConeShape)this.shape).angle );
			}
			else
			{
				throw new Exception( "Invalid shape type." );
			}


			if( this.velocityOverLifetime != null )
			{
				serializer.WriteList( "", "VelocityOverLifetimeKeys" );
				for( int i = 0; i < this.velocityOverLifetime.Length; i++ )
				{
					serializer.AppendClass( "VelocityOverLifetimeKeys" );
					serializer.WriteFloat( new Path( "VelocityOverLifetimeKeys.{0}", i ), "Time", this.velocityOverLifetime[i].Item1 );
					serializer.WriteVector3( new Path( "VelocityOverLifetimeKeys.{0}", i ), "Value", this.velocityOverLifetime[i].Item2 );
				}
			}

			if( this.colorOverLifetime != null )
			{
				serializer.WriteList( "", "ColorOverLifetimeKeys" );
				for( int i = 0; i < this.colorOverLifetime.Length; i++ )
				{
					serializer.AppendClass( "ColorOverLifetimeKeys" );
					serializer.WriteFloat( new Path( "ColorOverLifetimeKeys.{0}", i ), "Time", this.colorOverLifetime[i].Item1 );
					serializer.WriteColor( new Path( "ColorOverLifetimeKeys.{0}", i ), "Value", this.colorOverLifetime[i].Item2 );
				}
			}

			if( this.sizeOverLifetime != null )
			{
				serializer.WriteList( "", "SizeOverLifetimeKeys" );
				for( int i = 0; i < this.sizeOverLifetime.Length; i++ )
				{
					serializer.AppendClass( "SizeOverLifetimeKeys" );
					serializer.WriteFloat( new Path( "SizeOverLifetimeKeys.{0}", i ), "Time", this.sizeOverLifetime[i].Item1 );
					serializer.WriteFloat( new Path( "SizeOverLifetimeKeys.{0}", i ), "Value", this.sizeOverLifetime[i].Item2 );
				}
			}

			serializer.WriteString( "", "ParticleTexture", (string)this.particleTexture );
			if( this.emissionColor != null )
			{
				serializer.WriteColor( "", "EmissionColor", this.emissionColor.Value );
			}
		}
	}
}