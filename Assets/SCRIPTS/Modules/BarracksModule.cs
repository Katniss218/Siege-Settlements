using SS.UI;
using SS.Units;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	[RequireComponent( typeof( FactionMember ) )]
	public class BarracksModule : MonoBehaviour
	{
		public UnitDefinition[] spawnableUnits;

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
				selectable.onSelectionUIRedraw.AddListener( () =>
				{
					Damageable d = this.GetComponent<Damageable>();
					// If the barracks are not usable.
					if( d != null && !Buildings.Building.CheckUsable( d ) )
					{
						UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Building is damaged (<50% HP)" );

						return;
					}
					const string TEXT = "Select unit to make...";

					GameObject[] gridElements = new GameObject[spawnableUnits.Length];
					// Initialize the grid elements' GameObjects.
					for( int i = 0; i < spawnableUnits.Length; i++ )
					{
						UnitDefinition unitDef = this.spawnableUnits[i];
						// If the unit's techs required have not been researched yet, add unclickable button, otherwise, add normal button.
						if( Technologies.TechLock.CheckLocked( unitDef, FactionManager.factions[0].techs ) )
						{
							gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), unitDef.icon.Item2, null );
						}
						else
						{
							gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), unitDef.icon.Item2, () =>
							{
								AddUnitToBuildOrder( unitDef, 3 );
							} );
						}
					}
					// Create the actual UI.
					UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), TEXT );
					UIUtils.InstantiateScrollableGrid( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 25.0f, 5.0f ), new Vector2( -50.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );

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