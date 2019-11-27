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
using SS.Objects;

namespace SS.Objects.Modules
{
	[RequireComponent( typeof( FactionMember ) )]
	public class ResearchModule : SSModule, ISelectDisplayHandler,IPaymentReceiver
	{
		public const string KFF_TYPEID = "research";

		public UnityEvent onResearchBegin = new UnityEvent();

		public UnityEvent onResearchProgress = new UnityEvent();

		public UnityEvent onResearchEnd = new UnityEvent();

		public UnityEvent onPaymentReceived { get; private set; } = new UnityEvent();

		public TechnologyDefinition[] researchableTechnologies { get; set; }

		/// <summary>
		/// Contains the currently researched technology (Read only).
		/// </summary>
		public TechnologyDefinition researchedTechnology { get; private set; }


		/// <summary>
		/// Contains the multiplier used as the construction speed.
		/// </summary>
		public float researchSpeed { get; set; } = 1.0f;


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

		private float researchProgressRemaining = 0.0f;

		private Dictionary<string, int> resourcesRemaining = new Dictionary<string, int>();


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-

		
		void Awake()
		{
			this.onPaymentReceived = new UnityEvent();
			
			LevelDataManager.onTechStateChanged.AddListener( this.OnTechStateChanged );
		}
		
		void Update()
		{
			if( IsPaymentDone() )
			{
				if( this.researchedTechnology == null )
				{
					return;
				}
				this.ProgressResearching();
			}
		}

		void OnDestroy()
		{
			LevelDataManager.onTechStateChanged.RemoveListener( this.OnTechStateChanged );
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		private bool IsPaymentDone()
		{
			if( this.resourcesRemaining == null )
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
			if( this.resourcesRemaining == null )
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
			this.PaymentReceived_UI();
			this.onPaymentReceived?.Invoke();
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
			if( this.researchedTechnology != null )
			{
				throw new Exception( "You can't start researching technology, when another one is already being researched." );
			}

			this.researchedTechnology = def;
			this.researchProgressRemaining = 10.0f;

			this.resourcesRemaining = new Dictionary<string, int>( this.researchedTechnology.cost );
			this.ResearchBegin_UI();
			this.onResearchBegin?.Invoke();
		}

		private void ProgressResearching()
		{
			this.researchProgressRemaining -= this.researchSpeed * Time.deltaTime;
			if( this.researchProgressRemaining <= 0 )
			{
				this.EndResearching( true );
			}
			else
			{
				this.ResearchProgress_UI();
				this.onResearchProgress?.Invoke();
			}
		}

		public void EndResearching( bool isSuccess )
		{
			if( this.researchedTechnology == null )
			{
				throw new Exception( "You can't end researching, when there's no technology being researched." );
			}

			if( isSuccess )
			{
				LevelDataManager.SetTech( this.factionMember.factionId, this.researchedTechnology.id, TechnologyResearchProgress.Researched );
			}
			this.researchedTechnology = null;
			this.researchProgressRemaining = 0.0f;
			this.resourcesRemaining = null;
			this.ResearchEnd_UI();
			this.onResearchEnd?.Invoke();
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
			saveState.researchProgress = this.researchProgressRemaining;

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

			this.icon = def.icon;
			this.researchSpeed = def.researchSpeed;
			this.researchableTechnologies = new TechnologyDefinition[def.researchableTechnologies.Length];
			for( int i = 0; i < this.researchableTechnologies.Length; i++ )
			{
				this.researchableTechnologies[i] = DefinitionManager.GetTechnology( def.researchableTechnologies[i] );
			}

			// ------          DATA

			this.resourcesRemaining = data.resourcesRemaining;

			if( data.researchedTechnologyId == "" )
			{
				this.researchedTechnology = null;
			}
			else
			{
				this.researchedTechnology = DefinitionManager.GetTechnology( data.researchedTechnologyId );
			}
			this.researchProgressRemaining = data.researchProgress;
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
			GameObject listGO = UIUtils.InstantiateScrollableGrid( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 30.0f, 5.0f ), new Vector2( -60.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
			SelectionPanel.instance.obj.RegisterElement( "research.list", listGO.transform );
		}

		private void OnTechStateChanged( int factionId, string id, TechnologyResearchProgress newProgress )
		{
			if( factionId != this.factionMember.factionId )
			{
				return;
			}
			if( !Selection.IsDisplayedModule( this ) )
			{
				return;
			}
			if( this.IsPaymentDone() )
			{
				if( this.researchedTechnology == null )
				{
					if( SelectionPanel.instance.obj.GetElement( "research.list" ) != null )
					{
						SelectionPanel.instance.obj.ClearElement( "research.list" );
					}
					this.ShowList();
				}
			}
		}

		private void PaymentReceived_UI()
		{
			if( !Selection.IsDisplayedModule( this ) )
			{
				return;
			}
			Transform statusUI = SelectionPanel.instance.obj.GetElement( "research.status" );
			if( statusUI != null )
			{
				UIUtils.EditText( statusUI.gameObject, "Waiting for resources ('" + this.researchedTechnology.displayName + "'): " + Status() );
			}
		}

		private void ResearchBegin_UI()
		{
			if( !Selection.IsDisplayedModule( this ) )
			{
				return;
			}
			if( SelectionPanel.instance.obj.GetElement( "research.list" ) != null )
			{
				SelectionPanel.instance.obj.ClearElement( "research.list" );
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
		
		private void ResearchProgress_UI()
		{
			if( !Selection.IsDisplayedModule( this ) )
			{
				return;
			}
			Transform statusUI = SelectionPanel.instance.obj.GetElement( "research.status" );
			if( statusUI != null )
			{
				UIUtils.EditText( statusUI.gameObject, "Researching...: '" + this.researchedTechnology.displayName + "' - " + (int)this.researchProgressRemaining + " s." );
			}
		}

		private void ResearchEnd_UI()
		{
			if( !Selection.IsDisplayedModule( this ) )
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

		
		public void OnDisplay()
		{
			// If it's not usable - return, don't research anything.
			if( this.ssObject is IUsableToggle && !(this.ssObject as IUsableToggle).IsUsable() )
			{
				return;
			}

			if( this.factionMember.factionId != LevelDataManager.PLAYER_FAC )
			{
				return;
			}
			if( this.IsPaymentDone() )
			{
				if( this.researchedTechnology != null )
				{
					GameObject statusGO = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Researching...: '" + this.researchedTechnology.displayName + "' - " + (int)this.researchProgressRemaining + " s." );
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