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
			InteriorModule[] interiors = destination.GetModules<InteriorModule>();
			if( interiors.Length > 0 )
			{
				this.destinationInterior = interiors[0];
			}
			else
			{
				this.destinationInterior = null;
			}
		}


		public override bool IsOnValidObject( SSObject ssObject )
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


#warning MoveTo can be assigned when the unit is inside. It makes the unit exit (if possible) or reset to default (if not possible).

#warning MoveTo can be assigned to enter interiors. It makes the unit go towards the interior object, and when close enough to the entrance, it enters.
			
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

				if( this.destinationInterior != null )
				{
					// If the agent has travelled to the destination - switch back to the default Goal.
					if( this.navMeshAgent.hasPath )
					{
						if( Vector3.Distance( this.navMeshAgent.pathEndPosition, controller.transform.position ) <= Main.DEFAULT_NAVMESH_STOPPING_DIST_CUSTOM )
						{
							this.navMeshAgent.ResetPath();
							
							if( controller.ssObject is Unit )
							{
								Unit unit = (Unit)controller.ssObject;

#warning slot type specified in goal.
								unit.TrySetInside( this.destinationInterior, InteriorModule.SlotType.Generic );
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

		private void UpdateTargeting( TacticalGoalController controller )
		{
			if( this.isHostile )
			{
				SSObjectDFS ssobj = controller.GetComponent<SSObjectDFS>();
				for( int i = 0; i < this.attackModules.Length; i++ )
				{
					if( !Targeter.CanTarget( controller.transform.position, this.attackModules[i].attackRange, this.attackModules[i].target, ssobj ) )
					{
						this.attackModules[i].target = null;
					}
				}

				if( Random.Range( 0, 5 ) == 0 ) // Recalculate target only 20% of the time (not really noticeable, but gives a nice boost to FPS).
				{
					for( int i = 0; i < this.attackModules.Length; i++ )
					{
						if( this.attackModules[i].isReadyToAttack )
						{
							this.attackModules[i].FindTargetClosest();
						}
					}
				}
			}
			else
			{
				for( int i = 0; i < this.attackModules.Length; i++ )
				{
					if( this.attackModules[i].target != null )
					{
						this.attackModules[i].target = null;
					}
				}
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
			this.UpdateTargeting( controller );
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