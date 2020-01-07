using SS.Content;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.UI;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using SS.AI;
using Object = UnityEngine.Object;
using SS.Objects.SubObjects;

namespace SS.Objects.Heroes
{
	public static class HeroCreator
	{
		private const string GAMEOBJECT_NAME = "Hero";

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static void SetData( GameObject gameObject, HeroData data )
		{
			//
			//    CONTAINER GAMEOBJECT
			//
			// Set the position/movement information.
			gameObject.transform.SetPositionAndRotation( data.position, data.rotation );

			// Set the unit's movement parameters.
			NavMeshAgent navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
			navMeshAgent.enabled = true; // Enable the NavMeshAgent since the position is set (data.position).

			// Set the hero's native parameters.
			Hero hero = gameObject.GetComponent<Hero>();
			if( hero.guid != data.guid )
			{
				throw new Exception( "Mismatched guid." );
			}
			hero.factionId = data.factionId;
			if( data.health != null )
			{
				hero.health = data.health.Value;
			}
			if( data.movementSpeed != null )
			{
				hero.movementSpeedOverride = data.movementSpeed.Value;
			}
			if( data.rotationSpeed != null )
			{
				hero.rotationSpeedOverride = data.rotationSpeed.Value;
			}

			//
			//    MODULES
			//

			SSObjectCreator.AssignModuleData( hero, data );

			TacticalGoalController tacticalGoalController = gameObject.GetComponent<TacticalGoalController>();
			if( data.tacticalGoalData != null )
			{
				tacticalGoalController.goal = data.tacticalGoalData.GetGoal();
			}
		}

		private static GameObject CreateHero( HeroDefinition def, Guid guid )
		{
			GameObject gameObject = new GameObject( GAMEOBJECT_NAME + " - '" + def.id + "'" );
			gameObject.layer = ObjectLayer.HEROES;

			//
			//    CONTAINER GAMEOBJECT
			//

			GameObject hudGameObject = Object.Instantiate( (GameObject)AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/hero_hud" ), Main.camera.WorldToScreenPoint( gameObject.transform.position ), Quaternion.identity, Main.objectHUDCanvas );

			HUD hud = hudGameObject.GetComponent<HUD>();
			hud.isVisible = Main.isHudForcedVisible;


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
			hero.hud = hud;
			hero.guid = guid;
			hero.definitionId = def.id;
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
					SSObjectHelper.ReDisplayDisplayed();
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
				if( hero.hasBeenHiddenSinceLastDamage )
				{
					return;
				}
				if( isLocked )
				{
					hero.hud.isVisible = true;
				}
				else
				{
					if( Selection.IsSelected( hero ) )
					{
						return;
					}
					if( (object)MouseOverHandler.currentObjectMouseOver == hero )
					{
						return;
					}
					hero.hud.isVisible = false;
				}
			};

			Main.onHudLockChange.AddListener( onHudLockChangeListener );

			hero.onSelect.AddListener( () =>
			{
				if( Main.isHudForcedVisible ) { return; }
				if( MouseOverHandler.currentObjectMouseOver == gameObject )
				{
					return;
				}
				hero.hud.isVisible = true;
			} );

			hero.onDeselect.AddListener( () =>
			{
				if( Main.isHudForcedVisible ) { return; }
				if( MouseOverHandler.currentObjectMouseOver == gameObject )
				{
					return;
				}
				hero.hud.isVisible = false;
			} );


			// Make the hero damageable.
			hero.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				hero.hud.SetHealthBarFill( hero.healthPercent );
				if( deltaHP < 0 )
				{
					hero.hud.isVisible = true;
					hero.hasBeenHiddenSinceLastDamage = true;
				}

				if( !Selection.IsDisplayed( hero ) )
				{
					return;
				}
				Transform healthUI = SelectionPanel.instance.obj.GetElement( "hero.health" );
				if( healthUI != null )
				{
					UIUtils.EditText( healthUI.gameObject, SSObjectDFS.GetHealthDisplay( hero.health, hero.healthMax ) );
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

			SSObjectCreator.AssignSubObjects( gameObject, def );

			//
			//    MODULES
			//

			SSObjectCreator.AssignModules( hero, def );

			TacticalGoalController tacticalGoalController = gameObject.AddComponent<TacticalGoalController>();
			tacticalGoalController.goal = TacticalGoalController.GetDefaultGoal();

			return gameObject;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		/// <summary>
		/// Creates a new HeroData from a GameObject.
		/// </summary>
		/// <param name="hero">The GameObject to extract the save state from. Must be a hero.</param>
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
			if( hero.movementSpeedOverride != null )
			{
				data.movementSpeed = hero.movementSpeedOverride;
			}
			if( hero.rotationSpeedOverride != null )
			{
				data.rotationSpeed = hero.rotationSpeedOverride;
			}

			//
			// MODULES
			//

			SSObjectCreator.ExtractModulesToData( hero, data );

			data.tacticalGoalData = hero.GetComponent<TacticalGoalController>().goal.GetData();

			return data;
		}

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static GameObject Create( HeroDefinition def, Guid guid )
		{
			return CreateHero( def, guid );
		}
	}
}