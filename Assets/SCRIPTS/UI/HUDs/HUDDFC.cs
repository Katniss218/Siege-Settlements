using SS.Objects;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI.HUDs
{
	[DisallowMultipleComponent]
	[RequireComponent( typeof( HudContainer ) )]
	public class HUDDFC : MonoBehaviour
	{
		public const float DAMAGE_DISPLAY_DURATION = 1.0f;

		/// <summary>
		/// when the health is 0, the image will be filled this much.
		/// </summary>
		public float min = 0.25f;
		/// <summary>
		/// when the health is 1, the image will be filled this much.
		/// </summary>
		public float max = 0.75f;


		private List<Image> colored = new List<Image>();
		private Color color = Color.white;

		public void AddColored( Image image )
		{
			this.colored.Add( image );
			image.color = this.color;
		}
		public void RemoveColored( Image image )
		{
			this.colored.Remove( image );
		}


		protected Image healthBar = null;

		protected TextMeshProUGUI selectionGroup = null;


		private HudContainer __hudContainer = null;
		public HudContainer hudContainer
		{
			get
			{
				if( this.__hudContainer == null )
				{
					this.__hudContainer = this.GetComponent<HudContainer>();
				}
				return this.__hudContainer;
			}
		}

		public byte? group { get; private set; }




		/// <summary>
		/// If true, the reason for displaying the HUD was taking damage (in this case, the HUD wants to stay displayed for a period of time).
		/// </summary>
		public bool isDisplayedDueToDamage { get; set; }

		/// <summary>
		/// Snaps the HUD to it's holder SSObject.
		/// </summary>
		public void SnapToHolder()
		{
			this.transform.position = Main.camera.WorldToScreenPoint( this.hudContainer.holder.transform.position );
		}



		/// <summary>
		/// Called by the hud holder whan it wants to show the hud (checks if should be shown).
		/// </summary>
		public void ConditionalShow() // on mouse enter.
		{
			if( Main.isHudForcedVisible )
			{
				return;
			}
			if( this.hudContainer.holder is SSObjectDFC )
			{
				if( Selection.IsSelected( (SSObjectDFC)this.hudContainer.holder ) )
				{
					return;
				}
			}
			this.hudContainer.isVisible = true;
		}

		/// <summary>
		/// Called by the hud holder when it wants to hide the hud (checks if should be hidden).
		/// </summary>
		public void ConditionalHide() // on mouse exit.
		{
			if( Main.isHudForcedVisible )
			{
				return;
			}
			if( this.isDisplayedDueToDamage )
			{
				return;
			}
			if( this.hudContainer.holder is SSObjectDFC )
			{
				if( Selection.IsSelected( (SSObjectDFC)this.hudContainer.holder ) )
				{
					return;
				}
			}
			this.hudContainer.isVisible = false;
		}


		//
		//		This is specific to holder being damageable & selectable.
		//


		private void UpdateHideAfterDamage()
		{
			// Don't hide the hud if it's already hidden.
			if( !this.isDisplayedDueToDamage )
			{
				return;
			}

			// Don't hide the hud, if the HUD is forced visible.
			if( Main.isHudForcedVisible )
			{
				return;
			}

			if( this.hudContainer.holder is SSObjectDFC )
			{
				// Don't hide the HUD if DAMAGE_DISPLAY_DURATION seconds, after taking damage, didn't pass yet.
				if( Time.time <= ((SSObjectDFC)this.hudContainer.holder).lastDamageTakenTimestamp + DAMAGE_DISPLAY_DURATION )
				{
					return;
				}

				// Don't hide the HUD if the object is selected.
				if( Selection.IsSelected( (SSObjectDFC)this.hudContainer.holder ) )
				{
					return;
				}

				// Don't hide if the hud holder is being moused over.
				if( MouseOverHandler.currentObjectMousedOver == this.hudContainer.holder.gameObject )
				{
					return;
				}
			}

			this.hudContainer.isVisible = false;
			this.isDisplayedDueToDamage = false;
		}


		//
		//
		//


		void Update()
		{
			// Move the HUD to the containing object.
			if( this.hudContainer.isVisible || this.group != null )
			{
				this.SnapToHolder();
			}

			this.UpdateHideAfterDamage();
		}



		public void SetSelectionGroup( byte? group )
		{
			this.group = group;
			if( group == null )
			{
				selectionGroup.gameObject.SetActive( false );
			}
			else
			{
				if( !selectionGroup.gameObject.activeSelf )
				{
					selectionGroup.gameObject.SetActive( true );
				}
				selectionGroup.text = "" + ((group + 1) % 10); // 0->1, 1->2, ..., 9->0
			}
		}

		/// <summary>
		/// Colors the specified image components with a given faction color.
		/// </summary>
		public void SetColor( Color color )
		{
			this.color = color;

			for( int i = 0; i < this.colored.Count; i++ )
			{
				this.colored[i].color = color;
			}
		}

		/// <summary>
		/// Sets the fill amount of the health bar to the specified value (percent of health remaining).
		/// </summary>
		public void SetHealthBarFill( float percentHealth )
		{
			float scale = this.max - this.min;
			float fillAmount = (percentHealth * scale) + this.min;

			this.healthBar.fillAmount = fillAmount;
		}
	}
}