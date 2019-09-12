using SS.ResourceSystem;
using System;
using UnityEngine;
using UnityEngine.Events;
using Katniss.Utils;
using Object = UnityEngine.Object;

namespace SS.Buildings
{
	/// <summary>
	/// Represents a building that's being constructed.
	/// </summary>
	public class ConstructionSite : MonoBehaviour, IPaymentProgress
	{
		public class _UnityEvent_ResourceStack : UnityEvent<ResourceStack> { }

		/// <summary>
		/// An array of resource types needed for construction (Read Only).
		/// </summary>
		[SerializeField] private string[] resourceIds;

		/// <summary>
		/// An array of remaining resources, per resource type (use resourceIds[i] to get the Id) (Read Only).
		/// </summary>
		[SerializeField] private float[] resourcesRemaining;

		[SerializeField] private float[] healthToResourcesConv;

		//public Func<float> progress { get; private set; }
		public Func<bool> IsDone { get; private set; }

		public int GetWantedAmount( string resourceId )
		{
			for( int i = 0; i < this.resourceIds.Length; i++ )
			{
				if( this.resourceIds[i] == resourceId )
				{
					return SpecialRound( this.resourcesRemaining[i] );
				}
			}
			return 0;
		}

		/// <summary>
		/// Assigns a cost (in resources) to construct the building.
		/// </summary>
		public void AssignResources( ResourceStack[] requiredResources )
		{
			this.resourceIds = new string[requiredResources.Length];
			this.resourcesRemaining = new float[requiredResources.Length];

			for( int i = 0; i < requiredResources.Length; i++ )
			{
				this.resourceIds[i] = requiredResources[i].id;
				this.resourcesRemaining[i] = requiredResources[i].amount;
			}
		}

		// Rounds down if the decimal is <=0.5, up when >0.5
		private static int SpecialRound( float remainingResAmt )
		{
			int floored = Mathf.FloorToInt( remainingResAmt );
			if( remainingResAmt - floored <= 0.5f )
			{
				return floored;
			}
			return floored + 1;
		}

		/// <summary>
		/// Starts the construction / repair of the specified building.
		/// </summary>
		public static void StartConstructionOrRepair( GameObject building )
		{
			Building buildingComp = building.GetComponent<Building>();
			Damageable damageable = building.GetComponent<Damageable>();
			MeshRenderer meshRenderer = building.transform.Find( GameObjectUtils.GRAPHICS_GAMEOBJECT_NAME ).GetComponent<MeshRenderer>();

			// Repairing is mandatory once the building's health drops below 50%.
			// And allowed anytime the health is below 100%.
			if( damageable.health == damageable.healthMax )
			{
				Debug.LogError( "You can't start repairing a building that's full HP." );
			}

			ConstructionSite constructionSite = building.AddComponent<ConstructionSite>();
			ResourceStack[] cost = buildingComp.cachedDefinition.cost;
			constructionSite.AssignResources( cost );

			constructionSite.healthToResourcesConv = new float[cost.Length];
			float totalResNeeded = 0; // total res needed
			for( int i = 0; i < constructionSite.resourcesRemaining.Length; i++ )
			{
				totalResNeeded += constructionSite.resourcesRemaining[i];
			}
			for( int i = 0; i < constructionSite.healthToResourcesConv.Length; i++ )
			{
				constructionSite.healthToResourcesConv[i] = constructionSite.resourcesRemaining[i] / totalResNeeded;
			}

			// Set the method for checking progress of the construction.
			constructionSite.IsDone = () =>
			{
				for( int i = 0; i < constructionSite.resourceIds.Length; i++ )
				{
					int roundedAmount = SpecialRound( constructionSite.resourcesRemaining[i] );

					if( roundedAmount != 0 )
					{
						return false;
					}
				}
				return true;
			};

			// cache the function listener so we can reference it to remove it when the construction is complete.
			UnityAction<float> onHealthChange_setRemaining = ( float deltaHP ) =>
			{
				meshRenderer.material.SetFloat( "_Progress", damageable.healthPercent );
				if( deltaHP < 0 )
				{
					for( int i = 0; i < constructionSite.resourceIds.Length; i++ )
					{
						constructionSite.resourcesRemaining[i] += (cost[i].amount / (damageable.healthMax * (1 - 0.1f))) * constructionSite.healthToResourcesConv[i] * -deltaHP;
					}
				}
			};

			damageable.onHealthChange.AddListener( onHealthChange_setRemaining );

			PaymentReceiver paymentReceiver = building.AddComponent<PaymentReceiver>();
			paymentReceiver.paymentProgress = constructionSite;

			// Every time the construction progresses:
			// - Heal the building depending on the amount of resources (100% health => total amount of resources as specified in the definition).
			// - Emit particles.
			// - Play sound.
			paymentReceiver.onPaymentMade.AddListener( ( ResourceStack resource ) =>
			{
				for( int i = 0; i < constructionSite.resourceIds.Length; i++ )
				{
					// Check if we even want the received resources.
					if( constructionSite.resourceIds[i] == resource.id )
					{
						int roundedRemaining = SpecialRound( constructionSite.resourcesRemaining[i] );
						if( roundedRemaining == 0 )
						{
							break;
						}
						// Received more than was needed (invalid behavior).
						if( roundedRemaining < resource.amount )
						{
							throw new Exception( "Received amount of '" + resource.id + "' (" + resource.amount + ") was more than the required amount (" + roundedRemaining + ")." );
						}

						float healAmt = ((damageable.healthMax * (1 - 0.1f)) / cost[i].amount) * constructionSite.healthToResourcesConv[i] * resource.amount;
						// If it would be healed above the max health (due to rounding up the actual resource amount received), heal it just to the max health.
						// Otherwise, heal it normally.
						if( damageable.health + healAmt > damageable.healthMax )
						{
							damageable.health = damageable.healthMax;
						}
						else
						{
							damageable.health += healAmt;
						}

						constructionSite.resourcesRemaining[i] -= resource.amount;
						if( constructionSite.resourcesRemaining[i] < 0 )
						{
							constructionSite.resourcesRemaining[i] = 0;
						}

						Main.particleSystem.transform.position = building.transform.position + new Vector3( 0, 0.125f, 0 );
						ParticleSystem.ShapeModule shape = Main.particleSystem.GetComponent<ParticleSystem>().shape;

						shape.scale = new Vector3( buildingComp.cachedDefinition.size.x, 0.25f, buildingComp.cachedDefinition.size.z );
						shape.position = Vector3.zero;
						Main.particleSystem.GetComponent<ParticleSystem>().Emit( 36 );

						AudioManager.PlayNew( buildingComp.cachedDefinition.buildSoundEffect.Item2, 1.0f, 1.0f );


						break;
					}
				}
				Debug.LogError( "Received resource was not wanted." );
			} );

			paymentReceiver.onProgressComplete.AddListener( () =>
			{
				// The building shouldn't drop into the ground, when it's not "being constructed".
				damageable.onHealthChange.RemoveListener( onHealthChange_setRemaining );
				Object.Destroy( constructionSite );
			} );

			// When the construction starts, set the _Progress attrribute of the material to the current health percent (to make the building appear as being constructed).
			meshRenderer.material.SetFloat( "_Progress", damageable.healthPercent );
		}
	}
}