using KFF;
using SS.DataStructures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS
{
	public class ResourceDeposit : MonoBehaviour, IDefinableBy<ResourceDepositDefinition>
	{
		string id;
		string resourceId; // the id of the resource extracted by mining this deposit.

		private int amt; // the amt still left.
		private int amtMax; // the max amt.

		bool isTypeExtracted; // if true, the resource doesn't take time to mine.

		Transform graphicsTransform;
		
		public void AssignDefinition( ResourceDepositDefinition def )
		{
			this.id = def.id;
			this.resourceId = def.resourceId;
			this.isTypeExtracted = def.isExtracted;
			this.graphicsTransform.localScale = def.scale;
		}
		
		public void SerializeData( KFFSerializer serializer )
		{
			serializer.WriteInt( "", "Amount", this.amt );
			serializer.WriteInt( "", "MaxAmount", this.amtMax );
		}

		public void DeserializeData( KFFSerializer serializer )
		{
			this.amt = serializer.ReadInt( "Amount" );
			this.amtMax = serializer.ReadInt( "MaxAmount" );
		}
		
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

		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		public static GameObject Create( ResourceDepositDefinition def, Vector3 pos, Quaternion rot )
		{
			GameObject container = new GameObject( "Resource Deposit (\"" + def.id + "\")" );

			GameObject gfx = new GameObject( "graphics" );
			gfx.transform.SetParent( container.transform );

			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;

			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			meshRenderer.material = ResourceDepositUtils.CreateMaterial( def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f );

			NavMeshObstacle obstacle = container.AddComponent<NavMeshObstacle>();
			obstacle.size = Vector3.one * 0.3f;
			obstacle.carving = true;

			container.transform.SetPositionAndRotation( pos, rot );

			ResourceDeposit resourceDepositComponent = container.AddComponent<ResourceDeposit>();
			resourceDepositComponent.graphicsTransform = gfx.transform;
			resourceDepositComponent.AssignDefinition( def );

			return container;
		}
	}
}