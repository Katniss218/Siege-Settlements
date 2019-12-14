using Katniss.Utils;
using SS.Content;
using SS.Levels.SaveStates;
using SS.Objects;
using SS.Objects.Extras;
using SS.Objects.Modules;
using SS.ResourceSystem;
using SS.ResourceSystem.Payment;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace SS.AI.Goals
{
	public class TacticalDropOffGoal : TacticalGoal
	{
		public const string KFF_TYPEID = "drop_off";

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

		public enum ObjectDropOffMode : byte
		{
			INVENTORY,
			PAYMENT
		}

		public DestinationType destination { get; private set; }
		public Vector3? destinationPos { get; private set; }
		public SSObject destinationObject { get; private set; }
		public ObjectDropOffMode objectDropOffMode { get; set; }
		
		/// <summary>
		/// Specified which resource is going to be dropped off (null for all).
		/// </summary>
		public string resourceId { get; set; }

		public bool isHostile { get; set; }


		private Vector3 oldDestination;
		private InventoryModule inventory;
		private NavMeshAgent navMeshAgent;
		private IAttackModule[] attackModules;

		public TacticalDropOffGoal()
		{
			this.isHostile = true;
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
			this.destination = DestinationType.OBJECT;
			this.destinationPos = null;
			this.destinationObject = destination;
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		private bool IsOnValidObject( SSObject ssObject )
		{
			return ssObject is INavMeshAgent && ssObject.GetModules<InventoryModule>().Length > 0;
		}

		public override void Start( TacticalGoalController controller )
		{
			if( !IsOnValidObject( controller.ssObject ) )
			{
				controller.goal = TacticalGoalController.GetDefaultGoal();
				throw new Exception( this.GetType().Name + "Was added to an invalid object " + controller.ssObject.GetType().Name );
			}
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
				if( Vector3.Distance( this.navMeshAgent.pathEndPosition, controller.transform.position ) <= Main.DEFAULT_NAVMESH_STOPPING_DIST_CUSTOM )
				{
					this.navMeshAgent.ResetPath();
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

		public static void ExtractFrom( Vector3 position, Quaternion rotation, Dictionary<string,int> resourcesCarried )
		{
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
						data.guid = Guid.NewGuid();
						data.position = position;
						data.rotation = rotation;


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
		}

		private void UpdateTargeting( TacticalGoalController controller )
		{
			if( this.isHostile )
			{
				IFactionMember fac = controller.GetComponent<IFactionMember>();
				for( int i = 0; i < this.attackModules.Length; i++ )
				{
					if( !Targeter.CanTarget( controller.transform.position, this.attackModules[i].attackRange, this.attackModules[i].targeter.target, fac.factionMember ) )
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
							this.attackModules[i].targeter.TrySetTarget( controller.transform.position, this.attackModules[i].attackRange, Targeter.TargetingMode.CLOSEST );
						}
					}
				}
			}
			else
			{
				for( int i = 0; i < this.attackModules.Length; i++ )
				{
					if( this.attackModules[i].targeter.target != null )
					{
						this.attackModules[i].targeter.target = null;
					}
				}
			}
		}

		private void OnArrivalObject( TacticalGoalController controller )
		{
			if( this.objectDropOffMode == ObjectDropOffMode.INVENTORY )
			{
				InventoryModule destinationInventory = this.destinationObject.GetModules<InventoryModule>()[0];

				Dictionary<string, int> resourcesCarried = this.inventory.GetAll();

				foreach( var kvp in resourcesCarried )
				{
					// If the goal wants a specific resource - disregard any other resources.
					if( !string.IsNullOrEmpty( this.resourceId ) && kvp.Key != this.resourceId )
					{
						continue;
					}

					int spaceLeft = destinationInventory.GetSpaceLeft( kvp.Key );
					if( spaceLeft > 0 )
					{
						int amountCarried = kvp.Value;
						int amountDroppedOff = spaceLeft < amountCarried ? spaceLeft : amountCarried;

						destinationInventory.Add( kvp.Key, amountDroppedOff );
						this.inventory.Remove( kvp.Key, amountDroppedOff );

						ResourceDefinition def = DefinitionManager.GetResource( kvp.Key );
						AudioManager.PlaySound( def.dropoffSound );
					}
				}
			}
			else if( this.objectDropOffMode == ObjectDropOffMode.PAYMENT )
			{
				IPaymentReceiver[] paymentReceivers = this.destinationObject.GetComponents<IPaymentReceiver>();

				for( int i = 0; i < paymentReceivers.Length; i++ )
				{
					Dictionary<string, int> wantedRes = paymentReceivers[i].GetWantedResources();

					foreach( var kvp in wantedRes )
					{
						// If the goal wants a specific resource - disregard any other resources.
						if( !string.IsNullOrEmpty( this.resourceId ) && kvp.Key != this.resourceId )
						{
							continue;
						}

						int amountInInv = this.inventory.Get( kvp.Key );

						if( amountInInv == 0 )
						{
							continue;
						}

						int amountPayed = amountInInv > kvp.Value ? kvp.Value : amountInInv;

						this.inventory.Remove( kvp.Key, amountPayed );
						paymentReceivers[i].ReceivePayment( kvp.Key, amountPayed );
						ResourceDefinition resDef = DefinitionManager.GetResource( kvp.Key );
						AudioManager.PlaySound( resDef.dropoffSound );
					}
					// If there is no resources to pay (everything spent).
					if( this.inventory.isEmpty )
					{
						break;
					}
				}
			}
			this.navMeshAgent.ResetPath();
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
				}

				// Clear the path, when it's in range.
				this.navMeshAgent.ResetPath();
				controller.goal = TacticalGoalController.GetDefaultGoal();
			}
			else
			{
				this.navMeshAgent.ResetPath();
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
					this.navMeshAgent.ResetPath();
					controller.goal = TacticalGoalController.GetDefaultGoal();
					return;
				}

				if( this.destinationObject == controller.ssObject )
				{
					Debug.LogWarning( controller.ssObject.definitionId + ": Destination was set to itself." );
					this.navMeshAgent.ResetPath();
					controller.goal = TacticalGoalController.GetDefaultGoal();
					return;
				}

				if( (this.destinationObject is IUsableToggle) && !((IUsableToggle)this.destinationObject).IsUsable() )
				{
					this.navMeshAgent.ResetPath();
					controller.goal = TacticalGoalController.GetDefaultGoal();
					return;
				}
			}

			// If it's not usable - return, don't move.
			if( (controller.ssObject is IUsableToggle) && !((IUsableToggle)controller.ssObject).IsUsable() )
			{
				return;
			}

			this.UpdatePosition( controller );
			this.UpdateTargeting( controller );

			if( this.destination == DestinationType.OBJECT )
			{
				if( PhysicsDistance.OverlapInRange( controller.transform, this.destinationObject.transform, 0.75f ) )
				{
					this.OnArrivalObject( controller );
				}
			}
			else if( this.destination == DestinationType.POSITION )
			{
				if( Vector3.Distance( controller.transform.position, this.destinationPos.Value ) <= 0.75f )
				{
					this.OnArrivalPosition( controller );
				}
			}
		}
		

		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-

		
		public override TacticalGoalData GetData()
		{
			TacticalDropOffGoalData data = new TacticalDropOffGoalData()
			{
				resourceId = this.resourceId,
				isHostile = this.isHostile
			};
			data.destination = this.destination;
			if( this.destination == DestinationType.OBJECT )
			{
				data.destinationObjectGuid = this.destinationObject.guid;
			}
			else if( this.destination == DestinationType.POSITION )
			{
				data.destinationPosition = this.destinationPos;
			}

			return data;
		}

		public override void SetData( TacticalGoalData _data )
		{
			TacticalDropOffGoalData data = (TacticalDropOffGoalData)_data;

			this.resourceId = data.resourceId;

			this.destination = data.destination;

			if( this.destination == DestinationType.OBJECT )
			{
				this.destinationObject = SSObject.Find( data.destinationObjectGuid.Value );
			}
			else if( this.destination == DestinationType.POSITION )
			{
				this.destinationPos = data.destinationPosition;
			}

			this.isHostile = data.isHostile;
		}
	}
}