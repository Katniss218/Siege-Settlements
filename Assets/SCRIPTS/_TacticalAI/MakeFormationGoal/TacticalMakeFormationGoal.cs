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

		
		public override bool IsOnValidObject( SSObject ssObject )
		{
			return ssObject is Unit;
		}

		public override void Start( TacticalGoalController controller )
		{
			this.navMeshAgent = ((IMovable)controller.ssObject).navMeshAgent;
			this.attackModules = controller.GetComponents<IAttackModule>();
			this.unitFormationSelf = (Unit)controller.ssObject;
		}

		private void UpdatePosition( TacticalGoalController controller )
		{
			Vector3 currDestPos = this.beacon.transform.position;

			if( this.oldDestination != currDestPos )
			{
				this.navMeshAgent.SetDestination( currDestPos );
			}

			this.oldDestination = currDestPos;

			return;
		}

		// returns units that want to make formation with this unit.
		private List<Unit> GetUnitsInRange( Vector3 position, float searchRange, string id, int factionId )
		{
			Collider[] colliders = Physics.OverlapSphere( position, searchRange, ObjectLayer.UNITS_MASK );
			List<Unit> ret = new List<Unit>();

			for( int i = 0; i < colliders.Length; i++ )
			{
				Unit unit = colliders[i].GetComponent<Unit>();
				if( unit.definitionId != id )
				{
					continue;
				}

				if( unit == this.unitFormationSelf )
				{
					continue;
				}

				if( unit.factionId != factionId )
				{
					continue;
				}

				TacticalGoalController controller = unit.GetComponent<TacticalGoalController>();
				if( !(controller.goal is TacticalMakeFormationGoal) )
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

		public override void Update( TacticalGoalController controller )
		{
			if( this.beacon == null )
			{
				this.navMeshAgent.ResetPath();
				controller.goal = TacticalGoalController.GetDefaultGoal();
				return;
			}

			// If it's not usable - return, don't move.
			if( controller.ssObject is IUsableToggle && !(controller.ssObject as IUsableToggle).IsUsable() )
			{
				return;
			}

			this.UpdatePosition( controller );
			this.UpdateTargeting( controller, this.isHostile, this.attackModules );

			if( Vector3.Distance( this.navMeshAgent.pathEndPosition, controller.transform.position ) <= Main.DEFAULT_NAVMESH_STOPPING_DIST_CUSTOM )
			{
				// If the agent has travelled to the destination - switch back to the default Goal.
				if( this.navMeshAgent.hasPath )
				{
					this.navMeshAgent.ResetPath();
				}
			}

			if( (SSObject)this.beacon == controller.ssObject )
			{
				if( !beacon.CanChangePopulation() )
				{
					return;
				}

				List<Unit> unitsNearby = this.GetUnitsInRange( controller.transform.position, 0.5f, this.unitFormationSelf.definitionId, this.unitFormationSelf.factionId );

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




		public static Vector3 GridToWorld( Vector2Int grid, int sizeX, int sizeZ, Vector3 gridCenter, float gridSpacing )
		{
			float camRotY = Main.cameraPivot.rotation.eulerAngles.y;

			Vector3 offset = new Vector3( (sizeX - 1) / 2.0f, 0, (sizeZ - 1) / 2.0f );

			Vector3 gridRelativeToCenterLocal = new Vector3( grid.x, 0, grid.y ) - offset;
			Vector3 gridRelativeToCenterLocalRotated = Quaternion.Euler( 0, camRotY, 0 ) * (gridRelativeToCenterLocal);

			Vector3 global = gridRelativeToCenterLocalRotated * gridSpacing + gridCenter;

			return global;
		}

		public struct MovementGridInfo
		{
			public Dictionary<SSObject, Vector2Int> positions;

			public int sizeX;
			public int sizeZ;
		}

		/// <summary>
		/// Returns normalized grid positions (0,0; 0,1; 0,2; 1,0; 1,1; etc.) for any number of specified gameObjects.
		/// </summary>
		public static MovementGridInfo GetGridPositions( List<SSObject> objects )
		{
			int count = objects.Count;
			int sideLen = Mathf.CeilToInt( Mathf.Sqrt( count ) );

			Dictionary<SSObject, Vector2Int> ret = new Dictionary<SSObject, Vector2Int>();

			int i = 0;
			int x = 0, z = 0;
			for( x = 0; x < sideLen; x++ )
			{
				for( z = 0; z < sideLen; z++ )
				{
					// If we calculated every object, return (since the sideLen ^ 2 can be bigger than the number of objects).
					if( i >= count )
					{
						return new MovementGridInfo() { positions = ret, sizeX = x + 1, sizeZ = z + 1 };
					}

					// Add the new object to the grid (position needs to be flipped for some reason, if it's not, the positions don't match the dimentions).
					ret.Add( objects[i], new Vector2Int( z, x ) );

					i++;
				}
			}

			return new MovementGridInfo() { positions = ret, sizeX = x, sizeZ = z };
		}
	}
}