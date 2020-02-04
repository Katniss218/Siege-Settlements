using SS.Content;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI.HUDs
{
	public class HeroHUD : HUDDFSC
	{
		public string displayName
		{
			set
			{
				this.displayNameText.text = value;
			}
		}

		public string displayTitle
		{
			set
			{
				this.displayTitleText.text = value;
			}
		}


		private TextMeshProUGUI displayNameText = null;
		private TextMeshProUGUI displayTitleText = null;
	

		public void Awake()
		{
			this.min = 0.2f;
			this.max = 0.8f;

			GameObject hud = Instantiate<GameObject>( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/HeroHUD" ), this.hudContainer.toggleable.transform );

			this.selectionGroup = Instantiate<GameObject>( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/selgroup" ), this.hudContainer.transform ).GetComponent<TextMeshProUGUI>();

			this.healthBar = hud.transform.Find( "Health Bar" ).GetComponent<Image>();
			this.displayNameText = hud.transform.Find( "Name" ).GetComponent<TextMeshProUGUI>();
			this.displayTitleText = hud.transform.Find( "Title" ).GetComponent<TextMeshProUGUI>();

			Image foreground = hud.transform.Find( "Foreground" ).GetComponent<Image>();

			this.AddColored( this.healthBar );
			this.AddColored( foreground );
		}
	}
}