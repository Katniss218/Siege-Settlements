using SS.Levels;
using SS.Levels.SaveStates;
using SS.Objects.SubObjects;
using SS.UI;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SS.Objects.Heroes
{
	public static class HeroCreator
	{
		private const string GAMEOBJECT_NAME = "Hero";

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static void SetData( Hero hero, HeroData data )
		{
			//
			//    CONTAINER GAMEOBJECT
			//
			// Set the position/movement information.
			hero.transform.SetPositionAndRotation( data.position, data.rotation );

			// Set the unit's movement parameters.
			NavMeshAgent navMeshAgent = hero.navMeshAgent;
			navMeshAgent.enabled = true; // Enable the NavMeshAgent since the position is set (data.position).

			// Set the hero's native parameters.
			if( hero.guid != data.guid )
			{
				throw new Exception( "Mismatched guid." );
			}
			hero.factionId = data.factionId;
			if( data.health != null )
			{
				hero.health = data.health.Value;
			}

			//
			//    MODULES
			//

			SSObjectCreator.AssignModuleData( hero, data );
			
			if( data.tacticalGoalData != null )
			{
				hero.controller.SetGoalData( data.tacticalGoalData, data.tacticalGoalTag );
			}
		}

		private static Hero CreateHero( HeroDefinition def, Guid guid )
		{
			GameObject gameObject = new GameObject( GAMEOBJECT_NAME + " - '" + def.id + "'" );
			gameObject.layer = ObjectLayer.HEROES;

			//
			//    CONTAINER GAMEOBJECT
			//

			BoxCollider collider = gameObject.AddComponent<BoxCollider>();
			collider.size = new Vector3( def.radius * 2.0f, def.height, def.radius * 2.0f );
			collider.center = new Vector3( 0.0f, def.height / 2.0f, 0.0f );

			// Add a kinematic rigidbody to the hero (required by the NavMeshAgent).
			Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
			rigidbody.isKinematic = true;

			// Add the NavMeshAgent to the hero, to make it movable.
			NavMeshAgent navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
			navMeshAgent.radius = def.radius;
			navMeshAgent.height = def.height;
			navMeshAgent.baseOffset = Main.DEFAULT_NAVMESH_BASE_OFFSET;
			navMeshAgent.acceleration = Main.DEFAULT_NAVMESH_ACCELERATION;
			navMeshAgent.stoppingDistance = Main.DEFAULT_NAVMESH_STOPPING_DIST;
			navMeshAgent.enabled = false; // Disable the NavMeshAgent for as long as the position is not set (data.position).
			navMeshAgent.avoidancePriority = 1;

			Hero hero = gameObject.AddComponent<Hero>();
			hero.guid = guid;
			hero.definitionId = def.id;
			hero.isSelectable = true;
			hero.displayName = def.displayName;
			hero.displayTitle = def.displayTitle;
			hero.icon = def.icon;
			hero.movementSpeed = def.movementSpeed;
			hero.rotationSpeed = def.rotationSpeed;
			hero.hurtSound = def.hurtSoundEffect;
			hero.deathSound = def.deathSoundEffect;

			hero.viewRange = def.viewRange;
			hero.healthMax = def.healthMax;
			hero.health = def.healthMax;
			hero.armor = def.armor;

			hero.onFactionChange.AddListener( ( int fromFac, int toFac ) =>
			{
				Color color = LevelDataManager.factions[hero.factionId].color;

				hero.hud.SetColor( color );
				MeshSubObject[] meshes = hero.GetSubObjects<MeshSubObject>();
				for( int i = 0; i < meshes.Length; i++ )
				{
					meshes[i].GetMaterial().SetColor( "_FactionColor", color );
				}
				MeshPredicatedSubObject[] meshes2 = hero.GetSubObjects<MeshPredicatedSubObject>();
				for( int i = 0; i < meshes2.Length; i++ )
				{
					meshes2[i].GetMaterial().SetColor( "_FactionColor", color );
				}

				// Re-Display the object

				if( Selection.IsDisplayed( hero ) )
				{
					SSObjectUtils.ReDisplayDisplayed();
				}
			} );

			hero.onHealthPercentChanged.AddListener( () =>
			{
				hero.hud.SetHealthBarFill( hero.healthPercent );

				MeshSubObject[] meshes = hero.GetSubObjects<MeshSubObject>();
				for( int i = 0; i < meshes.Length; i++ )
				{
					meshes[i].GetMaterial().SetFloat( "_Dest", 1 - hero.healthPercent );
				}
				MeshPredicatedSubObject[] meshes2 = hero.GetSubObjects<MeshPredicatedSubObject>();
				for( int i = 0; i < meshes2.Length; i++ )
				{
					meshes2[i].GetMaterial().SetFloat( "_Dest", 1 - hero.healthPercent );
				}
			} );



			UnityAction<bool> onHudLockChangeListener = ( bool isLocked ) =>
			{
				if( hero.hud.isDisplayedDueToDamage )
				{
					return;
				}
				if( isLocked )
				{
					hero.hud.hudContainer.isVisible = true;
				}
				else
				{
					if( Selection.IsSelected( hero ) )
					{
						return;
					}
					if( (object)MouseOverHandler.currentObjectMousedOver == hero )
					{
						return;
					}
					hero.hud.hudContainer.isVisible = false;
				}
			};

			Main.onHudLockChange.AddListener( onHudLockChangeListener );

			hero.onSelect.AddListener( () =>
			{
				if( Main.isHudForcedVisible ) { return; }
				if( MouseOverHandler.currentObjectMousedOver == gameObject )
				{
					return;
				}
				hero.hud.hudContainer.isVisible = true;
			} );

			hero.onDeselect.AddListener( () =>
			{
				if( Main.isHudForcedVisible ) { return; }
				if( MouseOverHandler.currentObjectMousedOver == gameObject )
				{
					return;
				}
				hero.hud.hudContainer.isVisible = false;
			} );


			// Make the hero damageable.
			hero.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				hero.hud.SetHealthBarFill( hero.healthPercent );
				if( deltaHP < 0 )
				{
					hero.hud.hudContainer.isVisible = true;
					hero.hud.isDisplayedDueToDamage = true;
				}

				if( !Selection.IsDisplayed( hero ) )
				{
					return;
				}
				Transform healthUI = SelectionPanel.instance.obj.GetElement( "hero.health" );
				if( healthUI != null )
				{
					UIUtils.EditText( healthUI.gameObject, SSObjectDFC.GetHealthString( hero.health, hero.healthMax ) );
				}
			} );

			// Make the hero deselect itself, and destroy it's UI when killed.
			hero.onDeath.AddListener( () =>
			{
				Object.Destroy( hero.hud.gameObject );

				if( Selection.IsSelected( hero ) )
				{
					Selection.Deselect( hero );
				}
				// Remove the now unused listeners.
				Main.onHudLockChange.RemoveListener( onHudLockChangeListener );
			} );

			//
			//    SUB-OBJECTS
			//

			SSObjectCreator.AssignSubObjects( hero, def );

			//
			//    MODULES
			//

			SSObjectCreator.AssignModules( hero, def );
			
			return hero;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		

		public static HeroData GetData( Hero hero )
		{
			if( hero.guid == null )
			{
				throw new Exception( "Guid was not assigned." );
			}

			HeroData data = new HeroData();
			data.guid = hero.guid;

			data.position = hero.transform.position;
			data.rotation = hero.transform.rotation;

			data.factionId = hero.factionId;

			if( hero.health != hero.healthMax )
			{
				data.health = hero.health;
			}

			//
			// MODULES
			//

			SSObjectCreator.ExtractModulesToData( hero, data );

			data.tacticalGoalData = hero.controller.GetGoalData();

			return data;
		}

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static Hero Create( HeroDefinition def, Guid guid )
		{
			return CreateHero( def, guid );
		}
	}
}