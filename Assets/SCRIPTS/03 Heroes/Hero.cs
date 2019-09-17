using SS.Content;
using SS.Modules;
using SS.UI;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Heroes
{
	public static class HeroCreator
	{
		public static GameObject Create( HeroDefinition def, Vector3 pos, Quaternion rot, int factionId )
		{
			if( def == null )
			{
				throw new System.ArgumentNullException( "Definition can't be null" );
			}
			GameObject container = new GameObject( "Hero (\"" + def.id + "\"), (f: " + factionId + ")" );
			container.layer = ObjectLayer.HEROES;

			GameObject gfx = new GameObject( "graphics" );
			gfx.transform.SetParent( container.transform );

			container.transform.SetPositionAndRotation( pos, rot );

			// Add a mesh to the unit.
			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;

			// Add a material to the unit.
			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateColoredDestroyable( FactionManager.factions[factionId].color, def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f, 0.0f );
						
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
			navMeshAgent.baseOffset = -0.1f;
			navMeshAgent.acceleration = 8.0f;
			navMeshAgent.stoppingDistance = 0.125f;
			navMeshAgent.radius = def.radius;
			navMeshAgent.height = def.height;
			navMeshAgent.speed = def.movementSpeed;
			navMeshAgent.angularSpeed = def.rotationSpeed;

			HUDScaled ui = Object.Instantiate( AssetManager.GetPrefab( AssetManager.RESOURCE_ID + "Prefabs/hero_hud" ), Main.camera.WorldToScreenPoint( pos ), Quaternion.identity, Main.worldUIs ).GetComponent<HUDScaled>();

			ui.transform.Find( "Name" ).GetComponent<TMPro.TextMeshProUGUI>().text = def.displayName;
			ui.transform.Find( "Title" ).GetComponent<TMPro.TextMeshProUGUI>().text = def.displayTitle;

			// Make the unit belong to a faction.
			FactionMember factionMember = container.AddComponent<FactionMember>();
			factionMember.onFactionChange.AddListener( () =>
			{
				Color color = FactionManager.factions[factionMember.factionId].color;
				ui.SetColor( color );
				meshRenderer.material.SetColor( "_FactionColor", color );
			} );
			// We set the faction after assigning the listener, to automatically set the color to the appropriate value.
			factionMember.factionId = factionId;

			// Make the unit damageable.
			Damageable damageable = container.AddComponent<Damageable>();
			damageable.onHealthChange.AddListener( (float deltaHP) =>
			{
				meshRenderer.material.SetFloat( "_Dest", 1 - damageable.healthPercent );
				ui.SetHealthBarFill( damageable.healthPercent );

				SelectionManager.ForceSelectionUIRedraw( selectable );
			} );
			// Make the unit deselect itself, and destroy it's UI when killed.
			damageable.onDeath.AddListener( () =>
			{
				Object.Destroy( ui.gameObject );
				// for breakup make several meshes that are made up of the original one, attach physics to them.
				// let the physics play for a few seconds (randomize durations for each piece), then disable rigidbodies, and pull them downwards, reducing their scale at the same time.
				// when the scale reaches 0.x, remove the piece.

				// also, play a poof from some particle system for smoke or something at the moment of death.
				if( SelectionManager.IsSelected( selectable ) )
				{
					SelectionManager.Deselect( selectable ); // We have all of the references of this unit here, so we can just simply pass it like this. Amazing, right?
				}

			} );
			selectable.onSelectionUIRedraw.AddListener( () =>
			{
				UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), def.displayName );
				UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), def.displayTitle );
				UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, -50.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), (int)damageable.health + "/" + (int)damageable.healthMax );
			} );
			damageable.healthMax = def.healthMax;
			damageable.Heal();
			damageable.armor = def.armor;

			// If the new unit is melee, setup the melee module.
			if( def.melee != null )
			{
				MeleeModule.AddTo( container, def.melee );
			}

			// If the new unit is ranged, setup the ranged module.
			if( def.ranged != null )
			{
				RangedModule.AddTo( container, def.ranged );
			}


			// Make the unit update it's UI's position every frame.
			container.AddComponent<EveryFrameSingle>().onUpdate = () =>
			{
				ui.transform.position = Main.camera.WorldToScreenPoint( container.transform.position );
			};

			return container;
		}
	}
}