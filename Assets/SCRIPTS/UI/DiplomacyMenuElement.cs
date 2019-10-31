using SS.Content;
using SS.Diplomacy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
	[DisallowMultipleComponent]
	public class DiplomacyMenuElement : MonoBehaviour
	{
		[SerializeField] private Image colorImage = null;
		[SerializeField] private Image relationImage = null;

		[SerializeField] private TextMeshProUGUI displayNameText = null;


		public void MarkAsPlayer()
		{
			Object.Destroy( this.relationImage.gameObject );
		}

		public void SetColor( Color c )
		{
			this.colorImage.color = c;
		}

		public void SetRelation( DiplomaticRelation relation )
		{
			if( relation == DiplomaticRelation.Ally )
			{
				this.relationImage.sprite = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/diplo_ally" );
			}
			else if( relation == DiplomaticRelation.Neutral )
			{
				this.relationImage.sprite = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/diplo_neutral" );
			}
			else if( relation == DiplomaticRelation.Enemy )
			{
				this.relationImage.sprite = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/diplo_enemy" );
			}
			else
			{
				throw new System.Exception( "Invalid DiplomaticRelation" );
			}
		}

		public void SetDisplayname( string t )
		{
			this.displayNameText.text = t;
		}
	}
}