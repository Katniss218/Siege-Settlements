using SS.Content;
using SS.Extras;
using SS.Inventories;
using SS.ResourceSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS
{
	public abstract partial class TAIGoal
	{
		[RequireComponent( typeof( IInventory ) )]
		[RequireComponent( typeof( NavMeshAgent ) )]
		public class DropoffToNew : TAIGoal
		{
			/// <summary>
			/// The spot at which to drop off the deposit.
			/// </summary>
			public Vector3 destination { get; private set; }

			private IInventory inventory;

			private NavMeshAgent navMeshAgent;


			private void DropOff( Vector3 direction )
			{
				// Creates deposit(s) from the inventory's items.
				// Clears the inventory.
				// Stops the agent.

				if( Physics.Raycast( this.gameObject.transform.position + direction.normalized + new Vector3( 0, 5, 0 ), Vector3.down, out RaycastHit hitInfo ) )
				{
					Dictionary<string,int> resourcesCarried = this.inventory.GetAll();
					if( hitInfo.collider.gameObject.layer == ObjectLayer.TERRAIN )
					{
						foreach( var kvp in resourcesCarried )
						{
							ResourceDefinition resourceDef = DataManager.Get<ResourceDefinition>( kvp.Key );
							ResourceDepositDefinition newDepositDef = DataManager.Get<ResourceDepositDefinition>( resourceDef.defaultDeposit );
							int capacity = newDepositDef.resources[kvp.Key];
							int remaining = kvp.Value;
							while( remaining > 0 )
							{
								GameObject obj;
								Dictionary<string, int> dict = new Dictionary<string, int>();
								if( remaining >= capacity )
								{
									dict.Add( kvp.Key, capacity );
								}
								else
								{
									dict.Add( kvp.Key, remaining );
								}
								remaining -= capacity;
								obj = ResourceDepositCreator.Create( newDepositDef, hitInfo.point, Quaternion.identity, dict );
								AudioManager.PlayNew( resourceDef.dropoffSound.Item2 );
							}
						}
						this.inventory.Clear();
					}
					this.navMeshAgent.ResetPath();
				}
			}

			void Start()
			{
				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
				this.navMeshAgent.SetDestination( this.destination );
				this.inventory = this.GetComponent<IInventory>();
			}

			void Update()
			{
				if( Vector3.Distance( this.transform.position, this.destination ) < 0.75f )
				{
					if( !this.inventory.isEmpty )
					{
						Vector3 direction = (this.destination - this.transform.position).normalized;
						this.DropOff( direction );
					}
					else
					{
						Object.Destroy( this );
					}
				}
			}

			/// <summary>
			/// Assigns a new PickUpResource TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, Vector3 destination )
			{
				TAIGoal.ClearGoal( gameObject );

				DropoffToNew dropOffDeposit = gameObject.AddComponent<TAIGoal.DropoffToNew>();

				dropOffDeposit.destination = destination;
			}
		}
	}
}