using UnityEngine;
using UnityEngine.AI;

namespace SS.Buildings
{
	public static class Building
	{
		public static GameObject Create( BuildingDefinition def, Vector3 pos, Quaternion rot, int factionId, bool isUnderConstruction = false )
		{
			if( def == null )
			{
				throw new System.Exception( "Definition can't be null" );
			}
			GameObject container = new GameObject( "Building (\"" + def.id + "\"), (f: " + factionId + ")" );
			container.isStatic = true;

			GameObject gfx = new GameObject( "graphics" );
			gfx.transform.SetParent( container.transform );
			gfx.isStatic = true;

			container.transform.SetPositionAndRotation( pos, rot );

			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			// Apply the mesh.
			meshFilter.mesh = def.mesh.Item2;

			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			// Apply the material's properties.
			meshRenderer.sharedMaterial = Main.materialFactionColoredConstructible;
			meshRenderer.material.SetTexture( "_Albedo", def.albedo.Item2 );
			meshRenderer.material.SetFloat( "_Height", def.mesh.Item2.bounds.size.y );

			meshRenderer.material.SetTexture( "_Normal", def.normal.Item2 );
			meshRenderer.material.SetTexture( "_Emission", null );
			meshRenderer.material.SetFloat( "_Metallic", 0.0f );
			meshRenderer.material.SetFloat( "_Smoothness", 0.5f );


			BoxCollider collider = container.AddComponent<BoxCollider>();
			collider.size = def.size;
			collider.center = new Vector3( 0f, def.size.y / 2f, 0f );

			Selectable selectable = container.AddComponent<Selectable>();

			NavMeshObstacle navMeshObstacle = container.AddComponent<NavMeshObstacle>();
			navMeshObstacle.size = def.size;
			navMeshObstacle.center = new Vector3( 0f, def.size.y / 2f, 0f );
			navMeshObstacle.carving = true;

			BuildingUI ui = Object.Instantiate( Main.buildingUI, Main.camera.WorldToScreenPoint( pos ), Quaternion.identity, Main.worldUIs ).GetComponent<BuildingUI>();

			FactionMember factionMember = container.AddComponent<FactionMember>();
			factionMember.onFactionChange.AddListener( ( FactionMember obj ) =>
			{
				Color color = FactionManager.factions[obj.factionId].color;
				ui.SetFactionColor( color );
				meshRenderer.material.SetColor( "_FactionColor", color );
			} );
			factionMember.factionId = factionId;


			Damageable damageable = container.AddComponent<Damageable>();
			damageable.healthMax = def.healthMax;
			damageable.Heal();
			damageable.slashArmor = def.slashArmor;
			damageable.pierceArmor = def.pierceArmor;
			damageable.concussionArmor = def.concussionArmor;
			damageable.onHealthChange.AddListener( ( Damageable obj ) =>
			{
				ui.SetHealthFill( obj.healthPercent );
			} );
			damageable.onDeath.AddListener( ( Damageable obj ) =>
			{
				Object.Destroy( ui.gameObject );
				// for breakup make several meshes that are made up of the original one, attach physics to them.
				// let the physics play for a few seconds (randomize durations for each piece), then disable rigidbodies, and pull them downwards, reducing their scale at the same time.
				// when the scale reaches 0.x, remove the piece.

				// also, play a poof from some particle system for smoke or something at the moment of death.
				SelectionManager.Deselect( selectable ); // We have all of the references of this unit here, so we can just simply pass it like this. Amazing, right?
			} );

			if( isUnderConstruction )
			{
				ConstructionSite constructionSite = container.AddComponent<ConstructionSite>();
				constructionSite.AssignResources( def.cost );
			}
			else
			{
				meshRenderer.material.SetFloat( "_Progress", 1 );
			}

			container.AddComponent<EveryFrameSingle>().everyFrame = () =>
			{
				if( container.transform.hasChanged )
				{
					ui.transform.position = Main.camera.WorldToScreenPoint( container.transform.position );
				}
			};


			return container;
		}
	}
}