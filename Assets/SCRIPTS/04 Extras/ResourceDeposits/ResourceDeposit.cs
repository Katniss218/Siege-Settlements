using SS.Inventories;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Extras
{
	[RequireComponent(typeof( NavMeshObstacle ) )]
	public class ResourceDeposit : MonoBehaviour
	{
		public const float MINING_SPEED = 2.0f;

		public string id { get; set; }
		
		/// <summary>
		/// If true, the resource can be mined instantly.
		/// </summary>
		public bool isTypeExtracted { get; set; }

		public AudioClip pickupSound { get; set; }
		public AudioClip dropoffSound { get; set; }
		
		public IInventory inventory { get; private set; }
		
		
		void Awake()
		{
			inventory = GetComponent<IInventory>();
		}

		void Start()
		{
			
		}
	}
}