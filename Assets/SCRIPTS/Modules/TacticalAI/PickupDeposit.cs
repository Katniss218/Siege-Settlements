using Katniss.Utils;
using SS.Extras;
using SS.Inventories;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS
{
	public abstract partial class TAIGoal
	{
		[RequireComponent( typeof( IInventory ) )]
		[RequireComponent( typeof( NavMeshAgent ) )]
		public class PickupDeposit : TAIGoal
		{
			public ResourceDeposit depositToCollect { get; private set; }

			private float amtCollected = 0;

			private IInventory inventory;
			private NavMeshAgent navMeshAgent;

			void Start()
			{
				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
				this.navMeshAgent.SetDestination( this.depositToCollect.transform.position );
				this.inventory = this.GetComponent<IInventory>();
			}

			void Update()
			{
				if( this.depositToCollect == null )
				{
					Object.Destroy( this ); // if the deposit was picked up, stop the AI.
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
								break; // Only one resource at a time.
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
									break; // Only one resource at a time.
								}
							}
						}
					}

					if( amountPickedUp != 0 )
					{
						this.depositToCollect.inventory.Remove( idPickedUp, amountPickedUp );
						AudioManager.PlayNew( this.depositToCollect.pickupSound );
					}
				}
			}

			/// <summary>
			/// Assigns a new PickUpResource TAI goal to the GameObject.
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