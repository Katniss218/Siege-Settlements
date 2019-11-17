using Katniss.Utils;
using SS.Content;
using SS.Extras;
using SS.Modules;
using SS.Modules.Inventories;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS
{
	public abstract partial class TAIGoal
	{
		public class PickupInventory : TAIGoal
		{
			/// <summary>
			/// The deposit to move to and pick up.
			/// </summary>
			public GameObject destination { get; private set; }

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

				IInventory inventoryToPickupFrom = this.destination.GetComponent<IInventory>();
				Dictionary<string, int> resourcesInInventory = inventoryToPickupFrom.GetAll();

				foreach( var kvp in resourcesInInventory )
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
							inventoryToPickupFrom.Remove( idPickedUp, amountPickedUp );
							AudioManager.PlaySound( DefinitionManager.GetResource( idPickedUp ).pickupSound );
						}
						break; // Only pick up one resource at a time.
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
					Object.Destroy( this );
				}
			}


			public override TAIGoalData GetData()
			{
				PickupInventoryData data = new PickupInventoryData();

				data.destinationGuid = Main.GetGuid( this.destination );

				return data;
			}

			/// <summary>
			/// Assigns a new PickupDeposit TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, GameObject destination )
			{
				TAIGoal.ClearGoal( gameObject );

				PickupInventory pickupDeposit = gameObject.AddComponent<TAIGoal.PickupInventory>();

				pickupDeposit.destination = destination;
			}
		}
	}
}