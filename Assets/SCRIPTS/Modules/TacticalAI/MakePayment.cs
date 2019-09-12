using Katniss.Utils;
using SS.ResourceSystem;
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


			private void PayFromInventory( string id, int maxAmountPossible )
			{
				int amtWanted = this.receiver.paymentProgress.GetWantedAmount( id );

				if( amtWanted == 0 )
				{
					return;
				}

				int amountPayed = maxAmountPossible > amtWanted ? amtWanted : maxAmountPossible;

				this.receiver.ReceivePayment( new ResourceStack( id, amountPayed ) );
				this.inventory.Remove( id, amountPayed );
			}

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
				if( RaycastDistance.IsInRange( this.receiver.gameObject, this.receiver.transform.position, this.transform.position, 0.75f ) )
				//if( Vector3.Distance( this.transform.position, this.receiver.transform.position ) < 2 )
				{
					this.navMeshAgent.ResetPath();

					List<ResourceStack> inventoryItems = this.inventory.GetAll();
					if( inventory != null )
					{
						foreach( ResourceStack stack in inventoryItems )
						{
							PayFromInventory( stack.id, stack.amount );
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