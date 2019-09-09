using SS.Data;
using SS.Extras;
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
			public Vector3 destination { get; private set; }

			void Start()
			{
				this.GetComponent<NavMeshAgent>().SetDestination( this.destination );
			}

			void Update()
			{
				if( Vector3.Distance( this.transform.position, destination ) < 1 )
				{
					IInventory inventory = this.GetComponent<IInventory>();

					if( inventory == null )
					{
						Destroy( this );
						throw new System.Exception( "TAIGoal.DropOffDeposit was added to an object that doesn't have inventory." );
					}

					List<ResourceStack> resourcesCarried = inventory.GetAll();

					if( resourcesCarried != null )
					{
						Vector3 direction = (destination - this.transform.position).normalized;
						if( Physics.Raycast( this.gameObject.transform.position + direction.normalized + new Vector3( 0, 5, 0 ), Vector3.down, out RaycastHit hitInfo ) )
						{
							// Create the dropped deposit in the world.
							if( hitInfo.collider.gameObject.layer == LayerMask.NameToLayer( "Terrain" ) )
							{
								foreach( ResourceStack stack in resourcesCarried )
									ResourceDeposit.Create( DataManager.Get<ResourceDepositDefinition>( DataManager.Get<ResourceDefinition>( stack.id ).defaultDeposit ), hitInfo.point, Quaternion.identity, stack.amount );
							}
							inventory.Clear();
						}
					}
					else
					{
						Destroy( this );
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