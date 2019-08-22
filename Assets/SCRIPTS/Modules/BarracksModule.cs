using SS.UI;
using SS.UI.Elements;
using SS.Units;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	[RequireComponent(typeof( FactionMember ) )]
	public class BarracksModule : MonoBehaviour
	{
		public List<UnitDefinition> availableUnits = new List<UnitDefinition>();

		List<UnitDefinition> buildOrderDef = new List<UnitDefinition>();
		List<float> buildOrderTimes = new List<float>();


		void AddUnitToBuildOrder( UnitDefinition def, int buildTime )
		{
			this.buildOrderDef.Add( def );
			this.buildOrderTimes.Add( buildTime );
		}

		// Start is called before the first frame update
		void Start()
		{
			Selectable selectable = this.GetComponent<Selectable>();

			if( selectable != null )
			{
				selectable.onSelect.AddListener( () =>
				{
					const string TEXT = "Select unit to make...";
					
					GameObject[] gridElements = new GameObject[availableUnits.Count];
					// Initialize the grid elements' GameObjects.
					for( int i = 0; i < availableUnits.Count; i++ )
					{
						UnitDefinition unitDef = this.availableUnits[i];
						gridElements[i] = UIUtils.CreateButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), unitDef.icon.Item2, new IconData( Color.white ), () =>
						{
							AddUnitToBuildOrder( unitDef, 3 );
						} );
					}
					// Create the actual UI.
					UIUtils.CreateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), TEXT, new TextData( Main.mainFont, 24, TMPro.FontStyles.Normal, TMPro.TextAlignmentOptions.Center, Color.white ) );
					UIUtils.CreateScrollableGrid( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 25.0f, 5.0f ), new Vector2( -50.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), new GridData( 72 ), gridElements );

				} );
			}
		}

		// Update is called once per frame
		void Update()
		{
			// If we are building something
			if( buildOrderDef.Count > 0 )
			{
				// Advance the current build item.
				buildOrderTimes[0] -= Time.deltaTime;
				if( buildOrderTimes[0] <= 0 )
				{
					Unit.Create( buildOrderDef[0], this.transform.position, Quaternion.identity, this.GetComponent<FactionMember>().factionId );

					buildOrderDef.RemoveAt( 0 );
					buildOrderTimes.RemoveAt( 0 );
				}
			}
		}
	}
}