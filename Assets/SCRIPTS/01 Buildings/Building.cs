using SS.DataStructures;
using SS.ResourceSystem;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Buildings
{
	/// <summary>
	/// Represents a building (buildings can't move, block other objects from moving, can be interacted with, and have a faction).
	/// </summary>
	public class Building : Damageable, IFactionMember, ISelectable, IDefinableBy<BuildingDefinition>
	{
		public string id { get; private set; }
		
		public int factionId { get; private set; }

		public void SetFaction( int id )
		{
			this.factionId = id;
			Color color = FactionManager.factions[id].color;
			this.ui.SetFactionColor( color );
			this.meshRenderer.material.SetColor( "_FactionColor", color );
		}

		private ConstructionData constructionData;

		/// <summary>
		/// Checks if the building is currently under construction (Read Only).
		/// </summary>
		public bool isUnderConstruction { get { return this.constructionData != null; } }

		private BuildingUI ui;


		private Transform graphicsTransform;
		new private BoxCollider collider;
		private MeshFilter meshFilter;
		private MeshRenderer meshRenderer;
		private NavMeshObstacle obstacle;



		
		void Awake()
		{
			this.collider = this.GetComponent<BoxCollider>();
			this.graphicsTransform = this.transform.GetChild( 0 );
			this.meshFilter = this.graphicsTransform.GetComponent<MeshFilter>();
			this.meshRenderer = this.graphicsTransform.GetComponent<MeshRenderer>();
			this.obstacle = this.GetComponent<NavMeshObstacle>();
		}

		void Start()
		{

		}
		
		void Update()
		{
			if( transform.hasChanged )
			{
				ui.transform.position = Main.camera.WorldToScreenPoint( this.transform.position );
			}
		}

		public override void Heal()
		{
			base.Heal();
			this.ui.SetHealthFill( this.healthPercent );
		}

		public override void Heal( float amount )
		{
			base.Heal( amount );
			this.ui.SetHealthFill( this.healthPercent );
		}

		public override void TakeDamage( DamageType type, float amount, float armorPenetration )
		{
			base.TakeDamage( type, amount, armorPenetration );
			this.ui.SetHealthFill( this.healthPercent );
		}

		public override void Die()
		{
			base.Die();

			SelectionManager.Deselect( this );
		}

		public void AdvanceConstruction( ResourceStack stack )
		{
			if( !this.isUnderConstruction )
			{
				throw new System.Exception( "AdvanceConstruction: The building is not under construction." );
			}
			for( int i = 0; i < constructionData.resourceIds.Length; i++ )
			{
				if( constructionData.resourceIds[i] == stack.id )
				{
					if( constructionData.resourcesRemaining[i] - stack.amount < 0 )
					{
						Debug.LogWarning( "AdvanceConstruction: the amount of resource added was more than needed." );
					}
					constructionData.resourcesRemaining[i] -= stack.amount;

					if( constructionData.IsCompleted() )
					{
						FinishConstruction();
					}
					else
					{
						this.meshRenderer.material.SetFloat( "_Progress", constructionData.GetPercentCompleted() );
					}
					break;
				}
			}
		}

		/// <summary>
		/// Overrides the required resources and instantly finishes construction.
		/// </summary>
		public void FinishConstruction()
		{
			if( !this.isUnderConstruction )
			{
				throw new System.Exception( "FinishConstruction: The building is not under construction." );
			}
			constructionData = null;
			this.meshRenderer.material.SetFloat( "_Progress", 1 );
		}
		
		public void AssignDefinition( BuildingDefinition def )
		{
			this.id = def.id;
			this.healthMax = def.healthMax;
			this.Heal();
			this.slashArmor = def.slashArmor;
			this.pierceArmor = def.pierceArmor;
			this.concussionArmor = def.concussionArmor;

			this.collider.size = def.size;
			this.collider.center = new Vector3( 0f, def.size.y / 2f, 0f );
			this.obstacle.size = def.size;
			this.obstacle.center = new Vector3( 0f, def.size.y / 2f, 0f );

			this.meshFilter.mesh = def.mesh.Item2;

			this.meshRenderer.material = Main.materialFactionColoredConstructible;
			this.meshRenderer.material.SetTexture( "_Albedo", def.albedo.Item2 );
			this.meshRenderer.material.SetFloat( "_Height", def.mesh.Item2.bounds.size.y );

			this.meshRenderer.material.SetTexture( "_Normal", def.normal.Item2 );
			this.meshRenderer.material.SetTexture( "_Emission", null );
			this.meshRenderer.material.SetFloat( "_Metallic", 0.0f );
			this.meshRenderer.material.SetFloat( "_Smoothness", 0.5f );
			
			if( this.constructionData != null )
			{
				this.constructionData = new ConstructionData( def.cost );

				this.meshRenderer.material.SetFloat( "_Progress", 0 );
			}
			else
			{
				this.meshRenderer.material.SetFloat( "_Progress", 1 );
			}

		}
		
		public static Building Create( BuildingDefinition def, Vector3 pos, Quaternion rot, int factionId, bool isUnderConstruction = false )
		{
			if( def == null )
			{
				throw new System.Exception( "Definition can't be null" );
			}
			GameObject container = new GameObject( "Building (\"" + def.id + "\"), (f: " + factionId + ")" );

			GameObject gfx = new GameObject( "graphics" );
			gfx.transform.SetParent( container.transform );

			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();


			BoxCollider collider = container.AddComponent<BoxCollider>();

			container.transform.position = pos;
			
			NavMeshObstacle navMeshObstacle = container.AddComponent<NavMeshObstacle>();
			navMeshObstacle.carving = true;

			Building building = container.AddComponent<Building>();
			building.ui = Instantiate( Main.buildingUI, Main.camera.WorldToScreenPoint( pos ), Quaternion.identity, Main.worldUIs ).GetComponent<BuildingUI>();
			building.SetFaction( factionId );
			if( isUnderConstruction )
			{
				building.constructionData = new ConstructionData( def.cost );
			}
			else
			{
				building.constructionData = null;
			}
			building.AssignDefinition( def );



			return building;
		}
	}
}