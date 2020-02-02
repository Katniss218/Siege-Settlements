using SS.Content;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI.HUDs
{
	public class BuildingHUD : HUDDFSC
	{
		protected GameObject unusableMark = null;
		
		public void Awake()
		{
			this.min = 0.0f;
			this.max = 1.0f;

			GameObject hud = Instantiate<GameObject>( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/BuildingHUD" ), this.hudContainer.toggleable.transform );
			this.unusableMark = Instantiate<GameObject>( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/unusable_mark" ), this.hudContainer.transform );
			this.HUDt = hud;

			this.healthBar = hud.transform.Find( "Health Bar" ).GetComponent<Image>();

			this.AddColored( this.healthBar );
		}
		

		public void SetUsable( bool isUsable )
		{
			this.unusableMark.SetActive( !isUsable );
		}
	}
}
