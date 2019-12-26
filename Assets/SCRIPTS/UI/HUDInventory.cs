using SS.ResourceSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDInventory : MonoBehaviour
{
	[SerializeField] private Image resourceIcon;
	[SerializeField] private TextMeshProUGUI amountText;
	
	public void DisplayResource( ResourceDefinition def, int amount )
	{
		resourceIcon.sprite = def.icon;
		if( !resourceIcon.gameObject.activeSelf )
		{
			resourceIcon.gameObject.SetActive( true );
		}
		amountText.text = "" + amount;
		if( !amountText.gameObject.activeSelf )
		{
			amountText.gameObject.SetActive( true );
		}
	}

	public void HideResource()
	{
		resourceIcon.sprite = null;
		resourceIcon.gameObject.SetActive( false );
		amountText.text = "";
		amountText.gameObject.SetActive( false );
	}
}
