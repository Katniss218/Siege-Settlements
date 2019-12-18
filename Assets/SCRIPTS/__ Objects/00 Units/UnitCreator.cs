using SS.Content;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.UI;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Object = UnityEngine.Object;
using SS.AI;
using SS.Objects.SubObjects;

namespace SS.Objects.Units
{
	public static class UnitCreator
	{
		private const string GAMEOBJECT_NAME = "Unit";


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static void SetDefData( GameObject gameObject, UnitDefinition def, UnitData data )
		{
			gameObject.name = GAMEOBJECT_NAME + " - '" + def.id + "'";

			//
			//    SUB-OBJECTS
			//

			SSObjectCreator.AssignSubObjects( gameObject, def );

			//
			//    CONTAINER GAMEOBJECT
			//

			// Set the position/movement information.
			gameObject.transform.SetPositionAndRotation( data.position, data.rotation );

			// Set the unit's size.
			BoxCollider collider = gameObject.GetComponent<BoxCollider>();
			collider.size = def.size;
			collider.center = new Vector3( 0.0f, def.size.y / 2.0f, 0.0f );

			// Set the unit's movement parameters.
			NavMeshAgent navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
			navMeshAgent.radius = def.size.x < def.size.z ? def.size.x * 0.5f : def.size.z * 0.5f;
			navMeshAgent.height = def.size.y;
			navMeshAgent.enabled = true; // Enable the NavMeshAgent since the position is set (data.position).

			// Set the unit's native parameters.
			Unit unit = gameObject.GetComponent<Unit>();
			unit.definitionId = def.id;
			unit.displayName = def.displayName;
			unit.icon = def.icon;
			unit.movementSpeed = def.movementSpeed;
			if( data.movementSpeedModifiers != null )
			{
				for( int i = 0; i < data.movementSpeedModifiers.Length; i++ )
				{
					unit.__movementSpeed[data.movementSpeedModifiers[i].id] = data.movementSpeedModifiers[i].value;
				}
			}
			else
			{
				unit.__movementSpeed["aa"] = UnityEngine.Random.Range( 0.5f, 3.0f );
			}
			unit.rotationSpeed = def.rotationSpeed;
			if( data.rotationSpeedModifiers != null )
			{
				for( int i = 0; i < data.rotationSpeedModifiers.Length; i++ )
				{
					unit.__rotationSpeed[data.rotationSpeedModifiers[i].id] = data.rotationSpeedModifiers[i].value;
				}
			}
			else
			{
				unit.__rotationSpeed["aa"] = UnityEngine.Random.Range( 0.5f, 3.0f );
			}


			unit.onFactionChange.AddListener( () =>
			{
				MeshSubObject[] meshes = unit.GetSubObjects<MeshSubObject>();
				Color color = LevelDataManager.factions[unit.factionId].color;

				for( int i = 0; i < meshes.Length; i++ )
				{
					meshes[i].GetMaterial().SetColor( "_FactionColor", color );
				}
			} );
			
			unit.onHealthPercentChanged.AddListener( () =>
			{
				MeshSubObject[] meshes = unit.GetSubObjects<MeshSubObject>();

				unit.hud.SetHealthBarFill( unit.healthPercent );
				for( int i = 0; i < meshes.Length; i++ )
				{
					meshes[i].GetMaterial().SetFloat( "_Dest", 1 - unit.healthPercent );
				}
			} );

			unit.factionId = data.factionId;
			unit.viewRange = def.viewRange;

			unit.healthMax = def.healthMax;
			if( data.maxHealthModifiers != null )
			{
				for( int i = 0; i < data.maxHealthModifiers.Length; i++ )
				{
					unit.__healthMax[data.maxHealthModifiers[i].id] = data.maxHealthModifiers[i].value;
				}
			}
			else
			{
#warning TODO! - handle invalid health in a better way then throwing exceptions.
				unit.__healthMax["aa"] = UnityEngine.Random.Range( 0.5f, 3.0f );
			}
			unit.health = data.health;
			unit.armor = def.armor;

			//
			//    MODULES
			//

			SSObjectCreator.AssignModules( gameObject, def, data );

			TacticalGoalController tacticalGoalController = gameObject.AddComponent<TacticalGoalController>();
			if( data.tacticalGoalData != null )
			{
				tacticalGoalController.goal = data.tacticalGoalData.GetInstance();
			}
		}

