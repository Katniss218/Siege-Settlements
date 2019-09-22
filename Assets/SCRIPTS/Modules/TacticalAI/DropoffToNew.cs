﻿using SS.Content;
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
		public class DropoffToNew : TAIGoal
		{
			/// <summary>
			/// The spot at which to drop off the deposit.
			/// </summary>
			public Vector3 destination { get; private set; }

			private IInventory inventory;

			private NavMeshAgent navMeshAgent;


			public static void DropOffInventory( IInventory inventory, Vector3 position )
			{
				if( inventory.isEmpty )
				{
					throw new System.Exception( "Inventory was empty." );
				}

				Dictionary<string, int> resourcesCarried = inventory.GetAll();

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
						obj = ResourceDepositCreator.Create( newDepositDef, position, Quaternion.identity, dict );
						AudioManager.PlayNew( resourceDef.dropoffSound.Item2 );
					}
				}
				inventory.Clear();
			}
		
			void Start()
			{
				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
				this.inventory = this.GetComponent<IInventory>();
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
				if( Vector3.Distance( this.transform.position, this.destination ) < 0.75f )
				{
					if( !this.inventory.isEmpty )
					{
						Vector3 direction = (this.destination - this.transform.position).normalized;

						if( Physics.Raycast( this.gameObject.transform.position + direction.normalized + new Vector3( 0, 5, 0 ), Vector3.down, out RaycastHit hitInfo ) )
						{
							if( hitInfo.collider.gameObject.layer == ObjectLayer.TERRAIN )
							{
								Vector3 depositPosition = hitInfo.point;
								DropOffInventory( this.inventory, depositPosition );
							}
							this.navMeshAgent.ResetPath();
						}
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