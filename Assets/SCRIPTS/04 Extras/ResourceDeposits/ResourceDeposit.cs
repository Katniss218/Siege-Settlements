using SS.Inventories;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Extras
{
	[RequireComponent(typeof( NavMeshObstacle ) )]
	public class ResourceDeposit : MonoBehaviour
	{
		public const float MINING_SPEED = 2.0f;

		public Guid guid { get; set; }

		public string id { get; set; }
		
		/// <summary>
		/// If true, the resource can be mined instantly.
		/// </summary>
		public bool isTypeExtracted { get; set; }

		public AudioClip miningSound { get; set; }
		
		public IInventory inventory { get; private set; }
		
		
		void Awake()
		{
			this.inventory = GetComponent<IInventory>();
		}

		void Start()
		{
			
		}
	}
}