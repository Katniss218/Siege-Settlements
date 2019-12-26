using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
	public class HUDInterior : MonoBehaviour
	{
		public class Element
		{
			Image icon;

			public Element( Image icon )
			{
				this.icon = icon;
			}

			public void SetSprite( Sprite s )
			{
				icon.sprite = s;
				icon.gameObject.SetActive( true );
				icon.rectTransform.sizeDelta = s.rect.size / 2.0f;
			}

			public void ClearSprite()
			{
				icon.sprite = null;
				icon.gameObject.SetActive( false );
			}
		}

		[SerializeField] private Sprite upper1 = null;
		[SerializeField] private Sprite upper2 = null;
		[SerializeField] private Sprite upper3 = null;

		[SerializeField] private Sprite lower1 = null;
		[SerializeField] private Sprite lower2 = null;
		[SerializeField] private Sprite lower3 = null;

		[SerializeField] private Transform upperContainer = null;
		[SerializeField] private Transform lowerContainer = null;

		[SerializeField] private Vector2 spriteCenterOffset = Vector2.zero;

		public Element[] slots { get; private set; }
		public Element[] civilianSlots { get; private set; }
		public Element[] workerSlots { get; private set; }

		private Sprite GetSpriteUpper( int index, int rowLimit )
		{
			if( index == 0 )
			{
				return upper2;
			}
			if( index % rowLimit == 0 )
			{
				return upper1;
			}
			return upper3;
		}

		private Sprite GetSpriteLower( int index, int rowLimit )
		{
			if( index == 0 )
			{
				return lower2;
			}
			if( index % rowLimit == 0 )
			{
				return lower1;
			}
			return lower3;
		}
		
		private Image SpawnLower( int index )
		{
			GameObject go = new GameObject( "lower - " + index );
			RectTransform t = go.AddComponent<RectTransform>();
			t.SetParent( this.lowerContainer );

			Image image = go.AddComponent<Image>();
			image.sprite = GetSpriteLower( index, 6 );

			Mask mask = go.AddComponent<Mask>();

			t.position = Vector3.zero;

			GameObject icon = new GameObject( "ICON" );
			RectTransform t2 = icon.AddComponent<RectTransform>();
			t2.SetParent( t );
			t.localPosition = this.spriteCenterOffset;

			Image image2 = icon.AddComponent<Image>();
			icon.SetActive( false );

			return image2;
		}

		private Image SpawnUpper( int index )
		{
			GameObject go = new GameObject( "upper - " + index );
			RectTransform t = go.AddComponent<RectTransform>();
			t.SetParent( this.upperContainer );

			Image image = go.AddComponent<Image>();
			image.sprite = GetSpriteUpper( index, 6 );

			Mask mask = go.AddComponent<Mask>();

			t.position = Vector3.zero;

			GameObject icon = new GameObject( "ICON" );
			RectTransform t2 = icon.AddComponent<RectTransform>();
			t2.SetParent( t );
			t.localPosition = this.spriteCenterOffset;

			Image image2 = icon.AddComponent<Image>();
			icon.SetActive( false );

			return image2;
		}
		
		public Element GetSlotAny( int slotIndex )
		{
			if( slotIndex < this.slots.Length )
			{
				return this.slots[slotIndex];
			}
			if( slotIndex < this.civilianSlots.Length )
			{
				return this.civilianSlots[slotIndex - this.slots.Length];
			}
			if( slotIndex < this.workerSlots.Length )
			{
				return this.workerSlots[slotIndex - this.civilianSlots.Length];
			}
			return null;
		}

		public void SetSlotCount( int slots, int civilianSlots, int workerSlots )
		{
			this.slots = new Element[slots];
			this.civilianSlots = new Element[civilianSlots];
			this.workerSlots = new Element[workerSlots];

			for( int i = 0; i < workerSlots; i++ )
			{
				Image icon = SpawnUpper( i );
				this.workerSlots[i] = new Element( icon );
			}
			for( int i = 0; i < slots; i++ )
			{
				Image icon = SpawnLower( i );
				this.slots[i] = new Element( icon );
			}
			for( int i = 0; i < civilianSlots; i++ )
			{
				Image icon = SpawnLower( slots + i );
				this.civilianSlots[i] = new Element( icon );
			}
			// worker slots on top, rest on bottom, one after another.
		}
	}
}