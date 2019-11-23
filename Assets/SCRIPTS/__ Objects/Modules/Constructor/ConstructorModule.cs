using SS.Objects.Buildings;
using SS.Content;
using SS.Diplomacy;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Technologies;
using SS.UI;
using System;
using UnityEngine;

namespace SS.Modules
{
	public class ConstructorModule : SSModule
	{
		public BuildingDefinition[] constructibleBuildings { get; set; }

		private FactionMember factionMember;
		
		private Selectable selectable;
				
		void Awake()
		{
			this.selectable = this.GetComponent<Selectable>();
			this.factionMember = this.GetComponent<FactionMember>();

			this.selectable.onHighlight.AddListener( this.ConstructorOnSelect );
			LevelDataManager.onTechStateChanged.AddListener( this.OnTechChange );

			Damageable damageable = this.GetComponent<Damageable>();
			damageable.onDeath.AddListener( onDeath );
		}
		
		void Update()
		{

		}

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
			}
			GameObject listUI = UIUtils.InstantiateScrollableGrid( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 30.0f, 5.0f ), new Vector2( -60.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
			SelectionPanel.instance.obj.RegisterElement( "constr.list", listUI.transform );
		}


		private void ConstructorOnSelect()
		{
			if( this.factionMember.factionId != LevelDataManager.PLAYER_FAC )
			{
				return;
			}
			const string TEXT = "Select building to place...";

			ShowList();

			// Create the actual UI.
			GameObject statusUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), TEXT );
			SelectionPanel.instance.obj.RegisterElement( "constr.status", statusUI.transform );
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
			if( !Selection.IsHighlighted( this.selectable ) )
			{
				return;
			}
			if( SelectionPanel.instance.obj.GetElement( "constr.list" ) != null )
			{
				SelectionPanel.instance.obj.Clear( "constr.list" );
			}
			ShowList();
		}

		private void onDeath()
		{
			LevelDataManager.onTechStateChanged.RemoveListener( this.OnTechChange );
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public override ModuleData GetData()
		{
			ConstructorModuleData saveState = new ConstructorModuleData();

			return saveState;
		}

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

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

			this.constructibleBuildings = new BuildingDefinition[def.constructibleBuildings.Length];
			for( int i = 0; i < this.constructibleBuildings.Length; i++ )
			{
				this.constructibleBuildings[i] = DefinitionManager.GetBuilding( def.constructibleBuildings[i] );
			}
		}
	}
}