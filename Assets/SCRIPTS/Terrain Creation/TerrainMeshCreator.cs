using UnityEngine;

namespace SS.TerrainCreation
{
	public class TerrainMeshCreator
	{
		private byte resolution; // the size of the texture.

		/// How high the map is.
		private float heightScale;
		/// <summary>
		/// How big each segment is.
		/// </summary>
		public const int SEGMENT_SIZE = 16;

		/// How many meshes per egde? (square it to get actual mesh count)
		private int segments = 1;

		public float stepSize { get { return (float)SEGMENT_SIZE / (float)(resolution - 1); } }

		private Texture2D[,] heightMaps;
		
		
		public TerrainMeshCreator( byte resolution, int segments, Texture2D[,] heightMaps, float heightScale )
		{

			if( heightMaps.GetLength( 0 ) != segments || heightMaps.GetLength( 1 ) != segments )
			{
				throw new System.Exception( "The heightMaps array was of invalid dimensions. Expected size: [segments,segments]." );
			}
			this.resolution = resolution;
			this.segments = segments;
			this.heightMaps = heightMaps;
			this.heightScale = heightScale;
		}

#warning Rewrite this to handle a continuous texture.
		// single heightmap
		// single colormap

		/// <summary>
		/// Creates all meshes associated with the given information.
		/// </summary>
		public Mesh[,] CreateMeshes()
		{
			Mesh[,] meshes = new Mesh[segments, segments];
			for( int i = 0; i < segments; i++ )
			{
				for( int j = 0; j < segments; j++ )
				{
					meshes[i, j] = CreateMeshSegment( i, j );
				}
			}
			return meshes;
		}

		private Mesh CreateMeshSegment( int i, int j )
		{
			// heightmap is this specific segment's heightmap.

			Vector3[] verts = new Vector3[resolution * resolution];
			Vector2[] uvs = new Vector2[resolution * resolution];
			int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];

			int vertIndex = 0;
			int triangleIndex = 0;

			Texture2D heightMap = this.heightMaps[i, j];

			System.Action<int, int, int> AddTriangle = ( int a, int b, int c ) =>
			{
				triangles[triangleIndex++] = a;
				triangles[triangleIndex++] = b;
				triangles[triangleIndex++] = c;
			};

			for( int x = 0; x < resolution; x++ )
			{
				for( int z = 0; z < resolution; z++ )
				{
					float uvX = (float)x / (float)resolution;
					float uvY = (float)z / (float)resolution;

					// getpixelbilinear
					float heightPerc = heightMap.GetPixel( x, z, 0 ).r;

					verts[vertIndex] = new Vector3( x * stepSize, heightPerc * heightScale, z * stepSize );
					uvs[vertIndex] = new Vector2( uvX, uvY );

					if( x < resolution - 1 && z < resolution - 1 )
					{
						if( Random.Range( 0, 2 ) == 0 )
						{
							AddTriangle( vertIndex, vertIndex + resolution + 1, vertIndex + resolution );
							AddTriangle( vertIndex + resolution + 1, vertIndex, vertIndex + 1 );
						}
						else
						{
							AddTriangle( vertIndex, vertIndex + 1, vertIndex + resolution );
							AddTriangle( vertIndex + 1, vertIndex + resolution + 1, vertIndex + resolution );
						}
					}


					vertIndex++;
				}
			}

			Mesh mesh = new Mesh();
			mesh.vertices = verts;
			mesh.uv = uvs;
			mesh.SetTriangles( triangles, 0 );
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();

			return mesh;
		}
	}
}