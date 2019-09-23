﻿using Katniss.Utils;
using SS.Content;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Modules;
using SS.UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace SS.Buildings
{
	public class BuildingCreator
	{
		private const string GAMEOBJECT_NAME = "Building";



		public static string GetDefinitionId( GameObject gameObject )
		{
			if( gameObject.layer != ObjectLayer.BUILDINGS )
			{
				throw new System.Exception( "The specified GameObject is not a building." );
			}

			Building building = gameObject.GetComponent<Building>();
			return building.defId;
		}

		/// <summary>
		/// Creates a new BuildingData from a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject to extract the save state from. Must be a building.</param>
		public static BuildingData GetSaveState( GameObject gameObject )
		{
#warning incomplete - modules.
			if( gameObject.layer != ObjectLayer.BUILDINGS )
			{
				throw new System.Exception( "The specified GameObject is not a building." );
			}

			BuildingData saveState = new BuildingData();

			saveState.position = gameObject.transform.position;
			saveState.rotation = gameObject.transform.rotation;
			
			FactionMember factionMember = gameObject.GetComponent<FactionMember>();
			saveState.factionId = factionMember.factionId;

			Damageable damageable = gameObject.GetComponent<Damageable>();
			saveState.health = damageable.health;

			ConstructionSite constructionSite = gameObject.GetComponent<ConstructionSite>();
			if( constructionSite != null )
			{
				saveState.constructionSaveState = constructionSite.GetSaveState();
			}

			return saveState;
		}
		
		public static GameObject Create( BuildingDefinition def, BuildingData data )
		{
			if( def == null )
			{
				throw new System.ArgumentNullException( "Definition can't be null." );
			}
			GameObject container = new GameObject( GAMEOBJECT_NAME + " ('" + def.id + "'), (f: " + data.factionId + ")" );
			container.layer = ObjectLayer.BUILDINGS;
			container.isStatic = true;

			GameObject gfx = new GameObject( GameObjectUtils.GRAPHICS_GAMEOBJECT_NAME );
			gfx.transform.SetParent( container.transform );
			gfx.isStatic = true;

			container.transform.SetPositionAndRotation( data.position, data.rotation );
			

			// Mesh
			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;

			// Material
			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateColoredConstructible( LevelManager.currentLevel.Value.factions[data.factionId].color, def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f, def.mesh.Item2.bounds.size.y, 1.0f );
			
			// Assign the definition to the building, so it can be accessed later.
			Building building = container.AddComponent<Building>();
			building.defId = def.id;
			building.entrance = def.entrance;
			building.placementNodes = def.placementNodes;
			building.StartToEndConstructionCost = def.cost;
			building.buildSoundEffect = def.buildSoundEffect.Item2;

			BoxCollider collider = container.AddComponent<BoxCollider>();
			collider.size = def.size;
			collider.center = new Vector3( 0.0f, def.size.y / 2.0f, 0.0f );

			Selectable selectable = container.AddComponent<Selectable>();
			selectable.icon = def.icon.Item2;

			NavMeshObstacle navMeshObstacle = container.AddComponent<NavMeshObstacle>();
			navMeshObstacle.size = def.size;
			navMeshObstacle.center = new Vector3( 0.0f, def.size.y / 2.0f, 0.0f );
			navMeshObstacle.carving = true;

			GameObject hudGameObject = Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/building_hud" ), Main.camera.WorldToScreenPoint( data.position ), Quaternion.identity, Main.worldUIs );
			hudGameObject.SetActive( Main.isHudLocked ); // Only show hud when it's locked.

			HUDUnscaled hud = hudGameObject.GetComponent<HUDUnscaled>();

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


			// Make the building belong to a faction.
			FactionMember factionMember = container.AddComponent<FactionMember>();
			factionMember.onFactionChange.AddListener( () =>
			{
				Color color = LevelManager.currentLevel.Value.factions[data.factionId].color;
				hud.SetColor( color );
				meshRenderer.material.SetColor( "_FactionColor", color );
			} );
			factionMember.factionId = data.factionId;


			if( def.barracks != null )
			{
				BarracksModule barracks = new BarracksModule();

				barracks.SetSaveState( data.barracksSaveState );
			}

			if( def.research != null )
			{
				ResearchModule research = new ResearchModule();

				research.SetSaveState( data.researchSaveState );
			}

			// Make the building damageable.
			Damageable damageable = container.AddComponent<Damageable>();
			damageable.healthMax = def.healthMax;
			damageable.armor = def.armor;
			// When the health is changed, make the building update it's healthbar and redraw the selection panel to show the changed health on it.
			damageable.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				hud.SetHealthBarFill( damageable.healthPercent );
				Selection.ForceSelectionUIRedraw( selectable );
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
				AudioManager.PlayNew( def.deathSoundEffect.Item2 );
				// Remove the now unused listeners.
				MouseOverHandler.onMouseEnter.RemoveListener( onMouseEnterListener );
				MouseOverHandler.onMouseEnter.RemoveListener( onMouseExitListener );
				Main.onHudLockChange.RemoveListener( onHudLockChangeListener );
			} );
			selectable.onSelectionUIRedraw.AddListener( () =>
			{
				UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), def.displayName );
				UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), (int)damageable.health + "/" + (int)damageable.healthMax );
				if( !Building.IsUsable( damageable ) )
				{
					UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, -50.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "The building is not usable (under construction/repair or <50% health)." );
				}
			} );
			// If the newly spawned building is not marked as being constructed:
			// - Set the health to max.
			if( data.constructionSaveState == null )
			{
				damageable.Heal();
				meshRenderer.material.SetFloat( "_Progress", 1.0f );
			}
			// If the newly spawned building is marked as being constructed:
			// - Set the health to 10% (construction's starting percent).
			// - Start the construction process.
			else
			{
				damageable.healthPercent = Building.STARTING_HEALTH_PERCENT;
				ConstructionSite.BeginConstructionOrRepair( container, data.constructionSaveState );
			}

			// Make the unit update it's UI's position every frame (buildings are static but the camera is not).
			container.AddComponent<EveryFrameSingle>().onUpdate = () =>
			{
				hud.transform.position = Main.camera.WorldToScreenPoint( container.transform.position );
			};


			return container;
		}
	}
}