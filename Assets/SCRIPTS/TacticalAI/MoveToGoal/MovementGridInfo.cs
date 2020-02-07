using SS.Objects;
using System.Collections.Generic;
using UnityEngine;

namespace SS.AI
{
	/// <summary>
	/// A struct for calculating non-overlapping positions for a number of specified SSObjects.
	/// </summary>
	public struct MovementGridInfo
	{
		/// <summary>
		/// A list of grid positions for the corresponding SSObjects.
		/// </summary>
		public Dictionary<SSObjectDFC, Vector2Int> positions { get; private set; }

		/// <summary>
		/// The number of rows in the grid.
		/// </summary>
		public int sizeX { get; private set; }
		/// <summary>
		/// The number of columns in the grid.
		/// </summary>
		public int sizeZ { get; private set; }

		/// <summary>
		/// Returns normalized grid positions (0,0; 0,1; 0,2; 1,0; 1,1; etc.) for any number of specified gameObjects.
		/// </summary>
		public MovementGridInfo( List<SSObjectDFC> objects )
		{
			int count = objects.Count;
			int sideLen = Mathf.CeilToInt( Mathf.Sqrt( count ) );

			Dictionary<SSObjectDFC, Vector2Int> ret = new Dictionary<SSObjectDFC, Vector2Int>();

			int i = 0;
			int x = 0, z = 0;
			for( x = 0; x < sideLen; x++ )
			{
				for( z = 0; z < sideLen; z++ )
				{
					// If we calculated every object, return (since the sideLen ^ 2 can be bigger than the number of objects).
					if( i >= count )
					{
						this.positions = ret;
						this.sizeX = x + 1;
						this.sizeZ = z + 1;
						return;
					}

					// Add the new object to the grid (position needs to be flipped for some reason, if it's not, the positions don't match the dimentions).
					ret.Add( objects[i], new Vector2Int( z, x ) );

					i++;
				}
			}

			this.positions = ret;
			this.sizeX = x;
			this.sizeZ = z;
		}

		/// <summary>
		/// Converts the grid's normalized position into a world-space position.
		/// </summary>
		/// <param name="grid">The normalized grid coordinate.</param>
		/// <param name="rotationWorld">The world-space rotation of the grid's center.</param>
		/// <param name="gridCenterWorld">The world-space position of the grid's center.</param>
		/// <param name="gridSpacingWorld">The spacing between world-space grid positions.</param>
		public Vector3 GridPosToWorld( Vector2Int grid, Quaternion rotationWorld, Vector3 gridCenterWorld, float gridSpacingWorld )
		{
			Vector3 offset = new Vector3( (this.sizeX - 1) / 2.0f, 0, (this.sizeZ - 1) / 2.0f );

			Vector3 gridRelativeToCenterLocal = new Vector3( grid.x, 0, grid.y ) - offset;
			Vector3 gridRelativeToCenterLocalRotated = rotationWorld * (gridRelativeToCenterLocal);

			Vector3 global = gridRelativeToCenterLocalRotated * gridSpacingWorld + gridCenterWorld;

			return global;
		}
	}
}