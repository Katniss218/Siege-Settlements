using SS.Extras;
using SS.ResourceSystem;
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
					Inventory inventory = this.GetComponent<Inventory>();

					if( inventory == null )
					{
						Destroy( this );
						throw new System.Exception( "TAIGoal.DropOffDeposit was added to an object that doesn't have inventory." );
					}

					if( inventory.isCarryingResource )
					{
						inventory.DropOffResource( (destination - this.transform.position).normalized );
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