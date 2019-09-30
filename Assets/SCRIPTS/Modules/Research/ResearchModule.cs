using SS.Buildings;
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

namespace SS.Modules
{
	[RequireComponent( typeof( FactionMember ) )]
	public class ResearchModule : Module, IPaymentReceiver
	{
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
			return new Dictionary<string, int>( this.resourcesRemaining );
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

						Selection.ForceSelectionUIRedraw( null ); // if it needs to update (e.g. civilian that could now build new buildings).
					}

					// Force the SelectionPanel.Object UI to update and show that we either have researched the tech, ot that the progress progressed.
					Selectable selectable = this.GetComponent<Selectable>();
					if( selectable != null )
					{
						Selection.ForceSelectionUIRedraw( selectable );
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

			if( selectable != null )
			{
				// if applied before, remove.
				selectable.onSelectionUIRedraw.RemoveListener( this.OnSelectionUIRedraw );
				// add.
				selectable.onSelectionUIRedraw.AddListener( this.OnSelectionUIRedraw );
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

		private void OnSelectionUIRedraw()
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
					UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Researching...: '" + this.researchedTechnology.displayName + "' - " + (int)this.researchProgress + " s." );
				}
				else
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
							gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), techDef.icon.Item2, () =>
							{
								StartResearching( techDef );
								// Force the Object UI to update and show that now we are researching a tech.
								Selection.ForceSelectionUIRedraw( selectable );
							} );
						}
						else
						{
							gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), techDef.icon.Item2, null );
						}
					}
					// Create the actual UI.
					UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Select tech to research..." );
					UIUtils.InstantiateScrollableGrid( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 75.0f, 5.0f ), new Vector2( -150.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
				}
			}
			else
			{
				UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Waiting for resources: '" + this.researchedTechnology.displayName + "'." );
			}
		}
	}
}