﻿using SS.Data;
using SS.ResourceSystem;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace SS.Buildings
{
	/// <summary>
	/// Represents a building that's being constructed.
	/// </summary>
	public class ConstructionSite : MonoBehaviour
	{
		public class _UnityEvent_ResourceStack : UnityEvent<ResourceStack> { }

		/// <summary>
		/// An array of resource types needed for construction (Read Only).
		/// </summary>
		private string[] resourceIds;
		/// <summary>
		/// An array of remaining resources, per resource type (use resourceIds[i] to get the Id) (Read Only).
		/// </summary>
		private int[] resourcesRemaining;

		/// <summary>
		/// The total cost of the building's construction/repair (sum of every resource).
		/// </summary>
		private int totalResources;

		/// <summary>
		/// Calculates how much (%) should be added to the building's health given the specified amount of resources.
		/// </summary>
		/// <param name="resourceAmt"></param>
		public float GetHealthPercentGained( int resourceAmt )
		{
			return (float)resourceAmt / ((float)this.totalResources * 0.9f);
		}

		/// <summary>
		/// This method should return a value between 0 and 1. 0 when the construction is at 0% progress, and 1 when at 100% progress.
		/// </summary>
		public Func<float> getPercentCompleted;

		private bool IsCompleted()
		{
			return this.getPercentCompleted() >= 1.0f;
		}

		/// <summary>
		/// Is called when the construction progresses (resources are added).
		/// </summary>
		public UnityEvent onConstructionStart = new UnityEvent();

		/// <summary>
		/// Is called when the construction progresses (resources are added).
		/// </summary>
		public _UnityEvent_ResourceStack onConstructionProgress = new _UnityEvent_ResourceStack();

		/// <summary>
		/// Is called when the construction progresses (resources are added).
		/// </summary>
		public UnityEvent onConstructionComplete = new UnityEvent();

		/// <summary>
		/// Assigns a cost (in resources) to construct the building.
		/// </summary>
		public void AssignResources( ResourceStack[] requiredResources )
		{
			this.totalResources = 0;
			this.resourceIds = new string[requiredResources.Length];
			this.resourcesRemaining = new int[requiredResources.Length];

			for( int i = 0; i < requiredResources.Length; i++ )
			{
				this.resourceIds[i] = requiredResources[i].id;
				this.resourcesRemaining[i] = requiredResources[i].amount;
				this.totalResources += requiredResources[i].amount;
			}
		}

		void Start()
		{
			onConstructionStart?.Invoke();
		}

		// placeholder for auto-building until the proper build mechanic comes into place.
		void Update()
		{
			if( Input.GetKeyDown( KeyCode.B ) ) // wood
			{
				AdvanceConstructionBy( new ResourceStack( "resource.wood", 50 ) );
			}
			if( Input.GetKeyDown( KeyCode.N ) ) // stone
			{
				AdvanceConstructionBy( new ResourceStack( "resource.stone", 50 ) );
			}
		}

		/// <summary>
		/// Advances the construction (gives the resources) by a specified amount.
		/// </summary>
		public void AdvanceConstructionBy( ResourceStack stack )
		{
			for( int i = 0; i < this.resourceIds.Length; i++ )
			{
				if( this.resourceIds[i] == stack.id )
				{
					this.onConstructionProgress?.Invoke( stack );
					if( this.IsCompleted() )
					{
						this.FinishConstruction();
					}
					break;
				}
			}
		}

		/// <summary>
		/// Overrides the required resources and instantly finishes construction. Removes the ConstructionSite script from the GameObject.
		/// </summary>
		public void FinishConstruction()
		{
			this.onConstructionComplete?.Invoke();
			Destroy( this );
		}


		/// <summary>
		/// Starts the construction / repair of the specified building.
		/// </summary>
		public static void StartConstructionOrRepair( GameObject building )
		{
			ObjectBase objectBase = building.GetComponent<ObjectBase>();
			Damageable damageable = building.GetComponent<Damageable>();

			// Repairing is mandatory once the building's health drops below 50%.
			// And allowed anytime the health is below 100%.
			if( damageable.health == damageable.healthMax )
			{
				Debug.LogError( "You can't start repairing a building that's full HP." );
			}

			ConstructionSite constructionSite = building.AddComponent<ConstructionSite>();
			constructionSite.AssignResources( DataManager.Get<BuildingDefinition>( objectBase.id ).cost );

			// Set the method for checking progress of the construction.
			// The construction is directly tied to the building's health.
			constructionSite.getPercentCompleted = () => damageable.healthPercent;

			// When the construction starts, set the _Progress attrribute of the material to the current health percent (to make the building appear as being constructed).
			constructionSite.onConstructionStart.AddListener( () =>
			{
				objectBase.meshRenderer.material.SetFloat( "_Progress", damageable.healthPercent );
			} );

			// Every time the construction progresses:
			// - Heal the building depending on the amount of resources (100% health => total amount of resources as specified in the definition).
			// - Emit particles.
			// - Play sound
			constructionSite.onConstructionProgress.AddListener( ( ResourceStack stack ) =>
			{
				damageable.Heal( constructionSite.GetHealthPercentGained( stack.amount ) * damageable.healthMax );

				objectBase.meshRenderer.material.SetFloat( "_Progress", damageable.healthPercent );

				Main.particleSystem.transform.position = building.transform.position + new Vector3( 0, 0.2f, 0 );
				ParticleSystem.ShapeModule shape = Main.particleSystem.GetComponent<ParticleSystem>().shape;

				BuildingDefinition def = DataManager.Get<BuildingDefinition>( objectBase.id );
				shape.scale = new Vector3( def.size.x, 0.4f, def.size.z );
				shape.position = Vector3.zero;
				Main.particleSystem.GetComponent<ParticleSystem>().Emit( 36 );
				AudioManager.PlayNew( def.buildSoundEffect.Item2, 0.5f, 1.0f );
			} );

			// When the construction is completed, set the _Progress attribute to fully built.
			constructionSite.onConstructionComplete.AddListener( () =>
			{
				objectBase.meshRenderer.material.SetFloat( "_Progress", 1f );
			} );
		}
	}
}