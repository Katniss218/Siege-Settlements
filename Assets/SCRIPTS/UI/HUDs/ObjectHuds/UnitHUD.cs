using SS.Content;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI.HUDs
{
	public class UnitHUD : HUDDFC
	{
		public void Awake()
		{
			GameObject hud = Instantiate<GameObject>( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/UnitHUD" ), this.hudContainer.toggleable.transform );

			this.selectionGroup = Instantiate<GameObject>( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/selgroup" ), this.hudContainer.transform ).GetComponent<TextMeshProUGUI>();

			this.healthBar = hud.transform.Find( "Health Bar" ).GetComponent<Image>();

			this.AddColored( this.healthBar );
		}
	}
}