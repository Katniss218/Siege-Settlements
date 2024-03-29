﻿using SS.Content;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
	[DisallowMultipleComponent]
	public class SelectionPanel : MonoBehaviour
	{
		/// <summary>
		/// The current mode that the Selection Panel is in.
		/// </summary>
		public SelectionPanelMode mode { get; private set; }
		
		/// <summary>
		/// Switches between the Object and List modes.
		/// </summary>
		public void SwitchMode()
		{
			if( this.mode == SelectionPanelMode.List )
			{
				this.SetModeObject();
			}
			else
			{
				this.SetModeList();
			}
		}

		public void SetMode( SelectionPanelMode mode )
		{
			if( this.mode == SelectionPanelMode.List )
			{
				this.SetModeList();
			}
			else
			{
				this.SetModeObject();
			}
		}

		private void SetModeObject()
		{
			this.mode = SelectionPanelMode.Object;

			this.selectionPanelImage.sprite = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/SelectionPanel" );
			this.list.transform.gameObject.SetActive( false );
			this.obj.transform.gameObject.SetActive( true );
		}

		private void SetModeList()
		{
			this.mode = SelectionPanelMode.List;

			this.selectionPanelImage.sprite = AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/SelectionPanelList" );
			this.list.transform.gameObject.SetActive( true );
			this.obj.transform.gameObject.SetActive( false );
		}
		
		public static SelectionPanel instance { get; private set; }
		public Transform moduleSubPanelTransform = null;



		
		[SerializeField] private SPObject __obj = null;
		public SPObject obj
		{
			get
			{
				return this.__obj;
			}
		}


		
		[SerializeField] private SPList __list = null;
		public SPList list
		{
			get
			{
				return this.__list;
			}
		}

		

		[SerializeField] private Image selectionPanelImage = null;


		private void Awake()
		{
			if( instance != null )
			{
				throw new System.Exception( "There is another selection panel active" );
			}
			instance = this;

			this.SetModeObject();
		}
	}
}