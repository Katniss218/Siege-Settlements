﻿using SS.UI;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Objects.Units
{
	public class Unit : SSObjectDFS, IHUDHolder, IDamageable, INavMeshAgent, IFactionMember, IMouseOverHandlerListener
	{
		public HUD hud { get; set; }

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

		private PopulationSize __population = PopulationSize.x1;
		public PopulationSize population
		{
			get
			{
				return this.__population;
			}
			set
			{
				// Recalculate here, before setting '__population'. (we need the from size - '__population', and the to size - 'value')
#warning Damageable needs to play damage sound when the health gets down due to damage, but not when it goes down due to unit split.
				// Needs to have the reason (source) of gain/loss of health as another parameter.
				// - Meaning, we need parameter in the event, and takeDamage shouldn't call the event in the 'health''s setter.

#warning TODO! - maybe we set the values for '1x', but then set the modifiers for them to whatever pop is there, and the game just magically calculates the appropriate value?
	// So you can set the direct value for the '1x', but the modified-value is what you access normally. - when the modified-value is set, it scaled it from *current to *1 and assigns it to raw.
				// ^ ^ ^ ^ ^ needs more thinking.
				// - multiplier would need to be calculated, for non-linear scaling, by the population


				// scale modules' values. (get modules of type - for each module, set values) - repeat for all relevant module types.

				// melee damage,
				// ranged projectile count,
				// inventory size
				// 

				this.__population = value;
			}
		}
		
		void Update()
		{
			if( hud.gameObject.activeSelf )
			{
#warning TODO! - only if the camera or transform has moved or rotated or scaled (cam).
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
				this.hud.gameObject.SetActive( false );
				this.hasBeenHiddenSinceLastDamage = false;
			}
		}

		public void OnMouseEnterListener()
		{
			if( Main.isHudLocked ) { return; }

			if( Selection.IsSelected( this ) )
			{
				return;
			}
			this.hud.gameObject.SetActive( true );
		}

		public void OnMouseStayListener()
		{

		}

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
			this.hud.gameObject.SetActive( false );
		}



		public override void OnDisplay()
		{
			SelectionPanel.instance.obj.SetIcon( this.icon );

			SelectionPanel.instance.obj.displayNameText.text = this.displayName;

			GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 300.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ), new Vector2( 0.5f, 1.0f ) ), "Health: " + (int)this.health + "/" + (int)this.healthMax );
			SelectionPanel.instance.obj.RegisterElement( "unit.health", healthUI.transform );
		}

		public override void OnHide()
		{

		}
	}
}