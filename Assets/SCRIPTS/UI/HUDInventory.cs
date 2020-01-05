using SS.ResourceSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
	public class HUDInventory : MonoBehaviour
	{
		[SerializeField] private Image resourceIcon = null;
		[SerializeField] private TextMeshProUGUI amountText = null;

		public void DisplayResource( ResourceDefinition def, int amount, bool plus )
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
			if( plus )
			{
				amountText.text += "*";
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
}