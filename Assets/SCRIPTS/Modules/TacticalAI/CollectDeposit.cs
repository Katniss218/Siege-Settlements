using SS.Extras;
using UnityEngine;
using UnityEngine.AI;

namespace SS
{
	public abstract partial class TAIGoal
	{
		[RequireComponent( typeof( NavMeshAgent ) )]
		public class CollectDeposit : TAIGoal
		{
			public ResourceDeposit depositToPickUp;

			void Start()
			{
				this.GetComponent<NavMeshAgent>().SetDestination( depositToPickUp.transform.position );
			}

			void Update()
			{
				if( this.depositToPickUp == null )
				{
					Destroy( this ); // if the deposit was picked up, stop the AI.
					return;
				}
				if( Vector3.Distance( this.transform.position, this.depositToPickUp.transform.position ) < 1 )
				{
					this.GetComponent<Inventory>().PickupResource( new ResourceSystem.ResourceStack( depositToPickUp.resourceId, 1 ) );
					depositToPickUp.PickUp( 1 );
				}
			}

			/// <summary>
			/// Assigns a new PickUpResource TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, ResourceDeposit depositToPickUp )
			{
				TAIGoal.ClearGoal( gameObject );

				CollectDeposit pickUpResource = gameObject.AddComponent<TAIGoal.CollectDeposit>();

				pickUpResource.depositToPickUp = depositToPickUp;
			}
		}
	}
}