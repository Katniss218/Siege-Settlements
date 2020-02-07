using SS.Objects.Modules;
using SS.Objects.SubObjects;
using SS.UI;
using SS.UI.HUDs;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Objects.Heroes
{
	[UseHud( typeof( HeroHUD ), "hud" )]
	public class Hero : SSObjectDFSC, IMovable, IMouseOverHandlerListener, IInteriorUser
	{
		private NavMeshAgent __navMeshAgent = null;
		public NavMeshAgent navMeshAgent
		{
			get
			{
				if( this.__navMeshAgent == null )
				{
					this.__navMeshAgent = this.GetComponent<NavMeshAgent>();
				}
				return this.__navMeshAgent;
			}
		}


		/// <summary>
		/// Gets or sets the display name of this Hero.
		/// </summary>
		public override string displayName
		{
			get => base.displayName;
			set
			{
				base.displayName = value;
				this.hud.displayName = value;
				if( Selection.IsDisplayed( this ) )
				{
					SelectionPanel.instance.obj.displayNameText.text = value;
				}
			}
		}

		private string __displayTitle = "<missing>";
		/// <summary>
		/// Gets or sets the display title of this Hero.
		/// </summary>
		public string displayTitle
		{
			get
			{
				return this.__displayTitle;
			}
			set
			{
				this.__displayTitle = value;
				this.hud.displayTitle = value;

				if( Selection.IsDisplayed( this ) )
				{
					Transform titleUI = SelectionPanel.instance.obj.GetElement( "hero.title" );
					if( titleUI != null )
					{
						UIUtils.EditText( titleUI.gameObject, value );
					}
				}
			}
		}

		/// <summary>
		/// Returns the hud that's attached to this object.
		/// </summary>
		public HeroHUD hud { get; set; }
		public override HUDDFSC hudDFSC { get { return this.hud; } }


		//
		//
		//

		float __movementSpeed;
		public float movementSpeed
		{
			get => this.__movementSpeed;
			set
			{
				this.__movementSpeed = value;
				this.navMeshAgent.speed = value;
			}
		}


		float __rotationSpeed;
		public float rotationSpeed
		{
			get => this.__rotationSpeed;
			set
			{
				this.__rotationSpeed = value;
				this.navMeshAgent.angularSpeed = value;
			}
		}

		/// <summary>
		/// The interior module the unit is currently in. Null if not is any interior.
		/// </summary>
		public InteriorModule interior { get; private set; }
		public InteriorModule.SlotType slotType { get; private set; }
		/// <summary>
		/// The slot of the interior module the unit is currently in.
		/// </summary>
		public int slotIndex { get; private set; }

		public bool isInside
		{
			get => this.interior != null;
		}
		public bool isInsideHidden { get; private set; } // if true, the unit is not visible - graphics (sub-objects) are disabled.



		/// <summary>
		/// Marks the unit as being inside.
		/// </summary>
		public void SetInside( InteriorModule interior, InteriorModule.SlotType slotType, int slotIndex )
		{
			if( slotType == InteriorModule.SlotType.Worker )
			{
				throw new System.Exception( "Can't put a hero in a worker slot." );
			}
			if( this.isInside )
			{
				return;
			}

			// - Interior fields

			InteriorModule.GetSlot( interior, slotType, slotIndex, out InteriorModule.Slot slot, out HUDInteriorSlot slotHud );

			slot.objInside = this;

			slotHud.SetHealth( this.healthPercent );
			slotHud.SetSprite( this.icon );

			this.navMeshAgent.enabled = false;

			// -

			this.transform.position = interior.SlotWorldPosition( slot );
			this.transform.rotation = interior.SlotWorldRotation( slot );

			if( slot.isHidden )
			{
				SubObject[] subObjects = this.GetSubObjects();

				for( int j = 0; j < subObjects.Length; j++ )
				{
					subObjects[j].gameObject.SetActive( false );
				}

				this.isInsideHidden = true;
			}

			this.interior = interior;
			this.slotIndex = slotIndex;
			this.slotType = slotType;
		}

		/// <summary>
		/// Marks the unit as being outside.
		/// </summary>
		public void SetOutside()
		{
			if( !this.isInside )
			{
				return;
			}


			// - Interior fields.

			InteriorModule.GetSlot( interior, this.slotType, this.slotIndex, out InteriorModule.Slot slot, out HUDInteriorSlot slotHud );

			slot.objInside = null;

			slotHud.SetHealth( null );
			slotHud.ClearSprite();


			// -

			this.transform.position = this.interior.EntranceWorldPosition();
			this.transform.rotation = Quaternion.identity;

			this.navMeshAgent.enabled = true;

			if( this.isInsideHidden )
			{
				SubObject[] subObjects = this.GetSubObjects();

				for( int i = 0; i < subObjects.Length; i++ )
				{
					subObjects[i].gameObject.SetActive( true );
				}
			}

			this.interior = null;
			this.slotIndex = -1;
			this.isInsideHidden = false;
		}


		//
		//
		//



		public void OnMouseEnterListener() => this.hud.ConditionalShow();
		public void OnMouseStayListener() { }
		public void OnMouseExitListener() => this.hud.ConditionalHide();


		public override void OnDisplay()
		{
			SelectionPanel.instance.obj.SetIcon( this.icon );

			SelectionPanel.instance.obj.displayNameText.text = this.displayName;

			GameObject titleUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 25.0f, -25.0f ), new Vector2( 200.0f, 25.0f ), new Vector2( 0.0f, 1.0f ), new Vector2( 0.0f, 1.0f ), new Vector2( 0.0f, 1.0f ) ), this.displayTitle );
			SelectionPanel.instance.obj.RegisterElement( "hero.title", titleUI.transform );

			GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 25.0f, -50.0f ), new Vector2( 200.0f, 25.0f ), new Vector2( 0.0f, 1.0f ), new Vector2( 0.0f, 1.0f ), new Vector2( 0.0f, 1.0f ) ), SSObjectDFSC.GetHealthString( this.health, this.healthMax ) );
			SelectionPanel.instance.obj.RegisterElement( "hero.health", healthUI.transform );
		}

		public override void OnHide()
		{

		}
	}
}