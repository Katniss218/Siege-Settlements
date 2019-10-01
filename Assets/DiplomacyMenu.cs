using KFF;
using SS.Content;
using SS.Diplomacy;
using SS.Levels;
using UnityEngine;

public class DiplomacyMenu : MonoBehaviour
{
	[SerializeField] private Transform diplomacyListContents = null;

	private void SpawnDiplomacyMenuElement( FactionDefinition def, DiplomaticRelation rel )
	{
		GameObject obj = Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/DiplomacyElement (UI)" ), diplomacyListContents );

		DiplomacyMenuElement diplomacyMenuElement = obj.GetComponent<DiplomacyMenuElement>();

		diplomacyMenuElement.SetColor( def.color );
		diplomacyMenuElement.SetDisplayname( def.displayName );
		diplomacyMenuElement.SetRelation( rel );
	}

	private void Start()
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
		for( int i = 0; i < LevelDataManager.numFactions; i++ )
		{
			if( i == LevelDataManager.PLAYER_FAC )
			{
				SpawnDiplomacyMenuElement( LevelDataManager.factions[i], DiplomaticRelation.Ally );
			}
			else
			{
				SpawnDiplomacyMenuElement( LevelDataManager.factions[i], LevelDataManager.diplomaticRelations[LevelDataManager.PLAYER_FAC, i] );
			}
		}
	}

	public void Clear()
	{
		for( int i = 0; i < diplomacyListContents.childCount; i++)
		{
			Destroy( diplomacyListContents.GetChild( i ).gameObject );
		}
	}
}