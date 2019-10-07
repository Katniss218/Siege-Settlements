using Katniss.Utils;
using SS.Content;
using SS.Diplomacy;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Modules;
using SS.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SS.Heroes
{
	public static class HeroCreator
	{
		private const float DEFAULT_ACCELERATION = 8.0f;
		private const float DEFAULT_STOPPING_DISTANCE = 0.125f;

		private const string GAMEOBJECT_NAME = "Hero";

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		private static void SetHeroDefinition( GameObject gameObject, HeroDefinition def )
		{
			//
			//    GRAPHICS GAMEOBJECT
			//

			GameObject gfx = gameObject.transform.Find( Main.GRAPHICS_GAMEOBJECT_NAME ).gameObject;


			// Set the hero's mesh and material.
			MeshFilter meshFilter = gfx.GetComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;
			
			MeshRenderer meshRenderer = gfx.GetComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateColoredDestroyable( FactionDefinition.DefaultColor, def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f, 0.0f );


			//
			//    CONTAINER GAMEOBJECT
			//

			// Set the hero's size.
			BoxCollider collider = gameObject.GetComponent<BoxCollider>();
			collider.size = new Vector3( def.radius * 2.0f, def.height, def.radius * 2.0f );
			collider.center = new Vector3( 0.0f, def.height / 2.0f, 0.0f );

			// Set the hero's selected icon.
			Selectable selectable = gameObject.GetComponent<Selectable>();
			selectable.icon = def.icon.Item2;

			// Set the unit's movement parameters.
			NavMeshAgent navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
			navMeshAgent.radius = def.radius;
			navMeshAgent.height = def.height;
			navMeshAgent.speed = def.movementSpeed;
			navMeshAgent.angularSpeed = def.rotationSpeed;

			// Set the hero's native parameters.
			Hero hero = gameObject.GetComponent<Hero>();
			hero.defId = def.id;
			hero.displayName = def.displayName;
			hero.displayTitle = def.displayTitle;
			
			hero.hudGameObject.transform.Find( "Name" ).GetComponent<TextMeshProUGUI>().text = def.displayName;
			hero.hudGameObject.transform.Find( "Title" ).GetComponent<TextMeshProUGUI>().text = def.displayTitle;
			
			// Set the hero's health.
			Damageable damageable = gameObject.GetComponent<Damageable>();
			damageable.healthMax = def.healthMax;
			damageable.armor = def.armor;


			//
			//    MODULES
			//

			// Remove old melee module (if present).
			MeleeModule melee = gameObject.GetComponent<MeleeModule>();
			if( melee != null )
			{
				Object.Destroy( melee );
			}
			// If the new unit is melee, setup the melee module.
			if( def.melee != null )
			{
				melee = gameObject.AddComponent<MeleeModule>();
				melee.SetDefinition( def.melee );
			}

			// Remove old ranged module (if present).
			RangedModule ranged = gameObject.GetComponent<RangedModule>();
			if( ranged != null )
			{
				Object.Destroy( ranged );
			}
			// If the new unit is ranged, setup the ranged module.
			if( def.ranged != null )
			{
				ranged = gameObject.AddComponent<RangedModule>();
				ranged.SetDefinition( def.ranged );
			}
		}

		private static void SetHeroData( GameObject gameObject, HeroData data )
		{

			//
			//    CONTAINER GAMEOBJECT
			//

			// Set the position/movement information.
			gameObject.transform.SetPositionAndRotation( data.position, data.rotation );

			NavMeshAgent navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
			navMeshAgent.enabled = true; // Enable the NavMeshAgent since the position is set (data.position).

			// Set the globally unique identifier.
			Hero hero = gameObject.GetComponent<Hero>();
			hero.guid = data.guid;

			// Set the faction id.
			FactionMember factionMember = gameObject.GetComponent<FactionMember>();
			factionMember.factionId = data.factionId;

			// Make the unit damageable.
			Damageable damageable = gameObject.GetComponent<Damageable>();
			damageable.health = data.health;

			//
			//    MODULES
			//

			TAIGoalData taiGoalData = data.taiGoalData;
			if( taiGoalData != null )
			{
				if( taiGoalData is MoveToData )
				{
					TAIGoal.MoveTo.AssignTAIGoal( gameObject, ((MoveToData)taiGoalData).destination );
				}
				else if( taiGoalData is DropoffToNewData )
				{
					TAIGoal.DropoffToNew.AssignTAIGoal( gameObject, ((DropoffToNewData)taiGoalData).destination );
				}

				else if( taiGoalData is DropoffToInventoryData )
				{
					TAIGoal.DropoffToInventory.AssignTAIGoal( gameObject, Main.GetGameObject( ((DropoffToInventoryData)taiGoalData).destinationGuid ) );
				}
				else if( taiGoalData is MakePaymentData )
				{
					TAIGoal.MakePayment.AssignTAIGoal( gameObject, Main.GetGameObject( ((MakePaymentData)taiGoalData).destinationGuid ) );
				}
				else if( taiGoalData is PickupDepositData )
				{
					TAIGoal.PickupDeposit.AssignTAIGoal( gameObject, Main.GetGameObject( ((PickupDepositData)taiGoalData).destinationGuid ) );
				}
			}

		}

		private static GameObject CreateHero()
		{
			GameObject container = new GameObject( GAMEOBJECT_NAME );
			container.layer = ObjectLayer.HEROES;


			//
			//    GRAPHICS GAMEOBJECT
			//

			GameObject gfx = new GameObject( Main.GRAPHICS_GAMEOBJECT_NAME );
			gfx.transform.SetParent( container.transform );
		
			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			
			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			

			//
			//    CONTAINER GAMEOBJECT
			//

			BoxCollider collider = container.AddComponent<BoxCollider>();

			Hero hero = container.AddComponent<Hero>();

			// Make the hero selectable.
			Selectable selectable = container.AddComponent<Selectable>();

			// Add a kinematic rigidbody to the hero (required by the NavMeshAgent).
			Rigidbody rigidbody = container.AddComponent<Rigidbody>();
			rigidbody.isKinematic = true;

			// Add the NavMeshAgent to the hero, to make it movable.
			NavMeshAgent navMeshAgent = container.AddComponent<NavMeshAgent>();
			navMeshAgent.baseOffset = Main.DEFAULT_NAVMESH_BASE_OFFSET;
			navMeshAgent.acceleration = DEFAULT_ACCELERATION;
			navMeshAgent.stoppingDistance = DEFAULT_STOPPING_DISTANCE;
			navMeshAgent.enabled = false; // Disable the NavMeshAgent for as long as the position is not set (data.position).


			GameObject hudGameObject = Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Object HUDs/hero_hud" ), Main.camera.WorldToScreenPoint( container.transform.position ), Quaternion.identity, Main.objectHUDCanvas );
			hudGameObject.SetActive( Main.isHudLocked ); // Only show hud when it's locked.

			hero.hudGameObject = hudGameObject;

			HUDScaled hud = hudGameObject.GetComponent<HUDScaled>();
			
			UnityAction<bool> onHudLockChangeListener = ( bool isLocked ) =>
			{
				if( isLocked )
				{
					hudGameObject.SetActive( true );
				}
				else
				{
					if( Selection.IsSelected( selectable ) )
					{
						return;
					}
					if( MouseOverHandler.currentObjectMouseOver == container )
					{
						return;
					}
					hudGameObject.SetActive( false );
				}
			};

			Main.onHudLockChange.AddListener( onHudLockChangeListener );

			UnityAction<GameObject> onMouseEnterListener = ( GameObject obj ) =>
			{
				if( Main.isHudLocked ) { return; }
				if( obj == container )
				{
					if( Selection.IsSelected( selectable ) )
					{
						return;
					}
					hudGameObject.SetActive( true );
				}
			};

			UnityAction<GameObject> onMouseExitListener = ( GameObject obj ) =>
			{
				if( Main.isHudLocked ) { return; }
				if( obj == container )
				{
					if( Selection.IsSelected( selectable ) )
					{
						return;
					}
					hudGameObject.SetActive( false );
				}
			};

			// Show HUD only when mouseovered or selected.
			MouseOverHandler.onMouseEnter.AddListener( onMouseEnterListener );
			MouseOverHandler.onMouseExit.AddListener( onMouseExitListener );

			selectable.onSelect.AddListener( () =>
			{
				if( Main.isHudLocked ) { return; }
				if( MouseOverHandler.currentObjectMouseOver == container )
				{
					return;
				}
				hudGameObject.SetActive( true );
			} );

			selectable.onDeselect.AddListener( () =>
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
				meshRenderer.material.SetColor( "_FactionColor", color );
			} );

			// Make the hero damageable.
			Damageable damageable = container.AddComponent<Damageable>();
			damageable.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				meshRenderer.material.SetFloat( "_Dest", 1 - damageable.healthPercent );
				hud.SetHealthBarFill( damageable.healthPercent );

				Selection.ForceSelectionUIRedraw( selectable );
			} );

			// Make the hero deselect itself, and destroy it's UI when killed.
			damageable.onDeath.AddListener( () =>
			{
				Object.Destroy( hud.gameObject );
				
				if( Selection.IsSelected( selectable ) )
				{
					Selection.Deselect( selectable ); // We have all of the references of this unit here, so we can just simply pass it like this. Amazing, right?
				}
				// Remove the now unused listeners.
				MouseOverHandler.onMouseEnter.RemoveListener( onMouseEnterListener );
				MouseOverHandler.onMouseEnter.RemoveListener( onMouseExitListener );
				Main.onHudLockChange.RemoveListener( onHudLockChangeListener );
			} );

			// Make the hero show it's parameters on the Selection Panel, when highlighted.
			selectable.onSelectionUIRedraw.AddListener( () =>
			{
				UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), hero.displayName );
				UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), hero.displayTitle );
				UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -50.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), (int)damageable.health + "/" + (int)damageable.healthMax );
			} );
			
			// Make the unit update it's UI's position every frame.
			container.AddComponent<EveryFrameSingle>().onUpdate = () =>
			{
				hud.transform.position = Main.camera.WorldToScreenPoint( container.transform.position );
			};

			return container;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static string GetDefinitionId( GameObject gameObject )
		{
			if( !Hero.IsValid( gameObject ) )
			{
				throw new Exception( "GameObject '" + gameObject.name + "' is not a valid hero." );
			}

			Hero hero = gameObject.GetComponent<Hero>();
			return hero.defId;
		}

		/// <summary>
		/// Creates a new HeroData from a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject to extract the save state from. Must be a hero.</param>
		public static HeroData GetData( GameObject gameObject )
		{
			if( !Hero.IsValid( gameObject ) )
			{
				throw new Exception( "GameObject '" + gameObject.name + "' is not a valid hero." );
			}

			HeroData data = new HeroData();

			data.guid = gameObject.GetComponent<Hero>().guid;

			data.position = gameObject.transform.position;
			data.rotation = gameObject.transform.rotation;

			FactionMember factionMember = gameObject.GetComponent<FactionMember>();
			data.factionId = factionMember.factionId;

			Damageable damageable = gameObject.GetComponent<Damageable>();
			data.health = damageable.health;

			TAIGoal taiGoal = gameObject.GetComponent<TAIGoal>();
			if( taiGoal != null )
			{
				data.taiGoalData = taiGoal.GetData();
			}

			return data;
		}

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static void SetData( GameObject gameObject, HeroData data )
		{
			if( !Hero.IsValid( gameObject ) )
			{
				throw new Exception( "GameObject '" + gameObject.name + "' is not a valid hero." );
			}

			SetHeroData( gameObject, data );
		}

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static GameObject CreateEmpty( Guid guid, HeroDefinition def )
		{
			GameObject gameObject = CreateHero();

			SetHeroDefinition( gameObject, def );

			Hero hero = gameObject.GetComponent<Hero>();
			hero.guid = guid;

			return gameObject;
		}

		public static GameObject Create( HeroDefinition def, HeroData data )
		{
			GameObject gameObject = CreateHero();

			SetHeroDefinition( gameObject, def );
			SetHeroData( gameObject, data );

			return gameObject;
		}
	}
}