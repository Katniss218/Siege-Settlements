using SS.Buildings;
using SS.Content;
using SS.Diplomacy;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.ResourceSystem;
using SS.ResourceSystem.Payment;
using SS.Technologies;
using SS.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace SS.Modules
{
	[RequireComponent( typeof( FactionMember ) )]
	public class ResearchModule : Module, IPaymentReceiver
	{
		public UnityEvent onResearchBegin = new UnityEvent();

		public UnityEvent onResearchProgress = new UnityEvent();

		public UnityEvent onResearchEnd = new UnityEvent();
		
		private ResearchModuleDefinition def;

		public TechnologyDefinition[] researchableTechnologies { get; set; }

		/// <summary>
		/// Contains the currently researched technology (Read only).
		/// </summary>
		public TechnologyDefinition researchedTechnology { get; private set; }

		/// <summary>
		/// Contains the progress of the research (Read Only).
		/// </summary>
		public float researchProgress { get; private set; }

		/// <summary>
		/// Contains the multiplier used as the construction speed.
		/// </summary>
		public float researchSpeed { get; set; }

		/// <summary>
		/// returns true if the research module is currently researching a technology (after payment has been completed).
		/// </summary>
		public bool isResearching
		{
			get
			{
				return this.researchedTechnology != null;
			}
		}

		private FactionMember factionMember;

		private Dictionary<string, int> resourcesRemaining = new Dictionary<string, int>();

		private Selectable selectable;

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		private bool IsPaymentDone()
		{
			if( resourcesRemaining == null )
			{
				return true;
			}
			foreach( var kvp in this.resourcesRemaining )
			{
				if( kvp.Value > 0 )
				{
					return false;
				}
			}
			return true;
		}



		public void ReceivePayment( string id, int amount )
		{
			if( resourcesRemaining == null )
			{
				Debug.LogWarning( "The payment of " + amount + "x '" + id + "' was not needed." );
				return;
			}
			if( this.resourcesRemaining.ContainsKey( id ) )
			{
				this.resourcesRemaining[id] -= amount;
				if( this.resourcesRemaining[id] < 0 )
				{
					this.resourcesRemaining[id] = 0;
				}
			}

			if( Selection.IsHighlighted( this.selectable ) )
			{
				Transform statusUI = SelectionPanel.instance.obj.GetElement( "research.status" );
				if( statusUI != null )
				{
					UIUtils.EditText( statusUI.gameObject, "Waiting for resources ('" + this.researchedTechnology.displayName + "'): " + Status() );
				}
			}
		}

		public Dictionary<string, int> GetWantedResources()
		{
			Dictionary<string, int> ret = new Dictionary<string, int>();
			if( this.resourcesRemaining == null )
			{
				return ret;
			}
			foreach( var kvp in this.resourcesRemaining )
			{
				if( kvp.Value > 0 )
				{
					ret.Add( kvp.Key, kvp.Value );
				}
			}
			return ret;
		}

		/// <summary>
		/// Begins researching a technology.
		/// </summary>
		/// <param name="def">The technology to research.</param>
		public void StartResearching( TechnologyDefinition def )
		{
			if( this.isResearching )
			{
				throw new Exception( "There is already technology being researched." );
			}
			this.researchedTechnology = def;
			this.researchProgress = 10.0f;

			this.resourcesRemaining = new Dictionary<string, int>( this.researchedTechnology.cost );
			this.onResearchBegin?.Invoke();
		}

		// Start is called before the first frame update
		void Awake()
		{
			this.factionMember = GetComponent<FactionMember>();

			this.selectable = this.GetComponent<Selectable>();
		}

		// Update is called once per frame
		void Update()
		{
			if( IsPaymentDone() )
			{
				if( this.isResearching )
				{
					this.researchProgress -= this.researchSpeed * Time.deltaTime;
					if( this.researchProgress <= 0 )
					{
						LevelDataManager.SetTech( this.factionMember.factionId, this.researchedTechnology.id, TechnologyResearchProgress.Researched );
						this.researchedTechnology = null;
						this.researchProgress = 0.0f;
						this.resourcesRemaining = null;
						this.onResearchEnd?.Invoke();
					}
					else
					{
						this.onResearchProgress?.Invoke();
					}
				}
			}
		}

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		/// <summary>
		/// Creates a new BarracksModuleSaveState from a GameObject.
		/// </summary>
		/// <param name="unit">The GameObject to extract the save state from.</param>
		public override ModuleData GetData()
		{
			ResearchModuleData saveState = new ResearchModuleData();

			saveState.resourcesRemaining = this.resourcesRemaining;
			if( this.researchedTechnology == null )
			{
				saveState.researchedTechnologyId = "";
			}
			else
			{
				saveState.researchedTechnologyId = this.researchedTechnology.id;
			}
			saveState.researchProgress = this.researchProgress;

			return saveState;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public override void SetDefData( ModuleDefinition _def, ModuleData _data )
		{
			if( !(_def is ResearchModuleDefinition) )
			{
				throw new Exception( "Provided definition is not of the correct type." );
			}
			if( _def == null )
			{
				throw new Exception( "Provided definition is null." );
			}

			if( !(_data is ResearchModuleData) )
			{
				throw new Exception( "Provided data is not of the correct type." );
			}
			if( _data == null )
			{
				throw new Exception( "Provided data is null." );
			}

			ResearchModuleDefinition def = (ResearchModuleDefinition)_def;
			ResearchModuleData data = (ResearchModuleData)_data;

			this.researchSpeed = def.researchSpeed;
			this.researchableTechnologies = new TechnologyDefinition[def.researchableTechnologies.Length];
			for( int i = 0; i < this.researchableTechnologies.Length; i++ )
			{
				this.researchableTechnologies[i] = DefinitionManager.GetTechnology( def.researchableTechnologies[i] );
			}

			Selectable selectable = this.GetComponent<Selectable>();

			// Only if the thing can be selected, display UI elements on the selection panel.
			if( selectable != null )
			{
				// add.
				selectable.onHighlight.AddListener( this.OnHighlight );

				this.onResearchBegin.RemoveListener( this.OnResearchBegin );
				this.onResearchBegin.AddListener( this.OnResearchBegin );

				this.onResearchProgress.RemoveListener( this.OnResearchProgress );
				this.onResearchProgress.AddListener( this.OnResearchProgress );

				this.onResearchEnd.RemoveListener( this.OnResearchEnd );
				this.onResearchEnd.AddListener( this.OnResearchEnd );

				if( this.factionMember != null )
				{
					LevelDataManager.onTechStateChanged.AddListener( OnTechStateChanged );
				}
			}


			this.resourcesRemaining = data.resourcesRemaining;

			if( data.researchedTechnologyId == "" )
			{
				this.researchedTechnology = null;
			}
			else
			{
				this.researchedTechnology = DefinitionManager.GetTechnology( data.researchedTechnologyId );
			}
			this.researchProgress = data.researchProgress;
		}
		

		void OnDestroy()
		{
			if( this.factionMember != null )
			{
				LevelDataManager.onTechStateChanged.RemoveListener( OnTechStateChanged );
			}
		}

		private string Status()
		{
			StringBuilder sb = new StringBuilder();

			if( this.resourcesRemaining == null )
			{
				return "null";
			}
			foreach( var kvp in this.resourcesRemaining )
			{
				if( kvp.Value != 0 )
				{
					ResourceDefinition resDef = DefinitionManager.GetResource( kvp.Key );
					sb.Append( kvp.Value + "x " + resDef.displayName );
				}
				sb.Append( ", " );
			}

			return sb.ToString();
		}

		private void ShowList()
		{
			GameObject[] gridElements = new GameObject[this.researchableTechnologies.Length];
			//TechnologyDefinition[] registeredTechnologies = DefinitionManager.GetAllTechnologies();
			//GameObject[] gridElements = new GameObject[registeredTechnologies.Length];
			// Add every available technology to the list.
			for( int i = 0; i < this.researchableTechnologies.Length; i++ )
			{
				TechnologyDefinition techDef = this.researchableTechnologies[i];
				// If it can be researched, add clickable button, otherwise add unclickable button that represents tech already researched/locked.
				if( LevelDataManager.factionData[this.factionMember.factionId].GetTech( techDef.id ) == TechnologyResearchProgress.Available )
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), techDef.icon, () =>
					{
						StartResearching( techDef );
						// Force the Object UI to update and show that now we are researching a tech.

					} );
				}
				else
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), techDef.icon, null );
				}
			}
			// Create the actual UI.
			GameObject listGO = UIUtils.InstantiateScrollableGrid( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 75.0f, 5.0f ), new Vector2( -150.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
			SelectionPanel.instance.obj.RegisterElement( "research.list", listGO.transform );
		}

		private void OnTechStateChanged( int factionId, string id, TechnologyResearchProgress newProgress )
		{
			if( factionId != this.factionMember.factionId )
			{
				return;
			}
			if( !Selection.IsHighlighted( this.selectable ) )
			{
				return;
			}
			if( this.IsPaymentDone() )
			{
				if( !this.isResearching )
				{
					if( SelectionPanel.instance.obj.GetElement( "research.list" ) != null )
					{
						SelectionPanel.instance.obj.Clear( "research.list" );
					}
					this.ShowList();
				}
			}
		}

		private void OnResearchBegin()
		{
			if( !Selection.IsHighlighted( this.selectable ) )
			{
				return;
			}
			if( SelectionPanel.instance.obj.GetElement( "research.list" ) != null )
			{
				SelectionPanel.instance.obj.Clear( "research.list" );
			}
			if( !this.IsPaymentDone() )
			{
				Transform statusUI = SelectionPanel.instance.obj.GetElement( "research.status" );
				if( statusUI != null )
				{
					UIUtils.EditText( statusUI.gameObject, "Waiting for resources ('" + this.researchedTechnology.displayName + "'): " + Status() );
				}
			}
		}
		
		private void OnResearchProgress()
		{
			if( !Selection.IsHighlighted( this.selectable ) )
			{
				return;
			}
			Transform statusUI = SelectionPanel.instance.obj.GetElement( "research.status" );
			if( statusUI != null )
			{
				UIUtils.EditText( statusUI.gameObject, "Researching...: '" + this.researchedTechnology.displayName + "' - " + (int)this.researchProgress + " s." );
			}
		}

		private void OnResearchEnd()
		{
			if( !Selection.IsHighlighted( this.selectable ) )
			{
				return;
			}
			Transform statusUI = SelectionPanel.instance.obj.GetElement( "research.status" );
			if( statusUI != null )
			{
				UIUtils.EditText( statusUI.gameObject, "Select tech to research..." );
			}

			this.ShowList();
		}


		private void OnHighlight()
		{
			// If the research facility is on a building, that is not usable.
			if( this.selectable.gameObject.layer == ObjectLayer.BUILDINGS )
			{
				Damageable damageable = this.GetComponent<Damageable>();

				if( damageable != null )
				{
					if( !Building.IsUsable( damageable ) )
					{
						return;
					}
				}
			}
			if( this.factionMember.factionId != LevelDataManager.PLAYER_FAC )
			{
				return;
			}
			if( this.IsPaymentDone() )
			{
				if( this.isResearching )
				{
					GameObject statusGO = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Researching...: '" + this.researchedTechnology.displayName + "' - " + (int)this.researchProgress + " s." );
					SelectionPanel.instance.obj.RegisterElement( "research.status", statusGO.transform );
				}
				else
				{
					ShowList();

					GameObject statusGO = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Select tech to research..." );
					SelectionPanel.instance.obj.RegisterElement( "research.status", statusGO.transform );

				}
			}
			else
			{
				GameObject statusGO = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Waiting for resources ('" + this.researchedTechnology.displayName + "'): " + Status() );
				SelectionPanel.instance.obj.RegisterElement( "research.status", statusGO.transform );
			}
		}
	}
}