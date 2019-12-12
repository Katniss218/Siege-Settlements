using SS.AI;
using SS.Content;
using SS.Diplomacy;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.UI;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SS.Objects.Buildings
{
	public class BuildingCreator
	{
		private const string GAMEOBJECT_NAME = "Building";


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static void SetDefData( GameObject gameObject, BuildingDefinition def, BuildingData data )
		{
			//
			//    SUB-OBJECTS
			//

			SSObjectCreator.AssignSubObjects( gameObject, def );
			
			//
			//    CONTAINER GAMEOBJECT
			//
			gameObject.transform.SetPositionAndRotation( data.position, data.rotation );

			// Set the building's size.
			BoxCollider collider = gameObject.GetComponent<BoxCollider>();
			collider.size = def.size;
			collider.center = new Vector3( 0.0f, def.size.y / 2.0f, 0.0f );

			NavMeshObstacle navMeshObstacle = gameObject.GetComponent<NavMeshObstacle>();
			navMeshObstacle.size = def.size;
			navMeshObstacle.center = new Vector3( 0.0f, def.size.y / 2.0f, 0.0f );
						
			// Set the building's native parameters.
			Building building = gameObject.GetComponent<Building>();
			building.definitionId = def.id;
			building.placementNodes = def.placementNodes;
			building.StartToEndConstructionCost = def.cost;
			building.buildSoundEffect = def.buildSoundEffect;
			building.displayName = def.displayName;
			building.deathSound = def.deathSoundEffect;
			building.icon = def.icon;

			// Make the building belong to a faction.
			FactionMember factionMember = gameObject.GetComponent<FactionMember>();

			// Set the building's health.
			Damageable damageable = gameObject.GetComponent<Damageable>();

			MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();

			factionMember.onFactionChange.AddListener( () =>
			{
				Color color = LevelDataManager.factions[factionMember.factionId].color;

				for( int i = 0; i < renderers.Length; i++ )
				{
					renderers[i].material.SetColor( "_FactionColor", color );
				}
			} );

			// Make the unit update it's healthbar and material when health changes.
			damageable.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				for( int i = 0; i < renderers.Length; i++ )
				{
					renderers[i].material.SetFloat( "_Dest", 1 - damageable.healthPercent );
				}
			} );

			factionMember.factionId = data.factionId;
			factionMember.viewRange = def.viewRange;

			damageable.healthMax = def.healthMax;
			damageable.health = data.health;
			damageable.armor = def.armor;
			
			//
			//    MODULES
			//

			SSObjectCreator.AssignModules( gameObject, def, data );

			TacticalGoalController tacticalGoalController = gameObject.AddComponent<TacticalGoalController>();
			if( data.tacticalGoalData != null )
			{
				tacticalGoalController.goal = data.tacticalGoalData.GetInstance();
			}

			//
			//    CONTAINER GAMEOBJECT
			//

			if( data.constructionSaveState == null )
			{
				for( int i = 0; i < renderers.Length; i++ )
				{
					renderers[i].material.SetFloat( "_YOffset", 0.0f );
				}
			}
			// If the building was under construction/repair, make it under c/r.
			else
			{
				// The health is set to 10% in the data passed as a parameter if the construction is fresh.
				ConstructionSite.BeginConstructionOrRepair( building, data.constructionSaveState );
			}
		}
		
		private static GameObject CreateBuilding( Guid guid )
		{
			GameObject container = new GameObject( GAMEOBJECT_NAME );
			container.layer = ObjectLayer.BUILDINGS;
			container.isStatic = true;
			
			//
			//    CONTAINER GAMEOBJECT
			//
			
			BoxCollider collider = container.AddComponent<BoxCollider>();

			Building building = container.AddComponent<Building>();
			building.guid = guid;
			
			NavMeshObstacle navMeshObstacle = container.AddComponent<NavMeshObstacle>();
			navMeshObstacle.carving = true;

			GameObject hudGameObject = Object.Instantiate( (GameObject)AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/building_hud" ), Main.camera.WorldToScreenPoint( container.transform.position ), Quaternion.identity, Main.objectHUDCanvas );
			hudGameObject.SetActive( Main.isHudLocked ); // Only show hud when it's locked.

			building.hud = hudGameObject;

			HUDUnscaled hud = hudGameObject.GetComponent<HUDUnscaled>();

			UnityAction<bool> onHudLockChangeListener = ( bool isLocked ) =>
			{
				if( building.hasBeenHiddenSinceLastDamage )
				{
					return;
				}
				if( isLocked )
				{
					hudGameObject.SetActive( true );
				}
				else
				{
					if( Selection.IsSelected( building ) )
					{
						return;
					}
					if( (object)MouseOverHandler.currentObjectMouseOver == building )
					{
						return;
					}
					hudGameObject.SetActive( false );
				}
			};

			Main.onHudLockChange.AddListener( onHudLockChangeListener );
			
			building.onSelect.AddListener( () =>
			{
				if( Main.isHudLocked ) { return; }
				if( MouseOverHandler.currentObjectMouseOver == container )
				{
					return;
				}
				hudGameObject.SetActive( true );
			} );

			building.onDeselect.AddListener( () =>
			{
				if( Main.isHudLocked ) { return; }
				if( MouseOverHandler.currentObjectMouseOver == container )
				{
					return;
				}
				hudGameObject.SetActive( false );
			} );
			
			// Make the building belong to a faction.
			FactionMember factionMember = container.AddComponent<FactionMember>();
			factionMember.onFactionChange.AddListener( () =>
			{
				Color color = LevelDataManager.factions[factionMember.factionId].color;
				hud.SetColor( color );
			} );
			

			// Make the building damageable.
			Damageable damageable = container.AddComponent<Damageable>();
			
			// When the health is changed, make the building update it's healthbar and redraw the selection panel to show the changed health on it.
			damageable.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				hud.SetHealthBarFill( damageable.healthPercent );
				if( deltaHP < 0 )
				{
					hudGameObject.SetActive( true );
					building.hasBeenHiddenSinceLastDamage = true;
				}
			} );

			// When the building dies:
			// - Destroy the building's UI.
			// - Deselect the building.
			// - Play the death sound.
			damageable.onDeath.AddListener( () =>
			{
				Object.Destroy( hud.gameObject );
				if( Selection.IsSelected( building ) )
				{
					Selection.Deselect( building ); // We have all of the references of this unit here, so we can just simply pass it like this. Amazing, right?
				}
				AudioManager.PlaySound( building.deathSound );
				// Remove the now unused listeners.
				Main.onHudLockChange.RemoveListener( onHudLockChangeListener );
			} );
			
			damageable.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				if( !Selection.IsDisplayed( building ) )
				{
					return;
				}
				Transform healthUI = SelectionPanel.instance.obj.GetElement( "building.health" );
				if( healthUI != null )
				{
					UIUtils.EditText( healthUI.gameObject, (int)damageable.health + "/" + (int)damageable.healthMax );
				}

				// If the health change changed the usability (health is above threshold).
				if( building.IsUsable() )
				{
					// If the building was not usable before the health change.
					SelectionPanel.instance.obj.TryClearElement( "building.unusable_flag" );
				}
			} );

			return container;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
		/// <summary>
		/// Creates a new BuildingData from a GameObject.
		/// </summary>
		/// <param name="building">The GameObject to extract the save state from. Must be a building.</param>
		public static BuildingData GetData( Building building )
		{
			if( building.guid == null )
			{
				throw new Exception( "Guid was not assigned." );
			}

			BuildingData data = new BuildingData();
			data.guid = building.guid;

			data.position = building.transform.position;
			data.rotation = building.transform.rotation;
			
			FactionMember factionMember = building.GetComponent<FactionMember>();
			data.factionId = factionMember.factionId;

			Damageable damageable = building.GetComponent<Damageable>();
			data.health = damageable.health;

			ConstructionSite constructionSite = building.GetComponent<ConstructionSite>();
			if( constructionSite != null )
			{
				data.constructionSaveState = constructionSite.GetSaveState();
			}

			//
			// MODULES
			//

			SSObjectCreator.ExtractModulesToData( building, data );

			data.tacticalGoalData = building.GetComponent<TacticalGoalController>().goal.GetData();

			return data;
		}

		
		
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static GameObject CreateEmpty( Guid guid )
		{
			GameObject gameObject = CreateBuilding( guid );
			
			return gameObject;
		}

		public static GameObject Create( BuildingDefinition def, BuildingData data )
		{
			GameObject gameObject = CreateBuilding( data.guid );

			SetDefData( gameObject, def, data );

			return gameObject;
		}
	}
}