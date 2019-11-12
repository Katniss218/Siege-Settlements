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

namespace SS.Buildings
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

			// Set the building's selected icon.
			Selectable selectable = gameObject.GetComponent<Selectable>();
			selectable.icon = def.icon;
			
			// Set the building's native parameters.
			Building building = gameObject.GetComponent<Building>();
			building.defId = def.id;
			building.entrance = def.entrance;
			building.placementNodes = def.placementNodes;
			building.StartToEndConstructionCost = def.cost;
			building.buildSoundEffect = def.buildSoundEffect;
			building.displayName = def.displayName;
			building.deathSound = def.deathSoundEffect;
			
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

			damageable.healthMax = def.healthMax;
			damageable.health = data.health;
			damageable.armor = def.armor;


			//
			//    MODULES
			//

			SSObjectCreator.AssignModules( gameObject, def, data );


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
				ConstructionSite.BeginConstructionOrRepair( gameObject, data.constructionSaveState );
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

			// Make the building selectable.
			Selectable selectable = container.AddComponent<Selectable>();

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
					if( building.hasBeenHiddenSinceLastDamage )
					{
						return;
					}
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
				if( Selection.IsSelected( selectable ) )
				{
					Selection.Deselect( selectable ); // We have all of the references of this unit here, so we can just simply pass it like this. Amazing, right?
				}
				AudioManager.PlaySound( building.deathSound );
				// Remove the now unused listeners.
				MouseOverHandler.onMouseEnter.RemoveListener( onMouseEnterListener );
				MouseOverHandler.onMouseEnter.RemoveListener( onMouseExitListener );
				Main.onHudLockChange.RemoveListener( onHudLockChangeListener );
			} );
			
			selectable.onHighlight.AddListener( () =>
			{
				GameObject nameUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), building.displayName );
				SelectionPanel.instance.obj.RegisterElement( "building.display_name", nameUI.transform );

				GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), (int)damageable.health + "/" + (int)damageable.healthMax );
				SelectionPanel.instance.obj.RegisterElement( "building.health", healthUI.transform );

				if( !Building.IsUsable( damageable ) )
				{
					GameObject unusableFlagUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -50.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "The building is not usable (under construction/repair or <50% health)." );
					SelectionPanel.instance.obj.RegisterElement( "building.unusable_flag", unusableFlagUI.transform );
				}
			} );

			damageable.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				if( !Selection.IsHighlighted( selectable ) )
				{
					return;
				}
				Transform healthUI = SelectionPanel.instance.obj.GetElement( "building.health" );
				if( healthUI != null )
				{
					UIUtils.EditText( healthUI.gameObject, (int)damageable.health + "/" + (int)damageable.healthMax );
				}

				// if it is now usable, but was unusable before - remove the unusable flag.
				if( Building.IsUsable( damageable ) )
				{
					if( SelectionPanel.instance.obj.GetElement( "building.unusable_flag" ) != null )
					{
						SelectionPanel.instance.obj.Clear( "building.unusable_flag" );
					}
				}
			} );


			// Make the unit update it's UI's position every frame (buildings are static but the camera is not).
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
			if( !Building.IsValid( gameObject ) )
			{
				throw new Exception( "GameObject '" + gameObject.name + "' is not a valid building." );
			}

			Building building = gameObject.GetComponent<Building>();
			return building.defId;
		}

		/// <summary>
		/// Creates a new BuildingData from a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject to extract the save state from. Must be a building.</param>
		public static BuildingData GetData( GameObject gameObject )
		{
			if( !Building.IsValid( gameObject ) )
			{
				throw new Exception( "GameObject '" + gameObject.name + "' is not a valid building." );
			}

			BuildingData data = new BuildingData();
			Building building = gameObject.GetComponent<Building>();
			if( building.guid == null )
			{
				throw new Exception( "Guid was not assigned." );
			}
			data.guid = building.guid.Value;

			data.position = gameObject.transform.position;
			data.rotation = gameObject.transform.rotation;
			
			FactionMember factionMember = gameObject.GetComponent<FactionMember>();
			data.factionId = factionMember.factionId;

			Damageable damageable = gameObject.GetComponent<Damageable>();
			data.health = damageable.health;

			ConstructionSite constructionSite = gameObject.GetComponent<ConstructionSite>();
			if( constructionSite != null )
			{
				data.constructionSaveState = constructionSite.GetSaveState();
			}

			//
			// MODULES
			//

			SSObjectCreator.ExtractModules( gameObject, data );


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