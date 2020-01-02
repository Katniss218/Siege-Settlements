using SS.Objects;
using SS.Objects.Modules;
using SS.Objects.Units;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS.AI.Goals
{
	public class TacticalMoveToGoal : TacticalGoal
	{
		public const string KFF_TYPEID = "move_to";

		private const float OBJECT_MODE_STOPPING_DISTANCE = 1.0f;

		public enum DestinationType : byte
		{
			POSITION,
			OBJECT
		}
		
		public DestinationType destination { get; private set; }
		public Vector3? destinationPos { get; private set; }
		public SSObject destinationObject { get; private set; }
		
		public bool isHostile { get; set; }


		private Vector3 oldDestination;
		private NavMeshAgent navMeshAgent;
		private IAttackModule[] attackModules;

		private InteriorModule destinationInterior;
		private InteriorModule.SlotType destinationSlotType;

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
			this.destinationPos = null;
			this.destinationObject = destination;
		}

		public void SetDestination( InteriorModule interior, InteriorModule.SlotType destinationSlotType )
		{
			this.destination = DestinationType.OBJECT;
			this.destinationPos = null;
			this.destinationObject = interior.ssObject;
			this.destinationInterior = interior;
			this.destinationSlotType = destinationSlotType;
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

		private void UpdatePosition( TacticalGoalController controller )
		{			
			if( this.destination == DestinationType.POSITION )
			{
				Vector3 currDestPos = this.destinationPos.Value;
				if( this.oldDestination != currDestPos )
				{
					this.navMeshAgent.SetDestination( currDestPos );
				}

				// If the agent has travelled to the destination - switch back to the default Goal.
				if( this.navMeshAgent.hasPath )
				{
					if( Vector3.Distance( this.navMeshAgent.pathEndPosition, controller.transform.position ) <= Main.DEFAULT_NAVMESH_STOPPING_DIST_CUSTOM )
					{
						this.navMeshAgent.ResetPath();
						controller.goal = TacticalGoalController.GetDefaultGoal();
						return;
					}
				}

				this.oldDestination = currDestPos;

				return;
			}
			if( this.destination == DestinationType.OBJECT )
			{
				Vector3 currDestPos = this.destinationObject.transform.position;

				if( this.destinationInterior != null )
				{
					if( controller.ssObject is Unit )
					{
						currDestPos = this.destinationInterior.EntranceWorldPosition();
					}
				}

				if( this.oldDestination != currDestPos )
				{
					this.navMeshAgent.SetDestination( currDestPos );
				}


				// If the agent has travelled to the destination - switch back to the default Goal.
				if( this.navMeshAgent.hasPath )
				{
					if( Vector3.Distance( this.navMeshAgent.pathEndPosition, controller.transform.position ) <= OBJECT_MODE_STOPPING_DISTANCE )
					{
						this.navMeshAgent.ResetPath();
						if( this.destinationInterior != null )
						{
							if( controller.ssObject is Unit )
							{
								Unit unit = (Unit)controller.ssObject;

								InteriorModule.SlotType slotType = this.destinationSlotType;
								int? slotIndex = this.destinationInterior.GetFirstValid( slotType, unit );

								if( slotIndex == null )
								{
									Debug.LogWarning( "Can't enter slot" );
									this.navMeshAgent.ResetPath();
									controller.goal = TacticalGoalController.GetDefaultGoal();
									return;
								}
								unit.SetInside( this.destinationInterior, slotType, slotIndex.Value );
								controller.goal = TacticalGoalController.GetDefaultGoal();
							}
						}

						return;
					}
				}
			

				this.oldDestination = currDestPos;
				
				return;
			}
		}

		

		public override void Update( TacticalGoalController controller )
		{
			if( controller.ssObject is Unit )
			{
				Unit unit = (Unit)controller.ssObject;

				if( unit.isInside )
				{
					unit.SetOutside();
				}
			}
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
			}
			// If it's not usable - return, don't move.
			if( controller.ssObject is IUsableToggle && !(controller.ssObject as IUsableToggle).IsUsable() )
			{
				return;
			}

			this.UpdatePosition( controller );
			if( attackModules.Length > 0 )
			{
				this.UpdateTargeting( controller, this.isHostile, this.attackModules );
			}
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
			TacticalMoveToGoalData data = (TacticalMoveToGoalData)_data;

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