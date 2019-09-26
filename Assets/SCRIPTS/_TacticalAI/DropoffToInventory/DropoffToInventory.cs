using Katniss.Utils;
using SS.Content;
using SS.Inventories;
using SS.ResourceSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS
{
	public abstract partial class TAIGoal
	{
		public class DropoffToInventory : TAIGoal
		{
			public GameObject destination { get; private set; }


			private NavMeshAgent navMeshAgent;
			private IInventory inventory;


			private void DropOff()
			{
				Dictionary<string, int> resourcesCarried = this.inventory.GetAll();

				foreach( var kvp in resourcesCarried )
				{
					int maxCapacity = this.destinationInventory.GetMaxCapacity( kvp.Key );
					if( maxCapacity > 0 )
					{
						int amount = this.destinationInventory.Get( kvp.Key );

						int spaceLeft = maxCapacity - amount;
						if( spaceLeft > 0 )
						{
							int dropOffAmt = kvp.Value;
							if( spaceLeft < kvp.Value )
							{
								dropOffAmt = spaceLeft;
							}

							this.destinationInventory.Add( kvp.Key, dropOffAmt );
							this.inventory.Remove( kvp.Key, dropOffAmt );
							ResourceDefinition def = DefinitionManager.GetResource( kvp.Key );
							AudioManager.PlayNew( def.dropoffSound.Item2 );
						}
					}
				}

				this.navMeshAgent.ResetPath();
			}

			void Start()
			{
				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
				this.inventory = this.GetComponent<IInventory>();
				if( this.navMeshAgent == null )
				{
					throw new System.Exception( "Can't add DropoffToInventory TAI goal to: " + this.gameObject.name );
				}
				if( this.inventory == null )
				{
					throw new System.Exception( "Can't add DropoffToInventory TAI goal to: " + this.gameObject.name );
				}
				if( this.destination == null )
				{
					Debug.LogWarning( "Not assigned destination to: " + this.gameObject.name );
					Object.Destroy( this );
				}

				this.navMeshAgent.SetDestination( this.destination.transform.position );
			}

			void Update()
			{
				// If the destination inventory can no longer hold any of the items in this inventory, stop the AI.
				foreach( var kvp in this.inventory.GetAll() )
				{
					if( this.destinationInventory.GetMaxCapacity( kvp.Key ) != this.destinationInventory.Get( kvp.Key ) )
					{
						continue;
					}
					Object.Destroy( this );
					return;
				}
				if( PhysicsDistance.OverlapInRange( this.transform, this.destinationTransform, 0.75f ) )
				{
					if( !this.inventory.isEmpty )
					{
						Vector3 direction = (this.destinationTransform.position - this.transform.position).normalized;
						this.DropOff();
					}
					else
					{
						Object.Destroy( this );
					}
				}
			}

			public override TAIGoalData GetData()
			{
				DropoffToInventoryData data = new DropoffToInventoryData();

				data.destinationInventoryObjIndex = wtf. in what array to lookup it?.

				throw new System.NotImplementedException();
			}

			/// <summary>
			/// Assigns a new DropoffToInventory TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, GameObject destination )
			{
				TAIGoal.ClearGoal( gameObject );

				DropoffToInventory dropOffDeposit = gameObject.AddComponent<TAIGoal.DropoffToInventory>();

				dropOffDeposit.destination = destination;
			}
		}
	}
}