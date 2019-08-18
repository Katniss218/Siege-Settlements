using UnityEngine;

namespace SS.UI
{
	/*[RequireComponent(typeof(RectTransform))]
	// Element inside the FlexibleGrid.
	public class FlexibleGridElement : MonoBehaviour
	{
		[SerializeField]
		private int __x;
		public int x
		{
			get
			{
				return this.__x;
			}
			set
			{
				this.__x = value;
				this.UpdatePositionAndSize();
			}
		}

		[SerializeField]
		private int __y;
		public int y
		{
			get
			{
				return this.__y;
			}
			set
			{
				this.__y = value;
				this.UpdatePositionAndSize();
			}
		}


		[SerializeField]
		private int __width;
		public int width
		{
			get
			{
				return this.__width;
			}
			set
			{
				this.__width = value;
				this.UpdatePositionAndSize();
			}
		}

		[SerializeField]
		private int __height;
		public int height
		{
			get
			{
				return this.__height;
			}
			set
			{
				this.__height = value;
				this.UpdatePositionAndSize();
			}
		}

		private FlexibleGrid grid;

		private RectTransform rectTransform;

		private void Awake()
		{

			this.rectTransform = this.GetComponent<RectTransform>();
			this.grid = this.transform.parent.GetComponent<FlexibleGrid>();
		}

		void Start()
		{
		}
		
		private void UpdatePositionAndSize()
		{
			this.rectTransform.pivot = Vector2.zero;
			this.rectTransform.anchorMin = Vector2.zero;
			this.rectTransform.anchorMax = Vector2.zero;
			this.rectTransform.anchoredPosition = new Vector2( this.grid.paddingLeft + this.x * this.grid.cellSize, this.grid.paddingRight + this.y * this.grid.cellSize );
			this.rectTransform.sizeDelta = new Vector2( this.width, this.height ) * this.grid.cellSize;
		}
		/*
#if UNITY_EDITOR
		void OnValidate()
		{
			this.rectTransform = this.GetComponent<RectTransform>();
			this.grid = this.transform.parent.GetComponent<FlexibleGrid>();

			this.UpdatePositionAndSize();
		}
#endif*
	}*/
}