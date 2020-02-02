using SS.Objects;
using UnityEngine;

namespace SS.UI
{
	public class HudContainer : MonoBehaviour
	{
		public static HudContainer CreateGameObject( SSObject @object )
		{
			GameObject containerGameObject = new GameObject();
			RectTransform rect = containerGameObject.AddComponent<RectTransform>();
			rect.SetParent( Main.objectHUDCanvas );
			rect.pivot = Vector2.zero;

			GameObject toggleableGameObject = new GameObject();
			RectTransform rectToggleable = toggleableGameObject.AddComponent<RectTransform>();
			rectToggleable.SetParent( rect );
			rectToggleable.pivot = Vector2.zero;

			HudContainer container = containerGameObject.AddComponent<HudContainer>();
			container.holder = @object;
			container.toggleable = toggleableGameObject;
			container.toggleable.SetActive( Main.isHudForcedVisible );

			return container;
		}


		public SSObject holder { get; set; }

		private bool __isVisible;
		public bool isVisible
		{
			get
			{
				return this.__isVisible;
			}
			set
			{
				this.__isVisible = value;
				this.toggleable.SetActive( value );
			}
		}

		public GameObject toggleable;
	}
}
