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
			ParticleSystem.MainModule main = particleSystem.main;
			main.simulationSpace = this.isWorldSpace ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
			main.startLifetime = new ParticleSystem.MinMaxCurve( this.lifetimeMin, this.lifetimeMax );
			main.startSize = new ParticleSystem.MinMaxCurve( this.startSizeMin, this.startSizeMax );
			main.startSpeed = new ParticleSystem.MinMaxCurve( this.startSpeedMin, this.startSpeedMax );

			ParticleSystem.ShapeModule shape = particleSystem.shape;
			if( this.shape is BoxShape )
			{
				shape.shapeType = ParticleSystemShapeType.Box;
				shape.scale = ((BoxShape)this.shape).size;
			}
			else if( this.shape is ConeShape )
			{
				shape.shapeType = ParticleSystemShapeType.Cone;
				shape.radius = ((ConeShape)this.shape).radius;
				shape.angle = ((ConeShape)this.shape).angle;
			}
			else if( this.shape is SphereShape )
			{
				shape.shapeType = ParticleSystemShapeType.Sphere;
				shape.radius = ((SphereShape)this.shape).radius;
			}
			else
			{
				throw new Exception( "Invalid shape" );
			}


			if( this.velocityOverLifetime != null )
			{
				ParticleSystem.VelocityOverLifetimeModule velocityOverTime = particleSystem.velocityOverLifetime;
				velocityOverTime.enabled = true;

				AnimationCurve x = new AnimationCurve();
				AnimationCurve y = new AnimationCurve();
				AnimationCurve z = new AnimationCurve();
				for( int i = 0; i < this.velocityOverLifetime.Length; i++ )
				{
					x.AddKey( this.velocityOverLifetime[i].Item1, this.velocityOverLifetime[i].Item2.x );
					y.AddKey( this.velocityOverLifetime[i].Item1, this.velocityOverLifetime[i].Item2.y );
					z.AddKey( this.velocityOverLifetime[i].Item1, this.velocityOverLifetime[i].Item2.z );
				}
				velocityOverTime.x = new ParticleSystem.MinMaxCurve( 1.0f, x );
				velocityOverTime.y = new ParticleSystem.MinMaxCurve( 1.0f, y );
				velocityOverTime.z = new ParticleSystem.MinMaxCurve( 1.0f, z );
			}

			if( this.colorOverLifetime != null )
			{
				ParticleSystem.ColorOverLifetimeModule sizeOverTime = particleSystem.colorOverLifetime;
				sizeOverTime.enabled = true;

				GradientColorKey[] ckeys = new GradientColorKey[this.colorOverLifetime.Length];
				GradientAlphaKey[] akeys = new GradientAlphaKey[this.colorOverLifetime.Length];
				for( int i = 0; i < this.colorOverLifetime.Length; i++ )
				{
					ckeys[i] = new GradientColorKey( this.colorOverLifetime[i].Item2, this.colorOverLifetime[i].Item1 );
					akeys[i] = new GradientAlphaKey( this.colorOverLifetime[i].Item2.a, this.colorOverLifetime[i].Item1 );
				}
				Gradient color = new Gradient();
				color.alphaKeys = akeys;
				color.colorKeys = ckeys;
				sizeOverTime.color = new ParticleSystem.MinMaxGradient( color );
			}

			if( this.sizeOverLifetime != null )
			{
				ParticleSystem.SizeOverLifetimeModule sizeOverTime = particleSystem.sizeOverLifetime;
				sizeOverTime.enabled = true;

				AnimationCurve size = new AnimationCurve();
				for( int i = 0; i < this.sizeOverLifetime.Length; i++ )
				{
					size.AddKey( this.sizeOverLifetime[i].Item1, this.sizeOverLifetime[i].Item2 );
				}
				sizeOverTime.size = new ParticleSystem.MinMaxCurve( 1.0f, size );
			}

			//shape

			ParticleSystem.EmissionModule emission = particleSystem.emission;
			emission.rateOverTime = this.emissionRateTime;

			ParticleSystemRenderer renderer = child.GetComponent<ParticleSystemRenderer>();
			renderer.material = MaterialManager.CreateParticles( this.particleTexture );





			SubObject subObject = child.AddComponent<SubObject>();
			subObject.subObjectId = this.subObjectId;

			return subObject;
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.subObjectId = Guid.ParseExact( serializer.ReadString( "SubObjectId" ), "D" );

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
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "SubObjectId", this.subObjectId.ToString( "D" ) );

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
		}
	}
}