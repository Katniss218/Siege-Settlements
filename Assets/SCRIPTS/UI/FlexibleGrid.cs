using Katniss.Utils;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SS.UI
{
	/*/// <summary>
	/// A grid that allows you to populate specified cells with elements of given size.
	/// </summary>
	public class FlexibleGrid : MonoBehaviour
	{
		[SerializeField]
		private float __cellSize = 72.0f;
		public float cellSize
		{
			get
			{
				return this.__cellSize;
			}
			set
			{
				this.__cellSize = value;
				this.UpdatePositionAndSize();
			}
		}



		[SerializeField]
		private int __width = 4;
		public int width // num of cells
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
		private int __height = 4;
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



		[SerializeField]
		private float __paddingLeft = 0.0f;
		public float paddingLeft
		{
			get
			{
				return this.__paddingLeft;
			}
			set
			{
				this.__paddingLeft = value;
				this.UpdatePositionAndSize();
			}
		}

		[SerializeField]
		private float __paddingRight = 0.0f;
		public float paddingRight
		{
			get
			{
				return this.__paddingRight;
			}
			set
			{
				this.__paddingRight = value;
				this.UpdatePositionAndSize();
			}
		}

		[SerializeField]
		private float __paddingTop = 0.0f;
		public float paddingTop
		{
			get
			{
				return this.__paddingTop;
			}
			set
			{
				this.paddingTop = value;
				this.UpdatePositionAndSize();
			}
		}

		[SerializeField]
		private float __paddingBottom = 0.0f;
		public float paddingBottom
		{
			get
			{
				return this.__paddingBottom;
			}
			set
			{
				this.__paddingBottom = value;
				this.UpdatePositionAndSize();
			}
		}
		
		private List<FlexibleGridElement> elements = new List<FlexibleGridElement>(); // holds references to elements in the grid.

		private RectTransform rectTransform = null;
		
		void Start()
		{
			this.rectTransform = this.GetComponent<RectTransform>();
		}

		void OnTransformChildrenChanged()
		{
			elements.Clear();
			for( int i =0; i < this.transform.childCount; i++ )
			{
				FlexibleGridElement ele = this.transform.GetChild( i ).GetComponent<FlexibleGridElement>();
				if( ele == null )
				{
					continue;
				}
				else
				{
					for( int j = 0; j < elements.Count; j++ )
					{
						Rect r = new Rect( ele.x, ele.y, ele.width, ele.height );
						Rect r2 = new Rect( elements[j].x, elements[j].y, elements[j].width, elements[j].height );
						if( r.Overlaps( r2 ) )
						{
							Debug.LogError( "Two or more elements are overlapping each other" );
						}
					}
					elements.Add( ele );
				}
			}
		}

		private void UpdatePositionAndSize()
		{
			this.rectTransform.sizeDelta = new Vector2( this.width, this.height ) * this.cellSize + new Vector2( this.paddingLeft + this.paddingRight, this.paddingTop + this.paddingBottom );
		}
		
		private GameObject AddElement( string name, RectInt size )
		{
			GameObject gameObject = new GameObject( name );

			RectTransform rt = gameObject.AddComponent<RectTransform>();
			rt.SetParent( this.rectTransform );

			FlexibleGridElement ele = gameObject.AddComponent<FlexibleGridElement>();
			ele.x = size.x;
			ele.y = size.y;
			ele.width = size.width;
			ele.height = size.height;

			// creates element and adds it to the grid.
			// positions the element, etc.
			elements.Add( ele );

			return gameObject;
		}

		// adds a text display to the grid (specifies bottom-left corner, and size)
		public void AddText( RectInt size, string _text )
		{
			GameObject gameObject = AddElement( "Text", size );

			GameObject textGameObject;
			RectTransform textTransform;
			GameObjectUtils.RectTransform( gameObject.transform, "Text", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one, out textGameObject, out textTransform );

			TextMeshProUGUI text = textGameObject.AddComponent<TextMeshProUGUI>();
			text.text = _text;
			text.font = Main.mainFont;
			text.fontSize = 20;
			text.fontStyle = FontStyles.Normal;
			text.color = Color.white;
			// text
			// - color, size, font, style
		}

		// adds a icon display to the grid (specifies bottom-left corner, and size)
		public void AddIcon( RectInt size )
		{
			// color
			// icon
		}

		// adds a clickable button with text to the grid (specifies bottom-left corner, and size)
		public void AddButton( RectInt size, string text )
		{
			// color
			// icon
			// text
			// - color, size, font, style
		}

		// adds a clickable button (just the icon) to the grid (specifies bottom-left corner, and size)
		public GameObject AddButton( RectInt size, Sprite icon, Color tint, UnityAction onClick )
		{
			GameObject gameObject = AddElement( "Button (I)", size );

			GameObject imageGameObject;
			RectTransform imageTransform;
			GameObjectUtils.RectTransform( gameObject.transform, "Text", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one, out imageGameObject, out imageTransform );

			Image image = imageGameObject.AddComponent<Image>();
			image.sprite = icon;
			image.color = tint;

			Button button = imageGameObject.AddComponent<Button>();
			button.onClick.AddListener( onClick );
			// color
			// icon

			return gameObject;
		}

		// adds a sliding value bar with text overlay to the grid (specifies bottom-left corner, and size)
		public void AddValueBar( RectInt size )
		{
			// percentfill
			// color (tint)
			// sprite (nullable)
			// background color (tint)
			// background sprite (nullable)
			// overlay text
			// - color, size, font, style
		}

		// adds a gameObject to the grid, and resizes it.
		public void AddGameObject( RectInt resize, GameObject obj )
		{

		}
		/*
#if UNITY_EDITOR
		void OnValidate()
		{
			rectTransform = GetComponent<RectTransform>();

			UpdatePositionAndSize();
		}
#endif*
	}*/
}