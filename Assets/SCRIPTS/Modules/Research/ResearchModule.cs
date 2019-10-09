﻿using SS.Buildings;
using SS.Content;
using SS.Diplomacy;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.ResourceSystem.Payment;
using SS.Technologies;
using SS.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SS.Modules
{
	[RequireComponent( typeof( FactionMember ) )]
	public class ResearchModule : Module, IPaymentReceiver
	{
		//public class UnityEvent_string_int : UnityEvent<string,int> { }

		public UnityEvent onResearchBegin = new UnityEvent();

		public UnityEvent onResearchProgress = new UnityEvent();

		public UnityEvent onResearchEnd = new UnityEvent();

		//public UnityEvent_string_int onPaymentReceived = new UnityEvent_string_int();

#warning general on technology research state changed (per fac?) to redraw when buildings/etc. might have gotten unlocked.
		private ResearchModuleDefinition def;

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
						LevelDataManager.factionData[this.factionMember.factionId].techs[this.researchedTechnology.id] = TechnologyResearchProgress.Researched;
						this.researchedTechnology = null;
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
		public ResearchModuleSaveState GetSaveState()
		{
			ResearchModuleSaveState saveState = new ResearchModuleSaveState();

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
		
		public void SetDefinition( ResearchModuleDefinition def )
		{
			this.researchSpeed = def.researchSpeed;
			Selectable selectable = this.GetComponent<Selectable>();

			// Only if the thing can be selected, display UI elements on the selection panel.
			if( selectable != null )
			{
				// if applied before, remove (don't add multiple times).
				selectable.onHighlight.RemoveListener( this.OnHighlight );
				// add.
				selectable.onHighlight.AddListener( this.OnHighlight );

				this.onResearchBegin.RemoveListener( this.OnResearchBegin );
				this.onResearchBegin.AddListener( this.OnResearchBegin );

				this.onResearchProgress.RemoveListener( this.OnResearchProgress );
				this.onResearchProgress.AddListener( this.OnResearchProgress );

				this.onResearchEnd.RemoveListener( this.OnResearchEnd );
				this.onResearchEnd.AddListener( this.OnResearchEnd );
			}
		}
		
		public void SetSaveState( ResearchModuleSaveState saveState )
		{
			this.resourcesRemaining = saveState.resourcesRemaining;

			if( saveState.researchedTechnologyId == "" )
			{
				this.researchedTechnology = null;
			}
			else
			{
				this.researchedTechnology = DefinitionManager.GetTechnology( saveState.researchedTechnologyId );
			}
			this.researchProgress = saveState.researchProgress;
		}


		private void ShowList()
		{
			TechnologyDefinition[] registeredTechnologies = DefinitionManager.GetAllTechnologies();
			GameObject[] gridElements = new GameObject[registeredTechnologies.Length];
			// Add every available technology to the list.
			for( int i = 0; i < registeredTechnologies.Length; i++ )
			{
				TechnologyDefinition techDef = registeredTechnologies[i];
				// If it can be researched, add clickable button, otherwise add unclickable button that represents tech already researched/locked.
				if( LevelDataManager.factionData[this.factionMember.factionId].techs[techDef.id] == TechnologyResearchProgress.Available )
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), techDef.icon.Item2, () =>
					{
						StartResearching( techDef );
						// Force the Object UI to update and show that now we are researching a tech.

					} );
				}
				else
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), techDef.icon.Item2, null );
				}
			}
			// Create the actual UI.
			GameObject listGO = UIUtils.InstantiateScrollableGrid( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 75.0f, 5.0f ), new Vector2( -150.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
			SelectionPanel.instance.obj.RegisterElement( "research.list", listGO.transform );
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
					UIUtils.EditText( statusUI.gameObject, "Waiting for resources: '" + this.researchedTechnology.displayName + "'." );
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
				GameObject statusGO = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Waiting for resources: '" + this.researchedTechnology.displayName + "'." );
				SelectionPanel.instance.obj.RegisterElement( "research.status", statusGO.transform );
			}
		}
	}
}