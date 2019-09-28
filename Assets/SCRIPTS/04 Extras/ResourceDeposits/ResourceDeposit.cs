using SS.Modules.Inventories;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Extras
{
	[RequireComponent(typeof( NavMeshObstacle ) )]
	public class ResourceDeposit : MonoBehaviour
	{
		public static bool IsValid( GameObject gameObject )
		{
			if( gameObject.layer != ObjectLayer.EXTRAS )
			{
				return false;
			}
			if( gameObject.GetComponent<ResourceDeposit>() == null )
			{
				return false;
			}
			return true;
		}

		private static List<ResourceDeposit> _allResourceDeposits = new List<ResourceDeposit>();

		public static ResourceDeposit[] GetAllResourceDeposits()
		{
			return _allResourceDeposits.ToArray();
		}


		public const float MINING_SPEED = 2.0f;

		public Guid guid { get; set; }

		public string defId { get; set; }
		
		public string displayName { get; set; }

		/// <summary>
		/// If true, the resource can be mined instantly.
		/// </summary>
		public bool isTypeExtracted { get; set; }

		public AudioClip miningSound { get; set; }
		
		public IInventory inventory { get; set; }
		
		
		void Start()
		{

		}

		void OnEnable()
		{
			_allResourceDeposits.Add( this );
		}

		void OnDisable()
		{
			_allResourceDeposits.Remove( this );
		}
	}
}