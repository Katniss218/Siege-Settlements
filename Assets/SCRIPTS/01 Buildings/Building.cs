﻿using SS.Data;
using SS.ResourceSystem;
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

			FactionMember factionMember = container.AddComponent<FactionMember>();
			factionMember.onFactionChange.AddListener( () =>
			{
				Color color = FactionManager.factions[factionMember.factionId].color;
				ui.SetFactionColor( color );
				meshRenderer.material.SetColor( "_FactionColor", color );
			} );
			factionMember.factionId = factionId;


			Damageable damageable = container.AddComponent<Damageable>();
			damageable.SetMaxHealth( def.healthMax, false );
			damageable.slashArmor = def.slashArmor;
			damageable.pierceArmor = def.pierceArmor;
			damageable.concussionArmor = def.concussionArmor;
			damageable.onHealthChange.AddListener( () =>
			{
				ui.SetHealthFill( damageable.healthPercent );
			} );
			damageable.onDeath.AddListener( () =>
			{
				Object.Destroy( ui.gameObject );
				SelectionManager.Deselect( selectable ); // We have all of the references of this unit here, so we can just simply pass it like this. Amazing, right?
				AudioManager.PlayNew( def.deathSoundEffect.Item2, 1.0f, 1.0f );
			} );

			if( isUnderConstruction )
			{
				damageable.SetHealthPercent( 0.1f );
				StartConstructionOrRepair( container ); // the condition for completion of construction is 100% health. Repairing is needed once the building's health drops below 50%. Allowed anytime the health is below 100%.
			}
			else
			{
				damageable.Heal();
				meshRenderer.material.SetFloat( "_Progress", 1 );
			}

			container.AddComponent<EveryFrameSingle>().everyFrame = () =>
			{
				ui.transform.position = Main.camera.WorldToScreenPoint( container.transform.position );
			};


			return container;
		}
		
		public static void StartConstructionOrRepair( GameObject building )
		{
			ObjectBase objectBase = building.GetComponent<ObjectBase>();
			Damageable damageable = building.GetComponent<Damageable>();

			// Repairing is mandatory once the building's health drops below 50%. Allowed anytime the health is below 100%.
			if( damageable.health == damageable.healthMax )
			{
				Debug.LogError( "You can't start repairing a building that's full HP." );
			}

			ConstructionSite constructionSite = building.AddComponent<ConstructionSite>();
			constructionSite.AssignResources( DataManager.FindDefinition<BuildingDefinition>( objectBase.id ).cost );
			constructionSite.onConstructionProgress.AddListener( ( ConstructionSite obj, ResourceStack stack ) =>
			{
				damageable.Heal( constructionSite.GetHealthPercentGained( stack.amount ) * damageable.healthMax );
				
				objectBase.meshRenderer.material.SetFloat( "_Progress", damageable.healthPercent );

				Main.particleSystem.transform.position = building.transform.position + new Vector3( 0, 0.2f, 0 );
				ParticleSystem.ShapeModule shape = Main.particleSystem.GetComponent<ParticleSystem>().shape;
				ParticleSystem.MainModule main = Main.particleSystem.GetComponent<ParticleSystem>().main;
				
				shape.shapeType = ParticleSystemShapeType.Box;
				BuildingDefinition def = DataManager.FindDefinition<BuildingDefinition>( objectBase.id );
				shape.scale = new Vector3( def.size.x, 0.4f, def.size.z );
				shape.position = new Vector3( 0, 0.2f, 0 );
				Main.particleSystem.GetComponent<ParticleSystem>().Emit( 36 );
				AudioManager.PlayNew( def.buildSoundEffect.Item2, 0.5f, 1.0f );
			} );
			constructionSite.onConstructionComplete.AddListener( () =>
			{
				objectBase.meshRenderer.material.SetFloat( "_Progress", 1f );
			} );
			constructionSite.isCompleted = () => damageable.healthPercent >= 1f;
			objectBase.meshRenderer.material.SetFloat( "_Progress", damageable.healthPercent );
		}
	}
}