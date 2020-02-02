using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
	public class HUDInteriorSlot : MonoBehaviour
	{
		[SerializeField] private Image icon = null;

		[SerializeField] private Image healthBar = null;

		public Image background = null;
		
		public Image HealthBar { get { return this.healthBar; } }

		//
		//
		//
		
		public void SetHealth( float? percent )
		{
			if( percent == null )
			{
				if( healthBar.gameObject.activeSelf )
				{
					healthBar.gameObject.SetActive( false );
				}
			}
			else
			{
				if( !healthBar.gameObject.activeSelf )
				{
					healthBar.gameObject.SetActive( true );
				}
				healthBar.fillAmount = percent.Value;
			}
		}

		public void SetColor( Color color )
		{
			healthBar.color = color;
		}

		public void SetSprite( Sprite sprite )
		{
			if( !this.icon.gameObject.activeSelf )
			{
				this.icon.gameObject.SetActive( true );
			}

			this.icon.sprite = sprite;
			this.icon.rectTransform.sizeDelta = sprite.rect.size / 2.0f;
		}

		public void ClearSprite()
		{
			if( this.icon.gameObject.activeSelf )
			{
				this.icon.gameObject.SetActive( false );
			}
		}


		public void SetVisible( bool isVisible )
		{
			this.icon.color = isVisible ? Color.white : new Color( 0.4f, 0.4f, 0.4f, 0.7f );
		}
	}
}