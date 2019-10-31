using Katniss.Utils;
using SS.Content;
using SS.Extras;
using SS.Modules.Inventories;
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
			public GameObject destination { get; private set; }


			private float amtCollected = 0;

			private NavMeshAgent navMeshAgent;
			private IInventory inventory;

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
				if( this.destination == null )
				{
					Debug.LogWarning( "Not assigned destination to: " + this.gameObject.name );
					Object.Destroy( this );
				}

				this.navMeshAgent.SetDestination( this.destination.transform.position );
			}

			private void PickUp()
			{
				string idPickedUp = "";
				int amountPickedUp = 0;

				ResourceDeposit depositToCollect = this.destination.GetComponent<ResourceDeposit>();
				if( depositToCollect.isTypeExtracted )
				{
					Dictionary<string, int> resourcesInDeposit = depositToCollect.inventory.GetAll();

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
								depositToCollect.inventory.Remove( idPickedUp, amountPickedUp );
								AudioManager.PlaySound( DefinitionManager.GetResource( idPickedUp ).pickupSound );
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
						Dictionary<string, int> resourcesInDeposit = depositToCollect.inventory.GetAll();

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
									depositToCollect.inventory.Remove( idPickedUp, amountPickedUp );
									AudioManager.PlaySound( depositToCollect.miningSound );
								}
								break; // Only pick up one resource at a time.
							}
						}
					}
				}
			}

			void Update()
			{
				// if the deposit was picked up (not on the map), stop the AI.
				if( this.destination == null )
				{
					this.navMeshAgent.ResetPath();
					Object.Destroy( this );
					return;
				}
				if( PhysicsDistance.OverlapInRange( this.transform, this.destination.transform, 0.75f ) )
				{
					// Clear the path, when it's in range of the deposit.
					this.navMeshAgent.ResetPath();

					this.PickUp();
				}
			}


			public override TAIGoalData GetData()
			{
				PickupDepositData data = new PickupDepositData();

				data.destinationGuid = Main.GetGuid( this.destination );

				return data;
			}

			/// <summary>
			/// Assigns a new PickupDeposit TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, GameObject destination )
			{
				TAIGoal.ClearGoal( gameObject );

				PickupDeposit pickupDeposit = gameObject.AddComponent<TAIGoal.PickupDeposit>();

				pickupDeposit.destination = destination;
			}
		}
	}
}