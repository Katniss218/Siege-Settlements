using Katniss.Utils;
using SS.Content;
using SS.Diplomacy;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Modules;
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

		private static void SetBuildingDefinition( GameObject gameObject, BuildingDefinition def )
		{
			
			//
			//    GRAPHICS GAMEOBJECT
			//

			GameObject gfx = gameObject.transform.Find( GameObjectUtils.GRAPHICS_GAMEOBJECT_NAME ).gameObject;


			// Set the building's mesh and material.
			MeshFilter meshFilter = gfx.GetComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;
			
			MeshRenderer meshRenderer = gfx.GetComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateColoredConstructible( FactionDefinition.DefaultColor, def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f, def.mesh.Item2.bounds.size.y, 1.0f );


			//
			//    CONTAINER GAMEOBJECT
			//

			// Set the building's size.
			BoxCollider collider = gameObject.GetComponent<BoxCollider>();
			collider.size = def.size;
			collider.center = new Vector3( 0.0f, def.size.y / 2.0f, 0.0f );

			NavMeshObstacle navMeshObstacle = gameObject.GetComponent<NavMeshObstacle>();
			navMeshObstacle.size = def.size;
			navMeshObstacle.center = new Vector3( 0.0f, def.size.y / 2.0f, 0.0f );

			// Set the building's selected icon.
			Selectable selectable = gameObject.GetComponent<Selectable>();
			selectable.icon = def.icon.Item2;
			
			// Set the building's native parameters.
			Building building = gameObject.GetComponent<Building>();
			building.defId = def.id;
			building.entrance = def.entrance;
			building.placementNodes = def.placementNodes;
			building.StartToEndConstructionCost = def.cost;
			building.buildSoundEffect = def.buildSoundEffect.Item2;
			building.displayName = def.displayName;
			building.deathSound = def.deathSoundEffect.Item2;

			// Set the building's health.
			Damageable damageable = gameObject.GetComponent<Damageable>();
			damageable.healthMax = def.healthMax;
			damageable.armor = def.armor;


			//
			//    MODULES
			//

			// Remove old melee module (if present).
			BarracksModule barracks = gameObject.GetComponent<BarracksModule>();
			if( barracks != null )
			{
				Object.Destroy( barracks );
			}
			// If the new unit is melee, setup the melee module.
			if( def.barracks != null )
			{
				barracks = gameObject.AddComponent<BarracksModule>();
				barracks.SetDefinition( def.barracks );
			}

			// Remove old ranged module (if present).
			ResearchModule research = gameObject.GetComponent<ResearchModule>();
			if( research != null )
			{
				Object.Destroy( research );
			}
			// If the new unit is ranged, setup the ranged module.
			if( def.research != null )
			{
				research = gameObject.AddComponent<ResearchModule>();
				research.SetDefinition( def.research );
			}
		}

		private static void SetBuildingData( GameObject gameObject, BuildingData data )
		{
			
			//
			//    GRAPHICS GAMEOBJECT
			//

			GameObject gfx = gameObject.transform.Find( GameObjectUtils.GRAPHICS_GAMEOBJECT_NAME ).gameObject;
			
			MeshRenderer meshRenderer = gfx.GetComponent<MeshRenderer>();
			

			//
			//    CONTAINER GAMEOBJECT
			//

			gameObject.transform.SetPositionAndRotation( data.position, data.rotation );

			
			// Assign the definition to the building, so it can be accessed later.
			Building building = gameObject.GetComponent<Building>();
			building.guid = data.guid;
			
			// Make the building belong to a faction.
			FactionMember factionMember = gameObject.GetComponent<FactionMember>();
			factionMember.factionId = data.factionId;

			// Make the building damageable.
			Damageable damageable = gameObject.GetComponent<Damageable>();
			damageable.health = data.health;

			if( data.constructionSaveState == null )
			{
				meshRenderer.material.SetFloat( "_Progress", 1.0f );
			}
			// If the building was under construction/repair, make it under c/r.
			else
			{
				// The health is set to 10% in the data passed as a parameter if the construction is fresh.
				ConstructionSite.BeginConstructionOrRepair( gameObject, data.constructionSaveState );
			}


			//
			//    MODULES
			//

			if( data.barracksSaveState != null )
			{
				BarracksModule barracks = gameObject.GetComponent<BarracksModule>();
				barracks.SetSaveState( data.barracksSaveState );
			}

			if( data.researchSaveState != null )
			{
				ResearchModule research = gameObject.GetComponent<ResearchModule>();
				research.SetSaveState( data.researchSaveState );
			}
		}

		private static GameObject CreateBuilding()
		{
			GameObject container = new GameObject( GAMEOBJECT_NAME );
			container.layer = ObjectLayer.BUILDINGS;
			container.isStatic = true;


			//
			//    GRAPHICS GAMEOBJECT
			//

			GameObject gfx = new GameObject( GameObjectUtils.GRAPHICS_GAMEOBJECT_NAME );
			gfx.transform.SetParent( container.transform );
			gfx.isStatic = true;

			
			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			
			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			

			//
			//    CONTAINER GAMEOBJECT
			//
			
			BoxCollider collider = container.AddComponent<BoxCollider>();

			Building building = container.AddComponent<Building>();

			// Make the building selectable.
			Selectable selectable = container.AddComponent<Selectable>();

			NavMeshObstacle navMeshObstacle = container.AddComponent<NavMeshObstacle>();
			navMeshObstacle.carving = true;

			GameObject hudGameObject = Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/building_hud" ), Main.camera.WorldToScreenPoint( container.transform.position ), Quaternion.identity, Main.worldUIs );
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
				Color color = LevelDataManager.factions[factionMember.factionId].color;
				hud.SetColor( color );
				meshRenderer.material.SetColor( "_FactionColor", color );
			} );
			

			// Make the building damageable.
			Damageable damageable = container.AddComponent<Damageable>();
			
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
				AudioManager.PlaySound( building.deathSound );
				// Remove the now unused listeners.
				MouseOverHandler.onMouseEnter.RemoveListener( onMouseEnterListener );
				MouseOverHandler.onMouseEnter.RemoveListener( onMouseExitListener );
				Main.onHudLockChange.RemoveListener( onHudLockChangeListener );
			} );

			// Make the building show it's parameters on the Selection Panel, when highlighted.
			selectable.onSelectionUIRedraw.AddListener( () =>
			{
				UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), building.displayName );
				UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), (int)damageable.health + "/" + (int)damageable.healthMax );
				if( !Building.IsUsable( damageable ) )
				{
					UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, -50.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "The building is not usable (under construction/repair or <50% health)." );
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

			data.guid = gameObject.GetComponent<Building>().guid;

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
			//    MODULES
			//

			BarracksModule barracks = gameObject.GetComponent<BarracksModule>();
			if( barracks != null )
			{
				data.barracksSaveState = barracks.GetSaveState();
			}

			ResearchModule research = gameObject.GetComponent<ResearchModule>();
			if( research != null )
			{
				data.researchSaveState = research.GetSaveState();
			}

			return data;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		


		public static void SetData( GameObject gameObject, BuildingData data )
		{
			if( !Building.IsValid( gameObject ) )
			{
				throw new Exception( "GameObject '" + gameObject.name + "' is not a valid building." );
			}
			SetBuildingData( gameObject, data );
		}


		
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static GameObject CreateEmpty( Guid guid, BuildingDefinition def )
		{
			GameObject gameObject = CreateBuilding();

			SetBuildingDefinition( gameObject, def );

			Building building = gameObject.GetComponent<Building>();
			building.guid = guid;

			return gameObject;
		}

		public static GameObject Create( BuildingDefinition def, BuildingData data )
		{
			GameObject gameObject = CreateBuilding();

			SetBuildingDefinition( gameObject, def );
			SetBuildingData( gameObject, data );

			return gameObject;
		}
	}
}