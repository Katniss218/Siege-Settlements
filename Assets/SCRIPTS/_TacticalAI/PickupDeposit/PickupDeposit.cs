using Katniss.Utils;
using SS.Content;
using SS.Objects.Extras;
using SS.Modules;
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
			public SSObject destination { get; private set; }


			private float amtCollected = 0;

			private NavMeshAgent navMeshAgent;
			private InventoryModule inventory;

			void Start()
			{
				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
#warning TODO! - replace with ssobject getmodule.
				this.inventory = this.GetComponent<InventoryModule>();
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
					return;
				}
				if( this.destination == this.gameObject )
				{
					Debug.LogWarning( "Destination assigned to itself: " + this.gameObject.name );
					Object.Destroy( this );
					return;
				}

				this.navMeshAgent.SetDestination( this.destination.transform.position );
			}

			private void PickUp()
			{
				string idPickedUp = "";
				int amountPickedUp = 0;

				ResourceDepositModule depositToCollect = this.destination.GetComponent<ResourceDepositModule>();
				amtCollected += ResourceDepositModule.MINING_SPEED * Time.deltaTime;
				int amtFloored = Mathf.FloorToInt( amtCollected );
				if( amtFloored >= 1 )
				{
					Dictionary<string, int> resourcesInDeposit = depositToCollect.GetAll();

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
								depositToCollect.Remove( idPickedUp, amountPickedUp );
								AudioManager.PlaySound( depositToCollect.miningSound );
							}
							break; // Only pick up one resource at a time.
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

				data.destinationGuid = this.destination.guid.Value;

				return data;
			}

			/// <summary>
			/// Assigns a new PickupDeposit TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, SSObject destination )
			{
				TAIGoal.ClearGoal( gameObject );

				PickupDeposit pickupDeposit = gameObject.AddComponent<TAIGoal.PickupDeposit>();

				pickupDeposit.destination = destination;
			}
		}
	}
}