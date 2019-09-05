using SS.Data;
using SS.Projectiles;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Heroes
{
	public class Hero : MonoBehaviour
	{
		/// <summary>
		/// Contains all of the original values for this hero. Might be not accurate to the overriden values on GameObjects (Read Only).
		/// </summary>
		public HeroDefinition cachedDefinition { get; private set; }


		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		public static GameObject Create( HeroDefinition def, Vector3 pos, Quaternion rot, int factionId )
		{
			if( def == null )
			{
				throw new System.Exception( "Definition can't be null" );
			}
			GameObject container = new GameObject( "Hero (\"" + def.id + "\"), (f: " + factionId + ")" );
			container.layer = LayerMask.NameToLayer( "Units" );

			GameObject gfx = new GameObject( "graphics" );
			gfx.transform.SetParent( container.transform );

			container.transform.SetPositionAndRotation( pos, rot );

			// Add a mesh to the unit.
			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;

			// Add a material to the unit.
			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = Main.materialFactionColoredDestroyable;
			// Set the material's properties to the appropriate values.
			meshRenderer.material.SetColor( "_FactionColor", FactionManager.factions[factionId].color );
			meshRenderer.material.SetTexture( "_Albedo", def.albedo.Item2 );
			meshRenderer.material.SetTexture( "_Normal", def.normal.Item2 );
			meshRenderer.material.SetTexture( "_Emission", null );
			meshRenderer.material.SetFloat( "_Metallic", 0.0f );
			meshRenderer.material.SetFloat( "_Smoothness", 0.5f );

			// Assign the definition to the unit, so it can be accessed later.
			Hero hero = container.AddComponent<Hero>();
			hero.cachedDefinition = def;

			BoxCollider collider = container.AddComponent<BoxCollider>();
			collider.size = new Vector3( def.radius * 2.0f, def.height, def.radius * 2.0f );
			collider.center = new Vector3( 0.0f, def.height / 2.0f, 0.0f );

			// Mask the unit as selectable.
			Selectable selectable = container.AddComponent<Selectable>();
			selectable.icon = def.icon.Item2;
			
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
			
			// Make the unit belong to a faction.
			FactionMember factionMember = container.AddComponent<FactionMember>();
			factionMember.onFactionChange.AddListener( () =>
			{
				Color color = FactionManager.factions[factionMember.factionId].color;
				//ui.SetFactionColor( color );
				meshRenderer.material.SetColor( "_FactionColor", color );
			} );
			// We set the faction after assigning the listener, to automatically set the color to the appropriate value.
			factionMember.factionId = factionId;

			// Make the unit damageable.
			Damageable damageable = container.AddComponent<Damageable>();
			damageable.onHealthChange.AddListener( () =>
			{
				meshRenderer.material.SetFloat( "_Dest", 1 - damageable.healthPercent );
				//ui.SetHealthFill( damageable.healthPercent );
			} );
			// Make the unit deselect itself, and destroy it's UI when killed.
			damageable.onDeath.AddListener( () =>
			{
				//Object.Destroy( ui.gameObject );
				// for breakup make several meshes that are made up of the original one, attach physics to them.
				// let the physics play for a few seconds (randomize durations for each piece), then disable rigidbodies, and pull them downwards, reducing their scale at the same time.
				// when the scale reaches 0.x, remove the piece.

				// also, play a poof from some particle system for smoke or something at the moment of death.
				if( SelectionManager.IsSelected( selectable ) )
				{
					SelectionManager.Deselect( selectable ); // We have all of the references of this unit here, so we can just simply pass it like this. Amazing, right?
				}

			} );
			damageable.healthMax = def.healthMax;
			damageable.Heal();
			damageable.armor = def.armor;
			
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
			/*container.AddComponent<EveryFrameSingle>().onUpdate = () =>
			{
				ui.transform.position = Main.camera.WorldToScreenPoint( container.transform.position );
			};*/

			return container;
		}
	}
}