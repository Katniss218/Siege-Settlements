using SS.Content;
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
		private static GameObject __text = null;
		private static GameObject text
		{
			get
			{
				if( __text == null )
				{
					__text = AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/UI Elements/text" );
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
					__icon = AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/UI Elements/icon" );
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
					__textButton = AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/UI Elements/button_text" );
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
					__iconButton = AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/UI Elements/button_icon" );
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
					__scrollableGrid = AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/UI Elements/scrollable_grid" );
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
					__scrollableList = AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/UI Elements/scrollable_list" );
				}
				return __scrollableList;
			}
		}

		private static GameObject __valueBar = null;
		private static GameObject valueBar
		{
			get
			{
				if( __valueBar == null )
				{
					__valueBar = AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/UI Elements/value_bar" );
				}
				return __valueBar;
			}
		}




		public static GameObject InstantiateText( Transform parent, GenericUIData basicData, string text )
		{
			GameObject obj = Object.Instantiate( UIUtils.text, parent );
			obj.GetComponent<RectTransform>().ApplyUIData( basicData );

			EditText( obj, text );
			
			return obj;
		}
		public static void EditText( GameObject obj, string newText )
		{
			TextMeshProUGUI textMesh = obj.GetComponent<TextMeshProUGUI>();

			textMesh.text = newText;
		}


		public static GameObject InstantiateIcon( Transform parent, GenericUIData basicData, Sprite icon )
		{
			GameObject obj = Object.Instantiate( UIUtils.icon, parent );
			obj.GetComponent<RectTransform>().ApplyUIData( basicData );

			EditIcon( obj, icon );

			return obj;
		}
		public static void EditIcon( GameObject obj, Sprite newIcon )
		{
			Image imageComp = obj.GetComponent<Image>();

			imageComp.sprite = newIcon;
		}


		public static GameObject InstantiateTextButton( Transform parent, GenericUIData basicData, string text, UnityAction onClick )
		{
			GameObject obj = Object.Instantiate( textButton, parent );
			obj.GetComponent<RectTransform>().ApplyUIData( basicData );

			EditTextButton( obj, text, onClick );

			return obj;
		}
		public static void EditTextButton( GameObject obj, string newText, UnityAction newOnClick )
		{
			TextMeshProUGUI textMesh = obj.transform.GetChild( 0 ).GetComponent<TextMeshProUGUI>();

			textMesh.text = newText;

			Button button = obj.GetComponent<Button>();
			if( newOnClick == null )
			{
				button.interactable = false;
			}
			else
			{
				button.onClick.RemoveAllListeners();
				button.onClick.AddListener( newOnClick );
			}
		}


		public static GameObject InstantiateIconButton( Transform parent, GenericUIData basicData, Sprite icon, UnityAction onClick )
		{
			GameObject obj = Object.Instantiate( iconButton, parent );
			obj.GetComponent<RectTransform>().ApplyUIData( basicData );

			EditIconButton( obj, icon, onClick );

			return obj;
		}
		public static void EditIconButton( GameObject obj, Sprite newIcon, UnityAction newOnClick )
		{
			Image imageComp = obj.GetComponent<Image>();

			imageComp.sprite = newIcon;

			Button button = obj.GetComponent<Button>();
			if( newOnClick == null )
			{
				button.interactable = false;
			}
			else
			{
				button.onClick.RemoveAllListeners();
				button.onClick.AddListener( newOnClick );
			}
		}


		public static GameObject InstantiateScrollableGrid( Transform parent, GenericUIData basicData, float cellSize, GameObject[] contents )
		{
			GameObject obj = Object.Instantiate( scrollableGrid, parent );
			obj.GetComponent<RectTransform>().ApplyUIData( basicData );

			EditScrollableGrid( obj, cellSize );

			Transform listT = obj.transform.Find( "Contents" );
			for( int i = 0; i < contents.Length; i++ )
			{
				contents[i].transform.SetParent( listT );
			}

			return obj;
		}
		public static void EditScrollableGrid( GameObject obj, float newCellSize )
		{
			Transform listT = obj.transform.Find( "Contents" );
			listT.GetComponent<GridLayoutGroup>().cellSize = new Vector2( newCellSize, newCellSize );
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

		public static GameObject InstantiateValueBar( Transform parent, GenericUIData basicData, Color color, float fillPercent, string text )
		{
			GameObject obj = Object.Instantiate( UIUtils.valueBar, parent );
			obj.GetComponent<RectTransform>().ApplyUIData( basicData );

			EditValueBar( obj, color, fillPercent, text );

			return obj;
		}
		public static void EditValueBar( GameObject obj, Color newColor, float fillPercent, string newText )
		{
			Image imageComp = obj.transform.Find("i").GetComponent<Image>();
			TextMeshProUGUI textComp = obj.transform.Find("t").GetComponent<TextMeshProUGUI>();

			imageComp.color = newColor;
			imageComp.fillAmount = fillPercent;
			textComp.text = newText;
		}

	}
}