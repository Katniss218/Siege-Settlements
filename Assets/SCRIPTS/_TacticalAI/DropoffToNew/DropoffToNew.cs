using SS.Content;
using SS.Objects.Extras;
using SS.Modules.Inventories;
using SS.Levels.SaveStates;
using SS.ResourceSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SS.Modules;

namespace SS
{
	public abstract partial class TAIGoal
	{
		/// <summary>
		/// Drops off the entire contents of the inventory at the specified location.
		/// </summary>
		public class DropoffToNew : TAIGoal
		{
			/// <summary>
			/// The spot at which to drop off the deposit.
			/// </summary>
			public Vector3 destination { get; private set; }

			private NavMeshAgent navMeshAgent;
			private InventoryModule inventory;


			/// <summary>
			/// Drops every resource in the inventory as a default Resource Deposit for that resource type.
			/// </summary>
			/// <param name="carrierInv">The inventory that should be dropped.</param>
			/// <param name="position">The position of the newly spawned deposits.</param>
			public static void DropOffInventory( InventoryModule carrierInv, Vector3 position )
			{
				if( carrierInv.isEmpty )
				{
					throw new System.Exception( "Inventory was empty." );
				}

				Dictionary<string, int> resourcesCarried = carrierInv.GetAll();
				
				foreach( var kvp in resourcesCarried )
				{
					ResourceDefinition resourceDef = DefinitionManager.GetResource( kvp.Key );
					
					ExtraDefinition def = DefinitionManager.GetExtra( resourceDef.defaultDeposit );

					ResourceDepositModuleDefinition depositDef = def.GetModule<ResourceDepositModuleDefinition>();
					int capacity = 0;
					for( int i = 0; i < depositDef.slots.Length; i++ )
					{
						if( depositDef.slots[i].resourceId == kvp.Key )
						{
							capacity = depositDef.slots[i].capacity;
						}
					}
					if( capacity != 0 )
					{
						int remaining = kvp.Value;
						while( remaining > 0 )
						{
							int resAmount = capacity;
							if( remaining < capacity )
							{
								resAmount = remaining;
							}
							remaining -= resAmount;

							ExtraData data = new ExtraData();
							data.position = position;
							data.rotation = Quaternion.identity;
							

							GameObject extra = ExtraCreator.Create( def, data );
							ResourceDepositModule resDepo = extra.GetComponent<ResourceDepositModule>();
							foreach( var slot in def.GetModule<ResourceDepositModuleDefinition>().slots )
							{
								resDepo.Add( slot.resourceId, resAmount );
							}
							AudioManager.PlaySound( resourceDef.dropoffSound );
						}
					}
				}
				carrierInv.Clear();
			}
		
			void Start()
			{
				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
#warning TODO! - replace with ssobject getmodule.
				this.inventory = this.GetComponent<InventoryModule>();
				if( this.navMeshAgent == null )
				{
					throw new System.Exception( "Can't add DropoffToNew TAI goal to: " + this.gameObject.name );
				}
				if( this.inventory == null )
				{
					throw new System.Exception( "Can't add DropoffToNew TAI goal to: " + this.gameObject.name );
				}

				this.navMeshAgent.SetDestination( this.destination );
			}

			void Update()
			{
				// If this inventory was emptied in the mean time, stop the AI.
				if( this.inventory.isEmpty )
				{
					Object.Destroy( this );
					return;
				}
				if( Vector3.Distance( this.transform.position, this.destination ) < 0.75f )
				{
					Vector3 direction = (this.destination - this.transform.position).normalized;
					if( Physics.Raycast( this.gameObject.transform.position + direction.normalized + new Vector3( 0, 5, 0 ), Vector3.down, out RaycastHit hitInfo ) )
					{
						if( hitInfo.collider.gameObject.layer == ObjectLayer.TERRAIN )
						{
							Vector3 depositPosition = hitInfo.point;
							DropOffInventory( this.inventory, depositPosition );

							Object.Destroy( this );
						}

						// Clear the path, when it's in range.
						this.navMeshAgent.ResetPath();
					}
					else
					{
						Object.Destroy( this );
					}
				}
			}

			public override TAIGoalData GetData()
			{
				DropoffToNewData data = new DropoffToNewData();
				data.destination = this.destination;
				return data;
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