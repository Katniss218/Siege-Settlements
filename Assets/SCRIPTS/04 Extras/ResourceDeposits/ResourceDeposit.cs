using UnityEngine;
using UnityEngine.AI;

namespace SS.Extras
{ // TODO - resource deposits are just extras with custom inventory.
	[RequireComponent(typeof( NavMeshObstacle ) )]
	public class ResourceDeposit : MonoBehaviour
	{
		public const float MINING_SPEED = 2.0f;

		public string id { get; set; }

		/// <summary>
		/// The id of the resource extracted by mining this deposit.
		/// </summary>
		public string resourceId { get; set; }

		/// <summary>
		/// The amount of resource in the deposit.
		/// </summary>
		public int amount { get; set; } // the amt still left.

		/// <summary>
		/// The capacity of the deposit.
		/// </summary>
		public int amountMax { get; set; } // the max amt.
		
		/// <summary>
		/// If true, the resource can be mined instantly.
		/// </summary>
		public bool isTypeExtracted { get; set; }

		public AudioClip pickupSound { get; set; }
		public AudioClip dropoffSound { get; set; }
		

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
	}
}