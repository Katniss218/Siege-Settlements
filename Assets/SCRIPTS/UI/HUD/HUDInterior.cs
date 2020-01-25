using SS.Content;
using UnityEngine;

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
		
		public HUDInteriorSlot[] slots { get; private set; }
		public HUDInteriorSlot[] workerSlots { get; private set; }


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

		private HUDInteriorSlot SpawnLower( int index )
		{
			GameObject go = Object.Instantiate<GameObject>( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/slot" ), this.lowerContainer );

			HUDInteriorSlot slot = go.GetComponent<HUDInteriorSlot>();
			slot.background.sprite = GetSpriteLower( index, 6 );
			
			return slot;
		}

		private HUDInteriorSlot SpawnUpper( int index )
		{
			GameObject go = Object.Instantiate<GameObject>( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/slot" ), this.upperContainer );

			HUDInteriorSlot slot = go.GetComponent<HUDInteriorSlot>();
			slot.background.sprite = GetSpriteUpper( index, 6 );
			
			return slot;
		}
		
		public void SetSlotCount( int slots, int workerSlots )
		{
			this.slots = new HUDInteriorSlot[slots];
			this.workerSlots = new HUDInteriorSlot[workerSlots];

			for( int i = 0; i < workerSlots; i++ )
			{
				this.workerSlots[i] = SpawnUpper( i );
			}
			for( int i = 0; i < slots; i++ )
			{
				this.slots[i] = SpawnLower( i );
			}
		}
	}
}