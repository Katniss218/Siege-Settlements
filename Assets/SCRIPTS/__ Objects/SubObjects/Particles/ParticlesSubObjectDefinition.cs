using KFF;
using SS.Content;
using System;
using UnityEngine;

namespace SS.Objects.SubObjects
{
	public class ParticlesSubObjectDefinition : SubObjectDefinition
	{
		public const string KFF_TYPEID = "PARTICLES";

		public const string SHAPE_SPHERE_KFFID = "sphere";
		public const string SHAPE_CONE_KFFID = "cone";
		public const string SHAPE_BOX_KFFID = "box";


		public abstract class Shape { }

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


		public override SubObject AddTo( SSObject ssObject )
		{
			if( this.shape == null )
			{
				throw new Exception( "Shape can't be null" );
			}


			var sub = ssObject.AddSubObject<ParticlesSubObject>( this.subObjectId );

			sub.Item1.transform.localPosition = this.localPosition;
			sub.Item1.transform.localRotation = this.localRotation;

			sub.Item2.SetShape( this.shape );
			sub.Item2.SetMaterial( this.particleTexture, this.emissionColor );
			sub.Item2.SetSimulationSpace( this.isWorldSpace ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local );
			sub.Item2.SetLifetime( this.lifetimeMin, this.lifetimeMax );
			sub.Item2.SetSize( this.startSizeMin, this.startSizeMax );
			sub.Item2.SetSpeed( this.startSpeedMin, this.startSpeedMax );
			sub.Item2.SetEmission( this.emissionRateTime );

			if( this.velocityOverLifetime != null )
			{
				sub.Item2.SetVelocityOverLifetime( this.velocityOverLifetime );
			}

			if( this.colorOverLifetime != null )
			{
				sub.Item2.SetColorOverLifetime( this.colorOverLifetime );
			}

			if( this.sizeOverLifetime != null )
			{
				sub.Item2.SetSizeOverLifetime( this.sizeOverLifetime );
			}
			return sub.Item2;
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
				this.isWorldSpace = serializer.ReadBool( "IsWorldSpace" );
			}
			catch
			{
#warning Get full KFF Paths from the serializer exception.
				throw new Exception( "Missing or invalid value of 'IsWorldSpace' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.lifetimeMin = serializer.ReadFloat( "LifetimeMin" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'LifetimeMin' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.lifetimeMax = serializer.ReadFloat( "LifetimeMax" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'LifetimeMax' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.emissionRateTime = serializer.ReadFloat( "EmissionRateTime" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'EmissionRateTime' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.startSizeMin = serializer.ReadFloat( "StartSizeMin" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'StartSizeMin' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.startSizeMax = serializer.ReadFloat( "StartSizeMax" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'StartSizeMax' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.startSpeedMin = serializer.ReadFloat( "StartSpeedMin" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'StartSpeedMin' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.startSpeedMax = serializer.ReadFloat( "StartSpeedMax" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'StartSpeedMax' (" + serializer.file.fileName + ")." );
			}

			string strShape = "";
			try
			{
				strShape = serializer.ReadString( "Shape.Type" );
			}
			catch
			{
				throw new Exception( "Missing 'Shape.Type' (" + serializer.file.fileName + ")." );
			}

			if( strShape == SHAPE_BOX_KFFID )
			{
				BoxShape shape = new BoxShape();
				try
				{
					shape.size = serializer.ReadVector3( "Shape.Size" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'Shape.Size' (" + serializer.file.fileName + ")." );
				}
				this.shape = shape;
			}
			else if( strShape == SHAPE_SPHERE_KFFID )
			{
				SphereShape shape = new SphereShape();
				try
				{
					shape.radius = serializer.ReadFloat( "Shape.Radius" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'Shape.Radius' (" + serializer.file.fileName + ")." );
				}
				this.shape = shape;
			}
			else if( strShape == SHAPE_CONE_KFFID )
			{
				ConeShape shape = new ConeShape();
				try
				{
					shape.radius = serializer.ReadFloat( "Shape.Radius" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'Shape.Radius' (" + serializer.file.fileName + ")." );
				}
				try
				{
					shape.angle = serializer.ReadFloat( "Shape.Angle" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'Shape.Angle' (" + serializer.file.fileName + ")." );
				}
				this.shape = shape;
			}
			else
			{
				throw new Exception( "Missing or invalid value of 'Shape.Type' - \"" + strShape + "\"." );
			}

			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "VelocityOverLifetimeKeys" );
			if( analysisData.isSuccess )
			{
				this.velocityOverLifetime = new Tuple<float, Vector3>[analysisData.childCount];
				try
				{
					for( int i = 0; i < this.velocityOverLifetime.Length; i++ )
					{
						float time = serializer.ReadFloat( new Path( "VelocityOverLifetimeKeys.{0}.Time", i ) );
						Vector3 value = serializer.ReadVector3( new Path( "VelocityOverLifetimeKeys.{0}.Value", i ) );
						this.velocityOverLifetime[i] = new Tuple<float, Vector3>( time, value );
					}
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'VelocityOverLifetimeKeys' (" + serializer.file.fileName + ")." );
				}
			}

			analysisData = serializer.Analyze( "ColorOverLifetimeKeys" );
			if( analysisData.isSuccess )
			{
				this.colorOverLifetime = new Tuple<float, Color>[analysisData.childCount];
				try
				{
					for( int i = 0; i < this.colorOverLifetime.Length; i++ )
					{
						float time = serializer.ReadFloat( new Path( "ColorOverLifetimeKeys.{0}.Time", i ) );
						Color value = serializer.ReadColor( new Path( "ColorOverLifetimeKeys.{0}.Value", i ) );
						this.colorOverLifetime[i] = new Tuple<float, Color>( time, value );
					}
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'ColorOverLifetimeKeys' (" + serializer.file.fileName + ")." );
				}
			}

			analysisData = serializer.Analyze( "SizeOverLifetimeKeys" );
			if( analysisData.isSuccess )
			{
				this.sizeOverLifetime = new Tuple<float, float>[analysisData.childCount];
				try
				{
					for( int i = 0; i < this.sizeOverLifetime.Length; i++ )
					{
						float time = serializer.ReadFloat( new Path( "SizeOverLifetimeKeys.{0}.Time", i ) );
						float value = serializer.ReadFloat( new Path( "SizeOverLifetimeKeys.{0}.Value", i ) );
						this.sizeOverLifetime[i] = new Tuple<float, float>( time, value );
					}
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'SizeOverLifetimeKeys' (" + serializer.file.fileName + ")." );
				}
			}

			try
			{
				this.particleTexture = serializer.ReadTexture2DFromAssets( "ParticleTexture" );
			}
			catch( KFFException )
			{
				throw new Exception( "Missing or invalid value of 'ParticleTexture' (" + serializer.file.fileName + ")." );
			}
			if( serializer.Analyze( "EmissionColor" ).isSuccess )
			{
				try
				{
					this.emissionColor = serializer.ReadColor( "EmissionColor" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'EmissionColor' (" + serializer.file.fileName + ")." );
				}
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