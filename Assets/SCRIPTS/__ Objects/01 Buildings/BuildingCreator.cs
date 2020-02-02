using SS.Content;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Objects.Modules;
using SS.Objects.SubObjects;
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

		public static void SetData( Building building, BuildingData data )
		{
			//
			//    CONTAINER GAMEOBJECT
			//
			building.transform.SetPositionAndRotation( data.position, data.rotation );

			// Set the building's native parameters.
			if( building.guid != data.guid )
			{
				throw new Exception( "Mismatched guid." );
			}
			building.health = data.health;
			building.factionId = data.factionId;

			//
			//    MODULES
			//

			SSObjectCreator.AssignModuleData( building, data );
			
			if( data.tacticalGoalData != null )
			{
				building.controller.SetGoalData( data.tacticalGoalData, data.tacticalGoalTag );
			}

			//
			//    CONTAINER GAMEOBJECT
			//

			if( data.constructionSaveState == null )
			{

				building.hud.SetHealthBarFill( building.healthPercent );

				MeshSubObject[] meshes = building.GetSubObjects<MeshSubObject>();
				for( int i = 0; i < meshes.Length; i++ )
				{
					meshes[i].GetMaterial().SetFloat( "_YOffset", 0.0f );
				}
				MeshPredicatedSubObject[] meshes2 = building.GetSubObjects<MeshPredicatedSubObject>();
				for( int i = 0; i < meshes2.Length; i++ )
				{
					meshes2[i].GetMaterial().SetFloat( "_YOffset", 0.0f );
				}
			}
			// If the building was under construction/repair, make it under c/r.
			else
			{
				// The health is set to 10% in the data passed as a parameter if the construction is fresh.
				ConstructionSite.BeginConstructionOrRepair( building, data.constructionSaveState );
			}
		}

		private static Building CreateBuilding( BuildingDefinition def, Guid guid )
		{
			GameObject gameObject = new GameObject( GAMEOBJECT_NAME + " - '" + def.id + "'" );
			gameObject.layer = ObjectLayer.BUILDINGS;
			gameObject.isStatic = true;

			//
			//    CONTAINER GAMEOBJECT
			//

			//GameObject hudGameObject = Object.Instantiate( (GameObject)AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/building_hud" ), Main.camera.WorldToScreenPoint( gameObject.transform.position ), Quaternion.identity, Main.objectHUDCanvas );

			//HUD hud = hudGameObject.GetComponent<HUD>();
			//hud.isVisible = Main.isHudForcedVisible;


			BoxCollider collider = gameObject.AddComponent<BoxCollider>();

			NavMeshObstacle navMeshObstacle = gameObject.AddComponent<NavMeshObstacle>();
			navMeshObstacle.carving = true;

			Building building = gameObject.AddComponent<Building>();
			//building.hud = hud;
			//hud.hudHolder = building;
			building.guid = guid;
			building.definitionId = def.id;
			building.displayName = def.displayName;
			building.icon = def.icon;
			building.placementNodes = def.placementNodes;
			building.StartToEndConstructionCost = def.cost;
			building.buildSoundEffect = def.buildSoundEffect;
			building.hurtSound = def.hurtSoundEffect;
			building.deathSound = def.deathSoundEffect;
			building.size = def.size;

			building.viewRange = def.viewRange;
			building.healthMax = def.healthMax;
			building.health = def.healthMax;
			building.armor = def.armor;

			building.onFactionChange.AddListener( ( int fromFac, int toFac ) =>
			{
				Color color = LevelDataManager.factions[building.factionId].color;

				building.hud.SetColor( color );

				MeshSubObject[] meshes = building.GetSubObjects<MeshSubObject>();
				for( int i = 0; i < meshes.Length; i++ )
				{
					meshes[i].GetMaterial().SetColor( "_FactionColor", color );
				}
				MeshPredicatedSubObject[] meshes2 = building.GetSubObjects<MeshPredicatedSubObject>();
				for( int i = 0; i < meshes2.Length; i++ )
				{
					meshes2[i].GetMaterial().SetColor( "_FactionColor", color );
				}

				// Re-Display the object

				if( Selection.IsDisplayed( building ) )
				{
					SSObjectHelper.ReDisplayDisplayed();
				}
			} );

			building.onHealthPercentChanged.AddListener( () =>
			{
				building.hud.SetHealthBarFill( building.healthPercent );
								
				MeshSubObject[] meshes = building.GetSubObjects<MeshSubObject>();
				for( int i = 0; i < meshes.Length; i++ )
				{
					meshes[i].GetMaterial().SetFloat( "_Dest", 1 - building.healthPercent );
				}
				MeshPredicatedSubObject[] meshes2 = building.GetSubObjects<MeshPredicatedSubObject>();
				for( int i = 0; i < meshes2.Length; i++ )
				{
					meshes2[i].GetMaterial().SetFloat( "_Dest", 1 - building.healthPercent );
				}
			} );




			UnityAction<bool> onHudLockChangeListener = ( bool isLocked ) =>
			{
				if( building.hud.isDisplayedDueToDamage )
				{
					return;
				}
				if( isLocked )
				{
					building.hud.hudContainer.isVisible = true;
				}
				else
				{
					if( Selection.IsSelected( building ) )
					{
						return;
					}
					if( (object)MouseOverHandler.currentObjectMousedOver == building )
					{
						return;
					}
					building.hud.hudContainer.isVisible = false;
				}
			};

			Main.onHudLockChange.AddListener( onHudLockChangeListener );

			building.onSelect.AddListener( () =>
			{
				if( Main.isHudForcedVisible ) { return; }
				if( MouseOverHandler.currentObjectMousedOver == gameObject )
				{
					return;
				}
				building.hud.hudContainer.isVisible = true;
			} );

			building.onDeselect.AddListener( () =>
			{
				if( Main.isHudForcedVisible ) { return; }
				if( MouseOverHandler.currentObjectMousedOver == gameObject )
				{
					return;
				}
				building.hud.hudContainer.isVisible = false;
			} );

			// When the health is changed, make the building update it's healthbar and redraw the selection panel to show the changed health on it.
			building.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				if( deltaHP < 0 )
				{
					building.hud.hudContainer.isVisible = true;
					building.hud.isDisplayedDueToDamage = true;

					if( building.healthPercent < Building.UNUSABLE_THRESHOLD ) // if the health dropped below the point of usability.
					{
						building.isUsable = false;
					}
				}


				if( !Selection.IsDisplayed( building ) )
				{
					return;
				}
				// If the health changes & repair hasn't already started - update the repair button.
				if( Building.CanStartRepair( building ) )
				{
					// clear the previous button (if any) & display new, possibly updated to be red or green.
					ActionPanel.instance.Clear( "building.ap.repair" );
					building.DisplayRepairButton();
				}
				Transform healthUI = SelectionPanel.instance.obj.GetElement( "building.health" );
				if( healthUI != null )
				{
					UIUtils.EditText( healthUI.gameObject, SSObjectDFSC.GetHealthString( building.health, building.healthMax ) );
				}

				// If the health change changed the usability (health is above threshold).
				if( building.isUsable )
				{
					// If the building was not usable before the health change.
					SelectionPanel.instance.obj.TryClearElement( "building.unusable_flag" );
				}
			} );

			building.onUsableStateChanged.AddListener( () =>
			{
				building.hud.SetUsable( building.isUsable );
			} );

			// When the building dies:
			// - Destroy the building's UI.
			// - Deselect the building.
			// - Play the death sound.
			building.onDeath.AddListener( () =>
			{
				Object.Destroy( building.hud.gameObject );
				if( Selection.IsSelected( building ) )
				{
					Selection.Deselect( building ); // We have all of the references of this unit here, so we can just simply pass it like this. Amazing, right?
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

			SSObjectCreator.AssignModules( building, def );

#warning todo
			/*InventoryModule[] inventory = building.GetModules<InventoryModule>();
			if( inventory.Length == 0 )
			{
				building.hud.GetComponent<HUDInventory>()?.Destroy();
			}
			InteriorModule[] interior = building.GetModules<InteriorModule>();
			if( interior.Length == 0 )
			{
				building.hud.GetComponent<HUDInterior>()?.Destroy();
			}*/

			return building;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		

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

			data.factionId = building.factionId;

			data.health = building.health;

			ConstructionSite constructionSite = building.GetComponent<ConstructionSite>();
			if( constructionSite != null )
			{
				data.constructionSaveState = constructionSite.GetSaveState();
			}

			//
			// MODULES
			//

			SSObjectCreator.ExtractModulesToData( building, data );

			data.tacticalGoalData = building.controller.GetGoalData();

			return data;
		}



		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static Building Create( BuildingDefinition def, Guid guid )
		{
			return CreateBuilding( def, guid );
		}
	}
}