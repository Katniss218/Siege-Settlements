using Katniss.Utils;
using SS.Content;
using SS.Inventories;
using SS.ResourceSystem;
using SS.ResourceSystem.Payment;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS
{
	public abstract partial class TAIGoal
	{
		public class MakePayment : TAIGoal
		{
			/// <summary>
			/// The spot at which to drop off the deposit.
			/// </summary>
			public IPaymentReceiver paymentReceiver { get; private set; }
			public Transform receiverTransform { get; private set; }

			private NavMeshAgent navMeshAgent;
			private IInventory inventory;


			void Start()
			{
				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
				this.inventory = this.GetComponent<IInventory>();
				if( this.navMeshAgent == null )
				{
					throw new System.Exception( "Can't add MakePayment TAI goal to: " + this.gameObject.name );
				}
				if( this.inventory == null )
				{
					throw new System.Exception( "Can't add MakePayment TAI goal to: " + this.gameObject.name );
				}
				if( this.paymentReceiver == null )
				{
					Debug.LogWarning( "Not assigned payment receiver: " + this.gameObject.name );
					Object.Destroy( this );
				}

				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
				this.inventory = this.GetComponent<IInventory>();
				this.navMeshAgent.SetDestination( this.receiverTransform.position );
			}

			void Update()
			{
				// If the payment receiver was destroyed, stop the payment, stop the AI.
				if( this.paymentReceiver == null )
				{
					this.navMeshAgent.ResetPath();
					Object.Destroy( this );
					return;
				}

				if( PhysicsDistance.OverlapInRange( this.transform, this.receiverTransform, 0.75f ) )
				{
					this.navMeshAgent.ResetPath();

					if( this.inventory != null )
					{
						Dictionary<string, int> wantedRes = this.paymentReceiver.GetWantedResources();
						
						foreach( var kvp in wantedRes )
						{
							int amountInInv = this.inventory.Get( kvp.Key );
							
							if( amountInInv == 0 )
							{
								continue;
							}

							int amountPayed = amountInInv > kvp.Value ? kvp.Value : amountInInv;

							this.inventory.Remove( kvp.Key, amountPayed );
							this.paymentReceiver.ReceivePayment( kvp.Key, amountPayed );
							ResourceDefinition resDef = DataManager.Get<ResourceDefinition>( kvp.Key );
							AudioManager.PlayNew( resDef.dropoffSound.Item2 );
						}
					}
					Object.Destroy( this );
				}
			}

			/// <summary>
			/// Assigns a new MakePayment TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, Transform receiverTransform, IPaymentReceiver receiver )
			{
				TAIGoal.ClearGoal( gameObject );

				MakePayment dropOffDeposit = gameObject.AddComponent<TAIGoal.MakePayment>();

				dropOffDeposit.paymentReceiver = receiver;
				dropOffDeposit.receiverTransform = receiverTransform;
			}
		}
	}
}