using SS.Content;
using SS.Diplomacy;
using SS.Levels;
using UnityEngine;

namespace SS.UI
{
	[DisallowMultipleComponent]
	public class DiplomacyMenu : MonoBehaviour
	{
		[SerializeField] private Transform diplomacyListContents = null;

		private void SpawnDiplomacyMenuElement( FactionDefinition def, DiplomaticRelation rel )
		{
			GameObject obj = Instantiate( (GameObject)AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/DiplomacyElement (UI)" ), diplomacyListContents );

			DiplomacyMenuElement diplomacyMenuElement = obj.GetComponent<DiplomacyMenuElement>();

			diplomacyMenuElement.SetColor( def.color );
			diplomacyMenuElement.SetDisplayname( def.displayName );
			diplomacyMenuElement.SetRelation( rel );
		}

		private void SpawnDiplomacyMenuElementPlayer( FactionDefinition def )
		{
			GameObject obj = Instantiate( (GameObject)AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/DiplomacyElement (UI)" ), diplomacyListContents );

			DiplomacyMenuElement diplomacyMenuElement = obj.GetComponent<DiplomacyMenuElement>();

			diplomacyMenuElement.SetColor( def.color );
			diplomacyMenuElement.SetDisplayname( def.displayName );
			diplomacyMenuElement.MarkAsPlayer();
		}

		void OnEnable()
		{
			LevelDataManager.onRelationChanged.AddListener( this.OnRelationChangedListener );
		}

		void OnDisable()
		{
			LevelDataManager.onRelationChanged.RemoveListener( this.OnRelationChangedListener );
		}

		void Start()
		{
			ForceRefresh();
		}

		private void OnRelationChangedListener( int fac1, int fac2, DiplomaticRelation rel )
		{
			ForceRefresh();
		}

		public void ForceRefresh()
		{
			Clear();
			Init();
		}

		public void Init()
		{
			for( int i = 0; i < LevelDataManager.factionCount; i++ )
			{
				if( i == LevelDataManager.PLAYER_FAC )
				{
					SpawnDiplomacyMenuElementPlayer( LevelDataManager.factions[i] );
				}
				else
				{
					SpawnDiplomacyMenuElement( LevelDataManager.factions[i], LevelDataManager.GetRelation(LevelDataManager.PLAYER_FAC, i) );
				}
			}
		}

		public void Clear()
		{
			for( int i = 0; i < diplomacyListContents.childCount; i++ )
			{
				Object.Destroy( diplomacyListContents.GetChild( i ).gameObject );
			}
		}
	}
}