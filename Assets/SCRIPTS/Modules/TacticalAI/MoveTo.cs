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

			/// <summary>
			/// Assigns a new MoveTo TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, Vector3 destination )
			{
				TAIGoal.ClearGoal( gameObject );

				MoveTo moveTo = gameObject.AddComponent<TAIGoal.MoveTo>();

				moveTo.destination = destination;
			}





			public static Vector3 GridToWorld( Vector2Int grid, Vector3 gridCenter, float gridSpacing )
			{
				float camRotY = Main.cameraPivot.rotation.eulerAngles.y;

				Vector3 gridRelativeToCenterLocal = new Vector3( grid.x, 0, grid.y ) - new Vector3( gridSpacing / 2f, 0, gridSpacing / 2f );

				Vector3 gridRelativeToCenterLocalRotated = Quaternion.Euler( 0, camRotY, 0 ) * (gridRelativeToCenterLocal);

				Vector3 global = gridRelativeToCenterLocalRotated * gridSpacing + gridCenter;
				
				return global;
			}

			public struct MovementGridInfo
			{
				public Dictionary<GameObject, Vector2Int> positions;

				public int sizeX;
				public int sizeZ;
			}
			
			/// <summary>
			/// Returns normalized grid positions (0,0; 0,1; 0,2; 1,0; 1,1; etc.) for any number of specified gameObjects.
			/// </summary>
			public static MovementGridInfo GetGridPositions( List<GameObject> objects )
			{
				int count = objects.Count;
				int sideLen = Mathf.CeilToInt( Mathf.Sqrt( count ) );

				Dictionary<GameObject, Vector2Int> ret = new Dictionary<GameObject, Vector2Int>();

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

						// Add the new object to the grid.
						ret.Add( objects[i], new Vector2Int( x, z ) );

						i++;
					}
				}

				return new MovementGridInfo() { positions = ret, sizeX = x, sizeZ = z };
			}
		}
	}
}