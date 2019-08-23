using SS.Data;
using SS.Units;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Buildings
{
	public static class Building
	{
		// The amount of health that the building marked as being constructed is going to start with.
		private static float STARTING_HEALTH_PERCENT = 0.1f;
		// If a building drops below this value, it can't be used, and needs to be repaired.
		private static float USABILITY_THRESHOLD = 0.5f;


		public static bool CheckUsable( Damageable building )
		{
			// Building's can't be used when they are being constructed/repaired or in the state of not-usable (<USABILITY_THRESHOLD% HP).
			if( building.GetComponent<ConstructionSite>() != null )
			{
				return false;
			}
			return building.healthPercent >= 0.5f;
		}


		public static GameObject Create( BuildingDefinition def, Vector3 pos, Quaternion rot, int factionId, bool isUnderConstruction = false )
		{
			if( def == null )
			{
				throw new System.Exception( "Definition can't be null" );
			}
			GameObject container = new GameObject( "Building (\"" + def.id + "\"), (f: " + factionId + ")" );
			container.isStatic = true;
			container.layer = LayerMask.NameToLayer( "Buildings" );

			GameObject gfx = new GameObject( "graphics" );
			gfx.transform.SetParent( container.transform );
			gfx.isStatic = true;

			container.transform.SetPositionAndRotation( pos, rot );

			// Mesh
			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;

			// Material
			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = Main.materialFactionColoredConstructible;
			meshRenderer.material.SetTexture( "_Albedo", def.albedo.Item2 );
			meshRenderer.material.SetFloat( "_Height", def.mesh.Item2.bounds.size.y );

			meshRenderer.material.SetTexture( "_Normal", def.normal.Item2 );
			meshRenderer.material.SetTexture( "_Emission", null );
			meshRenderer.material.SetFloat( "_Metallic", 0.0f );
			meshRenderer.material.SetFloat( "_Smoothness", 0.5f );

			// Assign the definition to the building, so it can be accessed later.
			ObjectBase objectBase = container.AddComponent<ObjectBase>();
			objectBase.id = def.id;

			BoxCollider collider = container.AddComponent<BoxCollider>();
			collider.size = def.size;
			collider.center = new Vector3( 0.0f, def.size.y / 2.0f, 0.0f );

			Selectable selectable = container.AddComponent<Selectable>();
			selectable.icon = def.icon.Item2;

			NavMeshObstacle navMeshObstacle = container.AddComponent<NavMeshObstacle>();
			navMeshObstacle.size = def.size;
			navMeshObstacle.center = new Vector3( 0.0f, def.size.y / 2.0f, 0.0f );
			navMeshObstacle.carving = true;

			BuildingUI ui = Object.Instantiate( Main.buildingUI, Main.camera.WorldToScreenPoint( pos ), Quaternion.identity, Main.worldUIs ).GetComponent<BuildingUI>();

			// Make the building belong to a faction.
			FactionMember factionMember = container.AddComponent<FactionMember>();
			factionMember.onFactionChange.AddListener( () =>
			{
				Color color = FactionManager.factions[factionMember.factionId].color;
				ui.SetFactionColor( color );
				meshRenderer.material.SetColor( "_FactionColor", color );
			} );
			factionMember.factionId = factionId;

			if( def.isBarracks )
			{
				BarracksModule barracks = container.AddComponent<BarracksModule>();
				barracks.spawnableUnits = new UnitDefinition[def.barracksUnits.Length];// DataManager.GetAllOfType<UnitDefinition>();
				for( int i = 0; i < barracks.spawnableUnits.Length; i++ )
				{
					barracks.spawnableUnits[i] = DataManager.Get<UnitDefinition>( def.barracksUnits[i] );
				}
			}
			if( def.isResearch )
			{
				ResearchModule research = container.AddComponent<ResearchModule>();
			}

			// Make the building damageable.
			Damageable damageable = container.AddComponent<Damageable>();
			damageable.SetMaxHealth( def.healthMax, false );
			damageable.slashArmor = def.slashArmor;
			damageable.pierceArmor = def.pierceArmor;
			damageable.concussionArmor = def.concussionArmor;
			// When the health is changed, make the building update it's healthbar.
			damageable.onHealthChange.AddListener( () =>
			{
				ui.SetHealthFill( damageable.healthPercent );
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

			// If the newly spawned building is marked as being constructed:
			// - Set the health to 10% (construction's starting percent).
			// - Start the construction process.
			if( isUnderConstruction )
			{
				damageable.SetHealthPercent( Building.STARTING_HEALTH_PERCENT );
				ConstructionSite.StartConstructionOrRepair( container ); // the condition for completion of construction is 100% health. Repairing is needed once the building's health drops below 50%. Allowed anytime the health is below 100%.
			}
			// If the newly spawned building is not marked as being constructed:
			// - Set the health to max.
			else
			{
				damageable.Heal();
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