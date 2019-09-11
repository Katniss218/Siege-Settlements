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

			private float amtCollected = 0;

			private NavMeshAgent navMeshAgent;

			void Start()
			{
				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
				this.navMeshAgent.SetDestination( this.depositToCollect.transform.position );
			}

			void Update()
			{
				if( this.depositToCollect == null )
				{
					Object.Destroy( this ); // if the deposit was picked up, stop the AI.
					return;
				}
				// TODO ----- make it so the unit can pick up deposit even if the deposit itself is big enough to be above 2 units in size.
				if( Vector3.Distance( this.transform.position, this.depositToCollect.transform.position ) < 1 )
				{
					IInventory inventory = this.GetComponent<IInventory>();

					if( inventory == null )
					{
						Destroy( this );
						throw new System.Exception( "TAIGoal.CollectDeposit was added to an object that doesn't have IInventory." );
					}

					// Clear the path, when it's in range of the deposit.
					this.navMeshAgent.ResetPath();

					int amountPickedUp = 0;
					if( this.depositToCollect.isTypeExtracted )
					{
						amountPickedUp = inventory.Add( this.depositToCollect.resourceId, 1 );
					}
					else
					{
						amtCollected += ResourceDeposit.MINING_SPEED * Time.deltaTime;
						int amtFloored = Mathf.FloorToInt( amtCollected );
						if( amtFloored >= 1 )
						{
							amountPickedUp = inventory.Add( this.depositToCollect.resourceId, amtFloored );
							amtCollected -= amtFloored;
						}
					}

					if( amountPickedUp != 0 )
					{
						this.depositToCollect.PickUp( amountPickedUp );
					}
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