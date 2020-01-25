using SS.Objects;
using SS.Objects.Modules;
using SS.Objects.Units;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS.AI.Goals
{
	public class TacticalMakeFormationGoal : TacticalGoal
	{
		public const string KFF_TYPEID = "move_to";

		public enum DestinationType : byte
		{
			POSITION,
			OBJECT
		}

		private const float INTERACTION_DISTANCE = 0.5f;

		public bool isHostile { get; set; }
		public Unit beacon { get; set; }


		private Vector3 oldDestination;
		private NavMeshAgent navMeshAgent;
		private Unit unitFormationSelf;
		private IAttackModule[] attackModules;


		public TacticalMakeFormationGoal()
		{
			this.isHostile = true;
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override bool CanBeAddedTo( SSObject ssObject )
		{
			return ssObject is Unit;
		}

		public override void Start( TacticalGoalController controller )
		{
			this.navMeshAgent = ((IMovable)controller.ssObject).navMeshAgent;
			this.attackModules = controller.GetComponents<IAttackModule>();
			this.unitFormationSelf = (Unit)controller.ssObject;
		}


		// returns units that want to make formation with this unit.
		private static List<Unit> GetUnitsInRange( Unit unitSelf, float searchRange )
		{
			Collider[] colliders = Physics.OverlapSphere( unitSelf.transform.position, searchRange, ObjectLayer.UNITS_MASK );
			List<Unit> ret = new List<Unit>();

			for( int i = 0; i < colliders.Length; i++ )
			{
				Unit unit = colliders[i].GetComponent<Unit>();
				if( unit.definitionId != unitSelf.definitionId )
				{
					continue;
				}

				if( unit == unitSelf )
				{
					continue;
				}

				if( unit.factionId != unitSelf.factionId )
				{
					continue;
				}

				TacticalGoalController controller = unit.GetComponent<TacticalGoalController>();
				if( !(controller.currentGoal is TacticalMakeFormationGoal) )
				{
					continue;
				}

				if( !unit.CanChangePopulation() )
				{
					continue;
				}

				ret.Add( unit );
			}

			return ret;
		}

		private void UpdatePosition( TacticalGoalController controller )
		{
			Vector3 currDestPos = this.beacon.transform.position;

			if( this.oldDestination != currDestPos )
			{
				this.navMeshAgent.SetDestination( currDestPos + ((controller.transform.position - currDestPos).normalized * 0.025f) );
			}

			if( Vector3.Distance( this.navMeshAgent.pathEndPosition, controller.transform.position ) <= Main.DEFAULT_NAVMESH_STOPPING_DIST_CUSTOM )
			{
				// If the agent has travelled to the destination - switch back to the default Goal.
				if( this.navMeshAgent.hasPath )
				{
					this.navMeshAgent.ResetPath();
				}
			}

			this.oldDestination = currDestPos;

			return;
		}


		public override void Update( TacticalGoalController controller )
		{
			if( this.beacon == null )
			{
				this.navMeshAgent.ResetPath();
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


			if( (SSObject)this.beacon == controller.ssObject )
			{
				if( !beacon.CanChangePopulation() )
				{
					return;
				}

				List<Unit> unitsNearby = GetUnitsInRange( this.unitFormationSelf, INTERACTION_DISTANCE );

				Unit.Join( this.unitFormationSelf, unitsNearby );
			}
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override TacticalGoalData GetData()
		{
			TacticalMakeFormationGoalData data = new TacticalMakeFormationGoalData()
			{
				isHostile = this.isHostile
			};

			data.beaconGuid = this.beacon.guid;

			return data;
		}

		public override void SetData( TacticalGoalData _data )
		{
			TacticalMakeFormationGoalData data = (TacticalMakeFormationGoalData)_data;

			this.beacon = SSObject.Find( data.beaconGuid.Value ) as Unit;

			this.isHostile = data.isHostile;
		}
	}
}