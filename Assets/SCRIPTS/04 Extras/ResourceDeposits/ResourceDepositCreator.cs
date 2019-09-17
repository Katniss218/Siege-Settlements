using UnityEngine;
using UnityEngine.AI;

namespace SS.Extras
{
	public static class ResourceDepositCreator
	{
		public static GameObject Create( ResourceDepositDefinition def, Vector3 pos, Quaternion rot, int amountOfResource )
		{
			if( def == null )
			{
				throw new System.Exception( "Definition can't be null" );
			}
			GameObject container = new GameObject( "Resource Deposit (\"" + def.id + "\")" );
			container.layer = ObjectLayer.EXTRAS;

			GameObject gfx = new GameObject( "graphics" );
			gfx.transform.SetParent( container.transform );

			container.transform.SetPositionAndRotation( pos, rot );

			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;

			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			meshRenderer.material = def.shaderType == ShaderType.PlantSolid ? Main.materialPlantSolid : Main.materialSolid;
			meshRenderer.material.EnableKeyword( "_NORMALMAP" );
			meshRenderer.material.SetTexture( "_BaseMap", def.albedo.Item2 );
			meshRenderer.material.SetTexture( "_BumpMap", def.normal.Item2 );
			meshRenderer.material.SetFloat( "_BumpScale", 1.0f );
			meshRenderer.material.SetTexture( "_EmissionMap", null );
			meshRenderer.material.SetFloat( "_Metallic", def.isMetallic ? 1.0f : 0.0f );
			meshRenderer.material.SetFloat( "_Smoothness", def.smoothness );

			BoxCollider collider = container.AddComponent<BoxCollider>();
			collider.size = def.size;
			collider.center = new Vector3( 0f, def.size.y / 2f, 0f );

			NavMeshObstacle obstacle = container.AddComponent<NavMeshObstacle>();
			obstacle.size = def.size;
			obstacle.center = new Vector3( 0f, def.size.y / 2f, 0f );
			obstacle.carving = true;

			ResourceDeposit resourceDepositComponent = container.AddComponent<ResourceDeposit>();
			resourceDepositComponent.id = def.id;
			resourceDepositComponent.resourceId = def.resourceId;
			resourceDepositComponent.isTypeExtracted = def.isExtracted;
			resourceDepositComponent.amount = amountOfResource;
			resourceDepositComponent.amountMax = amountOfResource;

			resourceDepositComponent.pickupSound = def.pickupSoundEffect.Item2;
			resourceDepositComponent.dropoffSound = def.dropoffSoundEffect.Item2;

			return container;
		}
	}
}