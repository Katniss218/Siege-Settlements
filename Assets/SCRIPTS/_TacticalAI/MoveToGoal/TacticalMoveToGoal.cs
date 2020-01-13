using Katniss.Utils;
using SS.Objects;
using SS.Objects.Modules;
using SS.Objects.Units;
using UnityEngine;
using UnityEngine.AI;

namespace SS.AI.Goals
{
	public class TacticalMoveToGoal : TacticalGoal
	{
		public const string KFF_TYPEID = "move_to";

		private const float OBJECT_MODE_STOPPING_DISTANCE = 0.75f;
		private const float INTERIOR_MODE_STOPPING_DISTANCE = 0.75f;

		public enum DestinationType : byte
		{
			POSITION,
			OBJECT,
			INTERIOR
		}

		public DestinationType destination { get; private set; }
		public Vector3 destinationPos { get; private set; }
		public SSObject destinationObject { get; private set; }
		public InteriorModule destinationInterior { get; private set; }
		public InteriorModule.SlotType interiorSlotType { get; private set; }

		public bool isHostile { get; set; }


		private Vector3 oldDestination;
		private NavMeshAgent navMeshAgent;
		private IAttackModule[] attackModules;


		public TacticalMoveToGoal()
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
			this.destinationInterior = null;
		}

		public void SetDestination( SSObject destination )
		{
			this.destination = DestinationType.OBJECT;
			this.destinationObject = destination;
		}

		public void SetDestination( InteriorModule interior, InteriorModule.SlotType destinationSlotType )
		{
			this.destination = DestinationType.INTERIOR;
			this.destinationObject = null;
			this.destinationInterior = interior;
			this.interiorSlotType = destinationSlotType;
		}


		public override bool CanBeAddedTo( SSObject ssObject )
		{
			return ssObject is IMovable;
		}

		public override void Start( TacticalGoalController controller )
		{
			this.navMeshAgent = (controller.ssObject as IMovable).navMeshAgent;
			this.attackModules = controller.GetComponents<IAttackModule>();
		}

		public override void OnSuccess()
		{
			if( this.navMeshAgent.isOnNavMesh )
			{
				this.navMeshAgent.ResetPath();
			}
			base.OnSuccess();
		}

		public override void OnFail()
		{
			if( this.navMeshAgent.isOnNavMesh )
			{
				this.navMeshAgent.ResetPath();
			}
			base.OnFail();
		}

		private void UpdatePosition( TacticalGoalController controller )
		{
			if( this.destination == DestinationType.POSITION )
			{
				Vector3 currDestPos = this.destinationPos;
				if( this.oldDestination != currDestPos )
				{
#warning setdestination needs to take into account the side from which it's coming. Not perfect, but works in most simple & obvious conditions.
					this.navMeshAgent.SetDestination( currDestPos );
				}

				// If the agent has travelled to the destination - switch back to the default Goal.
				if( this.navMeshAgent.hasPath )
				{
					if( Vector3.Distance( this.navMeshAgent.pathEndPosition, controller.transform.position ) <= Main.DEFAULT_NAVMESH_STOPPING_DIST_CUSTOM )
					{
						controller.ExitCurrent( TacticalGoalExitCondition.SUCCESS );
						return;
					}
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


				if( PhysicsDistance.OverlapInRange( controller.transform, this.destinationObject.transform, OBJECT_MODE_STOPPING_DISTANCE ) )
				{
					controller.ExitCurrent( TacticalGoalExitCondition.SUCCESS );
					return;
				}

				this.oldDestination = currDestPos;
				return;
			}
			if( this.destination == DestinationType.INTERIOR )
			{
				Vector3 currDestPos = this.destinationInterior.transform.position;
				
				if( this.oldDestination != currDestPos )
				{
					this.navMeshAgent.SetDestination( currDestPos );
				}
				
				currDestPos = this.destinationInterior.EntranceWorldPosition();
				
				
				// If the agent has travelled to the destination - switch back to the default Goal.
				if( PhysicsDistance.OverlapInRange( controller.transform, this.destinationInterior.transform, INTERIOR_MODE_STOPPING_DISTANCE ) )
				{
					if( this.destinationInterior.ssObject is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)this.destinationInterior.ssObject).isUsable )
					{
						controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
					}
					if( controller.ssObject is Unit )
					{
						Unit unit = (Unit)controller.ssObject;

						InteriorModule.SlotType slotType = this.interiorSlotType;
						int? slotIndex = this.destinationInterior.GetFirstValid( slotType, unit );

						if( slotIndex == null )
						{
							Debug.LogWarning( "Can't enter slot: " + this.destinationInterior.ssObject.definitionId );
							controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
							return;
						}
						unit.SetInside( this.destinationInterior, slotType, slotIndex.Value );
						controller.ExitCurrent( TacticalGoalExitCondition.SUCCESS );
						return;
					}
					controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
					return;
				}
				
				this.oldDestination = currDestPos;
				return;
			}
		}



		public override void Update( TacticalGoalController controller )
		{
			if( controller.ssObject is IInteriorUser )
			{
				IInteriorUser interiorUser = (IInteriorUser)controller.ssObject;

				if( interiorUser.isInside )
				{
					interiorUser.SetOutside();
				}
			}

			// If the object was picked up/destroyed/etc. (is no longer on the map), stop the Goal.
			if( (this.destination == DestinationType.OBJECT) && this.destinationObject == null )
			{
				controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
				return;
			}

			// If the object was picked up/destroyed/etc. (is no longer on the map), stop the Goal.
			if( (this.destination == DestinationType.INTERIOR) && this.destinationInterior == null )
			{
				controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
				return;
			}

			// If it's not usable - return, don't move.
			if( controller.ssObject is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)controller.ssObject).isUsable )
			{
				controller.ExitCurrent( TacticalGoalExitCondition.FAILURE );
				return;
			}

			if( attackModules.Length > 0 )
			{
				this.UpdateTargeting( controller, this.isHostile, this.attackModules );
			}

			this.UpdatePosition( controller );
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override TacticalGoalData GetData()
		{
			TacticalMoveToGoalData data = new TacticalMoveToGoalData()
			{
				isHostile = this.isHostile
			};
			data.destinationType = this.destination;
			if( this.destination == DestinationType.POSITION )
			{
				data.destinationPosition = this.destinationPos;
			}
			else if( this.destination == DestinationType.OBJECT )
			{
				data.destinationObjectGuid = this.destinationObject.guid;
			}
			else if( this.destination == DestinationType.INTERIOR )
			{
				data.destinationObjectGuid = this.destinationInterior.ssObject.guid;
				data.interiorModuleId = this.destinationInterior.moduleId;
				data.interiorSlotType = this.interiorSlotType;
			}

			return data;
		}

		public override void SetData( TacticalGoalData _data )
		{
			TacticalMoveToGoalData data = (TacticalMoveToGoalData)_data;

			this.destination = data.destinationType;
			
			if( this.destination == DestinationType.POSITION )
			{
				this.destinationPos = data.destinationPosition;
			}
			else if( this.destination == DestinationType.OBJECT )
			{
				this.SetDestination( SSObject.Find( data.destinationObjectGuid ) );
			}
			else if( this.destination == DestinationType.INTERIOR )
			{
				this.SetDestination( SSObject.Find( data.destinationObjectGuid ).GetModule<InteriorModule>( data.interiorModuleId ), data.interiorSlotType );
			}

			this.isHostile = data.isHostile;
		}
	}
}