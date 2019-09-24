using Katniss.Utils;
using SS.Content;
using SS.Extras;
using SS.Inventories;
using SS.ResourceSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS
{
	public abstract partial class TAIGoal
	{
		public class PickupDeposit : TAIGoal
		{
			/// <summary>
			/// The deposit to move to and pick up.
			/// </summary>
			public ResourceDeposit depositToCollect { get; private set; }

			private float amtCollected = 0;

			private IInventory inventory;
			private NavMeshAgent navMeshAgent;

			void Start()
			{
				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
				this.inventory = this.GetComponent<IInventory>();
				if( this.navMeshAgent == null )
				{
					throw new System.Exception( "Can't add PickupDeposit TAI goal to: " + this.gameObject.name );
				}
				if( this.inventory == null )
				{
					throw new System.Exception( "Can't add PickupDeposit TAI goal to: " + this.gameObject.name );
				}
				if( this.depositToCollect == null )
				{
					Debug.LogWarning( "Not assigned deposit to collect: " + this.gameObject.name );
					Object.Destroy( this );
				}

				this.navMeshAgent.SetDestination( this.depositToCollect.transform.position );
			}

			void Update()
			{
				// if the deposit was picked up (not on the map), stop the AI.
				if( this.depositToCollect == null )
				{
					Object.Destroy( this );
					return;
				}
				// if the deposit was emptied (but still on the map), stop the AI.
				if( depositToCollect.inventory.isEmpty )
				{
					Object.Destroy( this );
					return;
				}
				if( PhysicsDistance.OverlapInRange( this.transform, this.depositToCollect.transform, 0.75f ) )
				{
					// Clear the path, when it's in range of the deposit.
					this.navMeshAgent.ResetPath();

					string idPickedUp = "";
					int amountPickedUp = 0;
					if( this.depositToCollect.isTypeExtracted )
					{
						Dictionary<string, int> resourcesInDeposit = this.depositToCollect.inventory.GetAll();

						foreach( var kvp in resourcesInDeposit )
						{
							if( kvp.Value == 0 )
							{
								continue;
							}
							if( this.inventory.GetMaxCapacity( kvp.Key ) != 0 )
							{
								amountPickedUp = this.inventory.Add( kvp.Key, kvp.Value );
								idPickedUp = kvp.Key;

								if( amountPickedUp > 0 )
								{
									this.depositToCollect.inventory.Remove( idPickedUp, amountPickedUp );
									AudioManager.PlayNew( DefinitionManager.Get<ResourceDefinition>( idPickedUp ).pickupSound.Item2 );
								}
								break; // Only pick up one resource at a time.
							}
						}
					}
					else
					{
						amtCollected += ResourceDeposit.MINING_SPEED * Time.deltaTime;
						int amtFloored = Mathf.FloorToInt( amtCollected );
						if( amtFloored >= 1 )
						{
							Dictionary<string, int> resourcesInDeposit = this.depositToCollect.inventory.GetAll();

							foreach( var kvp in resourcesInDeposit )
							{
								if( kvp.Value == 0 )
								{
									continue;
								}
								if( this.inventory.GetMaxCapacity( kvp.Key ) != 0 )
								{
									amountPickedUp = this.inventory.Add( kvp.Key, amtFloored );
									idPickedUp = kvp.Key;
									amtCollected -= amtFloored;

									if( amountPickedUp > 0 )
									{
										this.depositToCollect.inventory.Remove( idPickedUp, amountPickedUp );
										AudioManager.PlayNew( this.depositToCollect.miningSound );
									}
									break; // Only pick up one resource at a time.
								}
							}
						}
					}
				}
			}

			/// <summary>
			/// Assigns a new PickupDeposit TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, ResourceDeposit depositToPickUp )
			{
				TAIGoal.ClearGoal( gameObject );

				PickupDeposit pickUpResource = gameObject.AddComponent<TAIGoal.PickupDeposit>();

				pickUpResource.depositToCollect = depositToPickUp;
			}
		}
	}
}