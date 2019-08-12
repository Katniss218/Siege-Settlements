﻿using KFF;
using SS.DataStructures;
using UnityEngine;
using UnityEngine.AI;

namespace SS
{
	public class ResourceDeposit : MonoBehaviour, IDefinableBy<ResourceDepositDefinition>
	{
		public string id { get; private set; }

		public string resourceId { get; private set; } // the id of the resource extracted by mining this deposit.

		public int amt { get; private set; } // the amt still left.
		public int amtMax { get; private set; } // the max amt.

		public bool isTypeExtracted { get; private set; } // if true, the resource doesn't take time to mine.

		private Transform graphicsTransform;
		private MeshFilter meshFilter;
		private MeshRenderer meshRenderer;
		private NavMeshObstacle obstacle;


		
		/*
		// amount that the miner wants to extract, the inventory of the miner.
		public void ExtractResource( int desiredAmt, IInventory inventory )
		{
			int amtMined = desiredAmt;
			if( amtMined < this.amt )
			{
				amtMined = this.amt;
			}


			int spaceLeft = inventory.canHold( this.resourceId, (ushort)amtMined );


			if( spaceLeft > 0 )
			{
				if( spaceLeft < amtMined )
				{
					amtMined = spaceLeft;
				}

				inventory.Add( this.resourceId, (ushort)amtMined );
				this.amt -= amtMined;

				if( this.amt <= 0 )
				{
					Destroy( this.gameObject );
				}
			}
			Debug.LogWarning( "Tried to extract resource but can't hold any more of it." );
		}
		*/

		void Awake()
		{
			this.graphicsTransform = this.transform.GetChild( 0 );
			this.meshFilter = this.graphicsTransform.GetComponent<MeshFilter>();
			this.meshRenderer = this.graphicsTransform.GetComponent<MeshRenderer>();
			this.obstacle = this.GetComponent<NavMeshObstacle>();
		}

		void Start()
		{

		}
		
		void Update()
		{

		}

		public void AssignDefinition( ResourceDepositDefinition def )
		{
			this.id = def.id;
			this.resourceId = def.resourceId;
			this.isTypeExtracted = def.isExtracted;
			this.obstacle.size = def.size;
			this.obstacle.center = new Vector3( 0f, def.size.y / 2f, 0f );
			this.meshFilter.mesh = def.mesh.Item2;
			this.meshRenderer.material = ResourceDepositUtils.CreateMaterial( def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.5f );
		}

		public static GameObject Create( ResourceDepositDefinition def, Vector3 pos, Quaternion rot )
		{
			if( def == null )
			{
				throw new System.Exception( "Definition can't be null" );
			}
			GameObject container = new GameObject( "Resource Deposit (\"" + def.id + "\")" );

			GameObject gfx = new GameObject( "graphics" );
			gfx.transform.SetParent( container.transform );

			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();

			NavMeshObstacle obstacle = container.AddComponent<NavMeshObstacle>();
			obstacle.carving = true;

			container.transform.SetPositionAndRotation( pos, rot );

			ResourceDeposit resourceDepositComponent = container.AddComponent<ResourceDeposit>();
			resourceDepositComponent.AssignDefinition( def );

			return container;
		}
	}
}