using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.SubObjects
{
	[RequireComponent( typeof( ParticleSystem ) )]
	public class ParticlesSubObject : SubObject
	{
		ParticleSystem __particleSystem;
		new ParticleSystem particleSystem
		{
			get
			{
				if( this.__particleSystem == null )
				{
					this.__particleSystem = this.GetComponent<ParticleSystem>();
				}
				return this.__particleSystem;
			}
		}

		ParticleSystemRenderer __particleSystemRenderer;
		ParticleSystemRenderer particleSystemRenderer
		{
			get
			{
				if( this.__particleSystemRenderer == null )
				{
					this.__particleSystemRenderer = this.GetComponent<ParticleSystemRenderer>();
				}
				return this.__particleSystemRenderer;
			}
		}


		//
		//	Main properties
		//

		
		public void SetLifetime( float min, float max )
		{
			ParticleSystem.MainModule main = particleSystem.main;
			main.startLifetime = new ParticleSystem.MinMaxCurve( min, max );
		}

		public void SetSize( float min, float max )
		{
			ParticleSystem.MainModule main = particleSystem.main;
			main.startSize = new ParticleSystem.MinMaxCurve( min, max );
		}

		public void SetSpeed( float min, float max )
		{
			ParticleSystem.MainModule main = particleSystem.main;
			main.startSpeed = new ParticleSystem.MinMaxCurve( min, max );
		}

		public void SetSimulationSpace( ParticleSystemSimulationSpace space )
		{
			ParticleSystem.MainModule main = particleSystem.main;
			main.simulationSpace = space;
		}


		//
		//	Base stats (shape/emission)
		//


		public void SetShape( ParticlesSubObjectDefinition.Shape shape )
		{
			ParticleSystem.ShapeModule shapeModule = this.particleSystem.shape;
			if( shape is ParticlesSubObjectDefinition.BoxShape )
			{
				shapeModule.shapeType = ParticleSystemShapeType.Box;
				shapeModule.scale = ((ParticlesSubObjectDefinition.BoxShape)shape).size;
			}
			else if( shape is ParticlesSubObjectDefinition.ConeShape )
			{
				shapeModule.shapeType = ParticleSystemShapeType.Cone;
				shapeModule.radius = ((ParticlesSubObjectDefinition.ConeShape)shape).radius;
				shapeModule.angle = ((ParticlesSubObjectDefinition.ConeShape)shape).angle;
			}
			else if( shape is ParticlesSubObjectDefinition.SphereShape )
			{
				shapeModule.shapeType = ParticleSystemShapeType.Sphere;
				shapeModule.radius = ((ParticlesSubObjectDefinition.SphereShape)shape).radius;
			}
			else
			{
				throw new Exception( "Invalid shape" );
			}
		}

		public void SetEmission( float overTime )
		{
			ParticleSystem.EmissionModule emission = this.particleSystem.emission;
			emission.rateOverTime = overTime;
		}


		//
		//	Additional stats (particle system modules)
		//


		public void SetVelocityOverLifetime( Tuple<float, Vector3>[] values )
		{
			ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = this.particleSystem.velocityOverLifetime;
			velocityOverLifetimeModule.enabled = true;

			AnimationCurve x = new AnimationCurve();
			AnimationCurve y = new AnimationCurve();
			AnimationCurve z = new AnimationCurve();
			for( int i = 0; i < values.Length; i++ )
			{
				x.AddKey( values[i].Item1, values[i].Item2.x );
				y.AddKey( values[i].Item1, values[i].Item2.y );
				z.AddKey( values[i].Item1, values[i].Item2.z );
			}
			velocityOverLifetimeModule.x = new ParticleSystem.MinMaxCurve( 1.0f, x );
			velocityOverLifetimeModule.y = new ParticleSystem.MinMaxCurve( 1.0f, y );
			velocityOverLifetimeModule.z = new ParticleSystem.MinMaxCurve( 1.0f, z );
		}

		public void SetColorOverLifetime( Tuple<float, Color>[] values )
		{
			ParticleSystem.ColorOverLifetimeModule sizeOverTime = this.particleSystem.colorOverLifetime;
			sizeOverTime.enabled = true;

			GradientColorKey[] ckeys = new GradientColorKey[values.Length];
			GradientAlphaKey[] akeys = new GradientAlphaKey[values.Length];
			for( int i = 0; i < values.Length; i++ )
			{
				ckeys[i] = new GradientColorKey( values[i].Item2, values[i].Item1 );
				akeys[i] = new GradientAlphaKey( values[i].Item2.a, values[i].Item1 );
			}
			Gradient color = new Gradient();
			color.alphaKeys = akeys;
			color.colorKeys = ckeys;
			sizeOverTime.color = new ParticleSystem.MinMaxGradient( color );
		}

		public void SetSizeOverLifetime( Tuple<float, float>[] values )
		{
			ParticleSystem.SizeOverLifetimeModule sizeOverTime = this.particleSystem.sizeOverLifetime;
			sizeOverTime.enabled = true;

			AnimationCurve size = new AnimationCurve();
			for( int i = 0; i < values.Length; i++ )
			{
				size.AddKey( values[i].Item1, values[i].Item2 );
			}
			sizeOverTime.size = new ParticleSystem.MinMaxCurve( 1.0f, size );
		}


		//
		//	Rendering
		//

		
		public void SetMaterial( Texture2D colorMap, Color? emission )
		{
			this.particleSystemRenderer.material = MaterialManager.CreateParticles( colorMap, emission );
			List<ParticleSystemVertexStream> streams = new List<ParticleSystemVertexStream>()
			{
				 ParticleSystemVertexStream.Position,
				 ParticleSystemVertexStream.Color,
				 ParticleSystemVertexStream.UV
			};
			this.particleSystemRenderer.SetActiveVertexStreams( streams );
		}
	}
}