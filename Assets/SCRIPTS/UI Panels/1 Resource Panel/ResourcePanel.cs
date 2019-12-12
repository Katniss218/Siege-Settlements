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
		private class ResourceListEntry
		{
			public TMP_Text text;
			public Transform container;
			public int amount;
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

		public void InitReset()
		{
			ResourceDefinition[] definedRes = DefinitionManager.GetAllResources();
			
			this.SetEntries( definedRes );
		}
		
		private void AddEntry( string id, Sprite i, int amount )
		{
			GameObject container = Instantiate( (GameObject)AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Resource (UI)" ), resourceEntryContainer );
			container.name = "resource: '" + id + "'";

			Transform iconTransform = container.transform.Find( "Icon" );
			Image iconImage = iconTransform.GetComponent<Image>();
			iconImage.sprite = i;

			Transform textTransform = container.transform.Find( "Amount" );
			TextMeshProUGUI textText = textTransform.GetComponent<TextMeshProUGUI>();
			textText.text = amount.ToString();


			entries.Add( id, new ResourceListEntry() { container = container.transform, text = textText, amount = amount } );
		}

		public void UpdateResourceEntry( string id, int amount )
		{
			ResourceListEntry entry;
			if( entries.TryGetValue( id, out entry ) )
			{
				entry.amount = amount;
				entry.text.text = entry.amount.ToString();
			}
			else
			{
				throw new System.Exception( "Didn't find resource '" + id + "'." );
			}
		}

		public void UpdateResourceEntryDelta( string id, int amountDelta )
		{
			ResourceListEntry entry;
			if( entries.TryGetValue( id, out entry ) )
			{
				entry.amount += amountDelta;
				entry.text.text = entry.amount.ToString();
			}
			else
			{
				throw new System.Exception( "Didn't find resource '" + id + "'." );
			}
		}

		public void SetEntries( ResourceDefinition[] resources )
		{
			for( int i = 0; i < resources.Length; i++ )
			{
				this.AddEntry( resources[i].id, resources[i].icon, 0 );
			}
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