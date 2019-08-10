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
			
			int numMeshes = serializer.ScopeChildCount();
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

					serializer.MoveScope( "Vertices", true );
					Vector3[] verts = new Vector3[serializer.ScopeChildCount()];
					for( int j = 0; j < verts.Length; j++ )
					{
						verts[j] = serializer.ReadVector3( j.ToString() );
					}
					serializer.MoveScope( "<", true );

					serializer.MoveScope( "Normals", true );
					Vector3[] normals = new Vector3[serializer.ScopeChildCount()];
					for( int j = 0; j < normals.Length; j++ )
					{
						normals[j] = serializer.ReadVector3( j.ToString() );
					}
					serializer.MoveScope( "<", true );

					serializer.MoveScope( "UVs", true );
					Vector2[] uvs = new Vector2[serializer.ScopeChildCount()];
					for( int j = 0; j < uvs.Length; j++ )
					{
						uvs[j] = serializer.ReadVector2( j.ToString() );
					}
					serializer.MoveScope( "<", true );

					serializer.MoveScope( "Faces", true );
					int numFaces = serializer.ScopeChildCount();
					int[] triangles = new int[numFaces * 3];
					for( int j = 0; j < numFaces; j++ )
					{
						triangles[(j * 3)] = serializer.ReadInt( j.ToString() + ".0" ) - 1;
						triangles[(j * 3) +1] = serializer.ReadInt( j.ToString() + ".1" ) - 1;
						triangles[(j * 3) +2] = serializer.ReadInt( j.ToString() + ".2" ) - 1;
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