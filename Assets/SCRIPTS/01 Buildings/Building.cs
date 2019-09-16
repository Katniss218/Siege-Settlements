using SS.Modules;
using SS.UI;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Buildings
{
	public class Building : MonoBehaviour
	{
		// The amount of health that the building marked as being constructed is going to start with.
		private const float STARTING_HEALTH_PERCENT = 0.1f;
		// If a building drops below this value, it can't be used, and needs to be repaired.
		private const float USABILITY_THRESHOLD = 0.5f;

		/// <summary>
		/// Contains all of the original values for this building. Might be not accurate to the overriden values on GameObjects (Read Only).
		/// </summary>
		public BuildingDefinition cachedDefinition { get; private set; }

		public Vector3 entrance { get; private set; }


		public static bool CheckUsable( Damageable building )
		{
			// Building's can't be used when they are being constructed/repaired or in the state of not-usable (<USABILITY_THRESHOLD% HP).
			if( building.GetComponent<ConstructionSite>() != null )
			{
				return false;
			}
			return building.healthPercent >= 0.5f;
		}


		public static GameObject Create( BuildingDefinition def, Vector3 pos, Quaternion rot, int factionId, bool isUnderConstruction = false )
		{
			if( def == null )
			{
				throw new System.ArgumentNullException( "Definition can't be null" );
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

			// Assign the definition to the building, so it can be accessed later.
			Building building = container.AddComponent<Building>();
			building.cachedDefinition = def;
			building.entrance = def.entrance;

			BoxCollider collider = container.AddComponent<BoxCollider>();
			collider.size = def.size;
			collider.center = new Vector3( 0.0f, def.size.y / 2.0f, 0.0f );

			Selectable selectable = container.AddComponent<Selectable>();
			selectable.icon = def.icon.Item2;

			NavMeshObstacle navMeshObstacle = container.AddComponent<NavMeshObstacle>();
			navMeshObstacle.size = def.size;
			navMeshObstacle.center = new Vector3( 0.0f, def.size.y / 2.0f, 0.0f );
			navMeshObstacle.carving = true;

			HUDUnscaled ui = Object.Instantiate( Main.buildingHUD, Main.camera.WorldToScreenPoint( pos ), Quaternion.identity, Main.worldUIs ).GetComponent<HUDUnscaled>();

			// Make the building belong to a faction.
			FactionMember factionMember = container.AddComponent<FactionMember>();
			factionMember.onFactionChange.AddListener( () =>
			{
				Color color = FactionManager.factions[factionMember.factionId].color;
				ui.SetColor( color );
				meshRenderer.material.SetColor( "_FactionColor", color );
			} );
			factionMember.factionId = factionId;
			

			if( def.barracks != null )
			{
				BarracksModule.AddTo( container, def.barracks );
			}
			
			if( def.research != null )
			{
				ResearchModule.AddTo( container, def.research );
			}

			// Make the building damageable.
			Damageable damageable = container.AddComponent<Damageable>();
			damageable.healthMax = def.healthMax;
			damageable.armor = def.armor;
			// When the health is changed, make the building update it's healthbar.
			damageable.onHealthChange.AddListener( (float deltaHP) =>
			{

				ui.SetHealthBarFill( damageable.healthPercent );
				SelectionManager.ForceSelectionUIRedraw( selectable );
			} );
			// When the building dies:
			// - Destroy the building's UI.
			// - Deselect the building.
			// - Play the death sound.
			damageable.onDeath.AddListener( () =>
			{
				Object.Destroy( ui.gameObject );
				if( SelectionManager.IsSelected( selectable ) )
				{
					SelectionManager.Deselect( selectable ); // We have all of the references of this unit here, so we can just simply pass it like this. Amazing, right?
				}
				AudioManager.PlayNew( def.deathSoundEffect.Item2, 1.0f, 1.0f );
			} );
			selectable.onSelectionUIRedraw.AddListener( () =>
			{
				UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), def.displayName );
				UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), (int)damageable.health + "/" + (int)damageable.healthMax );
				if( !Building.CheckUsable( damageable ) )
				{
					UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, -50.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "The building is not usable (under construction/repair or <50% health)." );
				}
			} );
			// If the newly spawned building is marked as being constructed:
			// - Set the health to 10% (construction's starting percent).
			// - Start the construction process.
			if( isUnderConstruction )
			{
				damageable.healthPercent = Building.STARTING_HEALTH_PERCENT;
				ConstructionSite.StartConstructionOrRepair( container ); // the condition for completion of construction is 100% health. Repairing is needed once the building's health drops below 50%. Allowed anytime the health is below 100%.
			}
			// If the newly spawned building is not marked as being constructed:
			// - Set the health to max.
			else
			{
				damageable.Heal();
				meshRenderer.material.SetFloat( "_Progress", 1.0f );
			}

			// Make the unit update it's UI's position every frame (buildings are static but the camera is not).
			container.AddComponent<EveryFrameSingle>().onUpdate = () =>
			{
				ui.transform.position = Main.camera.WorldToScreenPoint( container.transform.position );
			};


			return container;
		}


#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.white;

			Matrix4x4 toWorld = this.transform.localToWorldMatrix;
			for( int i = 0; i < this.cachedDefinition.placementNodes.Length; i++ )
			{
				Gizmos.DrawSphere( toWorld.MultiplyVector( this.cachedDefinition.placementNodes[i] ) + this.transform.position, 0.05f );
			}
		}
#endif
	}
}