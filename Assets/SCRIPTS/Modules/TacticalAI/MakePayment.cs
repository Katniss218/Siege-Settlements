using Katniss.Utils;
using SS.Inventories;
using SS.ResourceSystem.Payment;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS
{
	public abstract partial class TAIGoal
	{
		[RequireComponent( typeof( NavMeshAgent ) )]
		[RequireComponent( typeof( IInventory ) )]
		public class MakePayment : TAIGoal
		{
			/// <summary>
			/// The spot at which to drop off the deposit.
			/// </summary>
			public PaymentReceiver receiver { get; private set; }

			private NavMeshAgent navMeshAgent;
			private IInventory inventory;


			void Start()
			{
				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
				this.inventory = this.GetComponent<IInventory>();
				this.navMeshAgent.SetDestination( this.receiver.transform.position );
			}

			void Update()
			{
				if( this.receiver == null )
				{
					// If the payment receiver was destroyed, stop the payment.
					this.navMeshAgent.ResetPath();
					Object.Destroy( this );
					return;
				}

				if( PhysicsDistance.OverlapInRange( this.transform, this.receiver.transform, 0.75f ) )
				{
					this.navMeshAgent.ResetPath();

					if( this.inventory != null )
					{
						Dictionary<string, int> inventoryItems = this.inventory.GetAll();
						foreach( var kvp in inventoryItems )
						{
							int amtWanted = this.receiver.GetWantedAmount( kvp.Key );
							if( amtWanted == 0 )
							{
								return;
							}

							int amountPayed = kvp.Value > amtWanted ? amtWanted : kvp.Value;

							this.inventory.Remove( kvp.Key, amountPayed );
							this.receiver.ReceivePayment( kvp.Key, amountPayed );
						}
					}
					Object.Destroy( this );
				}
			}

			/// <summary>
			/// Assigns a new MakePayment TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, PaymentReceiver receiver )
			{
				TAIGoal.ClearGoal( gameObject );

				MakePayment dropOffDeposit = gameObject.AddComponent<TAIGoal.MakePayment>();

				dropOffDeposit.receiver = receiver;
			}
		}
	}
}