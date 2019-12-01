using Katniss.Utils;
using SS.Content;
using SS.Objects.Modules;
using SS.Objects;
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
			private SSObject __destination = null;
			public SSObject destination
			{
				get
				{
					return this.__destination;
				}
				set
				{
					if( value != null )
					{
						if( value is IUsableToggle && !(value as IUsableToggle).IsUsable() )
						{
							Debug.LogWarning( "Tried to dropoff items to inventory that is not usable." );
							Destroy( this );
							return;
						}
						if( value.GetComponent<InventoryModule>() == null )
						{
							Debug.LogWarning( "Tried to dropoff items to non-inventory." );
							Destroy( this );
							return;
						}
					}
					this.__destination = value;
				}
			}



			private NavMeshAgent navMeshAgent;
			private InventoryModule inventory;


			private void OnArrival()
			{
				Dictionary<string, int> resourcesCarried = this.inventory.GetAll();
				
				InventoryModule destinationInventory = this.destination.GetComponent<InventoryModule>();

				foreach( var kvp in resourcesCarried )
				{
					int spaceLeftDst = destinationInventory.GetSpaceLeft( kvp.Key );
					if( spaceLeftDst > 0 )
					{
						int amountCarried = kvp.Value;
						int amountDroppedOff = spaceLeftDst < amountCarried ? spaceLeftDst : amountCarried;
						
						destinationInventory.Add( kvp.Key, amountDroppedOff );
						this.inventory.Remove( kvp.Key, amountDroppedOff );

						ResourceDefinition def = DefinitionManager.GetResource( kvp.Key );
						AudioManager.PlaySound( def.dropoffSound );
					}
				}

				this.navMeshAgent.ResetPath();
			}

			void Start()
			{
				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
				this.inventory = this.GetComponent<InventoryModule>();
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
					return;
				}
				if( this.destination == this.gameObject )
				{
					Debug.LogWarning( "Destination assigned to itself: " + this.gameObject.name );
					Object.Destroy( this );
					return;
				}

				this.navMeshAgent.SetDestination( this.destination.transform.position );
			}

			void Update()
			{
				// If the destination has been destroyed.
				if( this.destination == null )
				{
					Object.Destroy( this );
					return;
				}
				// If this inventory was emptied in the mean time, stop the AI.
				if( this.inventory.isEmpty )
				{
					Object.Destroy( this );
					return;
				}
				if( PhysicsDistance.OverlapInRange( this.transform, this.destination.transform, 0.75f ) )
				{
					// Clear the path, when it's in range.
					this.navMeshAgent.ResetPath();
					
					this.OnArrival();
					Object.Destroy( this );
				}
			}

			public override TAIGoalData GetData()
			{
				DropoffToInventoryData data = new DropoffToInventoryData();

				data.destinationGuid = this.destination.guid.Value;

				return data;
			}

			/// <summary>
			/// Assigns a new DropoffToInventory TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, SSObject destination )
			{
				TAIGoal.ClearGoal( gameObject );

				DropoffToInventory dropOffDeposit = gameObject.AddComponent<TAIGoal.DropoffToInventory>();

				dropOffDeposit.destination = destination;
			}
		}
	}
}