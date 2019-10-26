using System.Runtime.InteropServices;
using UnityEngine;

namespace SS
{
	public class KSMImporter
	{
		private static int pos;

		public static Mesh Import( string path )
		{
			byte[] bytes = System.IO.File.ReadAllBytes( path );

			if( bytes[0] != 0x5F && bytes[1] != 0x4B && bytes[2] != 0x53 && bytes[3] != 0x4D )
			{
				throw new System.Exception( "Invalid KSM header (Block: 0-3) - Expected value of '0x5F4B534D', big endian." );
			}

			if( bytes[4] != 0x76 && bytes[5] != 0x65 && bytes[6] != 0x72 && bytes[7] != 0x74 )
			{
				throw new System.Exception( "Invalid 'vert' header (Block: 4-7) - Expected value of '0x76657274', big endian." );
			}

			// vertex count - Block: 8-11
			int vertexCount = BytesToInt32BE( bytes, 8 );

			Vector3[] vertices = new Vector3[vertexCount];
			Vector3[] normals = new Vector3[vertexCount];
			Vector2[] uvs = new Vector2[vertexCount];

			pos = 12;

			for( int i = 0; i < vertexCount; i++ )
			{
				float vx = BytesToFloat32( bytes, pos );
				pos += 4;
				float vy = BytesToFloat32( bytes, pos );
				pos += 4;
				float vz = BytesToFloat32( bytes, pos );
				pos += 4;

				float nx = BytesToFloat32( bytes, pos );
				pos += 4;
				float ny = BytesToFloat32( bytes, pos );
				pos += 4;
				float nz = BytesToFloat32( bytes, pos );
				pos += 4;

				float u = BytesToFloat32( bytes, pos );
				pos += 4;
				float v = BytesToFloat32( bytes, pos );
				pos += 4;

				vertices[i] = new Vector3( vx, vy, vz );
				normals[i] = new Vector3( nx, ny, nz );
				uvs[i] = new Vector2( u, v );
			}


			if( bytes[pos + 0] != 0x66 && bytes[pos + 1] != 0x61 && bytes[pos + 2] != 0x63 && bytes[pos + 3] != 0x65 )
			{
				throw new System.Exception( "Invalid 'face' header (Block: " + (pos + 0) + "-" + (pos + 3) + ") - Expected value of '0x66616365', big endian." );
			}
			pos += 4;

			// face count
			int faceCount = BytesToInt32BE( bytes, pos );
			pos += 4;

			int[] indices = new int[faceCount * 3];

			for( int i = 0; i < faceCount; i++ )
			{
				indices[i * 3] = BytesToInt32( bytes, pos ) - 1;
				pos += 4;
				indices[(i * 3) + 1] = BytesToInt32( bytes, pos ) - 1;
				pos += 4;
				indices[(i * 3) + 2] = BytesToInt32( bytes, pos ) - 1;
				pos += 4;
			}
			Mesh mesh = new Mesh();
			mesh.name = "a";

			Debug.Log( vertexCount );
			Debug.Log( faceCount );
			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.uv = uvs;
			for( int i = 0; i < 20; i++ ) {
				Debug.Log( vertices[i] );
					
			}
			mesh.SetTriangles( indices, 0 );
			mesh.RecalculateBounds();
			mesh.RecalculateTangents();

			return mesh;
		}

		// Convert 4 bytes to an Int32.
		private static int BytesToInt32( byte[] bytes, int offset = 0 )
		{
			int value = 0;
			for( int i = 0; i < 4; i++ )
			{
				value |= (bytes[offset + i]) << (i * 8); // little endian
			}
			return value;
		}

		// Convert 4 bytes to an Int32.
		private static int BytesToInt32BE( byte[] bytes, int offset = 0 )
		{
			int value = (bytes[offset] << 24) | (bytes[offset+1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
			return value;
		}

		[StructLayout( LayoutKind.Explicit )]
		struct IntFloat
		{
			[FieldOffset( 0 )]
			public float FloatValue;

			[FieldOffset( 0 )]
			public int IntValue;
		}
		
		private static float BytesToFloat32( byte[] bytes, int offset = 0 )
		{
			IntFloat fl = new IntFloat();
			fl.IntValue = BytesToInt32( bytes, offset );
			return fl.FloatValue;
		}
	}
}