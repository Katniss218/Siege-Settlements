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

		public void DisplayResource( ResourceDefinition def, int amount, bool displayStar )
		{
			if( !resourceIcon.gameObject.activeSelf )
			{
				resourceIcon.gameObject.SetActive( true );
			}
			if( !amountText.gameObject.activeSelf )
			{
				amountText.gameObject.SetActive( true );
			}

			resourceIcon.sprite = def.icon;
			amountText.text = displayStar ? amount + "*" : amount + "";
		}

		public void HideResource()
		{
			resourceIcon.gameObject.SetActive( false );
			amountText.gameObject.SetActive( false );

			resourceIcon.sprite = null;
			amountText.text = "";
		}
	}
}