﻿using Katniss.ModifierAffectedValues;
using SS.UI;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Objects.Heroes
{
	public class Hero : SSObjectDFS, IHUDHolder, IDamageable, INavMeshAgent, IFactionMember, IMouseOverHandlerListener
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
				this.hud.transform.Find( "Name" ).GetComponent<TextMeshProUGUI>().text = value;
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

		/// <summary>
		/// Returns the hud that's attached to this object.
		/// </summary>
		public HUD hud { get; set; }

		public bool hasBeenHiddenSinceLastDamage { get; set; }


		//
		//
		//

		float __movementSpeed;
		public float movementSpeed
		{
			get
			{
				return this.__movementSpeed;
			}
			set
			{
				this.__movementSpeed = value;
				if( this.movementSpeedOverride == null ) // if an override is not present - set the speed.
				{
					this.navMeshAgent.speed = value;
				}
			}
		}

		float? __movementSpeedOverride;
		public float? movementSpeedOverride
		{
			get
			{
				return this.__movementSpeedOverride;
			}
			set
			{
				this.__movementSpeedOverride = value;
				if( value != null ) // if the new override is not null - set the speed.
				{
					this.navMeshAgent.speed = value.Value;
				}
			}
		}

		float __rotationSpeed;
		public float rotationSpeed
		{
			get
			{
				return this.__rotationSpeed;
			}
			set
			{
				this.__rotationSpeed = value;
				if( this.rotationSpeedOverride == null ) // if an override is not present - set the speed.
				{
					this.navMeshAgent.angularSpeed = value;
				}
			}
		}
		float? __rotationSpeedOverride;
		public float? rotationSpeedOverride
		{
			get
			{
				return this.__rotationSpeedOverride;
			}
			set
			{
				this.__rotationSpeedOverride = value;
				if( value != null ) // if the new override is not null - set the speed.
				{
					this.navMeshAgent.angularSpeed = value.Value;
				}
			}
		}

		//
		//
		//
		
		void Update()
		{
			if( this.hud.isVisible )
			{
				this.hud.transform.position = Main.camera.WorldToScreenPoint( this.transform.position );
			}

			if( !this.hasBeenHiddenSinceLastDamage )
			{
				return;
			}
			if( Main.isHudForcedVisible )
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
				this.hud.isVisible = false;
				this.hasBeenHiddenSinceLastDamage = false;
			}
		}

		public void OnMouseEnterListener()
		{
			if( Main.isHudForcedVisible ) { return; }

			if( Selection.IsSelected( this ) )
			{
				return;
			}
			this.hud.isVisible = true;
		}

		public void OnMouseStayListener()
		{ }

		public void OnMouseExitListener()
		{
			if( Main.isHudForcedVisible ) { return; }

			if( this.hasBeenHiddenSinceLastDamage )
			{
				return;
			}
			if( Selection.IsSelected( this ) )
			{
				return;
			}
			this.hud.isVisible = false;
		}

		public override void OnDisplay()
		{
			SelectionPanel.instance.obj.SetIcon( this.icon );

			SelectionPanel.instance.obj.displayNameText.text = this.displayName;

			GameObject titleUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), this.displayTitle );
			SelectionPanel.instance.obj.RegisterElement( "hero.title", titleUI.transform );

			GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -50.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), SSObjectDFS.GetHealthDisplay( this.health, this.healthMax ) );
			SelectionPanel.instance.obj.RegisterElement( "hero.health", healthUI.transform );
		}

		public override void OnHide()
		{

		}
	}
}