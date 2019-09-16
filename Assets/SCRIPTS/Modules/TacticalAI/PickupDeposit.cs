﻿using Katniss.Utils;
using SS.Extras;
using SS.Inventories;
using UnityEngine;
using UnityEngine.AI;

namespace SS
{
	public abstract partial class TAIGoal
	{
		[RequireComponent( typeof( NavMeshAgent ) )]
		public class PickupDeposit : TAIGoal
		{
			public ResourceDeposit depositToCollect { get; private set; }

			private float amtCollected = 0;

			private NavMeshAgent navMeshAgent;

			void Start()
			{
				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
				this.navMeshAgent.SetDestination( this.depositToCollect.transform.position );
			}

			void Update()
			{
				if( this.depositToCollect == null )
				{
					Object.Destroy( this ); // if the deposit was picked up, stop the AI.
					return;
				}
				if( RaycastDistance.IsInRange( this.depositToCollect.gameObject, this.depositToCollect.transform.position, this.transform.position, 0.75f ) )
				//	if( Vector3.Distance( this.transform.position, this.depositToCollect.transform.position ) < 1 )
				{
					IInventory inventory = this.GetComponent<IInventory>();

					if( inventory == null )
					{
						Destroy( this );
						throw new System.Exception( "TAIGoal.CollectDeposit was added to an object that doesn't have IInventory." );
					}

					// Clear the path, when it's in range of the deposit.
					this.navMeshAgent.ResetPath();

					int amountPickedUp = 0;
					if( this.depositToCollect.isTypeExtracted )
					{
						if( inventory.CanHold( this.depositToCollect.resourceId ) )
						{
							amountPickedUp = inventory.Add( this.depositToCollect.resourceId, this.depositToCollect.amount );
						}
					}
					else
					{
						amtCollected += ResourceDeposit.MINING_SPEED * Time.deltaTime;
						int amtFloored = Mathf.FloorToInt( amtCollected );
						if( amtFloored >= 1 )
						{
							amountPickedUp = inventory.Add( this.depositToCollect.resourceId, amtFloored );
							amtCollected -= amtFloored;
						}
					}

					if( amountPickedUp != 0 )
					{
						this.depositToCollect.PickUp( amountPickedUp );
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