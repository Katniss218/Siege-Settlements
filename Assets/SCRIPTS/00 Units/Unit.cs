using SS.Buildings;
using SS.Data;
using SS.ResourceSystem;
using SS.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace SS.Units
{
	public class Unit : MonoBehaviour
	{
		/// <summary>
		/// Contains all of the original values for this unit. Might be not accurate to the overriden values on GameObjects (Read Only).
		/// </summary>
		public UnitDefinition cachedDefinition { get; private set; }

		public static GameObject Create( UnitDefinition def, Vector3 pos, Quaternion rot, int factionId )
		{
			if( def == null )
			{
				throw new System.ArgumentNullException( "Definition can't be null" );
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
			Unit unit = container.AddComponent<Unit>();
			unit.cachedDefinition = def;

			BoxCollider collider = container.AddComponent<BoxCollider>();
			collider.size = new Vector3( def.radius * 2.0f, def.height, def.radius * 2.0f );
			collider.center = new Vector3( 0.0f, def.height / 2.0f, 0.0f );

			// Mask the unit as selectable.
			Selectable selectable = container.AddComponent<Selectable>();
			selectable.icon = def.icon.Item2;
			// If the unit is constructor (civilian), make it show the build menu, when highlighted.
			if( def.isConstructor )
			{
				selectable.onSelectionUIRedraw.AddListener( ConstructorOnSelect );
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

			HUDScaled ui = Object.Instantiate( Main.unitHUD, Main.camera.WorldToScreenPoint( pos ), Quaternion.identity, Main.worldUIs ).GetComponent<HUDScaled>();
			Image hudResourceIcon = ui.transform.Find( "Resource" ).Find("Icon").GetComponent<Image>();
			TextMeshProUGUI hudAmount = ui.transform.Find( "Amount" ).GetComponent<TextMeshProUGUI>();


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
			damageable.healthMax = def.healthMax;
			damageable.Heal();
			damageable.armor = def.armor;

			InventorySingle inventory = container.AddComponent<InventorySingle>();
			inventory.maxCapacity = 10;
			inventory.onAdd.AddListener( ( string id, int amtAdded ) =>
			{
				List<ResourceStack> res = inventory.GetAll(); // res[0] is guaranteed to be the contained value in the InventorySingle (if List is not null).
				hudResourceIcon.sprite = DataManager.Get<ResourceDefinition>( res[0].id ).icon.Item2;
				hudAmount.text = res[0].amount.ToString();

				hudResourceIcon.gameObject.SetActive( true );
				hudAmount.gameObject.SetActive( true );
			} );
			inventory.onRemove.AddListener( ( string id, int amtRemoved ) =>
			{
				if( inventory.isEmpty )
				{
					hudResourceIcon.gameObject.SetActive( false );
					hudAmount.gameObject.SetActive( false );
				}
				else
				{
					List<ResourceStack> res = inventory.GetAll(); // res[0] is guaranteed to be the contained value in the InventorySingle (if List is not null).
					hudResourceIcon.sprite = DataManager.Get<ResourceDefinition>( res[0].id ).icon.Item2;
					hudAmount.text = res[0].amount.ToString();
				}
			} );
			/*if( factionId == 0 ) // If player, update the resource panel.
			{
				inventory.onAdd.AddListener( ( string id, int amtAdded ) =>
				{
					Main.resourcePanel.UpdateResourceEntry( inventory.resourceId, inventory.resourceAmount );
				} );
				inventory.onRemove.AddListener( ( string id, int amtRemoved ) =>
				{
					Main.resourcePanel.UpdateResourceEntry( inventory.resourceId, inventory.resourceAmount );
				} );
			}*/
			
			// If the new unit is melee, setup the melee module.
			if( def.melee != null )
			{
				def.melee.AddTo( container );
			}

			// If the new unit is ranged, setup the ranged module.
			if( def.ranged != null )
			{
				def.ranged.AddTo( container );
			}


			// Make the unit update it's UI's position every frame.
			container.AddComponent<EveryFrameSingle>().onUpdate = () =>
			{
				ui.transform.position = Main.camera.WorldToScreenPoint( container.transform.position );
			};

			return container;
		}

		private static void ConstructorOnSelect()
		{
			const string TEXT = "Select building to place...";

			List<BuildingDefinition> bdef = DataManager.GetAllOfType<BuildingDefinition>();
			GameObject[] gridElements = new GameObject[bdef.Count];

			// Initialize the grid elements' GameObjects.
			for( int i = 0; i < bdef.Count; i++ )
			{
				BuildingDefinition buildingDef = bdef[i];

				// If the unit's techs required have not been researched yet, add unclickable button, otherwise, add normal button.
				if( Technologies.TechLock.CheckLocked( buildingDef, FactionManager.factions[0].techs ) )
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), buildingDef.icon.Item2, null );
				}
				else
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), buildingDef.icon.Item2, () =>
					{
						if( BuildPreview.isActive )
						{
							return;
						}
						BuildPreview.Create( buildingDef );
						SelectionManager.DeselectAll(); // deselect everything when the preview is active, to stop the player from performing other left-mouse-button input actions.
					} );
				}
			}
			// Create the actual UI.
			UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), TEXT );
			UIUtils.InstantiateScrollableGrid( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 25.0f, 5.0f ), new Vector2( -50.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
		}
	}
}