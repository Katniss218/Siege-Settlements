using SS.Objects.Buildings;
using SS.Content;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Technologies;
using SS.UI;
using UnityEngine;
using SS.ResourceSystem;

namespace SS.Objects.Modules
{
	public class ConstructorModule : SSModule, ISelectDisplayHandler
	{
		public const string KFF_TYPEID = "constructor";

		public BuildingDefinition[] constructibleBuildings { get; set; }
		

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
			GameObject listUI = UIUtils.InstantiateScrollableGrid( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 300.0f, 5.0f ), new Vector2( -330.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
			SelectionPanel.instance.obj.RegisterElement( "constr.list", listUI.transform );
		}

		


		private void OnTechChange( int factionId, string id, TechnologyResearchProgress newProgress )
		{
			if( factionId != (this.ssObject as IFactionMember).factionId )
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
			SelectionPanel.instance.obj.TryClearElement( "constr.list" );
			
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
		

		public override void SetData( ModuleData _data )
		{
			ConstructorModuleData data = ValidateDataType<ConstructorModuleData>( _data );	
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		public void OnDisplay()
		{
			if( !this.ssObject.IsDisplaySafe() )
			{
				return;
			}

			this.ShowList();

			// Create the actual UI.
			GameObject statusUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 137.5f, 0.0f ), new Vector2( -325.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Select building to place..." );
			SelectionPanel.instance.obj.RegisterElement( "constr.status", statusUI.transform );
		}

		public void OnHide()
		{

		}
	}
}