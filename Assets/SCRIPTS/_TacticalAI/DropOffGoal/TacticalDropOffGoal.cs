using Katniss.Utils;
using SS.Content;
using SS.Levels.SaveStates;
using SS.Objects;
using SS.Objects.Extras;
using SS.Objects.Modules;
using SS.ResourceSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS.AI.Goals
{
	public class TacticalDropOffGoal : TacticalGoal
	{
		public enum DestinationType : byte
		{
			POSITION,
			OBJECT
		}

		public enum GoalHostileMode : byte
		{
			ALL,
			NONE
		}

		public DestinationType destination { get; private set; }
		public Vector3? destinationPos { get; private set; }
		public SSObject destinationObject { get; private set; }
		
		/// <summary>
		/// Specified which resource is going to be dropped off (null for all).
		/// </summary>
		public string resourceId { get; set; }

		public GoalHostileMode hostileMode { get; set; }


		private Vector3 oldDestination;
		private InventoryModule inventory;
		private NavMeshAgent navMeshAgent;
		private IAttackModule[] attackModules;

		public TacticalDropOffGoal()
		{
			this.hostileMode = GoalHostileMode.ALL;
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public void SetDestination( Vector3 destination )
		{
			this.destination = DestinationType.POSITION;
			this.destinationPos = destination;
			this.destinationObject = null;
		}

		public void SetDestination( SSObject destination )
		{
#warning TODO! - disable assigning itself as the destination.
			this.destination = DestinationType.OBJECT;
			this.destinationPos = null;
			this.destinationObject = destination;
			this.oldDestination = this.destinationObject.transform.position;
		}



		public override void Start( TacticalGoalController controller )
		{
			this.inventory = controller.ssObject.GetModules<InventoryModule>()[0];
			this.navMeshAgent = (controller.ssObject as INavMeshAgent).navMeshAgent;
			this.attackModules = controller.GetComponents<IAttackModule>();
		}

		private void UpdatePosition( TacticalGoalController controller )
		{
			if( this.destination == DestinationType.POSITION )
			{
				Vector3 currDestPos = this.destinationPos.Value;
				if( this.oldDestination != currDestPos )
				{
					this.navMeshAgent.SetDestination( currDestPos );
				}

				// If the agent has travelled to the destination - switch back to the Idle Goal.
				if( navMeshAgent.desiredVelocity.magnitude < 0.01f )
				{
					controller.goal = TacticalGoalController.GetDefaultGoal();
				}

				this.oldDestination = currDestPos;

				return;
			}
			if( this.destination == DestinationType.OBJECT )
			{
				Vector3 currDestPos = this.destinationObject.transform.position;
				if( this.oldDestination != currDestPos )
				{
					this.navMeshAgent.SetDestination( currDestPos );
				}

				this.oldDestination = currDestPos;

				return;
			}
		}

		private void UpdateTargeting( TacticalGoalController controller )
		{
			if( hostileMode == GoalHostileMode.NONE )
			{
#warning TODO! - needs to stop targeting whatever it was targeting (if applicable).
				return;
			}

			if( hostileMode == GoalHostileMode.ALL )
			{
				// If it's not usable - return, don't attack.
				if( controller.ssObject is IUsableToggle && !(controller.ssObject as IUsableToggle).IsUsable() )
				{
					return;
				}

				IFactionMember fac = controller.GetComponent<IFactionMember>();
				for( int i = 0; i < this.attackModules.Length; i++ )
				{
					if( !Targeter.CanTarget( fac.factionMember, this.attackModules[i].targeter.target, controller.transform.position, this.attackModules[i].targeter.searchRange ) )
					{
						this.attackModules[i].targeter.target = null;
					}
				}

				if( Random.Range( 0, 5 ) == 0 ) // Recalculate target only 20% of the time (not really noticeable, but gives a nice boost to FPS).
				{
					for( int i = 0; i < this.attackModules.Length; i++ )
					{
						if( this.attackModules[i].isReadyToAttack )
						{
							this.attackModules[i].targeter.TrySetTarget( controller.transform.position );
						}
					}
				}

				return;
			}
		}

		private void OnArrivalObject( TacticalGoalController controller, InventoryModule destinationInventory )
		{
			Dictionary<string, int> resourcesCarried = this.inventory.GetAll();

			//InventoryModule destinationInventory = this.destinationObject.GetModules<InventoryModule>()[0];

			foreach( var kvp in resourcesCarried )
			{
#warning TODO! - ugly code.
				// If the goal wants a specific resource - disregard any other resources.
				if( !string.IsNullOrEmpty( this.resourceId ) && kvp.Key != this.resourceId )
				{
					continue;
				}
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

			controller.goal = TacticalGoalController.GetDefaultGoal();
		}

		private void OnArrivalPosition( TacticalGoalController controller )
		{
			Vector3 direction = (this.destinationPos.Value - controller.transform.position).normalized;
			if( Physics.Raycast( controller.transform.position + direction.normalized + new Vector3( 0, 5, 0 ), Vector3.down, out RaycastHit hitInfo ) )
			{
				if( hitInfo.collider.gameObject.layer == ObjectLayer.TERRAIN )
				{
					Vector3 depositPosition = hitInfo.point;
					if( this.inventory.isEmpty )
					{
						throw new System.Exception( "Inventory was empty." );
					}

					Dictionary<string, int> resourcesCarried = this.inventory.GetAll();

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
								data.position = depositPosition;
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
					this.inventory.Clear();

					controller.goal = TacticalGoalController.GetDefaultGoal();
				}

				// Clear the path, when it's in range.
				this.navMeshAgent.ResetPath();
			}
			else
			{
				controller.goal = TacticalGoalController.GetDefaultGoal();
			}
		}



		public override void Update( TacticalGoalController controller )
		{
			// If the object was picked up/destroyed/etc. (is no longer on the map), stop the Goal.
			if( this.destination == DestinationType.OBJECT )
			{
				if( this.destinationObject == null )
				{
					controller.goal = TacticalGoalController.GetDefaultGoal();
					return;
				}
			}
			this.UpdatePosition( controller );
			this.UpdateTargeting( controller );

			if( this.destination == DestinationType.OBJECT )
			{
				if( PhysicsDistance.OverlapInRange( controller.transform, this.destinationObject.transform, 0.75f ) )
				{
					InventoryModule destinationInventory = this.destinationObject.GetModules<InventoryModule>()[0];

					this.OnArrivalObject( controller, destinationInventory );
				}
			}
			else if( this.destination == DestinationType.POSITION )
			{
				if( Vector3.Distance( controller.transform.position, this.destinationPos.Value ) <= 0.75f )
				{
					this.OnArrivalPosition( controller );
				}
			}
#warning TODO! - drop off.
		}
		

		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-

		
		public override TacticalGoalData GetData()
		{
			return new TacticalDropOffGoalData()
			{
				resourceId = this.resourceId,
				destination = this.destination,
				destinationObjectGuid = this.destinationObject.guid,
				destinationPosition = this.destinationPos,
				hostileMode = this.hostileMode
			};
		}

		public override void SetData( TacticalGoalData _data )
		{
			TacticalDropOffGoalData data = (TacticalDropOffGoalData)_data;

			this.resourceId = data.resourceId;

			this.destination = data.destination;

			if( this.destination == DestinationType.OBJECT )
			{
				this.destinationObject = Main.GetSSObject( data.destinationObjectGuid.Value );
			}
			else if( this.destination == DestinationType.POSITION )
			{
				this.destinationPos = data.destinationPosition;
			}

			this.hostileMode = data.hostileMode;
		}
	}
}