using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Katniss.Utils;
using SS.ResourceSystem.Payment;
using Object = UnityEngine.Object;

namespace SS.Buildings
{
	/// <summary>
	/// Represents a building that's being constructed.
	/// </summary>
	public class ConstructionSite : MonoBehaviour, IPaymentProgress
	{
		public class _UnityEvent_string_int : UnityEvent<string, int> { }

		/// <summary>
		/// An array of resource types needed for construction (Read Only).
		/// </summary>
		[SerializeField] private string[] resourceIds;

		/// <summary>
		/// Resources needed to progress from Building.STARTING_HEALTH_PERCENT health to 100%.
		/// </summary>
		[SerializeField] private int[] initialResources;

		/// <summary>
		/// An array of remaining resources, per resource type (use resourceIds[i] to get the Id) (Read Only).
		/// </summary>
		[SerializeField] private float[] resourcesRemaining;

		/// <summary>
		/// Each entry represents a percentage of total resources needed (initial).
		/// </summary>
		[SerializeField] private float[] healthToResources;

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
		/// Assigns a cost (in resources) to fully construct the building. Resets any progression in resources.
		/// </summary>
		public void SetRequiredResources( Dictionary<string, int> requiredResources )
		{
			this.resourceIds = new string[requiredResources.Count];
			this.resourcesRemaining = new float[requiredResources.Count];
			this.healthToResources = new float[requiredResources.Count];
			this.initialResources = new int[requiredResources.Count];

			float totalResourcesNeeded = 0;
			int i = 0;
			foreach( var id in requiredResources.Keys )
			{
				int amount = requiredResources[id];
				this.resourceIds[i] = id;
				this.resourcesRemaining[i] = amount;
				this.initialResources[i] = amount;

				totalResourcesNeeded += amount;

				i++;
			}

			// Once we have our total, calculate how much each resource contributes to the total.
			for( i = 0; i < this.healthToResources.Length; i++ )
			{
				this.healthToResources[i] = this.resourcesRemaining[i] / totalResourcesNeeded;
			}

		}

		// Rounds down when the decimal is <=0.5, rounds up when >0.5
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
		public static void BeginConstructionOrRepair( GameObject gameObject )
		{
			Damageable damageable = gameObject.GetComponent<Damageable>();
			if( !Building.IsRepairable( damageable ) )
			{
				Debug.LogError( gameObject.name + " - Building is not repairable." );
			}

			Building building = gameObject.GetComponent<Building>();
			

			ConstructionSite constructionSite = gameObject.AddComponent<ConstructionSite>();
			constructionSite.SetRequiredResources( building.StartToEndConstructionCost );
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

			MeshRenderer meshRenderer = gameObject.transform.Find( GameObjectUtils.GRAPHICS_GAMEOBJECT_NAME ).GetComponent<MeshRenderer>();

			// cache the function listener so we can reference it to remove it when the construction is complete.
			UnityAction<float> onHealthChange_whenConstructing = ( float deltaHP ) =>
			{
				meshRenderer.material.SetFloat( "_Progress", damageable.healthPercent );
				if( deltaHP < 0 )
				{
					for( int i = 0; i < constructionSite.resourceIds.Length; i++ )
					{
						float resAmt = (constructionSite.initialResources[i] / (damageable.healthMax * (1 - 0.1f))) * constructionSite.healthToResources[i] * -deltaHP;

						constructionSite.resourcesRemaining[i] += resAmt;
					}
				}
			};

			damageable.onHealthChange.AddListener( onHealthChange_whenConstructing );

			PaymentReceiver paymentReceiver = gameObject.AddComponent<PaymentReceiver>();
			paymentReceiver.paymentProgress = constructionSite;

			// Every time the construction progresses:
			// - Heal the building depending on the amount of resources (100% health => total amount of resources as specified in the definition).
			// - Emit particles.
			// - Play sound.
			paymentReceiver.onPaymentMade.AddListener( ( string id, int amount ) =>
			{
				for( int i = 0; i < constructionSite.resourceIds.Length; i++ )
				{
					// Check if we even want the received resources.
					if( constructionSite.resourceIds[i] == id )
					{
						int roundedRemaining = SpecialRound( constructionSite.resourcesRemaining[i] );
						if( roundedRemaining == 0 )
						{
							throw new Exception( "Received resource wasn't wanted." );
						}
						// Received more than was needed (invalid behavior).
						if( roundedRemaining < amount )
						{
							throw new Exception( "Received amount of '" + id + "' (" + amount + ") was more than the required amount (" + roundedRemaining + ")." );
						}

						float healAmt = ((damageable.healthMax * (1 - 0.1f)) / constructionSite.initialResources[i]) * constructionSite.healthToResources[i] * amount;

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

						constructionSite.resourcesRemaining[i] -= amount;
						if( constructionSite.resourcesRemaining[i] < 0 )
						{
							constructionSite.resourcesRemaining[i] = 0;
						}

						Main.particleSystem.transform.position = gameObject.transform.position + new Vector3( 0, 0.125f, 0 );
						ParticleSystem.ShapeModule shape = Main.particleSystem.GetComponent<ParticleSystem>().shape;

						BoxCollider col = building.GetComponent<BoxCollider>();
						shape.scale = new Vector3( col.size.x, 0.25f, col.size.z );
						shape.position = Vector3.zero;
						Main.particleSystem.GetComponent<ParticleSystem>().Emit( 36 );

						AudioManager.PlayNew( building.buildSoundEffect, 1.0f, 1.0f );

						return;
					}
				}
			} );

			paymentReceiver.onProgressComplete.AddListener( () =>
			{
				// Remove onHealthChange_whenConstructing, so the damageable doesn't call listener, that doesn't exist (cause the construction ended).
				damageable.onHealthChange.RemoveListener( onHealthChange_whenConstructing );

				Object.DestroyImmediate( constructionSite );

				Selectable selectable = building.GetComponent<Selectable>();
				if( selectable != null )
				{
					SelectionManager.ForceSelectionUIRedraw( selectable ); // forse redraw to refresh after the const site has beed destroyed.
				}
			} );

			// When the construction starts, set the _Progress attrribute of the material to the current health percent (to make the building appear as being constructed).
			meshRenderer.material.SetFloat( "_Progress", damageable.healthPercent );
		}
	}
}