using UnityEngine;
using UnityEngine.AI;

namespace SS.Extras
{
	[RequireComponent(typeof( NavMeshObstacle ) )]
	public class ResourceDeposit : MonoBehaviour
	{
		public string id { get; private set; }

		/// <summary>
		/// The id of the resource extracted by mining this deposit.
		/// </summary>
		public string resourceId { get; private set; }

		/// <summary>
		/// The amount of resource in the deposit.
		/// </summary>
		public int amount { get; private set; } // the amt still left.

		/// <summary>
		/// The capacity of the deposit.
		/// </summary>
		public int amountMax { get; private set; } // the max amt.
		
		/// <summary>
		/// If true, the resource can be mined instantly.
		/// </summary>
		public bool isTypeExtracted { get; private set; }

		public AudioClip pickupSound { get; private set; }
		public AudioClip dropoffSound { get; private set; }

		public const float MINING_SPEED = 2.0f;


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

		public bool PickUp( int amt )
		{
			this.amount -= amt;
			if( this.amount <= 0 )
			{
				Destroy( this.gameObject );
				return true;
			}
			return false;
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

			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;

			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			meshRenderer.material = def.shaderType == ShaderType.PlantSolid ? Main.materialPlantSolid : Main.materialSolid;
			meshRenderer.material.SetTexture( "_Albedo", def.albedo.Item2 );

			meshRenderer.material.SetTexture( "_Normal", def.normal.Item2 );
			meshRenderer.material.SetTexture( "_Emission", null );
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