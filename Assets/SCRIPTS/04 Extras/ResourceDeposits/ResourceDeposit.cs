using SS.Data;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Extras
{
	[RequireComponent(typeof( NavMeshObstacle ) )]
	public class ResourceDeposit : MonoBehaviour, IDefinableBy<ResourceDepositDefinition>
	{
		public string id { get; private set; }

		/// <summary>
		/// The id of the resource extracted by mining this deposit.
		/// </summary>
		public string resourceId { get; private set; }

		public int amount { get; private set; } // the amt still left.
		public int amountMax { get; private set; } // the max amt.

#warning TODO! - Not-extracted deposits should take time to collect.
		/// <summary>
		/// If true, the resource can be mined instantly.
		/// </summary>
		public bool isTypeExtracted { get; private set; } 

		private Transform graphicsTransform;
		private MeshFilter meshFilter;
		private MeshRenderer meshRenderer;
		private NavMeshObstacle obstacle;
		new private BoxCollider collider;

		
		void Awake()
		{
			this.graphicsTransform = this.transform.GetChild( 0 );
			this.meshFilter = this.graphicsTransform.GetComponent<MeshFilter>();
			this.meshRenderer = this.graphicsTransform.GetComponent<MeshRenderer>();
			this.obstacle = this.GetComponent<NavMeshObstacle>();
			this.collider = this.GetComponent<BoxCollider>();
		}

		void Start()
		{

		}

		void Update()
		{

		}

		public void PickUp( int amt )
		{
			this.amount -= amt;
			if( this.amount <= 0 )
			{
				Destroy( this.gameObject );
			}
		}

		public void AssignDefinition( ResourceDepositDefinition def )
		{
			this.id = def.id;
			this.resourceId = def.resourceId;
			this.isTypeExtracted = def.isExtracted;
			this.obstacle.size = def.size;
			this.obstacle.center = new Vector3( 0f, def.size.y / 2f, 0f );
			this.collider.size = def.size;
			this.collider.center = new Vector3( 0f, def.size.y / 2f, 0f );
			this.meshFilter.mesh = def.mesh.Item2;
			this.meshRenderer.material = def.shaderType == ShaderType.PlantSolid ? Main.materialPlantSolid : Main.materialSolid;
			this.meshRenderer.material.SetTexture( "_Albedo", def.albedo.Item2 );

			this.meshRenderer.material.SetTexture( "_Normal", def.normal.Item2 );
			this.meshRenderer.material.SetTexture( "_Emission", null );
			this.meshRenderer.material.SetFloat( "_Metallic", def.isMetallic ? 1.0f : 0.0f );
			this.meshRenderer.material.SetFloat( "_Smoothness", def.smoothness );
		}

		public static GameObject Create( ResourceDepositDefinition def, Vector3 pos, Quaternion rot, int amountOfResource )
		{
			if( def == null )
			{
				throw new System.Exception( "Definition can't be null" );
			}
			GameObject container = new GameObject( "Resource Deposit (\"" + def.id + "\")" );
			container.layer = LayerMask.NameToLayer( "Extras" );

			GameObject gfx = new GameObject( "graphics" );
			gfx.transform.SetParent( container.transform );

			container.transform.SetPositionAndRotation( pos, rot );

			gfx.AddComponent<MeshFilter>();
			gfx.AddComponent<MeshRenderer>();

			BoxCollider collider = container.AddComponent<BoxCollider>();

			NavMeshObstacle obstacle = container.AddComponent<NavMeshObstacle>();
			obstacle.carving = true;

			ResourceDeposit resourceDepositComponent = container.AddComponent<ResourceDeposit>();
			resourceDepositComponent.AssignDefinition( def );
			resourceDepositComponent.amount = amountOfResource;
			resourceDepositComponent.amountMax = amountOfResource;

			return container;
		}
	}
}