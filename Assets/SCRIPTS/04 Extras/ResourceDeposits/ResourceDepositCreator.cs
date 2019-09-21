using SS.Content;
using SS.Inventories;
using SS.ResourceSystem;
using SS.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace SS.Extras
{
	public static class ResourceDepositCreator
	{
		public static GameObject Create( ResourceDepositDefinition def, Vector3 pos, Quaternion rot, Dictionary<string, int> resources )
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
			meshRenderer.material = def.shaderType == MaterialType.PlantOpaque ? MaterialManager.CreatePlantOpaque( def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f, 0.3333f ) : MaterialManager.CreateOpaque( def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f );

			BoxCollider collider = container.AddComponent<BoxCollider>();
			collider.size = def.size;
			collider.center = new Vector3( 0f, def.size.y / 2.0f, 0f );

			NavMeshObstacle obstacle = container.AddComponent<NavMeshObstacle>();
			obstacle.size = def.size;
			obstacle.center = new Vector3( 0f, def.size.y / 2.0f, 0f );
			obstacle.carving = true;

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

					ToolTip.Create( 200, def.displayName );

					foreach( var kvp in itemsInDeposit )
					{
						ResourceDefinition resourceDef = DataManager.Get<ResourceDefinition>( kvp.Key );
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
			InventoryConstrained.SlotInfo[] slotInfos = new InventoryConstrained.SlotInfo[def.resources.Count];
			int i = 0;
			foreach( var kvp in def.resources )
			{
				slotInfos[i] = new InventoryConstrained.SlotInfo( kvp.Key, kvp.Value );
				i++;
			}
			depositInventory.SetSlots( slotInfos );
			foreach( var kvp in resources )
			{
				int capacity = depositInventory.GetMaxCapacity( kvp.Key );
				if( capacity == 0 )
				{
					throw new System.Exception( "This deposit can't hold '" + kvp.Key + "'." );
				}
				else
				{
					if( capacity < kvp.Value )
					{
						Debug.LogWarning( "This deposit can't hold " + kvp.Value + " x '" + kvp.Key + "'. - " + (kvp.Value - capacity) + " x resource has been lost." );
						depositInventory.Add( kvp.Key, capacity );
					}
					else
					{
						depositInventory.Add( kvp.Key, kvp.Value );
					}
				}
			}

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

			ResourceDeposit resourceDepositComponent = container.AddComponent<ResourceDeposit>();
			resourceDepositComponent.id = def.id;
			resourceDepositComponent.isTypeExtracted = def.isExtracted;
			if( !def.isExtracted )
			{
				resourceDepositComponent.miningSound = def.mineSound.Item2;
			}
			else
			{
				resourceDepositComponent.miningSound = null;
			}

			return container;
		}
	}
}