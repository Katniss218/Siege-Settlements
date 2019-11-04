using SS.Buildings;
using SS.Content;
using SS.Modules.Inventories;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Modules;
using SS.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using SS.Diplomacy;
using SS.Technologies;

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

		public static void SetDefData( GameObject gameObject, UnitDefinition def, UnitData data )
		{

			//
			//    GRAPHICS GAMEOBJECT
			//

			GameObject gfx = gameObject.transform.Find( Main.GRAPHICS_GAMEOBJECT_NAME ).gameObject;


			// Set the unit's mesh and material.
			MeshFilter meshFilter = gfx.GetComponent<MeshFilter>();
			meshFilter.mesh = def.mesh;
			
			MeshRenderer meshRenderer = gfx.GetComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateColoredDestroyable( FactionDefinition.DefaultColor, def.albedo, def.normal, null, 0.0f, 0.25f, 0.0f );


			//
			//    CONTAINER GAMEOBJECT
			//

			// Set the position/movement information.
			gameObject.transform.SetPositionAndRotation( data.position, data.rotation );

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
			navMeshAgent.enabled = true; // Enable the NavMeshAgent since the position is set (data.position).

			// Set the unit's native parameters.
			Unit unit = gameObject.GetComponent<Unit>();
			unit.defId = def.id;
			unit.displayName = def.displayName;

			// Set the unit's selected icon.
			Selectable selectable = gameObject.GetComponent<Selectable>();
			selectable.icon = def.icon;

			FactionMember factionMember = gameObject.GetComponent<FactionMember>();
			factionMember.factionId = data.factionId;

			// Set the unit's health.
			Damageable damageable = gameObject.GetComponent<Damageable>();
			damageable.healthMax = def.healthMax;
			damageable.health = data.health;
			damageable.armor = def.armor;

			UnityAction<int, string, TechnologyResearchProgress> onTechChange = ( int factionId, string id, TechnologyResearchProgress newProgress ) =>
			{
				if( factionId != factionMember.factionId )
				{
					return;
				}
				if( factionId != LevelDataManager.PLAYER_FAC )
				{
					return;
				}
				if( !Selection.IsHighlighted( selectable ) )
				{
					return;
				}
				if( SelectionPanel.instance.obj.GetElement( "constr.list" ) != null )
				{
					SelectionPanel.instance.obj.Clear( "constr.list" );
				}
				ConstructorRefreshList();
			};

			UnityAction onDeath = () =>
			{
				LevelDataManager.onTechStateChanged.RemoveListener( onTechChange );
			};

			UnityAction constructorOnSelect = () =>
			{
				if( factionMember.factionId != LevelDataManager.PLAYER_FAC )
				{
					return;
				}
				const string TEXT = "Select building to place...";

				ConstructorRefreshList();

				// Create the actual UI.
				GameObject statusUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), TEXT );
				SelectionPanel.instance.obj.RegisterElement( "constr.status", statusUI.transform );
			};

			// If the unit is constructor (civilian), make it show the build menu.
			if( def.isConstructor )
			{
				selectable.onHighlight.AddListener( constructorOnSelect );
				LevelDataManager.onTechStateChanged.AddListener( onTechChange );

				damageable.onDeath.AddListener( onDeath );
			}
			// If the unit was constructor before (listener is added), but it's not a constructor anymore - remove listener.
			else
			{
				selectable.onHighlight.RemoveListener( constructorOnSelect );
			}

			//
			//    MODULES
			//

			Guid[] moduleDefIds;
			ModuleDefinition[] moduleDefinitions;

			Guid[] moduleDataIds;
			ModuleData[] moduleData;
			
			def.GetAllModules( out moduleDefIds, out moduleDefinitions );
			data.GetAllModules( out moduleDataIds, out moduleData );

			int moduleCount = moduleDefIds.Length;

			for( int i = 0; i < moduleCount; i++ )
			{
				for( int j = 0; j < moduleCount; j++ )
				{
					if( moduleDefIds[i] == moduleDataIds[j] )
					{
						moduleDefinitions[i].AddModule( gameObject, moduleData[i] );
						break;
					}
					else if( j == moduleCount - 1 )
					{
						throw new Exception( "No module data corresponding to moduleId of '" + moduleDefIds[i].ToString( "D" ) + "' was found." );
					}
				}
			}

			//InventoryUnconstrained inventory = gameObject.GetComponent<InventoryUnconstrained>();

			//InventoryUnconstrainedDefinition inventoryDef = new InventoryUnconstrainedDefinition();
			//inventoryDef.slotCapacity = 10;
			//inventoryDef.slotCount = 1;
			//inventory.SetDefData( inventoryDef, data.inventoryData );

			TAIGoalData taiGoalData = data.taiGoalData;
			if( taiGoalData != null )
			{
				TAIGoal.Assign( gameObject, taiGoalData );
			}
		}

		private static GameObject CreateUnit( Guid guid )
		{
			GameObject container = new GameObject( GAMEOBJECT_NAME );
			container.layer = ObjectLayer.UNITS;


			//
			//    GRAPHICS GAMEOBJECT
			//

			GameObject gfx = new GameObject( Main.GRAPHICS_GAMEOBJECT_NAME );
			gfx.transform.SetParent( container.transform );
			
			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			
			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
		

			//
			//    CONTAINER GAMEOBJECT
			//

			BoxCollider collider = container.AddComponent<BoxCollider>();

			Unit unit = container.AddComponent<Unit>();
			unit.guid = guid;

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
			navMeshAgent.enabled = false; // Disable the NavMeshAgent for as long as the position is not set (data.position).

			GameObject hudGameObject = Object.Instantiate( (GameObject)AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/unit_hud" ), Main.camera.WorldToScreenPoint( container.transform.position ), Quaternion.identity, Main.objectHUDCanvas );
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
			/*InventoryUnconstrained inventory = container.AddComponent<InventoryUnconstrained>();
			
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
					hudResourceIcon.sprite = DefinitionManager.GetResource( kvp.Key ).icon; // this can be null.
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
						hudResourceIcon.sprite = DefinitionManager.GetResource( kvp.Key ).icon; // this can be null.
						hudAmount.text = kvp.Value.ToString();
						break;
					}
				}
			} );*/

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
#warning TODO! - inventories need to work with HUD system (assign stuff on inv's awake/start).
				/*if( !inventory.isEmpty )
				{
					TAIGoal.DropoffToNew.DropOffInventory( inventory, container.transform.position );
				}*/
			} );

			selectable.onHighlight.AddListener( () =>
			{
				GameObject nameUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), unit.displayName );
				SelectionPanel.instance.obj.RegisterElement( "unit.name", nameUI.transform );

				GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), (int)damageable.health + "/" + (int)damageable.healthMax );
				SelectionPanel.instance.obj.RegisterElement( "unit.health", healthUI.transform );
			} );

			damageable.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				if( !Selection.IsHighlighted( selectable ) )
				{
					return;
				}
				Transform healthUI = SelectionPanel.instance.obj.GetElement( "unit.health" );
				if( healthUI != null )
				{
					UIUtils.EditText( healthUI.gameObject, (int)damageable.health + "/" + (int)damageable.healthMax );
				}
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

			Unit unit = gameObject.GetComponent<Unit>();
			if( unit.guid == null )
			{
				throw new Exception( "Guid not assigned." );
			}
			data.guid = unit.guid.Value;

			data.position = gameObject.transform.position;
			data.rotation = gameObject.transform.rotation;

			FactionMember factionMember = gameObject.GetComponent<FactionMember>();
			data.factionId = factionMember.factionId;

			Damageable damageable = gameObject.GetComponent<Damageable>();
			data.health = damageable.health;

			//
			// MODULES
			//

			Module[] modules = gameObject.GetComponents<Module>();
			for( int i = 0; i < modules.Length; i++ )
			{
				data.AddModuleData( modules[i].moduleId, modules[i].GetData() );
			}
			/*
			MeleeModule meleeModule = gameObject.GetComponent<MeleeModule>();
			if( meleeModule != null )
			{
				data.meleeData = (MeleeModuleData)meleeModule.GetData();
			}

			RangedModule rangedModule = gameObject.GetComponent<RangedModule>();
			if( rangedModule != null )
			{
				data.rangedData = (RangedModuleData)rangedModule.GetData();
			}
			*/
			//data.inventoryData = (InventoryUnconstrainedData)gameObject.GetComponent<InventoryUnconstrained>().GetData();

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




		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
		public static GameObject CreateEmpty( Guid guid )
		{
			GameObject gameObject = CreateUnit( guid );
			
			return gameObject;
		}

		public static GameObject Create( UnitDefinition def, UnitData data )
		{
			GameObject gameObject = CreateUnit( data.guid );

			SetDefData( gameObject, def, data );

			return gameObject;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		private static void ConstructorRefreshList()
		{
			BuildingDefinition[] registeredBuildings = DefinitionManager.GetAllBuildings();
			GameObject[] gridElements = new GameObject[registeredBuildings.Length];

			// Initialize the grid elements' GameObjects.
			for( int i = 0; i < registeredBuildings.Length; i++ )
			{
				BuildingDefinition buildingDef = registeredBuildings[i];

				// If the unit's techs required have not been researched yet, add unclickable button, otherwise, add normal button.
				if( Technologies.TechLock.CheckLocked( buildingDef, LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].GetAllTechs() ) )
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), buildingDef.icon, null );
				}
				else
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), buildingDef.icon, () =>
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
			GameObject listUI = UIUtils.InstantiateScrollableGrid( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 75.0f, 5.0f ), new Vector2( -150.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
			SelectionPanel.instance.obj.RegisterElement( "constr.list", listUI.transform );
		}

	}
}