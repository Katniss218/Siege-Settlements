using SS.Diplomacy;
using SS.UI;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Objects.Heroes
{
	public class Hero : SSObjectDFS, IHUDHolder, IDamageable, INavMeshAgent, IFactionMember, IMouseOverHandlerListener
	{
		public GameObject hud { get; set; }

		private string __displayTitle = "<missing>";

		/// <summary>
		/// Gets or sets the display name of this Hero.
		/// </summary>
		public override string displayName
		{
			get => base.displayName;
			set
			{
				base.displayName = value;
				this.hud.transform.Find( "Name" ).GetComponent<TextMeshProUGUI>().text = value;
				if( Selection.IsDisplayed( this ) )
				{
					SelectionPanel.instance.obj.displayNameText.text = value;
				}
			}
		}

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
				this.hud.transform.Find( "Title" ).GetComponent<TextMeshProUGUI>().text = value;
				
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


		public bool hasBeenHiddenSinceLastDamage { get; set; }
		
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


		public void OnMouseEnterListener()
		{
			if( Main.isHudLocked ) { return; }

			if( Selection.IsSelected( this ) )
			{
				return;
			}
			this.hud.SetActive( true );
		}

		public void OnMouseStayListener()
		{ }

		public void OnMouseExitListener()
		{
			if( Main.isHudLocked ) { return; }

			if( this.hasBeenHiddenSinceLastDamage )
			{
				return;
			}
			if( Selection.IsSelected( this ) )
			{
				return;
			}
			this.hud.SetActive( false );
		}

		void Update()
		{
			if( hud.activeSelf )
			{
				hud.transform.position = Main.camera.WorldToScreenPoint( this.transform.position );
			}

			if( !this.hasBeenHiddenSinceLastDamage )
			{
				return;
			}
			if( Main.isHudLocked )
			{
				return;
			}
			if( Selection.IsSelected( this ) )
			{
				return;
			}
			if( Time.time > this.lastDamageTakenTimestamp + SSObject.HUD_DAMAGE_DISPLAY_DURATION )
			{
				if( MouseOverHandler.currentObjectMouseOver == this.gameObject )
				{
					return;
				}
				this.hud.SetActive( false );
				this.hasBeenHiddenSinceLastDamage = false;
			}
		}

		public override void OnDisplay()
		{
			SelectionPanel.instance.obj.SetIcon( this.icon );

			SelectionPanel.instance.obj.displayNameText.text = this.displayName;

			GameObject titleUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), this.displayTitle );
			SelectionPanel.instance.obj.RegisterElement( "hero.title", titleUI.transform );

			GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -50.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), "Health: " + (int)this.health + "/" + (int)this.healthMax );
			SelectionPanel.instance.obj.RegisterElement( "hero.health", healthUI.transform );
		}

		public override void OnHide()
		{

		}
	}
}