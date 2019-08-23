using SS.Data;
using SS.Technologies;
using SS.UI;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	[RequireComponent(typeof(FactionMember))]
	public class ResearchModule : MonoBehaviour
	{
		public TechnologyDefinition tech { get; private set; }
		public float progress { get; private set; }

		public bool isResearching
		{
			get
			{
				return this.tech != null;
			}
		}

		private FactionMember factionMember;

		public void StartResearching( TechnologyDefinition def )
		{
			if( this.isResearching )
			{
				Debug.LogWarning( "There is already technology being researched" );
				return;
			}
			this.tech = def;
			this.progress = 10;
		}

		// Start is called before the first frame update
		void Start()
		{
			this.factionMember = GetComponent<FactionMember>();

			Selectable selectable = this.GetComponent<Selectable>();

			if( selectable != null )
			{
				selectable.onSelect.AddListener( () =>
				{
					if( this.isResearching )
					{
						UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Technology being researched: " + this.tech.displayName );
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
				this.progress -= Time.deltaTime;
				if( this.progress <= 0 )
				{
					FactionManager.factions[this.factionMember.factionId].techs[this.tech.id] = TechnologyResearchProgress.Researched;
					this.tech = null;
				}
			}
		}
	}
}