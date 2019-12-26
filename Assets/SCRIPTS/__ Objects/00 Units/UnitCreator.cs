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
using SS.Objects.Modules;

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
			if( data.health != null )
			{
				unit.health = data.health.Value;
			}
			if( data.movementSpeed != null )
			{
				unit.movementSpeedOverride = data.movementSpeed.Value;
			}
			if( data.rotationSpeed != null )
			{
				unit.rotationSpeedOverride = data.rotationSpeed.Value;
			}
			
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

			if( data.inside != null )
			{
				SSObject obj = SSObject.Find( data.inside.Item1 );
				InteriorModule interior = obj.GetModule<InteriorModule>( data.inside.Item2 );
				unit.SetInside( interior );
			}
#warning this (position) depends on the position of interior being assigned. If the position of an object with an interior is changed, the interior needs to move its contents with it.
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
			unit.movementSpeed = def.movementSpeed;
			unit.rotationSpeed = def.rotationSpeed;
			unit.size = def.size;

			unit.viewRange = def.viewRange;
			unit.healthMax = def.healthMax;
			unit.health = def.healthMax;
			unit.armor = def.armor;

			unit.onFactionChange.AddListener( () =>
			{
				Color color = LevelDataManager.factions[unit.factionId].color;

				unit.hud.SetColor( color );
				MeshSubObject[] meshes = unit.GetSubObjects<MeshSubObject>();
				for( int i = 0; i < meshes.Length; i++ )
				{
					meshes[i].GetMaterial().SetColor( "_FactionColor", color );
				}
				MeshPredicatedSubObject[] meshes2 = unit.GetSubObjects<MeshPredicatedSubObject>();
				for( int i = 0; i < meshes2.Length; i++ )
				{
					meshes2[i].GetMaterial().SetColor( "_FactionColor", color );
				}

				// Re-Display the object

				if( Selection.IsDisplayed( unit ) )
				{
					SSObjectHelper.ReDisplayDisplayed();
				}
			} );

			unit.onHealthPercentChanged.AddListener( () =>
			{
				unit.hud.SetHealthBarFill( unit.healthPercent );

				MeshSubObject[] meshes = unit.GetSubObjects<MeshSubObject>();
				for( int i = 0; i < meshes.Length; i++ )
				{
					meshes[i].GetMaterial().SetFloat( "_Dest", 1 - unit.healthPercent );
				}
				MeshPredicatedSubObject[] meshes2 = unit.GetSubObjects<MeshPredicatedSubObject>();
				for( int i = 0; i < meshes2.Length; i++ )
				{
					meshes2[i].GetMaterial().SetFloat( "_Dest", 1 - unit.healthPercent );
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

			unit.population = PopulationSize.x1;

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

			if( unit.health != unit.healthMax )
			{
				data.health = unit.health;
			}
			if( unit.movementSpeedOverride != null )
			{
				data.movementSpeed = unit.movementSpeedOverride;
			}
			if( unit.rotationSpeedOverride != null )
			{
				data.rotationSpeed = unit.rotationSpeedOverride;
			}
			data.population = unit.population;

			if( unit.isInside )
			{
#warning Proper slot-index.
				data.inside = new Tuple<Guid, Guid>(
						unit.interior.ssObject.guid,
						unit.interior.moduleId
					);
				data.insideSlotIndex = 0;
			}

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