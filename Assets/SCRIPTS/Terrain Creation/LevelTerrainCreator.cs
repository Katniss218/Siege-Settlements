using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 
namespace SS.TerrainCreation
{
	public static class LevelTerrainCreator
	{

		// The terrain consists of multiple terrain meshes, each with a single texture applied to it.
		// Each texture is 4096x4096, and each mesh is 64x64 units.

		private static int segmentResolution = 200;

		/// How high the map is.
		public static float terrainHeight = 6f;
		/// How big the entire map is (all segments combined).
		public static float terrainSize = 64;

		/// How many meshes per egde? (square it to get actual mesh count)
		private static int segments = 1;

		/// <summary>
		/// How big each segment is.
		/// </summary>
		private static float segmentSize { get { return terrainSize / segments; } }

		private static float stepSize { get { return segmentSize / segmentResolution; } }

		public static Mesh CreateMeshSegment( Texture2D heightMap )
		{
			// heightmap is this specific segment's heightmap.

			Vector3[] verts = new Vector3[segmentResolution* segmentResolution];
			Vector2[] uvs = new Vector2[segmentResolution * segmentResolution];
			int[] triangles = new int[(segmentResolution-1) * (segmentResolution - 1) * 6];

			int vertIndex = 0;
			int triangleIndex = 0;

			System.Action<int,int,int> AddTriangle = ( int a, int b, int c ) =>
			{
				triangles[triangleIndex++] = a;
				triangles[triangleIndex++] = b;
				triangles[triangleIndex++] = c;
			};

			for( int x = 0; x < segmentResolution; x++ )
			{
				for( int z = 0; z < segmentResolution; z++ )
				{
					uvs[vertIndex] = new Vector2( (float)x / (float)segmentResolution, (float)z / (float)segmentResolution );
					verts[vertIndex] = new Vector3( x * stepSize, heightMap.GetPixelBilinear( uvs[vertIndex].x, uvs[vertIndex].y ).grayscale * terrainHeight, z * stepSize );
					
					if( x < segmentResolution - 1 && z < segmentResolution - 1 )
					{
						AddTriangle( vertIndex, vertIndex + segmentResolution + 1, vertIndex + segmentResolution );
						AddTriangle( vertIndex + segmentResolution + 1, vertIndex, vertIndex + 1 );
					}


					vertIndex++;
				}
			}

			Mesh mesh = new Mesh();
			mesh.vertices = verts;
			mesh.uv = uvs;
			mesh.triangles = triangles;
			mesh.RecalculateNormals();

			return mesh;
		}
	}
}