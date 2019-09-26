using Katniss.Utils;
using SS.Buildings;
using SS.Content;
using SS.Inventories;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Modules;
using SS.ResourceSystem;
using SS.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SS.Units
{
	public static class UnitCreator
	{
		private const float DEFAULT_ACCELERATION = 8.0f;
		private const float DEFAULT_STOPPING_DISTANCE = 0.125f;
		private const string GAMEOBJECT_NAME = "Unit";

		
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		private static void SetUnitDefinition( GameObject gameObject, UnitDefinition def )
		{
			//
			//    GRAPHICS GAMEOBJECT
			//

			GameObject gfx = gameObject.transform.Find( GameObjectUtils.GRAPHICS_GAMEOBJECT_NAME ).gameObject;


			// Set the unit's mesh and material.
			MeshFilter meshFilter = gfx.GetComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;
			
			MeshRenderer meshRenderer = gfx.GetComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateColoredDestroyable( FactionDefinition.DefaultColor, def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f, 0.0f );


			//
			//    CONTAINER GAMEOBJECT
			//
			
			// Set the unit's size.
			BoxCollider collider = gameObject.GetComponent<BoxCollider>();
			collider.size = new Vector3( def.radius * 2.0f, def.height, def.radius * 2.0f );
			collider.center = new Vector3( 0.0f, def.height / 2.0f, 0.0f );
			
			// Set the unit's movement parameters.
			NavMeshAgent navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
			navMeshAgent.radius = def.radius;
			navMeshAgent.height = def.height;
			navMeshAgent.speed = def.movementSpeed;
			navMeshAgent.angularSpeed = def.rotationSpeed;

			// Set the unit's native parameters.
			Unit unit = gameObject.GetComponent<Unit>();
			unit.defId = def.id;
			unit.displayName = def.displayName;

			// Set the unit's selected icon.
			Selectable selectable = gameObject.GetComponent<Selectable>();
			selectable.icon = def.icon.Item2;

			// If the unit is constructor (civilian), make it show the build menu, hide it otherwise (if present).
			if( def.isConstructor )
			{
				selectable.onSelectionUIRedraw.AddListener( ConstructorOnSelect );
			}
			else
			{
				selectable.onSelectionUIRedraw.RemoveListener( ConstructorOnSelect );
			}
			
			// Set the unit's health.
			Damageable damageable = gameObject.GetComponent<Damageable>();
			damageable.healthMax = def.healthMax;
			damageable.armor = def.armor;


			//
			//    MODULES
			//

			// Remove old melee module (if present).
			MeleeModule melee = gameObject.GetComponent<MeleeModule>();
			if( melee != null )
			{
				Object.Destroy( melee );
			}
			// If the new unit is melee, setup the melee module.
			if( def.melee != null )
			{
				melee = gameObject.AddComponent<MeleeModule>();
				melee.SetDefinition( def.melee );
			}

			// Remove old ranged module (if present).
			RangedModule ranged = gameObject.GetComponent<RangedModule>();
			if( ranged != null )
			{
				Object.Destroy( ranged );
			}
			// If the new unit is ranged, setup the ranged module.
			if( def.ranged != null )
			{
				ranged = gameObject.AddComponent<RangedModule>();
				ranged.SetDefinition( def.ranged );
			}
		}

		private static void SetUnitData( GameObject gameObject, UnitData data )
		{

			//
			//    CONTAINER GAMEOBJECT
			//

			// Set the position/movement information.
			gameObject.transform.SetPositionAndRotation( data.position, data.rotation );

			// Set the globally unique identifier.
			Unit unit = gameObject.GetComponent<Unit>();
			unit.guid = data.guid;

			// Set the faction id.
			FactionMember factionMember = gameObject.GetComponent<FactionMember>();
			factionMember.factionId = data.factionId;

			// Make the unit damageable.
			Damageable damageable = gameObject.GetComponent<Damageable>();
			damageable.health = data.health;

			//
			//    MODULES
			//

			TAIGoalData taiGoalData = data.taiGoalData;
			if( taiGoalData != null )
			{
				if( taiGoalData is MoveToData )
				{
					TAIGoal.MoveTo.AssignTAIGoal( gameObject, ((MoveToData)taiGoalData).destination );
				}
				else if( taiGoalData is DropoffToNewData )
				{
					TAIGoal.DropoffToNew.AssignTAIGoal( gameObject, ((DropoffToNewData)taiGoalData).destination );
				}

				else if( taiGoalData is DropoffToInventoryData )
				{
					TAIGoal.DropoffToInventory.AssignTAIGoal( gameObject, Main.GetGameObject( ((DropoffToInventoryData)taiGoalData).destinationGuid ) );
				}
				else if( taiGoalData is MakePaymentData )
				{
					TAIGoal.MakePayment.AssignTAIGoal( gameObject, Main.GetGameObject( ((MakePaymentData)taiGoalData).destinationGuid ) );
				}
				else if( taiGoalData is PickupDepositData )
				{
					TAIGoal.PickupDeposit.AssignTAIGoal( gameObject, Main.GetGameObject( ((PickupDepositData)taiGoalData).destinationGuid ) );
				}
			}
		}

		private static GameObject CreateUnit()
		{
			GameObject container = new GameObject( GAMEOBJECT_NAME );
			container.layer = ObjectLayer.UNITS;


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

			Unit unit = container.AddComponent<Unit>();

			// Make the unit selectable.
			Selectable selectable = container.AddComponent<Selectable>();

			// Add a kinematic rigidbody to the unit (required by the NavMeshAgent).
			Rigidbody rigidbody = container.AddComponent<Rigidbody>();
			rigidbody.isKinematic = true;

			// Add the NavMeshAgent to the unit, to make it movable.
			NavMeshAgent navMeshAgent = container.AddComponent<NavMeshAgent>();
			navMeshAgent.baseOffset = Main.DEFAULT_NAVMESH_BASE_OFFSET;
			navMeshAgent.acceleration = DEFAULT_ACCELERATION;
			navMeshAgent.stoppingDistance = DEFAULT_STOPPING_DISTANCE;


			GameObject hudGameObject = Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/unit_hud" ), Main.camera.WorldToScreenPoint( container.transform.position ), Quaternion.identity, Main.worldUIs );
			hudGameObject.SetActive( Main.isHudLocked ); // Only show hud when it's locked.

			HUDScaled hud = hudGameObject.GetComponent<HUDScaled>();

			Image hudResourceIcon = hud.transform.Find( "Resource" ).Find( "Icon" ).GetComponent<Image>();
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


			// Add inventory.
			InventoryUnconstrained inventory = container.AddComponent<InventoryUnconstrained>();
			inventory.SetSlots( 1, 10 );

			// Make the inventory update the HUD wien resources are added/removed.
			inventory.onAdd.AddListener( ( string id, int amtAdded ) =>
			{
				// Set the icon to the first slot that contains a resource.
				foreach( var kvp in inventory.GetAll() )
				{
					if( kvp.Key == "" )
					{
						continue;
					}
					hudResourceIcon.sprite = DefinitionManager.GetResource( kvp.Key ).icon.Item2; // this can be null.
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
						hudResourceIcon.sprite = DefinitionManager.GetResource( kvp.Key ).icon.Item2; // this can be null.
						hudAmount.text = kvp.Value.ToString();
						break;
					}
				}
			} );

			// Make the unit change it's color when the faction is changed.
			FactionMember factionMember = container.AddComponent<FactionMember>();
			factionMember.onFactionChange.AddListener( () =>
			{
				Color color = LevelDataManager.factions[factionMember.factionId].color;
				hud.SetColor( color );
				meshRenderer.material.SetColor( "_FactionColor", color );
			} );
			

			// Make the unit damageable.
			Damageable damageable = container.AddComponent<Damageable>();

			// Make the unit update it's healthbar and material when health changes.
			damageable.onHealthChange.AddListener( ( float deltaHP ) =>
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

			// Make the unit show it's parameters on the Selection Panel, when highlighted.
			selectable.onSelectionUIRedraw.AddListener( () =>
			{
				UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), unit.displayName );
				UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), (int)damageable.health + "/" + (int)damageable.healthMax );
			} );
			
			// Make the unit update it's UI's position every frame.
			container.AddComponent<EveryFrameSingle>().onUpdate = () =>
			{
				hud.transform.position = Main.camera.WorldToScreenPoint( container.transform.position );
			};

			return container;
		}

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static string GetDefinitionId( GameObject gameObject )
		{
			if( !Unit.IsValid( gameObject ) )
			{
				throw new Exception( "GameObject '" + gameObject.name + "' is not a valid unit." );
			}

			Unit unit = gameObject.GetComponent<Unit>();
			return unit.defId;
		}

		/// <summary>
		/// Creates a new UnitData from a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject to extract the save state from. Must be a unit.</param>
		public static UnitData GetData( GameObject gameObject )
		{
			if( !Unit.IsValid( gameObject ) )
			{
				throw new Exception( "GameObject '" + gameObject.name + "' is not a valid unit." );
			}

			UnitData data = new UnitData();

			data.guid = gameObject.GetComponent<Unit>().guid;

			data.position = gameObject.transform.position;
			data.rotation = gameObject.transform.rotation;

			FactionMember factionMember = gameObject.GetComponent<FactionMember>();
			data.factionId = factionMember.factionId;

			Damageable damageable = gameObject.GetComponent<Damageable>();
			data.health = damageable.health;

			TAIGoal taiGoal = gameObject.GetComponent<TAIGoal>();
			if( taiGoal != null )
			{
				data.taiGoalData = taiGoal.GetData();
			}

			return data;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@



		public static void SetData( GameObject gameObject, UnitData data )
		{
			if( !Unit.IsValid( gameObject ) )
			{
				throw new Exception( "GameObject '" + gameObject.name + "' is not a valid unit." );
			}
			SetUnitData( gameObject, data );
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
		public static GameObject CreateEmpty( Guid guid, UnitDefinition def )
		{
			GameObject gameObject = CreateUnit();

			SetUnitDefinition( gameObject, def );

			Unit unit = gameObject.GetComponent<Unit>();
			unit.guid = guid;

			return gameObject;
		}

		public static GameObject Create( UnitDefinition def, UnitData data )
		{
			GameObject gameObject = CreateUnit();

			SetUnitDefinition( gameObject, def );
			SetUnitData( gameObject, data );

			return gameObject;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		private static void ConstructorOnSelect()
		{
			const string TEXT = "Select building to place...";

			BuildingDefinition[] registeredBuildings = DefinitionManager.GetAllBuildings();
			GameObject[] gridElements = new GameObject[registeredBuildings.Length];

			// Initialize the grid elements' GameObjects.
			for( int i = 0; i < registeredBuildings.Length; i++ )
			{
				BuildingDefinition buildingDef = registeredBuildings[i];

				// If the unit's techs required have not been researched yet, add unclickable button, otherwise, add normal button.
				if( Technologies.TechLock.CheckLocked( buildingDef, LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].techs ) )
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