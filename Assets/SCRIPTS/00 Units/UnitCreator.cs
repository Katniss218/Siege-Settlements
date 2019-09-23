﻿using Katniss.Utils;
using SS.Buildings;
using SS.Content;
using SS.Inventories;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Modules;
using SS.ResourceSystem;
using SS.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SS.Units
{
	public static class UnitCreator
	{
		private const string GAMEOBJECT_NAME = "Unit";



		public static string GetDefinitionId( GameObject gameObject )
		{
			if( gameObject.layer != ObjectLayer.UNITS )
			{
				throw new System.Exception( "The specified GameObject is not a unit." );
			}

			Unit unit = gameObject.GetComponent<Unit>();
			return unit.defId;
		}

		/// <summary>
		/// Creates a new UnitData from a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject to extract the save state from. Must be a unit.</param>
		public static UnitData GetSaveState( GameObject gameObject )
		{
#warning incomplete - modules.
			if( gameObject.layer != ObjectLayer.UNITS )
			{
				throw new System.Exception( "The specified GameObject is not a unit." );
			}

			UnitData saveState = new UnitData();

			saveState.position = gameObject.transform.position;
			saveState.rotation = gameObject.transform.rotation;
			
			FactionMember factionMember = gameObject.GetComponent<FactionMember>();
			saveState.factionId = factionMember.factionId;

			Damageable damageable = gameObject.GetComponent<Damageable>();
			saveState.health = damageable.health;

			return saveState;
		}

		public static GameObject Create( UnitDefinition def, UnitData data )
		{
			if( def == null )
			{
				throw new System.ArgumentNullException( "Definition can't be null." );
			}
			GameObject container = new GameObject( GAMEOBJECT_NAME + " ('" + def.id + "'), (f: " + data.factionId + ")" );
			container.layer = ObjectLayer.UNITS;

			GameObject gfx = new GameObject( GameObjectUtils.GRAPHICS_GAMEOBJECT_NAME );
			gfx.transform.SetParent( container.transform );

			container.transform.SetPositionAndRotation( data.position, data.rotation );
			

			// Add a mesh to the unit.
			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;

			// Add a material to the unit.
			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateColoredDestroyable( LevelManager.currentLevel.Value.factions[data.factionId].color, def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f, 0.0f );
			
			
			BoxCollider collider = container.AddComponent<BoxCollider>();
			collider.size = new Vector3( def.radius * 2.0f, def.height, def.radius * 2.0f );
			collider.center = new Vector3( 0.0f, def.height / 2.0f, 0.0f );

			Unit unit = container.AddComponent<Unit>();
			unit.defId = def.id;

			// Mask the unit as selectable.
			Selectable selectable = container.AddComponent<Selectable>();
			selectable.icon = def.icon.Item2;
			// If the unit is constructor (civilian), make it show the build menu, when highlighted.
			if( def.isConstructor )
			{
				selectable.onSelectionUIRedraw.AddListener( ConstructorOnSelect );
			}

			// Add a kinematic rigidbody to the unit (required by the NavMeshAgent).
			Rigidbody rigidbody = container.AddComponent<Rigidbody>();
			rigidbody.isKinematic = true;

			// Add the NavMeshAgent to the unit, to make it movable.
			NavMeshAgent navMeshAgent = container.AddComponent<NavMeshAgent>();
			navMeshAgent.baseOffset = Main.DEFAULT_NAVMESH_BASE_OFFSET;
			navMeshAgent.acceleration = 8.0f;
			navMeshAgent.stoppingDistance = 0.125f;
			navMeshAgent.radius = def.radius;
			navMeshAgent.height = def.height;
			navMeshAgent.speed = def.movementSpeed;
			navMeshAgent.angularSpeed = def.rotationSpeed;

			GameObject hudGameObject = Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/unit_hud" ), Main.camera.WorldToScreenPoint( data.position ), Quaternion.identity, Main.worldUIs );
			hudGameObject.SetActive( Main.isHudLocked ); // Only show hud when it's locked.

			HUDScaled hud = hudGameObject.GetComponent<HUDScaled>();

			Image hudResourceIcon = hud.transform.Find( "Resource" ).Find("Icon").GetComponent<Image>();
			TextMeshProUGUI hudAmount = hud.transform.Find( "Amount" ).GetComponent<TextMeshProUGUI>();

			UnityAction<bool> onHudLockChangeListener = ( bool isLocked ) =>
			{
				if( isLocked )
				{
					hudGameObject.SetActive( true );
				}
				else
				{
					if( Selection.IsSelected( selectable ) )
					{
						return;
					}
					if( MouseOverHandler.currentObjectMouseOver == container )
					{
						return;
					}
					hudGameObject.SetActive( false );
				}
			};

			Main.onHudLockChange.AddListener( onHudLockChangeListener );

			UnityAction<GameObject> onMouseEnterListener = ( GameObject obj ) =>
			{
				if( Main.isHudLocked ) { return; }
				if( obj == container )
				{
					if( Selection.IsSelected( selectable ) )
					{
						return;
					}
					hudGameObject.SetActive( true );
				}
			};

			UnityAction<GameObject> onMouseExitListener = ( GameObject obj ) =>
			{
				if( Main.isHudLocked ) { return; }
				if( obj == container )
				{
					if( Selection.IsSelected( selectable ) )
					{
						return;
					}
					hudGameObject.SetActive( false );
				}
			};
			
			// Show HUD only when mouseovered or selected.
			MouseOverHandler.onMouseEnter.AddListener( onMouseEnterListener );
			MouseOverHandler.onMouseExit.AddListener( onMouseExitListener );

			selectable.onSelect.AddListener( () =>
			{
				if( Main.isHudLocked ) { return; }
				if( MouseOverHandler.currentObjectMouseOver == container )
				{
					return;
				}
				hudGameObject.SetActive( true );
			} );

			selectable.onDeselect.AddListener( () =>
			{
				if( Main.isHudLocked ) { return; }
				if( MouseOverHandler.currentObjectMouseOver == container )
				{
					return;
				}
				hudGameObject.SetActive( false );
			} );


			InventoryUnconstrained inventory = container.AddComponent<InventoryUnconstrained>();
			inventory.SetSlots( 1, 10 );
			inventory.onAdd.AddListener( ( string id, int amtAdded ) =>
			{
				// Set the icon to the first slot that contains a resource.
				foreach( var kvp in inventory.GetAll() )
				{
					if( kvp.Key == "" )
					{
						continue;
					}
					hudResourceIcon.sprite = DataManager.Get<ResourceDefinition>( kvp.Key ).icon.Item2; // this can be null.
					hudAmount.text = kvp.Value.ToString();

					hudResourceIcon.gameObject.SetActive( true );
					hudAmount.gameObject.SetActive( true );
					break;
				}
			} );
			inventory.onRemove.AddListener( ( string id, int amtRemoved ) =>
			{
				if( inventory.isEmpty )
				{
					hudResourceIcon.gameObject.SetActive( false );
					hudAmount.gameObject.SetActive( false );
				}
				else
				{
					// Set the icon to the first slot that contains a resource.
					foreach( var kvp in inventory.GetAll() )
					{
						if( kvp.Key == "" )
						{
							continue;
						}
						hudResourceIcon.sprite = DataManager.Get<ResourceDefinition>( kvp.Key ).icon.Item2; // this can be null.
						hudAmount.text = kvp.Value.ToString();
						break;
					}
				}
			} );

			// Make the unit belong to a faction.
			FactionMember factionMember = container.AddComponent<FactionMember>();
			factionMember.onFactionChange.AddListener( () =>
			{
				Color color = LevelManager.currentLevel.Value.factions[data.factionId].color;
				hud.SetColor( color );
				meshRenderer.material.SetColor( "_FactionColor", color );
			} );
			// We set the faction after assigning the listener, to automatically set the color to the appropriate value.
			factionMember.factionId = data.factionId;

			// Make the unit damageable.
			Damageable damageable = container.AddComponent<Damageable>();
			damageable.onHealthChange.AddListener( (float deltaHP) =>
			{
				meshRenderer.material.SetFloat( "_Dest", 1 - damageable.healthPercent );
				hud.SetHealthBarFill( damageable.healthPercent );

				Selection.ForceSelectionUIRedraw( selectable );
			} );
			// Make the unit deselect itself, and destroy it's UI when killed.
			damageable.onDeath.AddListener( () =>
			{
				Object.Destroy( hud.gameObject );
				
				if( Selection.IsSelected( selectable ) )
				{
					Selection.Deselect( selectable ); // We have all of the references of this unit here, so we can just simply pass it like this. Amazing, right?
				}
				// Remove the now unused listeners.
				MouseOverHandler.onMouseEnter.RemoveListener( onMouseEnterListener );
				MouseOverHandler.onMouseEnter.RemoveListener( onMouseExitListener );
				Main.onHudLockChange.RemoveListener( onHudLockChangeListener );
				if( !inventory.isEmpty )
				{
					TAIGoal.DropoffToNew.DropOffInventory( inventory, container.transform.position );
				}
			} );

			selectable.onSelectionUIRedraw.AddListener( () =>
			{
				UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), def.displayName );
				UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), (int)damageable.health + "/" + (int)damageable.healthMax );
			} );

			damageable.healthMax = def.healthMax;
			damageable.Heal();
			damageable.armor = def.armor;
	
			// If the new unit is melee, setup the melee module.
			if( def.melee != null )
			{
				MeleeModule melee = container.AddComponent<MeleeModule>();
				melee.SetSaveState( def.melee );
			}

			// If the new unit is ranged, setup the ranged module.
			if( def.ranged != null )
			{
				RangedModule ranged = container.AddComponent<RangedModule>();
				ranged.SetSaveState( def.ranged );
			}


			// Make the unit update it's UI's position every frame.
			container.AddComponent<EveryFrameSingle>().onUpdate = () =>
			{
				hud.transform.position = Main.camera.WorldToScreenPoint( container.transform.position );
			};

			return container;
		}

		private static void ConstructorOnSelect()
		{
			const string TEXT = "Select building to place...";

			List<BuildingDefinition> bdef = DataManager.GetAllOfType<BuildingDefinition>();
			GameObject[] gridElements = new GameObject[bdef.Count];

			// Initialize the grid elements' GameObjects.
			for( int i = 0; i < bdef.Count; i++ )
			{
				BuildingDefinition buildingDef = bdef[i];

				// If the unit's techs required have not been researched yet, add unclickable button, otherwise, add normal button.
				if( Technologies.TechLock.CheckLocked( buildingDef, FactionManager.factions[FactionManager.PLAYER].techs ) )
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), buildingDef.icon.Item2, null );
				}
				else
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), buildingDef.icon.Item2, () =>
					{
						if( BuildPreview.isActive )
						{
							return;
						}
						BuildPreview.Create( buildingDef );
						Selection.DeselectAll(); // deselect everything when the preview is active, to stop the player from performing other left-mouse-button input actions.
					} );
				}
			}
			// Create the actual UI.
			UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), TEXT );
			UIUtils.InstantiateScrollableGrid( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 75.0f, 5.0f ), new Vector2( -150.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
		}
	}
}