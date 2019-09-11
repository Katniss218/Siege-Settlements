using SS.Data;
using SS.Technologies;
using SS.UI;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Modules
{
	[RequireComponent( typeof( FactionMember ) )]
	public class ResearchModule : Module
	{
		/// <summary>
		/// Contains the currently researched technology (Read only).
		/// </summary>
		public TechnologyDefinition technologyResearched { get; private set; }

		/// <summary>
		/// Contains the progress of the research (Read Only).
		/// </summary>
		public float researchProgress { get; private set; }

		/// <summary>
		/// Contains the multiplier used as the construction speed.
		/// </summary>
		public float researchSpeed { get; set; }

		/// <summary>
		/// returns true if the research module is currently researching a technology.
		/// </summary>
		public bool isResearching
		{
			get
			{
				return this.technologyResearched != null;
			}
		}

		private FactionMember factionMember;

		/// <summary>
		/// Begins researching a technology.
		/// </summary>
		/// <param name="def">The technology to research.</param>
		public void StartResearching( TechnologyDefinition def )
		{
			if( this.isResearching )
			{
				Debug.LogWarning( "There is already technology being researched" );
				return;
			}
			this.technologyResearched = def;
			this.researchProgress = 10.0f;
		}

		// Start is called before the first frame update
		void Start()
		{
			this.factionMember = GetComponent<FactionMember>();

			Selectable selectable = this.GetComponent<Selectable>();

			if( selectable != null )
			{
				//####
				// Assign the UI redraw pass.
				//####
				selectable.onSelectionUIRedraw.AddListener( () =>
				{
					Damageable d = this.GetComponent<Damageable>();
					// If the research facility is not usable.
					if( d != null && !Buildings.Building.CheckUsable( d ) )
					{
						// TODO ----- move this from modules to general building code (also move from barracks module).
						UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Building is damaged (<50% HP)" );

						return;
					}
					if( this.isResearching )
					{
						UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Researching...: " + this.technologyResearched.displayName + " - (" + (int)this.researchProgress + ")." );
					}
					else
					{
						const string TEXT = "Select tech to research...";

						List<TechnologyDefinition> techDefs = DataManager.GetAllOfType<TechnologyDefinition>();
						List<GameObject> gridElements = new List<GameObject>();
						// Add every available technology to the list.
						for( int i = 0; i < techDefs.Count; i++ )
						{
							TechnologyDefinition techDef = techDefs[i];
							// If it can be researched, add clickable button, otherwise add unclickable button that represents tech already researched/locked.
							if( FactionManager.factions[this.factionMember.factionId].techs[techDef.id] == TechnologyResearchProgress.Available )
							{
								gridElements.Add( UIUtils.InstantiateIconButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), techDef.icon.Item2, () =>
								{
									StartResearching( techDef );
									// Force the Object UI to update and show that now we are researching a tech.
									SelectionManager.ForceSelectionUIRedraw( selectable );
								} ) );
							}
							else
							{
								gridElements.Add( UIUtils.InstantiateIconButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), techDef.icon.Item2, null ) );
							}
						}
						// Create the actual UI.
						UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), TEXT );
						UIUtils.InstantiateScrollableGrid( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 25.0f, 5.0f ), new Vector2( -50.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements.ToArray() );
					}
				} );

			}
		}

		// Update is called once per frame
		void Update()
		{
			if( this.isResearching )
			{
				this.researchProgress -= this.researchSpeed * Time.deltaTime;
				if( this.researchProgress <= 0 )
				{
					FactionManager.factions[this.factionMember.factionId].techs[this.technologyResearched.id] = TechnologyResearchProgress.Researched;
					this.technologyResearched = null;
				}

				// Force the SelectionPanel.Object UI to update and show that we either have researched the tech, ot that the progress progressed.
				Selectable selectable = this.GetComponent<Selectable>();
				if( selectable != null )
				{
					SelectionManager.ForceSelectionUIRedraw( selectable );
				}
			}
		}
	}
}