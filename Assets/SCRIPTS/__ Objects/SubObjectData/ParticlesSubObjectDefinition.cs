using KFF;
using SS.Content;
using System;
using UnityEngine;

namespace SS.Objects.SubObjects
{
	public class ParticlesSubObjectDefinition : SubObjectDefinition
	{
		public const string KFF_TYPEID = "PARTICLES";
		
		public float lifetime { get; set; }
		public float emissionRadius { get; set; }
		public float emissionRateTime { get; set; }
		public float startSize { get; set; }
		public float endSize { get; set; }

		public AddressableAsset<Texture2D> particleTexture { get; set; }


		public override SubObject AddTo( GameObject gameObject )
		{
			GameObject child = new GameObject( "Sub-Object [" + KFF_TYPEID + "] '" + this.subObjectId.ToString( "D" ) + "'" );
			child.transform.SetParent( gameObject.transform );

			child.transform.localPosition = this.localPosition;
			child.transform.localRotation = this.localRotation;
			



			ParticleSystem particleSystem = child.AddComponent<ParticleSystem>();
			ParticleSystem.MainModule main = particleSystem.main;
			main.startSpeed = 0;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.startSize = 1.0f;
			main.startLifetime = this.lifetime;

			ParticleSystem.ShapeModule shape = particleSystem.shape;
			shape.radius = this.emissionRadius;
			shape.shapeType = ParticleSystemShapeType.Sphere;

			ParticleSystem.SizeOverLifetimeModule sizeOverTime = particleSystem.sizeOverLifetime;
			sizeOverTime.enabled = true;
			AnimationCurve curve = new AnimationCurve();
			curve.AddKey( 0.0f, this.startSize );
			curve.AddKey( 1.0f, this.endSize );
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
			
			this.lifetime = serializer.ReadFloat( "Lifetime" );
			this.emissionRadius = serializer.ReadFloat( "EmissionRadius" );
			this.emissionRateTime = serializer.ReadFloat( "EmissionRateTime" );
			this.startSize = serializer.ReadFloat( "StartSize" );
			this.endSize = serializer.ReadFloat( "EndSize" );

			this.particleTexture = serializer.ReadTexture2DFromAssets( "ParticleTexture" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "SubObjectId", this.subObjectId.ToString( "D" ) );
			
			serializer.WriteFloat( "", "Lifetime", this.lifetime );
			serializer.WriteFloat( "", "EmissionRadius", this.emissionRadius );
			serializer.WriteFloat( "", "EmissionRateTime", this.emissionRadius );
			serializer.WriteFloat( "", "StartSize", this.startSize );
			serializer.WriteFloat( "", "EndSize", this.endSize );

			serializer.WriteString( "", "ParticleTexture", (string)this.particleTexture );
		}
	}
}