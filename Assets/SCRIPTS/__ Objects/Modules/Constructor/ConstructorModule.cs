using SS.Objects.Buildings;
using SS.Content;
using SS.Diplomacy;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Technologies;
using SS.UI;
using System;
using UnityEngine;
using SS.ResourceSystem;

namespace SS.Objects.Modules
{
	public class ConstructorModule : SSModule, ISelectDisplayHandler
	{
		public const string KFF_TYPEID = "constructor";

		public BuildingDefinition[] constructibleBuildings { get; set; }

		private FactionMember __factionMember = null;
		public FactionMember factionMember
		{
			get
			{
				if( this.__factionMember == null )
				{
					this.__factionMember = this.GetComponent<FactionMember>();
				}
				return this.__factionMember;
			}
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		void Awake()
		{			
			LevelDataManager.onTechStateChanged.AddListener( this.OnTechChange );
		}

		void OnDestroy()
		{
			LevelDataManager.onTechStateChanged.RemoveListener( this.OnTechChange );
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		private void ShowList()
		{
			GameObject[] gridElements = new GameObject[this.constructibleBuildings.Length];

			// Initialize the grid elements' GameObjects.
			for( int i = 0; i < this.constructibleBuildings.Length; i++ )
			{
				BuildingDefinition buildingDef = this.constructibleBuildings[i];

				// If the unit's techs required have not been researched yet, add unclickable button, otherwise, add normal button.
				if( TechLock.CheckLocked( buildingDef, LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].GetAllTechs() ) )
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), buildingDef.icon, null );
				}
				else
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), buildingDef.icon, () =>
					{
						if( BuildPreview.isActive )
						{
							BuildPreview.Switch( buildingDef );
							return;
						}
						BuildPreview.Create( buildingDef );
					} );
				}
				ToolTipUIHandler toolTipUIhandler = gridElements[i].AddComponent<ToolTipUIHandler>();
				toolTipUIhandler.constructToolTip = () =>
				{
					ToolTip.Create( 450.0f, buildingDef.displayName );

					ToolTip.AddText( "Health: " + buildingDef.healthMax );
					ToolTip.Style.SetPadding( 100, 100 );
					foreach( var kvp in buildingDef.cost )
					{
						ResourceDefinition resourceDef = DefinitionManager.GetResource( kvp.Key );
						ToolTip.AddText( resourceDef.icon, kvp.Value.ToString() );
					}
				};
			}
			GameObject listUI = UIUtils.InstantiateScrollableGrid( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 30.0f, 5.0f ), new Vector2( -60.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
			SelectionPanel.instance.obj.RegisterElement( "constr.list", listUI.transform );
		}

		


		private void OnTechChange( int factionId, string id, TechnologyResearchProgress newProgress )
		{
			if( factionId != this.factionMember.factionId )
			{
				return;
			}
			if( factionId != LevelDataManager.PLAYER_FAC )
			{
				return;
			}
			if( !Selection.IsDisplayedModule( this ) )
			{
				return;
			}
			if( SelectionPanel.instance.obj.GetElement( "constr.list" ) != null )
			{
				SelectionPanel.instance.obj.ClearElement( "constr.list" );
			}
			this.ShowList();
		}



		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public override ModuleData GetData()
		{
			ConstructorModuleData saveState = new ConstructorModuleData();

			return saveState;
		}
		

		public override void SetDefData( ModuleDefinition _def, ModuleData _data )
		{
			if( !(_def is ConstructorModuleDefinition) )
			{
				throw new Exception( "Provided definition is not of the correct type." );
			}
			if( _def == null )
			{
				throw new Exception( "Provided definition is null." );
			}

			if( !(_data is ConstructorModuleData) )
			{
				throw new Exception( "Provided data is not of the correct type." );
			}
			if( _data == null )
			{
				throw new Exception( "Provided data is null." );
			}

			ConstructorModuleDefinition def = (ConstructorModuleDefinition)_def;
			ConstructorModuleData data = (ConstructorModuleData)_data;

			this.icon = def.icon;
			this.constructibleBuildings = new BuildingDefinition[def.constructibleBuildings.Length];
			for( int i = 0; i < this.constructibleBuildings.Length; i++ )
			{
				this.constructibleBuildings[i] = DefinitionManager.GetBuilding( def.constructibleBuildings[i] );
			}
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public void OnDisplay()
		{
			if( this.factionMember.factionId != LevelDataManager.PLAYER_FAC )
			{
				return;
			}

			this.ShowList();

			// Create the actual UI.
			GameObject statusUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Select building to place..." );
			SelectionPanel.instance.obj.RegisterElement( "constr.status", statusUI.transform );
		}
	}
}