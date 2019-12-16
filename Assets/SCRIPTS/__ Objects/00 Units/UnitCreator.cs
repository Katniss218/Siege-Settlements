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
			navMeshAgent.speed = def.movementSpeed;
			navMeshAgent.angularSpeed = def.rotationSpeed;
			navMeshAgent.enabled = true; // Enable the NavMeshAgent since the position is set (data.position).

			// Set the unit's native parameters.
			Unit unit = gameObject.GetComponent<Unit>();
			unit.definitionId = def.id;
			unit.displayName = def.displayName;
			unit.icon = def.icon;


			MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();

			unit.onFactionChange.AddListener( () =>
			{
				Color color = LevelDataManager.factions[unit.factionId].color;

				for( int i = 0; i < renderers.Length; i++ )
				{
					renderers[i].material.SetColor( "_FactionColor", color );
				}
			} );

			// Make the unit update it's healthbar and material when health changes.
#warning TODO! - we would want this to also update when the modifier changes the max health (so the proportion changes).
			// When the modifier is added, we need to update
			// When the modifier is removed, same
			// Wieh the modifier value changes, same
			unit.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				for( int i = 0; i < renderers.Length; i++ )
				{
					renderers[i].material.SetFloat( "_Dest", 1 - unit.healthPercent );
				}
			} );

			unit.factionId = data.factionId;
			unit.viewRange = def.viewRange;

			unit.healthMax = def.healthMax;
			unit.__healthMax["mod"] = UnityEngine.Random.Range( 1.0f, 2.0f ); // this needs to be set before the health is set, as the onHealthChange would have the non-modified value.
			unit.health = data.health;
			unit.armor = def.armor;
#warning This results in invalid behaviour - units are not crumbled at all, and their healthbars show full health.

			//
			//    MODULES
			//

			SSObjectCreator.AssignModules( gameObject, def, data );

			TacticalGoalController tacticalGoalController = gameObject.AddComponent<TacticalGoalController>();
			if( data.tacticalGoalData != null )
			{
				tacticalGoalController.goal = data.tacticalGoalData.GetInstance();
			}

			unit.RandomizeRanged();
#warning After all the fields (and modules) are set to 'x1''s scaling, set the scaling to the proper value (automatically recalculates all fields).
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
			hudGameObject.SetActive( Main.isHudLocked ); // Only show hud when it's locked.

			HUDScaled hud = hudGameObject.GetComponent<HUDScaled>();

			unit.hud = hudGameObject;
			
			UnityAction<bool> onHudLockChangeListener = ( bool isLocked ) =>
			{
				if( unit.hasBeenHiddenSinceLastDamage )
				{
					return;
				}
				if( isLocked )
				{
					hudGameObject.SetActive( true );
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
					hudGameObject.SetActive( false );
				}
			};

			Main.onHudLockChange.AddListener( onHudLockChangeListener );
			
			unit.onSelect.AddListener( () =>
			{
				if( Main.isHudLocked ) { return; }
				if( MouseOverHandler.currentObjectMouseOver == container )
				{
					return;
				}
				hudGameObject.SetActive( true );
			} );

			unit.onDeselect.AddListener( () =>
			{
				if( Main.isHudLocked ) { return; }
				if( MouseOverHandler.currentObjectMouseOver == container )
				{
					return;
				}
				hudGameObject.SetActive( false );
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
				hud.SetHealthBarFill( unit.healthPercent );
				if( deltaHP < 0 )
				{
					hudGameObject.SetActive( true );
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