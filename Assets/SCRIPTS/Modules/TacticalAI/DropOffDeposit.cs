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
		[RequireComponent( typeof( NavMeshAgent ) )]
		public class DropOffDeposit : TAIGoal
		{
			/// <summary>
			/// The spot at which to drop off the deposit.
			/// </summary>
			public Vector3 destination { get; private set; }

			private NavMeshAgent navMeshAgent;


			private void DropOff( IInventory inventory, Vector3 direction )
			{
				// Creates deposit(s) from the inventory's items.
				// Clears the inventory.
				// Stops the agent.

				if( Physics.Raycast( this.gameObject.transform.position + direction.normalized + new Vector3( 0, 5, 0 ), Vector3.down, out RaycastHit hitInfo ) )
				{
					Dictionary<string,int> resourcesCarried = inventory.GetAll();
					if( hitInfo.collider.gameObject.layer == ObjectLayer.TERRAIN )
					{
						foreach( var kvp in resourcesCarried )
						{
							ResourceDepositDefinition def = DataManager.Get<ResourceDepositDefinition>( DataManager.Get<ResourceDefinition>( kvp.Key ).defaultDeposit );
							int capacity = def.resources[kvp.Key];
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
								obj = ResourceDepositCreator.Create( def, hitInfo.point, Quaternion.identity, dict );
								AudioManager.PlayNew( obj.GetComponent<ResourceDeposit>().dropoffSound );
							}
						}
						inventory.Clear();
					}
					this.navMeshAgent.ResetPath();
				}
			}

			void Start()
			{
				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
				this.navMeshAgent.SetDestination( this.destination );
			}

			void Update()
			{
				if( Vector3.Distance( this.transform.position, destination ) < 1 )
				{
					IInventory inventory = this.GetComponent<IInventory>();

					if( inventory == null )
					{
						Object.Destroy( this );
						throw new System.Exception( "TAIGoal.DropOffDeposit was added to an object that doesn't have inventory." );
					}
					
					if( !inventory.isEmpty )
					{
						Vector3 direction = (destination - this.transform.position).normalized;
						this.DropOff( inventory, direction );
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

				DropOffDeposit dropOffDeposit = gameObject.AddComponent<TAIGoal.DropOffDeposit>();

				dropOffDeposit.destination = destination;
			}
		}
	}
}