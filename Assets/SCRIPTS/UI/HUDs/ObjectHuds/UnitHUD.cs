using SS.Content;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI.HUDs
{
	public class UnitHUD : HUDDFSC
	{
		public void Awake()
		{
			GameObject hud = CreateHudObject( this.hudContainer.toggleable.transform );
			this.HUDt = hud;

			this.healthBar = hud.transform.Find( "Health Bar" ).GetComponent<Image>();

			this.AddColored( this.healthBar );
		}

		private static GameObject CreateHudObject( Transform parent )
		{
			GameObject gameObject = Instantiate<GameObject>( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/UnitHUD" ), parent );

			return gameObject;
		}
	}
}