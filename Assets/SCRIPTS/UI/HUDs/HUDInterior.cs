using SS.Content;
using UnityEngine;

namespace SS.UI.HUDs
{
	[RequireComponent( typeof( HudContainer ) )]
	public class HUDInterior : MonoBehaviour
	{
		private static Sprite __upper1 = null;
		private static Sprite upper1
		{
			get
			{
				if( __upper1 == null ) { __upper1 = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/slot_u_1" ); }
				return __upper1;
			}
		}
		private static Sprite __upper2 = null;
		private static Sprite upper2
		{
			get
			{
				if( __upper2 == null ) { __upper2 = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/slot_u_2" ); }
				return __upper2;
			}
		}
		private static Sprite __upper3 = null;
		private static Sprite upper3
		{
			get
			{
				if( __upper3 == null ) { __upper3 = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/slot_u_3" ); }
				return __upper3;
			}
		}

		private static Sprite __lower1 = null;
		private static Sprite lower1
		{
			get
			{
				if( __lower1 == null ) { __lower1 = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/slot_l_1" ); }
				return __lower1;
			}
		}
		private static Sprite __lower2 = null;
		private static Sprite lower2
		{
			get
			{
				if( __lower2 == null ) { __lower2 = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/slot_l_2" ); }
				return __lower2;
			}
		}
		private static Sprite __lower3 = null;
		private static Sprite lower3
		{
			get
			{
				if( __lower3 == null ) { __lower3 = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/slot_l_3" ); }
				return __lower3;
			}
		}

		[SerializeField] private Transform upperContainer = null;
		[SerializeField] private Transform lowerContainer = null;

		public HUDInteriorSlot[] slots { get; private set; } = new HUDInteriorSlot[0];
		public HUDInteriorSlot[] workerSlots { get; private set; } = new HUDInteriorSlot[0];


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

		void Awake()
		{
			HUDDFC hudDFSC = this.GetComponent<HUDDFC>();
			GameObject hud = CreateHudObject( hudDFSC.hudContainer.toggleable.transform );

			this.upperContainer = hud.transform.Find( "Upper" ).transform;
			this.lowerContainer = hud.transform.Find( "Lower" ).transform;
		}
		
		private static GameObject CreateHudObject( Transform parent )
		{
			return Instantiate<GameObject>( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Object HUDs/Interior" ), parent );
		}
		

		public void SetSlotCount( int slots, int workerSlots )
		{
			HUDDFC hudDFSC = this.GetComponent<HUDDFC>();
			if( hudDFSC != null )
			{
				for( int i = 0; i < this.workerSlots.Length; i++ )
				{
					hudDFSC.RemoveColored( this.workerSlots[i].HealthBar );
				}
				for( int i = 0; i < this.slots.Length; i++ )
				{
					hudDFSC.RemoveColored( this.slots[i].HealthBar );
				}
			}

			for( int i = 0; i < this.workerSlots.Length; i++ )
			{
				Object.Destroy( this.workerSlots[i].gameObject );
			}
			for( int i = 0; i < this.slots.Length; i++ )
			{
				Object.Destroy( this.slots[i].gameObject );
			}

			this.slots = new HUDInteriorSlot[slots];
			this.workerSlots = new HUDInteriorSlot[workerSlots];

			for( int i = 0; i < workerSlots; i++ )
			{
				this.workerSlots[i] = this.SpawnUpper( i );
			}
			for( int i = 0; i < slots; i++ )
			{
				this.slots[i] = this.SpawnLower( i );
			}

			if( hudDFSC != null )
			{
				for( int i = 0; i < workerSlots; i++ )
				{
					hudDFSC.AddColored( this.workerSlots[i].HealthBar );
				}
				for( int i = 0; i < slots; i++ )
				{
					hudDFSC.AddColored( this.slots[i].HealthBar );
				}
			}
		}
	}
}