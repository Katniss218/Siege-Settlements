﻿using Katniss.Utils;
using SS.Content;
using SS.Modules.Inventories;
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
			public GameObject destination { get; private set; }

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
				if( this.destination == null )
				{
					Debug.LogWarning( "Not assigned destination to: " + this.gameObject.name );
					Object.Destroy( this );
				}

				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
				this.inventory = this.GetComponent<IInventory>();
				this.navMeshAgent.SetDestination( this.destination.transform.position );
			}

			// pays first ipayment receiver on the object. if has resources left in inv, pays the 2nd, etc.
			private void Pay( GameObject gameObject )
			{
				IPaymentReceiver[] paymentReceivers = gameObject.GetComponents<IPaymentReceiver>();

				for( int i = 0; i < paymentReceivers.Length; i++ )
				{
					Dictionary<string, int> wantedRes = paymentReceivers[i].GetWantedResources();

					foreach( var kvp in wantedRes )
					{
						int amountInInv = this.inventory.Get( kvp.Key );

						if( amountInInv == 0 )
						{
							continue;
						}

						int amountPayed = amountInInv > kvp.Value ? kvp.Value : amountInInv;

						this.inventory.Remove( kvp.Key, amountPayed );
						paymentReceivers[i].ReceivePayment( kvp.Key, amountPayed );
						ResourceDefinition resDef = DefinitionManager.GetResource( kvp.Key );
						AudioManager.PlayNew( resDef.dropoffSound.Item2 );
					}
					// If there is no resources to pay (everything spent).
					if( this.inventory.isEmpty )
					{
						break;
					}
				}
			}

			void Update()
			{
				// If the payment receiver was destroyed, stop the payment, stop the AI.
				if( this.destination == null )
				{
					this.navMeshAgent.ResetPath();
					Object.Destroy( this );
					return;
				}

				if( PhysicsDistance.OverlapInRange( this.transform, this.destination.transform, 0.75f ) )
				{
					this.navMeshAgent.ResetPath();

					if( this.inventory != null )
					{
						
					}
					Object.Destroy( this );
				}
			}


			public override TAIGoalData GetData()
			{
				MakePaymentData data = new MakePaymentData();
				
				data.destinationGuid = Main.GetGuid( this.destination );

				return data;
			}
			
			/// <summary>
			/// Assigns a new MakePayment TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, GameObject destination )
			{
				TAIGoal.ClearGoal( gameObject );

				MakePayment dropOffDeposit = gameObject.AddComponent<TAIGoal.MakePayment>();

				dropOffDeposit.destination = destination;
			}
		}
	}
}