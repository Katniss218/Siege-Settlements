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
		[SerializeField] private TextMeshProUGUI populationCounter = null;


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

		public void SetEntries( ResourceDefinition[] resources )
		{
			this.RemoveAllEntries();
			for( int i = 0; i < resources.Length; i++ )
			{
				this.AddEntry( resources[i].id, resources[i].icon, 0 );
			}
		}

		public void UpdateResourceEntry( string id, int amountTotal, int amountStored )
		{
			ResourceListEntry entry;
			if( entries.TryGetValue( id, out entry ) )
			{
				entry.amount = amountTotal;
				entry.text.text = amountTotal + " (" + amountStored + ")";
			}
			else
			{
				throw new System.Exception( "Didn't find resource '" + id + "'." );
			}
		}

		public void UpdatePopulationDisplay( int amount )
		{
			this.populationCounter.text = "pop: " + amount;
		}

		public void RemoveAllEntries()
		{
			foreach( var kvp in this.entries )
			{
				Object.Destroy( kvp.Value.container.gameObject );
			}
			this.entries.Clear();
		}
		
		public void RemoveResourceEntry( ResourceDefinition resource )
		{
			ResourceListEntry entry;
			if( this.entries.TryGetValue( resource.id, out entry ) )
			{
				Object.Destroy( entry.container.gameObject );
				this.entries.Remove( resource.id );
			}
		}
	}
}