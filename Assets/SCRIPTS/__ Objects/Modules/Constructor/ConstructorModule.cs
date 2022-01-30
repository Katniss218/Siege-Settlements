using SS.Objects.Buildings;
using SS.Content;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Technologies;
using SS.UI;
using UnityEngine;
using SS.ResourceSystem;
using System;

namespace SS.Objects.Modules
{
	public class ConstructorModule : SSModule, ISelectDisplayHandler
	{
		public const string KFF_TYPEID = "constructor";

		public BuildingDefinition[] constructibleBuildings { get; set; }


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		protected override void Awake()
		{			
			LevelDataManager.onTechStateChanged.AddListener( this.OnTechChange );

			this.ssObject.isSelectable = true;

			base.Awake();
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
						BuildingData buildingData = new BuildingData()
						{
							guid = Guid.NewGuid(),
							//position = this.transform.position, // gonna be reset to the position of the preview upon placing anyway.
							//rotation = this.transform.rotation,
							factionId = LevelDataManager.PLAYER_FAC,
							health = buildingDef.healthMax * Building.STARTING_HEALTH_PERCENT,
							constructionSaveState = new ConstructionSiteData()
						};

						BuildPreview.CreateOrSwitch( buildingDef, buildingData );
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
			GameObject listUI = UIUtils.InstantiateScrollableGrid( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 300.0f, 5.0f ), new Vector2( -330.0f, -30.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
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


		public override SSModuleData GetData()
		{
			ConstructorModuleData saveState = new ConstructorModuleData();

			return saveState;
		}
		

		public override void SetData( SSModuleData _data )
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
			GameObject statusUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 137.5f, 0.0f ), new Vector2( -325.0f, 25.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Select building to place..." );
			SelectionPanel.instance.obj.RegisterElement( "constr.status", statusUI.transform );
		}

		public void OnHide()
		{

		}
	}
}