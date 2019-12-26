using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
	public class HUDInterior : MonoBehaviour
	{
		[SerializeField] private Sprite upper1 = null;
		[SerializeField] private Sprite upper2 = null;
		[SerializeField] private Sprite upper3 = null;

		[SerializeField] private Sprite lower1 = null;
		[SerializeField] private Sprite lower2 = null;
		[SerializeField] private Sprite lower3 = null;

		[SerializeField] private Transform upperContainer = null;
		[SerializeField] private Transform lowerContainer = null;

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

		private void SpawnLower( int index )
		{
			GameObject go = new GameObject( "lower - " + index );
			RectTransform t = go.AddComponent<RectTransform>();
			t.SetParent( this.lowerContainer );

			Image image = go.AddComponent<Image>();
			image.sprite = GetSpriteLower( index, 6 );
		}

		private void SpawnUpper( int index )
		{
			GameObject go = new GameObject( "upper - " + index );
			RectTransform t = go.AddComponent<RectTransform>();
			t.SetParent( this.upperContainer );

			Image image = go.AddComponent<Image>();
			image.sprite = GetSpriteUpper( index, 6 );
		}

		public void SetSlotCount( int slots, int civilianSlots, int workerSlots )
		{
			for( int i = 0; i < workerSlots; i++ )
			{
				SpawnUpper( i );
			}
			for( int i = 0; i < slots + civilianSlots; i++ )
			{
				SpawnLower( i );
			}
			// worker slots on top, rest on bottom, one after another.
		}
	}
}