using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SS.UI
{
	/// <summary>
	/// Class containing methods for creating standardized Siege Settlements' UI elements at runtime.
	/// </summary>
	public static class UIUtils
	{
		// TODO ----- Move these fields to AssetsManager?
		private static GameObject __text = null;
		private static GameObject text
		{
			get
			{
				if( __text == null )
				{
					__text = Resources.Load<GameObject>( "Prefabs/UI/text" );
				}
				return __text;
			}
		}

		private static GameObject __icon = null;
		private static GameObject icon
		{
			get
			{
				if( __icon == null )
				{
					__icon = Resources.Load<GameObject>( "Prefabs/UI/icon" );
				}
				return __icon;
			}
		}

		private static GameObject __textButton = null;
		private static GameObject textButton
		{
			get
			{
				if( __textButton == null )
				{
					__textButton = Resources.Load<GameObject>( "Prefabs/UI/button_text" );
				}
				return __textButton;
			}
		}

		private static GameObject __iconButton = null;
		private static GameObject iconButton
		{
			get
			{
				if( __iconButton == null )
				{
					__iconButton = Resources.Load<GameObject>( "Prefabs/UI/button_icon" );
				}
				return __iconButton;
			}
		}

		private static GameObject __scrollableGrid = null;
		private static GameObject scrollableGrid
		{
			get
			{
				if( __scrollableGrid == null )
				{
					__scrollableGrid = Resources.Load<GameObject>( "Prefabs/UI/scrollable_grid" );
				}
				return __scrollableGrid;
			}
		}

		private static GameObject __scrollableList = null;
		private static GameObject scrollableList
		{
			get
			{
				if( __scrollableList == null )
				{
					__scrollableList = Resources.Load<GameObject>( "Prefabs/UI/scrollable_list" );
				}
				return __scrollableList;
			}
		}

		public static GameObject InstantiateText( Transform parent, GenericUIData basicData, string text )
		{
			GameObject obj = Object.Instantiate( UIUtils.text, parent );
			obj.GetComponent<RectTransform>().ApplyUIData( basicData );

			TextMeshProUGUI textMesh = obj.GetComponent<TextMeshProUGUI>();

			textMesh.text = text;

			return obj;
		}

		public static GameObject InstantiateIcon( Transform parent, GenericUIData basicData, Sprite icon )
		{
			GameObject obj = Object.Instantiate( UIUtils.icon, parent );
			obj.GetComponent<RectTransform>().ApplyUIData( basicData );

			Image imageComp = obj.GetComponent<Image>();

			imageComp.sprite = icon;

			return obj;
		}

		public static GameObject InstantiateTextButton( Transform parent, GenericUIData basicData, string text, UnityAction onClick )
		{
			GameObject obj = Object.Instantiate( textButton, parent );
			obj.GetComponent<RectTransform>().ApplyUIData( basicData );

			TextMeshProUGUI textMesh = obj.transform.GetChild( 0 ).GetComponent<TextMeshProUGUI>();

			textMesh.text = text;

			Button button = obj.GetComponent<Button>();
			button.onClick.AddListener( onClick );
			if( onClick == null )
			{
				button.interactable = false;
			}

			return obj;
		}

		public static GameObject InstantiateIconButton( Transform parent, GenericUIData basicData, Sprite icon, UnityAction onClick )
		{
			GameObject obj = Object.Instantiate( iconButton, parent );
			obj.GetComponent<RectTransform>().ApplyUIData( basicData );

			Image imageComp = obj.GetComponent<Image>();

			imageComp.sprite = icon;

			Button button = obj.GetComponent<Button>();
			button.onClick.AddListener( onClick );
			if( onClick == null )
			{
				button.interactable = false;
			}

			return obj;
		}

		public static GameObject InstantiateScrollableGrid( Transform parent, GenericUIData basicData, float cellSize, GameObject[] contents )
		{
			GameObject obj = Object.Instantiate( scrollableGrid, parent );
			obj.GetComponent<RectTransform>().ApplyUIData( basicData );

			Transform listT = obj.transform.Find( "Contents" );
			listT.GetComponent<GridLayoutGroup>().cellSize = new Vector2( cellSize, cellSize );
			for( int i = 0; i < contents.Length; i++ )
			{
				contents[i].transform.SetParent( listT );
			}

			return obj;
		}

		public static GameObject InstantiateScrollableList( Transform parent, GenericUIData basicData, GameObject[] contents )
		{
			GameObject obj = Object.Instantiate( scrollableList, parent );
			obj.GetComponent<RectTransform>().ApplyUIData( basicData );

			Transform listT = obj.transform.Find( "Contents" );
			for( int i = 0; i < contents.Length; i++ )
			{
				contents[i].transform.SetParent( listT );
			}

			return obj;
		}
	}
}