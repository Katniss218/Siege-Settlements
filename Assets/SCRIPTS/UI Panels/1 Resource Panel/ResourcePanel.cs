using SS.Content;
using SS.ResourceSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
	[DisallowMultipleComponent]
	public class ResourcePanel : MonoBehaviour
	{
		private struct ResourceListEntry
		{
			public TMP_Text text;
			public Transform container;
		}

		private Dictionary<string, ResourceListEntry> entries = new Dictionary<string, ResourceListEntry>();

		[SerializeField] private Transform resourceEntryContainer = null;


		public static ResourcePanel instance { get; private set; }
		
		void Awake()
		{
			if( instance != null )
			{
				throw new System.Exception( "There is another resource panel active" );
			}
			instance = this;
		}

		void Start()
		{
			ResourceDefinition[] definedRes = DefinitionManager.GetAllResources();

			for( int i = 0; i < definedRes.Length; i++ )
			{
				AddResourceEntry( definedRes[i] );
			}
		}

		void Update()
		{

		}

		private void AddEntry( string id, Sprite i, int amount )
		{
			GameObject container = Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Resource (UI)" ), resourceEntryContainer );
			container.name = "resource: '" + id + "'";

			Transform iconTransform = container.transform.Find( "Icon" );
			Image iconImage = iconTransform.GetComponent<Image>();
			iconImage.sprite = i;

			Transform textTransform = container.transform.Find( "Amount" );
			TextMeshProUGUI textText = textTransform.GetComponent<TextMeshProUGUI>();
			textText.text = amount.ToString();


			entries.Add( id, new ResourceListEntry() { container = container.transform, text = textText } );
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