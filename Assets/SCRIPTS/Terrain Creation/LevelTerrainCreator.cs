using SS.Content;
using SS.Levels;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS.TerrainCreation
{
	public static class LevelTerrainCreator
	{
		public const int RESOLUTION = 241;

		public static Transform terrainParent;

		private static TerrainMeshCreator meshCreator;
		private static NavMeshDataInstance navMeshDataInstance;


		public static void SpawnMap( Texture2D heightMaps, Texture2D albedoMaps, float height )
		{
			meshCreator = new TerrainMeshCreator( height, LevelDataManager.mapSegments, heightMaps );
			Mesh[,] meshes = meshCreator.CreateMeshes();
			
			for( int i = 0; i < LevelDataManager.mapSegments; i++ )
			{
				for( int j = 0; j < LevelDataManager.mapSegments; j++ )
				{
					GameObject terrainSegment = new GameObject( "Mesh" );
					terrainSegment.layer = ObjectLayer.TERRAIN;
					terrainSegment.transform.SetParent( terrainParent );
					terrainSegment.isStatic = true;

					MeshFilter meshFilter = terrainSegment.AddComponent<MeshFilter>();
					meshFilter.mesh = meshes[i, j];

					MeshRenderer meshRenderer = terrainSegment.AddComponent<MeshRenderer>();
					meshRenderer.material = new Material( AssetManager.GetMaterialPrototype( "builtin:Materials/__Terrain" ) );
					meshRenderer.material.SetTexture( "_BaseMap", albedoMaps );

					terrainSegment.transform.position = new Vector3( i * TerrainMeshCreator.SEGMENT_SIZE, 0, j * TerrainMeshCreator.SEGMENT_SIZE );

					// this takes up to 90% of the time to assign. (5k ms for 4x4)
					MeshCollider meshCollider = terrainSegment.AddComponent<MeshCollider>();
					meshCollider.sharedMesh = meshes[i, j];
				}
			}
		}



		public static void UpdateNavMesh()
		{
			List<NavMeshBuildSource> buildSources = new List<NavMeshBuildSource>();

			NavMeshBuilder.CollectSources( terrainParent.transform, ObjectLayer.TERRAIN_MASK, NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), buildSources );

			NavMeshBuildSettings meshBuildSettings = NavMesh.GetSettingsByIndex( 0 );
			meshBuildSettings.overrideVoxelSize = true;
			meshBuildSettings.voxelSize = 0.125f;
			meshBuildSettings.overrideTileSize = true;
			meshBuildSettings.tileSize = 32;

			Vector3 mapCenter = new Vector3( LevelDataManager.mapSize / 2.0f, LevelDataManager.mapHeight / 2.0f, LevelDataManager.mapSize / 2.0f );
			Vector3 mapExtents = new Vector3( LevelDataManager.mapSize, LevelDataManager.mapHeight * 2, LevelDataManager.mapSize );

			NavMeshData navData = NavMeshBuilder.BuildNavMeshData(
				meshBuildSettings,
				buildSources,
				new Bounds( mapCenter, mapExtents ),
				Vector3.down,
				Quaternion.Euler( Vector3.up )
			);
			navMeshDataInstance = NavMesh.AddNavMeshData( navData );
			
		}
	}
}