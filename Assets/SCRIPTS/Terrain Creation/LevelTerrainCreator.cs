using SS.Content;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 
namespace SS.TerrainCreation
{
	public static class LevelTerrainCreator
	{

		// The terrain consists of multiple terrain meshes, each with a single texture applied to it.
		// Each texture is 4096x4096, and each mesh is 64x64 units.

		static TerrainMeshCreator meshCreator;
		public static Transform terrainParent;

		private const int RESOLUTION = 241;

		public static void SpawnMap( Texture2D[,] heightMaps, Texture2D[,] albedoMaps, float height )
		{
			int segments = heightMaps.GetLength( 0 );
			if( heightMaps.GetLength( 0 ) != segments || heightMaps.GetLength( 1 ) != segments )
			{
				throw new System.Exception( "The heightMaps array was of invalid dimensions. Expected size: [segments,segments]." );
			}
			if( heightMaps.GetLength( 0 ) != segments || heightMaps.GetLength( 1 ) != segments )
			{
				throw new System.Exception( "The albedoMaps array was of invalid dimensions. Expected size: [segments,segments]." );
			}

			meshCreator = new TerrainMeshCreator( RESOLUTION, segments, heightMaps, height );
			Mesh[,] meshes = meshCreator.CreateMeshes();

			for( int i = 0; i < segments; i++ )
			{
				for( int j = 0; j < segments; j++ )
				{
					GameObject terrainSegment = new GameObject( "Mesh" );
					terrainSegment.layer = ObjectLayer.TERRAIN;
					terrainSegment.transform.SetParent( terrainParent );
					terrainSegment.isStatic = true;

					MeshFilter meshFilter = terrainSegment.AddComponent<MeshFilter>();
					meshFilter.mesh = meshes[i, j];

					MeshRenderer meshRenderer = terrainSegment.AddComponent<MeshRenderer>();
					meshRenderer.material = new Material( AssetManager.GetMaterialPrototype( "builtin:Materials/__Terrain" ) );
					meshRenderer.material.SetTexture( "_BaseMap", albedoMaps[i, j] );

					terrainSegment.transform.position = new Vector3( i * TerrainMeshCreator.SEGMENT_SIZE, 0, j * TerrainMeshCreator.SEGMENT_SIZE );

					terrainSegment.AddComponent<MeshCollider>().sharedMesh = meshFilter.mesh;
				}
			}
		}

		static NavMeshDataInstance navMeshDataInstance;

		public static void UpdateNavMesh()
		{
			List<NavMeshBuildSource> buildSources = new List<NavMeshBuildSource>();

			NavMeshBuilder.CollectSources( terrainParent.transform, ObjectLayer.TERRAIN_MASK, NavMeshCollectGeometry.RenderMeshes, 0, new List<NavMeshBuildMarkup>(), buildSources );

			NavMeshData navData = NavMeshBuilder.BuildNavMeshData(
				NavMesh.GetSettingsByID( 0 ),
				buildSources,
				new Bounds( Vector3.zero, new Vector3( 10000, 10000, 10000 ) ),
				Vector3.down,
				Quaternion.Euler( Vector3.up )
			);
			navMeshDataInstance = NavMesh.AddNavMeshData( navData );
		}
	}
}