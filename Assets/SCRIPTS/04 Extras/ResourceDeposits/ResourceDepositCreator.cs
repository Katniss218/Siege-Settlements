using Katniss.Utils;
using SS.Content;
using SS.Inventories;
using SS.Levels.SaveStates;
using SS.ResourceSystem;
using SS.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SS.Extras
{
	public static class ResourceDepositCreator
	{
		private const string GAMEOBJECT_NAME = "Resource Deposit";

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@


		private static void SetResourceDepositDefinition( GameObject gameObject, ResourceDepositDefinition def )
		{

			//
			//    GRAPHICS GAMEOBJECT
			//

			GameObject gfx = gameObject.transform.Find( GameObjectUtils.GRAPHICS_GAMEOBJECT_NAME ).gameObject;
			
			MeshFilter meshFilter = gfx.GetComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;

			MeshRenderer meshRenderer = gfx.GetComponent<MeshRenderer>();
			meshRenderer.material = def.shaderType == MaterialType.PlantOpaque ? MaterialManager.CreatePlantOpaque( def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f, 0.3333f ) : MaterialManager.CreateOpaque( def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f );


			//
			//    CONTAINER GAMEOBJECT
			//

			BoxCollider collider = gameObject.GetComponent<BoxCollider>();
			collider.size = def.size;
			collider.center = new Vector3( 0f, def.size.y / 2.0f, 0f );

			NavMeshObstacle obstacle = gameObject.GetComponent<NavMeshObstacle>();
			obstacle.size = def.size;
			obstacle.center = new Vector3( 0f, def.size.y / 2.0f, 0f );
			

			InventoryConstrained depositInventory = gameObject.GetComponent<InventoryConstrained>();
			InventoryConstrained.SlotInfo[] slotInfos = new InventoryConstrained.SlotInfo[def.resources.Count];
			int i = 0;
			foreach( var kvp in def.resources )
			{
				slotInfos[i] = new InventoryConstrained.SlotInfo( kvp.Key, kvp.Value );
				i++;
			}
			depositInventory.SetSlots( slotInfos );
			
			ResourceDeposit resourceDeposit = gameObject.GetComponent<ResourceDeposit>();
			resourceDeposit.defId = def.id;
			resourceDeposit.isTypeExtracted = def.isExtracted;
			if( !def.isExtracted )
			{
				resourceDeposit.miningSound = def.mineSound.Item2;
			}
			else
			{
				resourceDeposit.miningSound = null;
			}
		}

		private static void SetResourceDepositData( GameObject gameObject, ResourceDepositData data )
		{

			//
			//    CONTAINER GAMEOBJECT
			//

			gameObject.transform.SetPositionAndRotation( data.position, data.rotation );

			IInventory inventory = gameObject.GetComponent<IInventory>();

			foreach( var kvp in data.resources )
			{
				int capacity = inventory.GetMaxCapacity( kvp.Key );
				if( capacity == 0 )
				{
					throw new System.Exception( "This deposit can't hold '" + kvp.Key + "'." );
				}
				else
				{
					if( capacity < kvp.Value )
					{
						Debug.LogWarning( "This deposit can't hold " + kvp.Value + " x '" + kvp.Key + "'. - " + (kvp.Value - capacity) + " x resource has been lost." );
						inventory.Add( kvp.Key, capacity );
					}
					else
					{
						inventory.Add( kvp.Key, kvp.Value );
					}
				}
			}
		}

		private static GameObject CreateResourceDeposit()
		{
			GameObject container = new GameObject( GAMEOBJECT_NAME );
			container.layer = ObjectLayer.EXTRAS;


			//
			//    GRAPHICS GAMEOBJECT
			//

			GameObject gfx = new GameObject( GameObjectUtils.GRAPHICS_GAMEOBJECT_NAME );
			gfx.transform.SetParent( container.transform );
			
			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();

			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			

			//
			//    CONTAINER GAMEOBJECT
			//

			BoxCollider collider = container.AddComponent<BoxCollider>();

			NavMeshObstacle obstacle = container.AddComponent<NavMeshObstacle>();
			obstacle.carving = true;

			ResourceDeposit resourceDeposit = container.AddComponent<ResourceDeposit>();

			UnityAction<GameObject> showTooltip = ( GameObject gameObject ) =>
			{
				if( gameObject == container )
				{
					ResourceDeposit deposit = gameObject.GetComponent<ResourceDeposit>();
					if( deposit == null )
					{
						return;
					}

					Dictionary<string, int> itemsInDeposit = deposit.inventory.GetAll();

					ToolTip.Create( 200.0f, resourceDeposit.displayName );

					foreach( var kvp in itemsInDeposit )
					{
						ResourceDefinition resourceDef = DefinitionManager.GetResource( kvp.Key );
						ToolTip.AddText( resourceDef.icon.Item2, kvp.Value.ToString() + "/" + deposit.inventory.GetMaxCapacity( kvp.Key ) );
					}
					ToolTip.ShowAt( Input.mousePosition );
				}
			};
			UnityAction<GameObject> moveTooltip = ( GameObject gameObject ) =>
			{
				if( gameObject == container )
				{
					ToolTip.MoveTo( Input.mousePosition, true );
				}
			};

			UnityAction<GameObject> hideTooltip = ( GameObject gameObject ) =>
			{
				if( gameObject == container )
				{
					ToolTip.Hide();
				}
			};


			MouseOverHandler.onMouseEnter.AddListener( showTooltip );
			MouseOverHandler.onMouseStay.AddListener( moveTooltip );
			MouseOverHandler.onMouseExit.AddListener( hideTooltip );

			InventoryConstrained depositInventory = container.AddComponent<InventoryConstrained>();
			
			depositInventory.onAdd.AddListener( ( string id, int amount ) =>
			{
				if( MouseOverHandler.currentObjectMouseOver == container )
				{
					showTooltip( container );
				}
			} );

			depositInventory.onRemove.AddListener( ( string id, int amount ) =>
			{
				if( depositInventory.isEmpty )
				{
					if( MouseOverHandler.currentObjectMouseOver == container )
					{
						hideTooltip( container );
					}
					Object.Destroy( container );

					MouseOverHandler.onMouseEnter.RemoveListener( showTooltip );
					MouseOverHandler.onMouseStay.RemoveListener( moveTooltip );
					MouseOverHandler.onMouseExit.RemoveListener( hideTooltip );
				}
				else
				{
					if( MouseOverHandler.currentObjectMouseOver == container )
					{
						showTooltip( container );
					}
				}
			} );
			
			return container;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
		public static string GetDefinitionId( GameObject gameObject )
		{
			if( !ResourceDeposit.IsValid( gameObject ) )
			{
				throw new Exception( "GameObject '" + gameObject.name + "' is not a valid resource deposit." );
			}

			ResourceDeposit resourceDeposit = gameObject.GetComponent<ResourceDeposit>();
			return resourceDeposit.defId;
		}

		/// <summary>
		/// Creates a new ExtraData from a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject to extract the save state from. Must be an extra.</param>
		public static ResourceDepositData GetData( GameObject gameObject )
		{
			if( !ResourceDeposit.IsValid( gameObject ) )
			{
				throw new Exception( "GameObject '" + gameObject.name + "' is not a valid resource deposit." );
			}

			ResourceDepositData data = new ResourceDepositData();

			data.guid = gameObject.GetComponent<ResourceDeposit>().guid;

			data.position = gameObject.transform.position;
			data.rotation = gameObject.transform.rotation;

			return data;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static void SetData( GameObject gameObject, ResourceDepositData data )
		{
			if( !ResourceDeposit.IsValid( gameObject ) )
			{
				throw new Exception( "GameObject '" + gameObject.name + "' is not a valid resource deposit." );
			}

			SetResourceDepositData( gameObject, data );
		}



		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static GameObject CreateEmpty( Guid guid, ResourceDepositDefinition def )
		{
			GameObject gameObject = CreateResourceDeposit();

			SetResourceDepositDefinition( gameObject, def );

			ResourceDeposit resourceDeposit = gameObject.GetComponent<ResourceDeposit>();
			resourceDeposit.guid = guid;

			return gameObject;
		}

		public static GameObject Create( ResourceDepositDefinition def, ResourceDepositData data )
		{
			GameObject gameObject = CreateResourceDeposit();

			SetResourceDepositDefinition( gameObject, def );
			SetResourceDepositData( gameObject, data );

			return gameObject;
		}
	}
}