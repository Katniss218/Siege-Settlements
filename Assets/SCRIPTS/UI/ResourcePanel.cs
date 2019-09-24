using Katniss.Utils;
using SS.Content;
using SS.ResourceSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
	public class ResourcePanel : MonoBehaviour
	{
		private struct ResourceListEntry
		{
			public TMP_Text text;
			public Transform container;
		}

		Dictionary<string, ResourceListEntry> entries = new Dictionary<string, ResourceListEntry>();

		[SerializeField] private RectTransform maskedListTransform = null;

		void Awake()
		{
			
		}

		// Start is called before the first frame update
		void Start()
		{
			List<ResourceDefinition> definedRes = DefinitionManager.GetAllOfType<ResourceDefinition>();

			for( int i = 0; i < definedRes.Count; i++ )
			{
				AddResourceEntry( definedRes[i] );
			}
		}

		// Update is called once per frame
		void Update()
		{

		}

		private void AddEntry( string id, Sprite i, int amount )
		{
			GameObject container;
			RectTransform containerTransform;
			GameObjectUtils.RectTransform( maskedListTransform, id, Vector2.zero, new Vector2( maskedListTransform.sizeDelta.x, 28 ), Vector2.up, Vector2.zero, Vector2.zero, out container, out containerTransform );
			
			GameObject iconGameObject;
			RectTransform iconTransform;
			GameObjectUtils.RectTransform( containerTransform, "Icon", Vector2.zero, new Vector2( 28, 28 ), Vector2.zero, Vector2.zero, Vector2.zero, out iconGameObject, out iconTransform );

			Image iconImage = iconGameObject.AddComponent<Image>();
			iconImage.sprite = i;
			
			GameObject textGameObject;
			RectTransform textTransform;
			GameObjectUtils.RectTransform( containerTransform, "Amount", Vector2.zero, new Vector2( maskedListTransform.sizeDelta.x - 28, 28 ), Vector2.one, Vector2.one, Vector2.one, out textGameObject, out textTransform );
			
			TextMeshProUGUI textText = textGameObject.AddComponent<TextMeshProUGUI>();
			textText.text = amount.ToString();
			textText.font = AssetManager.GetFont( FontManager.UI_FONT_PATH );
			textText.color = FontManager.darkColor;
			textText.fontSize = 14;
			textText.alignment = TextAlignmentOptions.Right;
			textText.enableWordWrapping = true;
			textText.overflowMode = TextOverflowModes.Overflow;

			textTransform.sizeDelta = new Vector2( maskedListTransform.sizeDelta.x - 28, 28 );
			
			entries.Add( id, new ResourceListEntry() { container = containerTransform, text = textText } );
		}
		
		public void UpdateResourceEntry( string id, int amount )
		{
			ResourceListEntry entry;
			if( entries.TryGetValue( id, out entry ) )
			{
				entry.text.text = amount.ToString();
			}
			else
			{
				throw new System.Exception( "Didn't find resource " + id );
			}
		}

		public void AddResourceEntry( ResourceDefinition resource, int startAmt = 0 )
		{
			AddEntry( resource.id, resource.icon.Item2, startAmt );
		}

		public void RemoveResourceEntry( ResourceDefinition resource )
		{
			ResourceListEntry entry;
			if( entries.TryGetValue( resource.id, out entry ) )
			{
				Destroy( entry.container.gameObject );
				entries.Remove( resource.id );
			}
		}
	}
}