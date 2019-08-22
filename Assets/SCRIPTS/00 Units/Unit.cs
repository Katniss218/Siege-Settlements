using SS.Buildings;
using SS.Data;
using SS.Projectiles;
using SS.UI;
using SS.UI.Elements;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Units
{
	public static class Unit
	{
		public static GameObject Create( UnitDefinition def, Vector3 pos, Quaternion rot, int factionId )
		{
			if( def == null )
			{
				throw new System.Exception( "Definition can't be null" );
			}
			GameObject container = new GameObject( "Unit (\"" + def.id + "\"), (f: " + factionId + ")" );
			container.layer = LayerMask.NameToLayer( "Units" );

			GameObject gfx = new GameObject( "graphics" );
			gfx.transform.SetParent( container.transform );

			container.transform.SetPositionAndRotation( pos, rot );

			// Add a mesh to the unit.
			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;

			// Add a material to the unit.
			MeshRenderer meshRenderer  = gfx.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = Main.materialFactionColoredDestroyable;
			// Set the material's properties to the appropriate values.
			meshRenderer.material.SetColor( "_FactionColor", FactionManager.factions[factionId].color );
			meshRenderer.material.SetTexture( "_Albedo", def.albedo.Item2 );
			meshRenderer.material.SetTexture( "_Normal", def.normal.Item2 );
			meshRenderer.material.SetTexture( "_Emission", null );
			meshRenderer.material.SetFloat( "_Metallic", 0.0f );
			meshRenderer.material.SetFloat( "_Smoothness", 0.5f );

			// Assign the definition to the unit, so it can be accessed later.
			ObjectBase objectBase = container.AddComponent<ObjectBase>();
			objectBase.id = def.id;

			BoxCollider collider = container.AddComponent<BoxCollider>();
			collider.size = new Vector3( def.radius * 2.0f, def.height, def.radius * 2.0f );
			collider.center = new Vector3( 0.0f, def.height / 2.0f, 0.0f );

			// Mask the unit as selectable.
			Selectable selectable = container.AddComponent<Selectable>();
			selectable.icon = def.icon.Item2;
			// If the unit's type is civilian, make it show the build menu, when highlighted.
			if( def.id == "unit.civilian" )
			{
				selectable.onHighlight.AddListener( CivilianOnSelect );
			}
			
			// Add a kinematic rigidbody to the unit (required by the NavMeshAgent).
			Rigidbody rigidbody = container.AddComponent<Rigidbody>();
			rigidbody.isKinematic = true;
			
			// Add the NavMeshAgent to the unit, to make it movable.
			NavMeshAgent navMeshAgent = container.AddComponent<NavMeshAgent>();
			navMeshAgent.acceleration = 8.0f;
			navMeshAgent.stoppingDistance = 0.125f;
			navMeshAgent.radius = def.radius;
			navMeshAgent.height = def.height;
			navMeshAgent.speed = def.movementSpeed;
			navMeshAgent.angularSpeed = def.rotationSpeed;

			UnitUI ui = Object.Instantiate( Main.unitUI, Main.camera.WorldToScreenPoint( pos ), Quaternion.identity, Main.worldUIs ).GetComponent<UnitUI>();

			// Make the unit belong to a faction.
			FactionMember factionMember = container.AddComponent<FactionMember>();
			factionMember.onFactionChange.AddListener( () =>
			{
				Color color = FactionManager.factions[factionMember.factionId].color;
				ui.SetFactionColor( color );
				meshRenderer.material.SetColor( "_FactionColor", color );
			} );
			// We set the faction after assigning the listener, to automatically set the color to the appropriate value.
			factionMember.factionId = factionId;

			// Make the unit damageable.
			Damageable damageable = container.AddComponent<Damageable>();
			damageable.onHealthChange.AddListener( () =>
			{
				meshRenderer.material.SetFloat( "_Dest", 1 - damageable.healthPercent );
				ui.SetHealthFill( damageable.healthPercent );
			} );
			// Make the unit deselect itself, and destroy it's UI when killed.
			damageable.onDeath.AddListener( () =>
			{
				Object.Destroy( ui.gameObject );
				// for breakup make several meshes that are made up of the original one, attach physics to them.
				// let the physics play for a few seconds (randomize durations for each piece), then disable rigidbodies, and pull them downwards, reducing their scale at the same time.
				// when the scale reaches 0.x, remove the piece.

				// also, play a poof from some particle system for smoke or something at the moment of death.
				SelectionManager.Deselect( selectable ); // We have all of the references of this unit here, so we can just simply pass it like this. Amazing, right?

			} );
			damageable.SetMaxHealth( def.healthMax, true );
			damageable.slashArmor = def.slashArmor;
			damageable.pierceArmor = def.pierceArmor;
			damageable.concussionArmor = def.concussionArmor;

			InventoryModule inventory = container.AddComponent<InventoryModule>();
			inventory.maxCapacity = 45;
			inventory.onPickup.AddListener( () =>
			{
				Debug.Log( "Picked up something" );
			} );
			inventory.onDropOff.AddListener( () =>
			{
				Debug.Log( "Dropped off something" );
			} );
			if( factionId == 0 ) // If player, update the resource panel.
			{
				inventory.onPickup.AddListener( () =>
				{
					Main.resourcePanel.UpdateResourceEntry( inventory.resource.id, inventory.resource.amount );
				} );
				inventory.onDropOff.AddListener( () =>
				{
					Main.resourcePanel.UpdateResourceEntry( inventory.resource.id, inventory.resource.amount );
				} );
			}
			
			// If the unit has the capability to fight, add a target finder to it.
			ITargetFinder finder = null;
			if( def.isMelee || def.isRanged )
			{
				finder = container.AddComponent<TargetFinderModule>();
			}

			// If the new unit is melee, setup the melee module.
			if( def.isMelee )
			{
				DamageSource meleeDamageSource = container.AddComponent<DamageSource>();
				meleeDamageSource.damageType = def.meleeDamageType;
				meleeDamageSource.damage = def.meleeDamage;
				meleeDamageSource.armorPenetration = def.meleeArmorPenetration;
				
				MeleeModule melee = container.AddComponent<MeleeModule>();
				melee.damageSource = meleeDamageSource;
				melee.targetFinder = finder;
				melee.attackCooldown = def.meleeAttackCooldown;
				melee.attackRange = def.meleeAttackRange;
				melee.attackSoundEffect = def.meleeAttackSoundEffect.Item2;
			}

			// If the new unit is ranged, setup the ranged module.
			if( def.isRanged )
			{
				DamageSource rangedDamageSource = container.AddComponent<DamageSource>();
				rangedDamageSource.damageType = def.rangedDamageType;
				rangedDamageSource.damage = def.rangedDamage;
				rangedDamageSource.armorPenetration = def.rangedArmorPenetration;

				RangedModule ranged = container.AddComponent<RangedModule>();
				ranged.projectile = DataManager.Get<ProjectileDefinition>( def.rangedProjectileId );
				ranged.projectileCount = def.rangedProjectileCount;
				ranged.damageSource = rangedDamageSource;
				ranged.targetFinder = finder;
				ranged.attackRange = def.rangedAttackRange;
				ranged.attackCooldown = def.rangedAttackCooldown;
				ranged.velocity = def.rangedVelocity;
				ranged.localOffsetMin = def.rangedLocalOffsetMin;
				ranged.localOffsetMax = def.rangedLocalOffsetMax;
				ranged.attackSoundEffect = def.rangedAttackSoundEffect.Item2;
			}

			// Make the unit update it's UI's position every frame.
			container.AddComponent<EveryFrameSingle>().onUpdate = () =>
			{
				ui.transform.position = Main.camera.WorldToScreenPoint( container.transform.position );
			};
			
			return container;
		}

		private static void CivilianOnSelect()
		{
			const string TEXT = "Select building to place...";

			List<BuildingDefinition> bdef = DataManager.GetAllOfType<BuildingDefinition>();
			GameObject[] gridElements = new GameObject[bdef.Count];

			// Initialize the grid elements' GameObjects.
			for( int i = 0; i < bdef.Count; i++ )
			{
				BuildingDefinition buildingDef = bdef[i];
				gridElements[i] = UIUtils.CreateButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), buildingDef.icon.Item2, new IconData( Color.white ), () =>
				{
					if( BuildPreview.isActive )
					{
						return;
					}
					BuildPreview.Create( buildingDef );
					SelectionManager.DeselectAll(); // deselect everything when the preview is active, to stop weird glitches from happening.
					// TODO ----- change this to priority-queue-based input handler.
				} );
			}
			// Create the actual UI.
			UIUtils.CreateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), TEXT, new TextData( Main.mainFont, 24, TMPro.FontStyles.Normal, TMPro.TextAlignmentOptions.Center, Color.white ) );
			UIUtils.CreateScrollableGrid( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 25.0f, 5.0f ), new Vector2( -50.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), new GridData( 72 ), gridElements );

		}
	}
}