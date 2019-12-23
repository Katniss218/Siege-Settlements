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

		public static void SetData( GameObject gameObject, UnitData data )
		{
			//
			//    CONTAINER GAMEOBJECT
			//

			// Set the position/movement information.
			gameObject.transform.SetPositionAndRotation( data.position, data.rotation );

			// Set the unit's movement parameters.
			NavMeshAgent navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
			navMeshAgent.enabled = true; // Enable the NavMeshAgent since the position is set (data.position).

			// Set the unit's native parameters.
			Unit unit = gameObject.GetComponent<Unit>();
			if( unit.guid != data.guid )
			{
				throw new Exception( "Mismatched guid." );
			}
			unit.factionId = data.factionId;
			unit.health = data.health;

			//
			//    MODULES
			//

			SSObjectCreator.AssignModuleData( unit, data );

			TacticalGoalController tacticalGoalController = gameObject.AddComponent<TacticalGoalController>();
			if( data.tacticalGoalData != null )
			{
				tacticalGoalController.goal = data.tacticalGoalData.GetInstance();
			}

			unit.population = data.population;
		}

		private static GameObject CreateUnit( UnitDefinition def, Guid guid )
		{
			GameObject gameObject = new GameObject( GAMEOBJECT_NAME + " - '" + def.id + "'" );
			gameObject.layer = ObjectLayer.UNITS;

			//
			//    CONTAINER GAMEOBJECT
			//

			GameObject hudGameObject = Object.Instantiate( (GameObject)AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/unit_hud" ), Main.camera.WorldToScreenPoint( gameObject.transform.position ), Quaternion.identity, Main.objectHUDCanvas );

			HUD hud = hudGameObject.GetComponent<HUD>();
			hud.isVisible = Main.isHudForcedVisible;


			BoxCollider collider = gameObject.AddComponent<BoxCollider>();

			// Add a kinematic rigidbody to the unit (required by the NavMeshAgent).
			Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
			rigidbody.isKinematic = true;

			// Add the NavMeshAgent to the unit, to make it movable.
			NavMeshAgent navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
			navMeshAgent.baseOffset = Main.DEFAULT_NAVMESH_BASE_OFFSET;
			navMeshAgent.acceleration = Main.DEFAULT_NAVMESH_ACCELERATION;
			navMeshAgent.stoppingDistance = Main.DEFAULT_NAVMESH_STOPPING_DIST;
			navMeshAgent.enabled = false; // Disable the NavMeshAgent for as long as the position is not set (data.position).

			Unit unit = gameObject.AddComponent<Unit>();
			unit.hud = hud;
			unit.guid = guid;
			unit.definitionId = def.id;
			unit.displayName = def.displayName;
			unit.icon = def.icon;
			unit.movementSpeed.baseValue = def.movementSpeed;
			unit.rotationSpeed.baseValue = def.rotationSpeed;
			unit.size = def.size;

			unit.viewRange = def.viewRange;
			unit.healthMax = def.healthMax;
			unit.health = def.healthMax;
			unit.armor = def.armor;

			unit.onFactionChange.AddListener( () =>
			{
#warning TODO! - The Selection Panel display is not re-displayed when faction changes (enemy & friendly objects display differently).
				MeshSubObject[] meshes = unit.GetSubObjects<MeshSubObject>();
				Color color = LevelDataManager.factions[unit.factionId].color;

				unit.hud.SetColor( color );
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




			UnityAction<bool> onHudLockChangeListener = ( bool isLocked ) =>
			{
				if( unit.hasBeenHiddenSinceLastDamage )
				{
					return;
				}
				if( isLocked )
				{
					unit.hud.isVisible = true;
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
					unit.hud.isVisible = false;
				}
			};

			Main.onHudLockChange.AddListener( onHudLockChangeListener );

			unit.onSelect.AddListener( () =>
			{
				if( Main.isHudForcedVisible ) { return; }
				if( MouseOverHandler.currentObjectMouseOver == gameObject )
				{
					return;
				}
				unit.hud.isVisible = true;
			} );

			unit.onDeselect.AddListener( () =>
			{
				if( Main.isHudForcedVisible ) { return; }
				if( MouseOverHandler.currentObjectMouseOver == gameObject )
				{
					return;
				}
				unit.hud.isVisible = false;
			} );

			// Make the unit update it's healthbar and material when health changes.
			unit.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				if( deltaHP < 0 )
				{
					Debug.Log( "AA" );
					unit.hud.isVisible = true;
					unit.hasBeenHiddenSinceLastDamage = true;
				}


				if( !Selection.IsDisplayed( unit ) )
				{
					return;
				}
				Transform healthUI = SelectionPanel.instance.obj.GetElement( "unit.health" );
				if( healthUI != null )
				{
					UIUtils.EditText( healthUI.gameObject, SSObjectDFS.GetHealthDisplay( unit.health, unit.healthMax ) );
				}
			} );

			// Make the unit deselect itself, and destroy it's UI when killed.
			unit.onDeath.AddListener( () =>
			{
				Object.Destroy( unit.hud.gameObject );

				if( Selection.IsSelected( unit ) )
				{
					Selection.Deselect( unit ); // We have all of the references of this unit here, so we can just simply pass it like this. Amazing, right?
				}
				// Remove the now unused listeners.
				Main.onHudLockChange.RemoveListener( onHudLockChangeListener );
			} );

			//
			//    SUB-OBJECTS
			//

			SSObjectCreator.AssignSubObjects( gameObject, def );

			//
			//    MODULES
			//

			SSObjectCreator.AssignModules( gameObject, def );

			unit.population = def.defaultPopulationOnSpawn;

			return gameObject;
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
			data.population = unit.population;

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

		public static GameObject Create( UnitDefinition def, Guid guid )
		{
			return CreateUnit( def, guid );
		}
	}
}