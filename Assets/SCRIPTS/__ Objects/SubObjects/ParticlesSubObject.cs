using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.SubObjects
{
	[RequireComponent(typeof(ParticleSystem))]
	[RequireComponent(typeof(ParticleSystemRenderer))]
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


		public void SetShape( ParticlesSubObjectDefinition.Shape shape )
		{
			ParticleSystem.ShapeModule shapeModule = particleSystem.shape;
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
#warning TODO! - particlesystem params.
	}
}