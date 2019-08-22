using SS.Extras;
using UnityEngine;
using UnityEngine.AI;

namespace SS
{
	public abstract partial class TAIGoal
	{
		public class PickUpResource : TAIGoal
		{
			public ResourceDeposit depositToPickUp;

			public PickUpResource( ResourceDeposit depositToPickUp )
			{
				this.depositToPickUp = depositToPickUp;
			}

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
					this.GetComponent<InventoryModule>().PickupResource( new ResourceSystem.ResourceStack( depositToPickUp.resourceId, 1 ) );
					depositToPickUp.PickUp( 1 );
				}
			}
		}
	}
}