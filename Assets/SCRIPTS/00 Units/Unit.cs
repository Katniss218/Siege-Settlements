using SS.Projectiles;
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

			// Mesh
			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;

			// Material
			MeshRenderer meshRenderer  = gfx.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = Main.materialFactionColoredDestroyable;
			meshRenderer.material.SetColor( "_FactionColor", FactionManager.factions[factionId].color );
			meshRenderer.material.SetTexture( "_Albedo", def.albedo.Item2 );

			meshRenderer.material.SetTexture( "_Normal", def.normal.Item2 );
			meshRenderer.material.SetTexture( "_Emission", null );
			meshRenderer.material.SetFloat( "_Metallic", 0.0f );
			meshRenderer.material.SetFloat( "_Smoothness", 0.5f );


			BoxCollider collider = container.AddComponent<BoxCollider>();
			collider.size = new Vector3( def.radius * 2, def.height, def.radius * 2 );
			collider.center = new Vector3( 0f, def.height / 2f, 0f );

			Selectable selectable = container.AddComponent<Selectable>();
			if( def.id == "unit.civilian" )
			{
				selectable.onSelect.AddListener( ( Selectable obj ) =>
				{
					Debug.Log( "Selected Civilian" );
				} );
			}

			Rigidbody rigidbody = container.AddComponent<Rigidbody>();
			rigidbody.isKinematic = true;
			
			NavMeshAgent navMeshAgent = container.AddComponent<NavMeshAgent>();
			navMeshAgent.acceleration = 8;
			navMeshAgent.stoppingDistance = 0.125f;
			navMeshAgent.radius = def.radius;
			navMeshAgent.height = def.height;
			navMeshAgent.speed = def.movementSpeed;
			navMeshAgent.angularSpeed = def.rotationSpeed;

			UnitUI ui = Object.Instantiate( Main.unitUI, Main.camera.WorldToScreenPoint( pos ), Quaternion.identity, Main.worldUIs ).GetComponent<UnitUI>();

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
				meshRenderer.material.SetFloat( "_Dest", 1 - obj.healthPercent );
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
			}

			// If the new unit is ranged, setup the ranged module.
			if( def.isRanged )
			{
				DamageSource rangedDamageSource = container.AddComponent<DamageSource>();
				rangedDamageSource.damageType = def.rangedDamageType;
				rangedDamageSource.damage = def.rangedDamage;
				rangedDamageSource.armorPenetration = def.rangedArmorPenetration;

				RangedModule ranged = container.AddComponent<RangedModule>();
				ranged.projectile = DataManager.FindDefinition<ProjectileDefinition>( def.rangedProjectileId );
				ranged.projectileCount = def.rangedProjectileCount;
				ranged.damageSource = rangedDamageSource;
				ranged.targetFinder = finder;
				ranged.attackRange = def.rangedAttackRange;
				ranged.attackCooldown = def.rangedAttackCooldown;
				ranged.velocity = def.rangedVelocity;
				ranged.localOffsetMin = def.rangedLocalOffsetMin;
				ranged.localOffsetMax = def.rangedLocalOffsetMax;
			}

			container.AddComponent<EveryFrameSingle>().everyFrame = () =>
			{
				ui.transform.position = Main.camera.WorldToScreenPoint( container.transform.position );
			};


			return container;
		}
	}
}