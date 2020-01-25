using SS.Content;
using SS.Levels;
using SS.Objects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	class SelectionUtils
	{
		public static void Select( SSObjectDFS[] uniqueSelectables, SelectionMode selectionMode )
		{
			// Select selectables on the list (if not selected).
			if( selectionMode == SelectionMode.Add )
			{
				if( uniqueSelectables != null )
				{
					int numSelected = Selection.TrySelect( uniqueSelectables );

					if( numSelected > 0 )
					{
						AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/select" ), Main.cameraPivot.position );
					}
				}
				return;
			}
			// Deselect already selected that are not present on the list.
			// Select selectables on the list (if not selected).
			if( selectionMode == SelectionMode.Replace )
			{
				if( uniqueSelectables == null )
				{
					if( Selection.GetSelectedObjects().Length == 0 )
					{
						return;
					}

					Selection.DeselectAll();
					AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/deselect" ), Main.cameraPivot.position );
				}
				else
				{
					SSObjectDFS[] selectedObjs = Selection.GetSelectedObjects();
					bool playDeselect = false;

					for( int i = 0; i < selectedObjs.Length; i++ )
					{
						bool isToBeSelected = false;
						for( int j = 0; j < uniqueSelectables.Length; j++ )
						{
							if( selectedObjs[i] == uniqueSelectables[j] )
							{
								isToBeSelected = true;
							}
						}
						if( !isToBeSelected )
						{
							Selection.Deselect( selectedObjs[i] );
							playDeselect = true;
						}
					}
					int numSelected = Selection.TrySelect( uniqueSelectables );

					if( numSelected > 0 )
					{
						AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/select" ), Main.cameraPivot.position );
					}
					if( playDeselect )
					{
						AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/deselect" ), Main.cameraPivot.position );
					}
				}
				return;
			}

			throw new Exception( "Invalid selection mode" );
		}


		public static void SelectTheSame( string definitionId, SelectionMode selectionMode )
		{
			SSObjectDFS[] selectables = SSObjectDFS.GetAllDFS();

			List<SSObjectDFS> sameIdAndWithinView = new List<SSObjectDFS>();
			for( int i = 0; i < selectables.Length; i++ )
			{
				if( selectables[i].definitionId != definitionId )
				{
					continue;
				}

				if( selectables[i].factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}

				Vector3 viewportPos = Main.camera.WorldToViewportPoint( selectables[i].transform.position );
				if( (viewportPos.x < 0 || viewportPos.x > 1) || (viewportPos.y < 0 || viewportPos.y > 1) )
				{
					continue;
				}

				sameIdAndWithinView.Add( selectables[i] );
			}

			SSObjectDFS[] array = sameIdAndWithinView.ToArray();
			Select( array, selectionMode );
		}

		public static void SelectOnScreen( Vector2 pos1, Vector2 pos2, SelectionMode selectionMode )
		{
			SSObjectDFS[] selectables = SSObject.GetAllDFS();

			List<SSObjectDFS> ret = new List<SSObjectDFS>();

			for( int i = 0; i < selectables.Length; i++ )
			{
				if( Main.camera.InContainedScreen( selectables[i].gameObject.transform.position, pos1, pos2 ) )
				{
					ret.Add( selectables[i] );
				}
			}
			if( ret.Count == 0 )
			{
				SelectionUtils.Select( null, selectionMode );
			}
			else
			{
				SelectionUtils.Select( ret.ToArray(), selectionMode );
			}
		}
	}
}
