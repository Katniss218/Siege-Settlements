using SS.Content;
using SS.Objects.Modules;
using SS.Objects.SubObjects;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SS.Objects.Units
{
	public static class PopulationUnitUtils
	{
		/// <summary>
		/// Changes the physical size of the unit to match what it would be for a specified population size.
		/// </summary>
		public static void ScaleSize( Unit unit, PopulationSize population )
		{
			float x = unit.size.x;
			float z = unit.size.z;

			switch( population )
            {
				case PopulationSize.x1:
					x *= 1; z *= 1; break;
				case PopulationSize.x2:
					x *= 2; z *= 1; break;
				case PopulationSize.x4:
					x *= 2; z *= 2; break;
				case PopulationSize.x8:
					x *= 4; z *= 2; break;
            }

			unit.navMeshAgent.radius = ((x + z) / 2.0f) / 2.0f;
			unit.navMeshAgent.height = unit.size.y;
			unit.collider.size = new Vector3( x, unit.size.y, z );
			unit.collider.center = new Vector3( 0.0f, unit.size.y * 0.5f, 0.0f );
		}

		/// <summary>
		/// Scales the stats of any unit to match what would they become for a unit with the specified population size.
		/// </summary>
		public static void ScaleStats( Unit unit, PopulationSize sizeBefore, PopulationSize sizeAfter )
		{
			float populationRatio = (float)sizeAfter / (float)sizeBefore;

			float beforeFloat = (float)sizeBefore;
			float afterFloat = (float)sizeAfter;

			// override.
			// Calculate health first, assign later. Prevents weird behaviour due to clamping, which prevents invalid values from being assigned.
			float newHealthMax = unit.healthMax * populationRatio;
			float newHealth = unit.health * populationRatio;

			unit.healthMax = newHealthMax;
			unit.health = newHealth;

			ScaleSize( unit, sizeAfter );

			ScaleStats( unit.GetModules<InventoryModule>(), beforeFloat, afterFloat );

			ScaleStats( unit.GetModules<RangedModule>(), beforeFloat, afterFloat );

			ScaleStats( unit.GetModules<MeleeModule>(), beforeFloat, afterFloat );


#warning attack speed instead of damage/arrow count? BUT I want it to not be perfectly rythmic - instead with randomization.


			MeshPredicatedSubObject[] meshPopulationSubObjects = unit.GetSubObjects<MeshPredicatedSubObject>();
			for( int i = 0; i < meshPopulationSubObjects.Length; i++ )
			{
				meshPopulationSubObjects[i].lookupKey = (int)sizeAfter;
			}
		}



		private static void ScaleStats( InventoryModule[] inventories, float beforePopulation, float afterPopulation )
		{
			for( int i = 0; i < inventories.Length; i++ )
			{
				InventoryModule.Slot[] slots = inventories[i].GetSlots();

				for( int j = 0; j < slots.Length; j++ )
				{
					slots[j].capacityOverride = (int)(slots[j].capacity * afterPopulation);
				}

				inventories[i].SetSlots( slots );
			}
		}

		private static void ScaleStats( MeleeModule[] melee, float beforePopulation, float afterPopulation )
		{
			for( int i = 0; i < melee.Length; i++ )
			{
				melee[i].damageOverride = melee[i].damage * afterPopulation;
			}
		}

		private static void ScaleStats( RangedModule[] ranged, float beforePopulation, float afterPopulation )
		{
			for( int i = 0; i < ranged.Length; i++ )
			{
				ranged[i].projectileCountOverride = (int)(ranged[i].projectileCount * afterPopulation);
			}
		}



		/// <summary>
		/// Tries to join specified units with additional units to make a bigger unit.
		/// </summary>
		/// <param name="beacon">The unit to enlarge (increase population).</param>
		/// <param name="additionalUnits">Units to take the additional population from.</param>
		public static void Combine( Unit beacon, List<Unit> additionalUnits )
		{
#warning TODO! - refactor these two to use safe combine. - what is safe combine?
			byte selfPop = (byte)beacon.population;
			byte popTotal = selfPop; // total population of the new unit
			byte targetPop = 0; // total population of the new unit (if popTotal is a valid PopulationSize).

			float healthTotal = beacon.health;

			bool isSelectedAdditionalAny = false;

			foreach( var additional in additionalUnits )
			{
				if( !isSelectedAdditionalAny )
				{
					if( Selection.IsSelected( additional ) )
					{
						isSelectedAdditionalAny = true;
					}
				}

				popTotal += (byte)additional.population;

				// Find the population of the new, joined unit (total population of all units, clamped to highest valid population size).
				if( popTotal > selfPop )
				{
					healthTotal += additional.health;
					if( popTotal == 8 )
					{
						targetPop = 8;
						break;
					}
					if( popTotal == 4 )
					{
						targetPop = 4;
						break;
					}
					if( popTotal == 2 )
					{
						targetPop = 2;
						break;
					}
					if( popTotal == 1 )
					{
						targetPop = 1;
						break;
					}
				}
			}

			if( targetPop > selfPop )
			{
				int i = 0;
				do
				{
					selfPop += (byte)additionalUnits[i].population;

					additionalUnits[i].Die();

					i++;
					// Join as long as selfPop is less than targetPop AND as long as the selfPop is not a valid pop number.
				} while( selfPop < targetPop && (selfPop != 1 || selfPop != 2 || selfPop != 4 || selfPop != 8) );

				if( isSelectedAdditionalAny )
				{
					Selection.TrySelect( beacon );
				}

				// assign the new, "joined" population.
				beacon.SetPopulation( (PopulationSize)selfPop, true, true );
				beacon.health = healthTotal; // override health, since additionals can have any health percent and won't necessarily match beacon's health percent.
			}
		}

		/// <summary>
		/// Splits the unit so that one of the results has the population size equal to the specified value.
		/// </summary>
		/// <param name="beacon">The unit to split.</param>
		/// <param name="desiredPopulation">The 'beacon' unit, after splitting, will have this population. Set to null to split in half.</param>
		public static List<Unit> Split( Unit beacon, PopulationSize? desiredPopulation = null )
		{
			UnitDefinition beaconDef = DefinitionManager.GetUnit( beacon.definitionId );
			bool isSelected = Selection.IsSelected( beacon );

			byte populationPool = (byte)beacon.population;

			byte populationTarget = 1;
			if( desiredPopulation == null )
			{
				populationTarget = beacon.population == PopulationSize.x1 ? (byte)1 : (byte)((int)beacon.population / 2);
			}
			else
			{
				populationTarget = (byte)desiredPopulation;
			}

			if( populationTarget >= populationPool )
			{
				throw new Exception( "Tried to split into bigger or equally-sized unit." );
			}

			populationPool -= populationTarget;
			if( populationPool != 0 ) // if the population changed
			{
				beacon.SetPopulation( (PopulationSize)populationTarget, true, false );
			}

			float healthPercentSrc = beacon.healthPercent;

			List<Unit> newUnits = new List<Unit>();

			newUnits.Add( beacon );

			// Split into new units (largest-possible) as long as there is enough population in the pool.
			while( populationPool > 0 )
			{
				PopulationSize newSize = PopulationSize.x1;
				// Find the largest unit possible to make from the available population pool.
				if( populationPool >= 8 )
				{
					populationPool -= 8;
					newSize = PopulationSize.x8;
				}
				else if( populationPool >= 4 )
				{
					populationPool -= 4;
					newSize = PopulationSize.x4;
				}
				else if( populationPool >= 2 )
				{
					populationPool -= 2;
					newSize = PopulationSize.x2;
				}
				else if( populationPool >= 1 )
				{
					populationPool -= 1;
					newSize = PopulationSize.x1;
				}

				Vector3 position = beacon.transform.position + new Vector3( Random.Range( -0.01f, 0.01f ), Random.Range( -0.01f, 0.01f ), Random.Range( -0.01f, 0.01f ) );
				Quaternion rotation = beacon.transform.rotation;

				Unit unit = UnitCreator.Create( beaconDef, Guid.NewGuid(), position, rotation, beacon.factionId );
				unit.SetPopulation( newSize, true, true );
				unit.healthPercent = healthPercentSrc;

				newUnits.Add( unit );

				if( isSelected )
				{
					Selection.TrySelect( unit );
				}
			}

			return newUnits;
		}
	}
}