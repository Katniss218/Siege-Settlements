using SS.Objects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS
{
	public abstract partial class TAIGoal
	{
		public class MoveTo : TAIGoal
		{
			/// <summary>
			/// The position to move to.
			/// </summary>
			public Vector3 destination { get; private set; }


			private NavMeshAgent navMeshAgent;

			void Start()
			{
				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
				if( this.navMeshAgent == null )
				{
					throw new System.Exception( "Can't add MoveTo TAI goal to: " + this.gameObject.name );
				}

				this.navMeshAgent.SetDestination( this.destination );
			}

			private void Update()
			{
				if( navMeshAgent.desiredVelocity.magnitude < 0.01f )
				{
					Object.Destroy( this );
				}
			}

			public override TAIGoalData GetData()
			{
				MoveToData data = new MoveToData();
				data.destination = this.destination;
				return data;
			}

			/// <summary>
			/// Assigns a new MoveTo TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, Vector3 destination )
			{
				TAIGoal.ClearGoal( gameObject );

				MoveTo moveTo = gameObject.AddComponent<TAIGoal.MoveTo>();

				moveTo.destination = destination;
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
}