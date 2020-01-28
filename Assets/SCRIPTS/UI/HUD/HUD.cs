using SS.Objects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
	[DisallowMultipleComponent]
	public class HUD : MonoBehaviour
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

		/// <summary>
		/// The list of Image components that are affected by faction color.
		/// </summary>
		[SerializeField] Image[] colored = null;
		
		/// <summary>
		/// The Image component that displays current health percent.
		/// </summary>
		[SerializeField] Image healthBar = null;

		[SerializeField] TextMeshProUGUI selectionGroup = null;

		[SerializeField] GameObject unusableMark = null;

		[SerializeField] GameObject HUDt = null;

		public SSObjectDFSC hudHolder { get; set; }
		public byte? group { get; private set; }
		

		private bool __isVisible;
		public bool isVisible
		{
			get
			{
				return this.__isVisible;
			}
			set
			{
				this.__isVisible = value;
				this.HUDt.SetActive( value );
			}
		}
		
		/// <summary>
		/// If true, the reason for displaying the HUD was taking damage (in this case, the HUD wants to stay displayed for a period of time).
		/// </summary>
		public bool isDisplayedDueToDamage { get; set; }

		/// <summary>
		/// Snaps the HUD to it's holder SSObject.
		/// </summary>
		public void SnapToHolder()
		{
			this.transform.position = Main.camera.WorldToScreenPoint( this.hudHolder.transform.position );
		}



		/// <summary>
		/// Called by the hud holder whan it wants to show the hud (checks if should be shown).
		/// </summary>
		public void TryShow() // on mouse enter.
		{
			if( Main.isHudForcedVisible )
			{
				return;
			}
			if( Selection.IsSelected( this.hudHolder ) )
			{
				return;
			}
			this.isVisible = true;
		}

		/// <summary>
		/// Called by the hud holder when it wants to hide the hud (checks if should be hidden).
		/// </summary>
		public void TryHide() // on mouse exit.
		{
			if( Main.isHudForcedVisible )
			{
				return;
			}
			if( this.isDisplayedDueToDamage )
			{
				return;
			}
			if( Selection.IsSelected( this.hudHolder ) )
			{
				return;
			}
			this.isVisible = false;
		}


		//
		//		This is specific to holder being damageable & selectable.
		//


		void UpdateHideAfterDamage()
		{
			// Don't hide the HUD if DAMAGE_DISPLAY_DURATION seconds, after taking damage, didn't pass yet.
			if( Time.time <= this.hudHolder.lastDamageTakenTimestamp + DAMAGE_DISPLAY_DURATION )
			{
				return;
			}

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

			// Don't hide the HUD if the object is selected.
			if( Selection.IsSelected( this.hudHolder ) )
			{
				return;
			}

			// Don't hide if the hud holder is being moused over.
			if( MouseOverHandler.currentObjectMousedOver == this.hudHolder.gameObject )
			{
				return;
			}
			this.isVisible = false;
			this.isDisplayedDueToDamage = false;
		}


		//
		//
		//


		void Start()
		{
			
		}

		void Update()
		{
			// Move the HUD to the containing object.
			if( this.isVisible || this.group != null )
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
				selectionGroup.text = "" + ((group + 1)%10); // 0->1, 1->2, ..., 9->0
			}
		}

		public void SetUsable( bool isUsable )
		{
			this.unusableMark.SetActive( !isUsable );
		}

		/// <summary>
		/// Colors the specified image components with a given faction color.
		/// </summary>
		public void SetColor( Color c )
		{
			for( int i = 0; i < colored.Length; i++ )
			{
				colored[i].color = c;
			}

			HUDInterior interior = this.GetComponent<HUDInterior>();
			
			if( interior == null )
			{
				return;
			}
			if( interior.slots != null )
			{
				for( int i = 0; i < interior.slots.Length; i++ )
				{
					interior.slots[i].SetColor( c );
				}
			}
			if( interior.workerSlots != null )
			{
				for( int i = 0; i < interior.workerSlots.Length; i++ )
				{
					interior.workerSlots[i].SetColor( c );
				}
			}
		}

		/// <summary>
		/// Sets the fill amount of the health bar to the specified value (percent of health remaining).
		/// </summary>
		public void SetHealthBarFill( float percentHealth )
		{
			float scale = this.max - this.min;
			float p = percentHealth * scale;
			p += this.min;

			this.healthBar.fillAmount = p;
		}
	}
}