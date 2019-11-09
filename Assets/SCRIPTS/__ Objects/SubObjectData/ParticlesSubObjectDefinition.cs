using KFF;
using SS.Content;
using System;
using UnityEngine;

namespace SS.Objects.SubObjects
{
	public class ParticlesSubObjectDefinition : SubObjectDefinition
	{
		public const string KFF_TYPEID = "PARTICLES";
		
		public bool isWorldSpace { get; set; }

		public float lifetimeMin { get; set; }
		public float lifetimeMax { get; set; }
		public float emissionRadius { get; set; }
		public float emissionRateTime { get; set; }
		public float startSizeMin { get; set; }
		public float startSizeMax { get; set; }
		public float startSpeedMin { get; set; }
		public float startSpeedMax { get; set; }

		public Tuple<float,float>[] sizeOverLifetimeKeys { get; set; }

		public AddressableAsset<Texture2D> particleTexture { get; set; }


		public override SubObject AddTo( GameObject gameObject )
		{
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
			shape.radius = this.emissionRadius;
			shape.shapeType = ParticleSystemShapeType.Sphere;

			ParticleSystem.SizeOverLifetimeModule sizeOverTime = particleSystem.sizeOverLifetime;
			sizeOverTime.enabled = true;
			AnimationCurve curve = new AnimationCurve();
			for( int i = 0; i < this.sizeOverLifetimeKeys.Length; i++ )
			{
				curve.AddKey( this.sizeOverLifetimeKeys[i].Item1, this.sizeOverLifetimeKeys[i].Item2 );
			}
			sizeOverTime.size = new ParticleSystem.MinMaxCurve( 1.0f, curve );

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
			this.emissionRadius = serializer.ReadFloat( "EmissionRadius" );
			this.emissionRateTime = serializer.ReadFloat( "EmissionRateTime" );
			this.startSizeMin = serializer.ReadFloat( "StartSizeMin" );
			this.startSizeMax = serializer.ReadFloat( "StartSizeMax" );
			this.startSpeedMin = serializer.ReadFloat( "StartSpeedMin" );
			this.startSpeedMax = serializer.ReadFloat( "StartSpeedMax" );

			this.sizeOverLifetimeKeys = new Tuple<float, float>[serializer.Analyze( "SizeOverLifetimeKeys" ).childCount];
			for( int i = 0; i < this.sizeOverLifetimeKeys.Length; i++ )
			{
				float time = serializer.ReadFloat( new Path( "SizeOverLifetimeKeys.{0}.Time", i ) );
				float value = serializer.ReadFloat( new Path( "SizeOverLifetimeKeys.{0}.Value", i ) );
				this.sizeOverLifetimeKeys[i] = new Tuple<float, float>( time, value );
			}

			this.particleTexture = serializer.ReadTexture2DFromAssets( "ParticleTexture" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "SubObjectId", this.subObjectId.ToString( "D" ) );

			serializer.WriteBool( "", "IsWorldSpace", this.isWorldSpace );

			serializer.WriteFloat( "", "LifetimeMin", this.lifetimeMin );
			serializer.WriteFloat( "", "LifetimeMax", this.lifetimeMax );
			serializer.WriteFloat( "", "EmissionRadius", this.emissionRadius );
			serializer.WriteFloat( "", "EmissionRateTime", this.emissionRadius );
			serializer.WriteFloat( "", "StartSizeMin", this.startSizeMin );
			serializer.WriteFloat( "", "StartSizeMax", this.startSizeMax );
			serializer.WriteFloat( "", "StartSpeedMin", this.startSpeedMin );
			serializer.WriteFloat( "", "StartSpeedMax", this.startSpeedMax );

			serializer.WriteList( "", "SizeOverLifetimeKeys" );
			for( int i = 0; i < this.sizeOverLifetimeKeys.Length; i++ )
			{
				serializer.AppendClass( "SizeOverLifetimeKeys" );
				serializer.WriteFloat( new Path( "SizeOverLifetimeKeys.{0}", i ), "Time", this.sizeOverLifetimeKeys[i].Item1 );
				serializer.WriteFloat( new Path( "SizeOverLifetimeKeys.{0}", i ), "Value", this.sizeOverLifetimeKeys[i].Item2 );
			}

			serializer.WriteString( "", "ParticleTexture", (string)this.particleTexture );
		}
	}
}