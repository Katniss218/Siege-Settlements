using SS.Content;
using SS.Modules;
using SS.UI;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Buildings
{
	public class BuildingCreator
	{
		public static GameObject Create( BuildingDefinition def, Vector3 pos, Quaternion rot, int factionId, bool isUnderConstruction = false )
		{
			if( def == null )
			{
				throw new System.ArgumentNullException( "Definition can't be null" );
			}
			GameObject container = new GameObject( "Building (\"" + def.id + "\"), (f: " + factionId + ")" );
			container.isStatic = true;
			container.layer = ObjectLayer.BUILDINGS;

			GameObject gfx = new GameObject( "graphics" );
			gfx.transform.SetParent( container.transform );
			gfx.isStatic = true;

			container.transform.SetPositionAndRotation( pos, rot );

			// Mesh
			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;

			// Material
			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateColoredConstructible( FactionManager.factions[factionId].color, def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f, def.mesh.Item2.bounds.size.y, 1.0f );
			
			// Assign the definition to the building, so it can be accessed later.
			Building building = container.AddComponent<Building>();
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

			HUDUnscaled ui = Object.Instantiate( AssetManager.GetPrefab( AssetManager.RESOURCE_ID + "Prefabs/building_hud" ), Main.camera.WorldToScreenPoint( pos ), Quaternion.identity, Main.worldUIs ).GetComponent<HUDUnscaled>();

			// Make the building belong to a faction.
			FactionMember factionMember = container.AddComponent<FactionMember>();
			factionMember.onFactionChange.AddListener( () =>
			{
				Color color = FactionManager.factions[factionMember.factionId].color;
				ui.SetColor( color );
				meshRenderer.material.SetColor( "_FactionColor", color );
			} );
			factionMember.factionId = factionId;


			if( def.barracks != null )
			{
				BarracksModule.AddTo( container, def.barracks );
			}

			if( def.research != null )
			{
				ResearchModule.AddTo( container, def.research );
			}

			// Make the building damageable.
			Damageable damageable = container.AddComponent<Damageable>();
			damageable.healthMax = def.healthMax;
			damageable.armor = def.armor;
			// When the health is changed, make the building update it's healthbar.
			damageable.onHealthChange.AddListener( ( float deltaHP ) =>
			{

				ui.SetHealthBarFill( damageable.healthPercent );
				SelectionManager.ForceSelectionUIRedraw( selectable );
			} );
			// When the building dies:
			// - Destroy the building's UI.
			// - Deselect the building.
			// - Play the death sound.
			damageable.onDeath.AddListener( () =>
			{
				Object.Destroy( ui.gameObject );
				if( SelectionManager.IsSelected( selectable ) )
				{
					SelectionManager.Deselect( selectable ); // We have all of the references of this unit here, so we can just simply pass it like this. Amazing, right?
				}
				AudioManager.PlayNew( def.deathSoundEffect.Item2, 1.0f, 1.0f );
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
			// If the newly spawned building is marked as being constructed:
			// - Set the health to 10% (construction's starting percent).
			// - Start the construction process.
			if( isUnderConstruction )
			{
				damageable.healthPercent = Building.STARTING_HEALTH_PERCENT;
				ConstructionSite.BeginConstructionOrRepair( container );
			}
			// If the newly spawned building is not marked as being constructed:
			// - Set the health to max.
			else
			{
				damageable.Heal();
				meshRenderer.material.SetFloat( "_Progress", 1.0f );
			}

			// Make the unit update it's UI's position every frame (buildings are static but the camera is not).
			container.AddComponent<EveryFrameSingle>().onUpdate = () =>
			{
				ui.transform.position = Main.camera.WorldToScreenPoint( container.transform.position );
			};


			return container;
		}
	}
}