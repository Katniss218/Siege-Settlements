using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS
{
	public abstract partial class TAIGoal
	{
		[RequireComponent( typeof( NavMeshAgent ) )]
		public class MoveTo : TAIGoal
		{
			public Vector3 destination { get; private set; }

			void Start()
			{
				this.GetComponent<NavMeshAgent>().SetDestination( this.destination );
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

			public struct GridPositionInfo
			{
				public Dictionary<GameObject, Vector2Int> positions;

				public int sizeX;
				public int sizeZ;
			}

			/// <summary>
			/// Returns normalized grid positions (0,0; 0,1; 0,2; 1,0; 1,1; etc.) for any number of specified gameObjects.
			/// </summary>
			public static GridPositionInfo GetGridPositions( GameObject[] objects )
			{
				int sideLen = Mathf.CeilToInt( Mathf.Sqrt( objects.Length ) );

				Dictionary<GameObject, Vector2Int> ret = new Dictionary<GameObject, Vector2Int>();

				int i = 0;
				int x = 0, z = 0;
				for( x = 0; x < sideLen; x++ )
				{
					for( z = 0; z < sideLen; z++ )
					{
						// If we calculated every object, return (since the sideLen ^ 2 can be bigger than the number of objects).
						if( i >= objects.Length )
						{
							return new GridPositionInfo() { positions = ret, sizeX = x + 1, sizeZ = z + 1 };
						}

						// Add the new object to the grid.
						ret.Add( objects[i], new Vector2Int( x, z ) );

						i++;
					}
				}

				return new GridPositionInfo() { positions = ret, sizeX = x, sizeZ = z };
			}
		}
	}
}