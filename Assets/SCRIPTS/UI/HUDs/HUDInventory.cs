using SS.Content;
using SS.ResourceSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI.HUDs
{
	[RequireComponent( typeof( HudContainer ) )]
	public class HUDInventory : MonoBehaviour
	{
		private Image background = null;
		
		[SerializeField] private Image resourceIcon = null;
		[SerializeField] private TextMeshProUGUI amountText = null;
		
		void Awake()
		{
			HUDDFSC hudDFSC = this.GetComponent<HUDDFSC>();
			GameObject hud = CreateHudObject( hudDFSC.hudContainer.toggleable.transform, hudDFSC );

			this.background = hud.GetComponent<Image>();
			this.resourceIcon = hud.transform.Find( "__mask" ).GetChild( 0 ).GetComponent<Image>();
			this.amountText = hud.transform.Find( "Amount" ).GetComponent<TextMeshProUGUI>();
			hudDFSC.AddColored( this.background );
		}

		private static GameObject CreateHudObject( Transform parent, HUDDFSC hudDFSC )
		{
			if( hudDFSC is BuildingHUD )
			{
				return Instantiate<GameObject>( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/Inventory_Building" ), parent );
			}
			return Instantiate<GameObject>( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/Inventory_Unit" ), parent );
		}
		
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