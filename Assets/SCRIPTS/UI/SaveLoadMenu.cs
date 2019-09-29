using KFF;
using SS.Content;
using SS.Levels;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SS.UI
{
	public class SaveLoadMenu : MonoBehaviour
	{
		public struct SaveElementData
		{
			public string displayName; // the string displayed on the element.

			public string levelId;
			public string levelSaveStateId;

			public bool isSaveState; // if true, the element will be displayed with bold text and different style.
		}

		[SerializeField] private Transform listContentContainer = null;

		[SerializeField] private TMP_InputField levelNameInput = null;
		[SerializeField] private TMP_InputField levelSaveStateNameInput = null;


		private List<SaveElementData> CollectSaves()
		{
			// return every save that can be loaded.
			string[] directories = System.IO.Directory.GetDirectories( LevelManager.levelDirectoryPath );

			List<SaveElementData> ret = new List<SaveElementData>();

			KFFSerializer serializer;

			for( int i = 0; i < directories.Length; i++ )
			{
				string levelPath = directories[i] + System.IO.Path.DirectorySeparatorChar + "level.kff";
				if( System.IO.File.Exists( levelPath ) )
				{
					string levelId = System.IO.Path.GetFileNameWithoutExtension( directories[i] );


					serializer = KFFSerializer.ReadFromFile( levelPath, DefinitionManager.FILE_ENCODING );
					string levelName = serializer.ReadString( "DisplayName" );
					

					ret.Add( new SaveElementData() { displayName = levelName, levelId = levelId, levelSaveStateId = LevelManager.DEFAULT_LEVEL_SAVE_STATE_IDENTIFIER, isSaveState = false } );
					

					string[] saveStateDirectories = System.IO.Directory.GetDirectories( directories[i] + System.IO.Path.DirectorySeparatorChar + "SaveStates" );

					for( int j = 0; j < saveStateDirectories.Length; j++ )
					{
						string levelSaveStatePath = saveStateDirectories[j] + System.IO.Path.DirectorySeparatorChar + "level_save_state.kff";
						if( System.IO.File.Exists( levelSaveStatePath ) )
						{
							// Don't display default save states. Click on the level itself instead.
							string saveStateId = System.IO.Path.GetFileNameWithoutExtension( saveStateDirectories[j] );
							if( saveStateId == LevelManager.DEFAULT_LEVEL_SAVE_STATE_IDENTIFIER )
							{
								continue;
							}


							serializer = KFFSerializer.ReadFromFile( levelSaveStatePath, DefinitionManager.FILE_ENCODING );
							string levelSaveStateName = serializer.ReadString( "DisplayName" );
							
							ret.Add( new SaveElementData() { displayName = levelName + " - " + levelSaveStateName, levelId = levelId, levelSaveStateId = saveStateId, isSaveState = true } );
						}
					}
				}
			}

			return ret;
		}

		// Start is called before the first frame update
		void Start()
		{
			this.ForceRefresh();
		}

		// Update is called once per frame
		void Update()
		{

		}

		/// <summary>
		/// Forces the Save/Load Menu to refresh the list of saved levels.
		/// </summary>
		public void ForceRefresh()
		{
			this.RemoveAllSaveElements();

			List<SaveElementData> saveElements = this.CollectSaves();

			for( int i = 0; i < saveElements.Count; i++ )
			{
				this.SpawnSaveElement( saveElements[i] );
			}
		}

		private void RemoveAllSaveElements()
		{
			for( int i = 0; i < this.listContentContainer.childCount; i++ )
			{
				Object.Destroy( this.listContentContainer.GetChild( i ).gameObject );
			}
		}

		private GameObject SpawnSaveElement( SaveElementData data )
		{
			GameObject obj;
			if( data.isSaveState )
			{
				obj = Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Level Save State (UI)" ), this.listContentContainer );
			}
			else
			{
				obj = Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_IDENTIFIER + "Prefabs/Level (UI)" ), this.listContentContainer );
			}

			TextMeshProUGUI textMeshPro = obj.transform.GetChild( 0 ).GetChild( 0 ).GetComponent<TextMeshProUGUI>();
			textMeshPro.text = data.displayName;


			SaveLoadMenuElement ele = obj.GetComponent<SaveLoadMenuElement>();

			ele.levelId = data.levelId;
			ele.levelSaveStateId = data.levelSaveStateId;

			ele.levelNameInput = levelNameInput;
			ele.levelSaveStateNameInput = levelSaveStateNameInput;

			return obj;
		}

	}
}