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
			public ResourceDeposit depositToCollect { get; private set; }

			void Start()
			{
				this.GetComponent<NavMeshAgent>().SetDestination( this.depositToCollect.transform.position );
			}

			void Update()
			{
				if( this.depositToCollect == null )
				{
					Object.Destroy( this ); // if the deposit was picked up, stop the AI.
					return;
				}
				if( Vector3.Distance( this.transform.position, this.depositToCollect.transform.position ) < 1 )
				{
					IInventory inventory = this.GetComponent<IInventory>();

					if( inventory == null )
					{
						Destroy( this );
						throw new System.Exception( "TAIGoal.CollectDeposit was added to an object that doesn't have IInventory." );
					}

					int amountPickedUp = inventory.Add( this.depositToCollect.resourceId, 1 );
					if( amountPickedUp != 0 )
						this.depositToCollect.PickUp( amountPickedUp );
				}
			}

			/// <summary>
			/// Assigns a new PickUpResource TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, ResourceDeposit depositToPickUp )
			{
				TAIGoal.ClearGoal( gameObject );

				CollectDeposit pickUpResource = gameObject.AddComponent<TAIGoal.CollectDeposit>();

				pickUpResource.depositToCollect = depositToPickUp;
			}
		}
	}
}