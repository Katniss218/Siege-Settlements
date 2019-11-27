using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SS.UI
{
	public class ToolTipUIHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		private bool isMouseOver = false;

		public Action constructToolTip { get; set; }



		public void Update()
		{
			if( isMouseOver )
			{
				ToolTip.MoveTo( Input.mousePosition, true );
			}
		}

		public void OnDisable()
		{
			if( isMouseOver )
			{
				isMouseOver = false;
				ToolTip.Hide();
			}
		}

		public void OnPointerEnter( PointerEventData eventData )
		{
			this.isMouseOver = true;
			this.constructToolTip();
			ToolTip.ShowAt( Input.mousePosition );
		}

		public void OnPointerExit( PointerEventData eventData )
		{
			this.isMouseOver = false;
			ToolTip.Hide();
		}
	}
}