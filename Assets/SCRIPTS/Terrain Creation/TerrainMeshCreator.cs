using UnityEngine;

namespace SS.TerrainCreation
{
    public class TerrainMeshCreator
    {
        //private byte resolution; // the size of the texture.

        /// How high the map is.
        private float heightScale;

        /// <summary>
        /// How big each segment is. (real size units)
        /// </summary>
        //public const int SEGMENT_SIZE = 16;

        private const float RESOLUTION = 0.125f; // vertex spacing in meters (world units) (must be a reciprocal of an integer)
        public const int SEGMENT_SIZE = 4; // in meters
        private const int SEGMENT_WIDTH = (int)(SEGMENT_SIZE / RESOLUTION);
        private const int SEGMENT_VERT_COUNT = SEGMENT_WIDTH + 1; // SEGMENT_WIDTH - number of faces in each segment, += 1 vertices (edge) edge ones will overlap

        /// How many meshes per egde? (square it to get actual mesh count)
        private int segments = 1;

        public float vertexStepSize { get { return RESOLUTION; } }



        private Texture2D heightmap;

        // segments always of constant size.
        public TerrainMeshCreator( float heightScale, int noSegments, Texture2D heightmap )
        {
            if( heightmap.width != heightmap.height )
            {
                throw new System.Exception( "non-square heightmap was provided" );
            }

            this.heightScale = heightScale;
            this.segments = noSegments;
            this.heightmap = heightmap;
        }


        // how many vertices per segment?

#warning Rewrite this to handle a continuous texture.
        // single heightmap
        // single colormap
        // uv scaled in accordance with which segment it is.

        /// <summary>
        /// Creates all meshes associated with the given information.
        /// </summary>
        public Mesh[,] CreateMeshes()
        {
            Mesh[,] meshes = new Mesh[segments, segments];

            for( int x = 0; x < segments; x++ )
            {
                for( int z = 0; z < segments; z++ )
                {
                    meshes[x, z] = CreateMeshSegment( x, z );
                }
            }

            return meshes;
        }

        private Mesh CreateMeshSegment( int segX, int segZ ) // 0-based indices, go from 0 to noSegments-1
        {
            Vector3[] verts = new Vector3[SEGMENT_VERT_COUNT * SEGMENT_VERT_COUNT];
            Vector2[] uvs = new Vector2[SEGMENT_VERT_COUNT * SEGMENT_VERT_COUNT];
            int[] triangles = new int[(SEGMENT_VERT_COUNT - 1) * (SEGMENT_VERT_COUNT - 1) * 6];

            int vertIndex = 0;
            int triangleIndex = 0;

            System.Action<int, int, int> AddTriangle = ( int a, int b, int c ) =>
            {
                triangles[triangleIndex++] = a;
                triangles[triangleIndex++] = b;
                triangles[triangleIndex++] = c;
            };

            for( int x = 0; x < SEGMENT_VERT_COUNT; x++ )
            {
                for( int z = 0; z < SEGMENT_VERT_COUNT; z++ )
                {
                    float percInSegmentX = (float)x / (float)SEGMENT_WIDTH;
                    float uvX = (percInSegmentX / (float)this.segments) + ((float)segX / (float)this.segments);

                    float percInSegmentZ = (float)z / (float)SEGMENT_WIDTH;
                    float uvY = (percInSegmentZ / (float)this.segments) + ((float)segZ / (float)this.segments);

                    // getpixelbilinear
                    float heightPerc = heightmap.GetPixelBilinear( uvX, uvY, 0 ).r;

                    verts[vertIndex] = new Vector3( x * vertexStepSize, heightPerc * heightScale, z * vertexStepSize );
                    uvs[vertIndex] = new Vector2( uvX, uvY );

                    if( x < SEGMENT_VERT_COUNT - 1 && z < SEGMENT_VERT_COUNT - 1 )
                    {
                        if( Random.Range( 0, 2 ) == 0 )
                        {
                            AddTriangle( vertIndex, vertIndex + SEGMENT_VERT_COUNT + 1, vertIndex + SEGMENT_VERT_COUNT );
                            AddTriangle( vertIndex + SEGMENT_VERT_COUNT + 1, vertIndex, vertIndex + 1 );
                        }
                        else
                        {
                            AddTriangle( vertIndex, vertIndex + 1, vertIndex + SEGMENT_VERT_COUNT );
                            AddTriangle( vertIndex + 1, vertIndex + SEGMENT_VERT_COUNT + 1, vertIndex + SEGMENT_VERT_COUNT );
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