		private static GameObject CreateUnit( Guid guid )
		{
			GameObject container = new GameObject( GAMEOBJECT_NAME );
			container.layer = ObjectLayer.UNITS;

			//
			//    CONTAINER GAMEOBJECT
			//

			BoxCollider collider = container.AddComponent<BoxCollider>();

			Unit unit = container.AddComponent<Unit>();
			unit.guid = guid;
						
			// Add a kinematic rigidbody to the unit (required by the NavMeshAgent).
			Rigidbody rigidbody = container.AddComponent<Rigidbody>();
			rigidbody.isKinematic = true;

			// Add the NavMeshAgent to the unit, to make it movable.
			NavMeshAgent navMeshAgent = container.AddComponent<NavMeshAgent>();
			navMeshAgent.baseOffset = Main.DEFAULT_NAVMESH_BASE_OFFSET;
			navMeshAgent.acceleration = Main.DEFAULT_NAVMESH_ACCELERATION;
			navMeshAgent.stoppingDistance = Main.DEFAULT_NAVMESH_STOPPING_DIST;
			navMeshAgent.enabled = false; // Disable the NavMeshAgent for as long as the position is not set (data.position).

			GameObject hudGameObject = Object.Instantiate( (GameObject)AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/unit_hud" ), Main.camera.WorldToScreenPoint( container.transform.position ), Quaternion.identity, Main.objectHUDCanvas );
			
			HUD hud = hudGameObject.GetComponent<HUD>();

			unit.hud = hud;
			hud.isVisible = Main.isHudForcedVisible;

			UnityAction<bool> onHudLockChangeListener = ( bool isLocked ) =>
			{
				if( unit.hasBeenHiddenSinceLastDamage )
				{
					return;
				}
				if( isLocked )
				{
					hud.isVisible = true;
				}
				else
				{
					if( Selection.IsSelected( unit ) )
					{
						return;
					}
					if( (object)MouseOverHandler.currentObjectMouseOver == unit )
					{
						return;
					}
					hud.isVisible = false;
				}
			};

			Main.onHudLockChange.AddListener( onHudLockChangeListener );
			
			unit.onSelect.AddListener( () =>
			{
				if( Main.isHudForcedVisible ) { return; }
				if( MouseOverHandler.currentObjectMouseOver == container )
				{
					return;
				}
				hud.isVisible = true;
			} );

			unit.onDeselect.AddListener( () =>
			{
				if( Main.isHudForcedVisible ) { return; }
				if( MouseOverHandler.currentObjectMouseOver == container )
				{
					return;
				}
				hud.isVisible = false;
			} );

			// Make the unit change it's color when the faction is changed.
			unit.onFactionChange.AddListener( () =>
			{
				Color color = LevelDataManager.factions[unit.factionId].color;
				hud.SetColor( color );
			} );

			// Make the unit update it's healthbar and material when health changes.
			unit.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				if( deltaHP < 0 )
				{
					hud.isVisible = true;
					unit.hasBeenHiddenSinceLastDamage = true;
				}
			} );

			// Make the unit deselect itself, and destroy it's UI when killed.
			unit.onDeath.AddListener( () =>
			{
				Object.Destroy( hud.gameObject );

				if( Selection.IsSelected( unit ) )
				{
					Selection.Deselect( unit ); // We have all of the references of this unit here, so we can just simply pass it like this. Amazing, right?
				}
				// Remove the now unused listeners.
				Main.onHudLockChange.RemoveListener( onHudLockChangeListener );
			} );

			unit.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				if( !Selection.IsDisplayed( unit ) )
				{
					return;
				}
				Transform healthUI = SelectionPanel.instance.obj.GetElement( "unit.health" );
				if( healthUI != null )
				{
					UIUtils.EditText( healthUI.gameObject, (int)unit.health + "/" + (int)unit.healthMax );
				}
			} );
			
			return container;
		}

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
		/// <summary>
		/// Creates a new UnitData from a GameObject.
		/// </summary>
		/// <param name="unit">The GameObject to extract the save state from. Must be a unit.</param>
		public static UnitData GetData( Unit unit )
		{
			if( unit.guid == null )
			{
				throw new Exception( "Guid not assigned." );
			}

			UnitData data = new UnitData();
			data.guid = unit.guid;

			data.position = unit.transform.position;
			data.rotation = unit.transform.rotation;
			
			data.factionId = unit.factionId;
			
			data.health = unit.health;

			data.maxHealthModifiers = unit.__healthMax.GetModifiers();
			data.movementSpeedModifiers = unit.__movementSpeed.GetModifiers();
			data.rotationSpeedModifiers = unit.__rotationSpeed.GetModifiers();

			//
			// MODULES
			//

			SSObjectCreator.ExtractModulesToData( unit, data );

			data.tacticalGoalData = unit.GetComponent<TacticalGoalController>().goal.GetData();

			return data;
		}
		
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
	}
}