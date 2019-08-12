using KFF;
using System.Collections.Generic;
using UnityEngine;

namespace Katniss.Utils
{
	public static class MeshKFFSerializer
	{
		public static Mesh[] DeserializeKFF( KFFSerializer serializer )
		{
			KFF.DataStructures.Object array_root = serializer.MoveScope( "Objects", true );
			serializer.Analyze( "" );

			int numMeshes = serializer.aChildCount;
			List<Mesh> meshes = new List<Mesh>();

			// for each mesh.
			for( int i = 0; i < numMeshes; i++ )
			{
				serializer.MoveScope( i + "", true );

				string type = serializer.ReadString( "Type" );
				if( type.Equals( "MESH" ) )
				{
					Mesh newMesh = new Mesh();
					newMesh.name = serializer.ReadString( "Name" );

					serializer.Analyze( "Vertices" );
					Vector3[] verts = new Vector3[serializer.aChildCount];
					for( int j = 0; j < verts.Length; j++ )
					{
						verts[j] = serializer.ReadVector3( "Vertices." + j.ToString() );
					}

					serializer.Analyze( "Normals" );
					Vector3[] normals = new Vector3[serializer.aChildCount];
					for( int j = 0; j < normals.Length; j++ )
					{
						normals[j] = serializer.ReadVector3( "Normals." + j.ToString() );
					}

					serializer.Analyze( "UVs" );
					Vector2[] uvs = new Vector2[serializer.aChildCount];
					for( int j = 0; j < uvs.Length; j++ )
					{
						uvs[j] = serializer.ReadVector2( "UVs." + j.ToString() );
					}

					serializer.Analyze( "Faces" );
					int numFaces = serializer.aChildCount;
					int[] triangles = new int[numFaces * 3];
					for( int j = 0; j < numFaces; j++ )
					{
						triangles[(j * 3)] = serializer.ReadInt( "Faces." + j.ToString() + ".0" ) - 1;
						triangles[(j * 3) +1] = serializer.ReadInt( "Faces."+j.ToString() + ".1" ) - 1;
						triangles[(j * 3) +2] = serializer.ReadInt( "Faces."+j.ToString() + ".2" ) - 1;
					}
					serializer.MoveScope( "<", true );

					//Debug.Log( verts.Length );
					//Debug.Log( normals.Length );
					//Debug.Log( uvs.Length );
					//Debug.Log( triangles.Length );

					newMesh.vertices = verts;
					newMesh.normals = normals;
					newMesh.uv = uvs;
					newMesh.SetTriangles( triangles, 0 );
					newMesh.RecalculateBounds();

					meshes.Add( newMesh );
				}
				serializer.scopeRoot = array_root;
			}

			return meshes.ToArray();
		}
	}
}