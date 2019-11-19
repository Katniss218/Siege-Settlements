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
		public class PickupInventory : TAIGoal
		{
			/// <summary>
			/// The deposit to move to and pick up.
			/// </summary>
			public SSObject destination { get; private set; }

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

#warning TODO! - replace with ssobject getmodule.
				InventoryModule inventoryToPickupFrom = this.destination.GetComponent<InventoryModule>();
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

						Debug.Log( amountPickedUp + "x " + idPickedUp );
						if( amountPickedUp > 0 )
						{
							int amtRemoved = inventoryToPickupFrom.Remove( idPickedUp, amountPickedUp );
							Debug.Log( "R: " + amtRemoved + "x " + idPickedUp );
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

				data.destinationGuid = this.destination.guid.Value;

				return data;
			}

			/// <summary>
			/// Assigns a new PickupDeposit TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, SSObject destination )
			{
				TAIGoal.ClearGoal( gameObject );

				PickupInventory pickupDeposit = gameObject.AddComponent<TAIGoal.PickupInventory>();

				pickupDeposit.destination = destination;
			}
		}
	}
}