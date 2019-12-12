using SS.Content;
using SS.Diplomacy;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.UI;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using SS.AI;
using Object = UnityEngine.Object;

namespace SS.Objects.Heroes
{
	public static class HeroCreator
	{
		private const string GAMEOBJECT_NAME = "Hero";

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static void SetDefData( GameObject gameObject, HeroDefinition def, HeroData data )
		{
			//
			//    SUB-OBJECTS
			//

			SSObjectCreator.AssignSubObjects( gameObject, def );
			
			//
			//    CONTAINER GAMEOBJECT
			//
			// Set the position/movement information.
			gameObject.transform.SetPositionAndRotation( data.position, data.rotation );

			// Set the hero's size.
			BoxCollider collider = gameObject.GetComponent<BoxCollider>();
			collider.size = new Vector3( def.radius * 2.0f, def.height, def.radius * 2.0f );
			collider.center = new Vector3( 0.0f, def.height / 2.0f, 0.0f );
			

			// Set the unit's movement parameters.
			NavMeshAgent navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
			navMeshAgent.radius = def.radius;
			navMeshAgent.height = def.height;
			navMeshAgent.speed = def.movementSpeed;
			navMeshAgent.angularSpeed = def.rotationSpeed;
			navMeshAgent.enabled = true; // Enable the NavMeshAgent since the position is set (data.position).

			// Set the hero's native parameters.
			Hero hero = gameObject.GetComponent<Hero>();
			hero.definitionId = def.id;
			hero.displayName = def.displayName;
			hero.displayTitle = def.displayTitle;
			hero.icon = def.icon;

			// Set the faction id.
			FactionMember factionMember = gameObject.GetComponent<FactionMember>();

			// Set the hero's health.
			Damageable damageable = gameObject.GetComponent<Damageable>();

			MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();

			factionMember.onFactionChange.AddListener( () =>
			{
				Color color = LevelDataManager.factions[factionMember.factionId].color;

				for( int i = 0; i < renderers.Length; i++ )
				{
					renderers[i].material.SetColor( "_FactionColor", color );
				}
			} );

			// Make the unit update it's healthbar and material when health changes.
			damageable.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				for( int i = 0; i < renderers.Length; i++ )
				{
					renderers[i].material.SetFloat( "_Dest", 1 - damageable.healthPercent );
				}
			} );


			factionMember.factionId = data.factionId;
			factionMember.viewRange = def.viewRange;

			damageable.healthMax = def.healthMax;
			damageable.health = data.health;
			damageable.armor = def.armor;

			
			//
			//    MODULES
			//

			SSObjectCreator.AssignModules( gameObject, def, data );


			TacticalGoalController tacticalGoalController = gameObject.AddComponent<TacticalGoalController>();
			if( data.tacticalGoalData != null )
			{
				tacticalGoalController.goal = data.tacticalGoalData.GetInstance();
			}
		}
		
		private static GameObject CreateHero( Guid guid )
		{
			GameObject container = new GameObject( GAMEOBJECT_NAME );
			container.layer = ObjectLayer.HEROES;

			//
			//    CONTAINER GAMEOBJECT
			//

			BoxCollider collider = container.AddComponent<BoxCollider>();

			Hero hero = container.AddComponent<Hero>();
			hero.guid = guid;
			
			// Add a kinematic rigidbody to the hero (required by the NavMeshAgent).
			Rigidbody rigidbody = container.AddComponent<Rigidbody>();
			rigidbody.isKinematic = true;

			// Add the NavMeshAgent to the hero, to make it movable.
			NavMeshAgent navMeshAgent = container.AddComponent<NavMeshAgent>();
			navMeshAgent.baseOffset = Main.DEFAULT_NAVMESH_BASE_OFFSET;
			navMeshAgent.acceleration = Main.DEFAULT_NAVMESH_ACCELERATION;
			navMeshAgent.stoppingDistance = Main.DEFAULT_NAVMESH_STOPPING_DIST;
			navMeshAgent.enabled = false; // Disable the NavMeshAgent for as long as the position is not set (data.position).

			GameObject hudGameObject = Object.Instantiate( (GameObject)AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/hero_hud" ), Main.camera.WorldToScreenPoint( container.transform.position ), Quaternion.identity, Main.objectHUDCanvas );
			hudGameObject.SetActive( Main.isHudLocked ); // Only show hud when it's locked.

			hero.hud = hudGameObject;

			HUDScaled hud = hudGameObject.GetComponent<HUDScaled>();
			
			UnityAction<bool> onHudLockChangeListener = ( bool isLocked ) =>
			{
				if( hero.hasBeenHiddenSinceLastDamage )
				{
					return;
				}
				if( isLocked )
				{
					hudGameObject.SetActive( true );
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
					hudGameObject.SetActive( false );
				}
			};

			Main.onHudLockChange.AddListener( onHudLockChangeListener );
			
			hero.onSelect.AddListener( () =>
			{
				if( Main.isHudLocked ) { return; }
				if( MouseOverHandler.currentObjectMouseOver == container )
				{
					return;
				}
				hudGameObject.SetActive( true );
			} );

			hero.onDeselect.AddListener( () =>
			{
				if( Main.isHudLocked ) { return; }
				if( MouseOverHandler.currentObjectMouseOver == container )
				{
					return;
				}
				hudGameObject.SetActive( false );
			} );

			
			// Make the hero change it's color when the faction is changed.
			FactionMember factionMember = container.AddComponent<FactionMember>();
			factionMember.onFactionChange.AddListener( () =>
			{
				Color color = LevelDataManager.factions[factionMember.factionId].color;
				hud.SetColor( color );
			} );

			// Make the hero damageable.
			Damageable damageable = container.AddComponent<Damageable>();
			damageable.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				hud.SetHealthBarFill( damageable.healthPercent );
				if( deltaHP < 0 )
				{
					hudGameObject.SetActive( true );
					hero.hasBeenHiddenSinceLastDamage = true;
				}
			} );

			// Make the hero deselect itself, and destroy it's UI when killed.
			damageable.onDeath.AddListener( () =>
			{
				Object.Destroy( hud.gameObject );
				
				if( Selection.IsSelected( hero ) )
				{
					Selection.Deselect( hero );
				}
				// Remove the now unused listeners.
				Main.onHudLockChange.RemoveListener( onHudLockChangeListener );
			} );
			
			damageable.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				if( !Selection.IsDisplayed( hero ) )
				{
					return;
				}
				Transform healthUI = SelectionPanel.instance.obj.GetElement( "hero.health" );
				if( healthUI != null )
				{
					UIUtils.EditText( healthUI.gameObject, (int)damageable.health + "/" + (int)damageable.healthMax );
				}
			} );

			return container;
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

			FactionMember factionMember = hero.GetComponent<FactionMember>();
			data.factionId = factionMember.factionId;

			Damageable damageable = hero.GetComponent<Damageable>();
			data.health = damageable.health;

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

		public static GameObject CreateEmpty( Guid guid )
		{
			GameObject gameObject = CreateHero( guid );
			
			return gameObject;
		}

		public static GameObject Create( HeroDefinition def, HeroData data )
		{
			GameObject gameObject = CreateHero( data.guid );

			SetDefData( gameObject, def, data );

			return gameObject;
		}
	}
}