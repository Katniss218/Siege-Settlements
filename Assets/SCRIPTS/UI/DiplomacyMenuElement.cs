using SS.Content;
using SS.Diplomacy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiplomacyMenuElement : MonoBehaviour
{
	[SerializeField] private Image colorImage = null;
	[SerializeField] private Image relationImage = null;
	
	[SerializeField] private TextMeshProUGUI displayNameText = null;

	
	public void SetColor( Color c )
	{
		this.colorImage.color = c;
	}

	public void SetRelation( DiplomaticRelation rel )
	{
		if( rel == DiplomaticRelation.Ally )
		{
			relationImage.sprite = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Textures/diplo_ally" );
		}
		else if( rel == DiplomaticRelation.Neutral )
		{
			relationImage.sprite = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Textures/diplo_neutral" );
		}
		else if( rel == DiplomaticRelation.Enemy )
		{
			relationImage.sprite = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Textures/diplo_enemy" );
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
