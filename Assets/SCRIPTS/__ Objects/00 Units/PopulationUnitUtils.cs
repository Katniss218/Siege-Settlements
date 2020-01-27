using SS.Content;
using SS.Objects.Modules;
using SS.Objects.SubObjects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.Units
{
	public static class PopulationUnitUtils
	{
		/// <summary>
		/// Scales the size of the unit to match what it would be for a specified population size.
		/// </summary>
		public static void ScaleSize( Unit unit, PopulationSize population )
		{
			float x = unit.sizePerPopulation.x;
			float z = unit.sizePerPopulation.z;

			if( population == PopulationSize.x1 )
			{
				x *= 1;
				z *= 1;
			}
			else if( population == PopulationSize.x2 )
			{
				x *= 2;
				z *= 1;
			}
			else if( population == PopulationSize.x4 )
			{
				x *= 2;
				z *= 2;
			}
			else if( population == PopulationSize.x8 )
			{
				x *= 4;
				z *= 2;
			}
			unit.navMeshAgent.radius = ((x + z) / 2f) / 2;
			unit.navMeshAgent.height = unit.sizePerPopulation.y;
			unit.collider.size = new Vector3( x, unit.sizePerPopulation.y, z );
			unit.collider.center = new Vector3( 0.0f, unit.sizePerPopulation.y * 0.5f, 0.0f );
		}

		/// <summary>
		/// Scales the stats of any unit to match what would they become for a unit with the specified population size.
		/// </summary>
		public static void ScaleStats( Unit unit, PopulationSize before, PopulationSize after )
		{
			float populationRatio = (float)after / (float)before;

			float beforeFloat = (float)before;
			float afterFloat = (float)after;

			// override.
			// Calculate health first, assign later. Prevents weird behaviour due to clamping, which prevents invalid values from being assigned.
			float newHealthMax = unit.healthMax * populationRatio;
			float newHealth = unit.health * populationRatio;

			unit.healthMax = newHealthMax;
			unit.health = newHealth;

			ScaleSize( unit, after );

			ScaleStats( unit.GetModules<InventoryModule>(), beforeFloat, afterFloat );

			ScaleStats( unit.GetModules<RangedModule>(), beforeFloat, afterFloat );

			ScaleStats( unit.GetModules<MeleeModule>(), beforeFloat, afterFloat );


#warning attack speed instead of damage/arrow count? BUT I want it to not be perfectly rythmic - instead with randomization.
			// maybe make the attack modules have a pool of available attacks (like bows that are reloaded currently). and the bigger pop, the more of them are.


			MeshPredicatedSubObject[] meshPopulationSubObjects = unit.GetSubObjects<MeshPredicatedSubObject>();
			for( int i = 0; i < meshPopulationSubObjects.Length; i++ )
			{
				meshPopulationSubObjects[i].lookupKey = (int)after;
			}
		}



		private static void ScaleStats( InventoryModule[] inventories, float before, float after )
		{
			for( int i = 0; i < inventories.Length; i++ )
			{
				InventoryModule.SlotGroup[] slotGroups = inventories[i].GetSlots();

				for( int j = 0; j < slotGroups.Length; j++ )
				{
					slotGroups[j].capacityOverride = (int)(slotGroups[j].capacity * after);
				}

				inventories[i].SetSlots( slotGroups );
			}
		}

		private static void ScaleStats( MeleeModule[] melee, float before, float after )
		{
			for( int i = 0; i < melee.Length; i++ )
			{
				melee[i].damageOverride = melee[i].damage * after;
			}
		}

		private static void ScaleStats( RangedModule[] ranged, float before, float after )
		{
			for( int i = 0; i < ranged.Length; i++ )
			{
				ranged[i].projectileCountOverride = (int)(ranged[i].projectileCount * after);
			}
		}



		/// <summary>
		/// Tries to join specified units with additional units to make a bigger unit.
		/// </summary>
		/// <param name="beacon">The unit to enlarge (increase population).</param>
		/// <param name="additional">Units to take the additional population from.</param>
		public static void Combine( Unit beacon, List<Unit> additional )
		{
			byte selfPop = (byte)beacon.population;
			byte popTotal = selfPop; // total population of the new unit
			byte targetPop = 0; // total population of the new unit (if popTotal is a valid PopulationSize).

			float healthTotal = beacon.health;

			bool isSelectedAdditionalAny = false;

			for( int i = 0; i < additional.Count; i++ )
			{
				if( !isSelectedAdditionalAny )
				{
					if( Selection.IsSelected( additional[i] ) )
					{
						isSelectedAdditionalAny = true;
					}
				}

				popTotal += (byte)additional[i].population;

				// Find the population of the new, joined unit (total population of all units, clamped to highest valid population size).
				if( popTotal > selfPop )
				{
					healthTotal += additional[i].health;
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
					selfPop += (byte)additional[i].population;

					additional[i].Die();

					i++;
					// Join as long as selfPop is less than targetPop AND as long as the selfPop is not a valid pop number.
				} while( selfPop < targetPop && (selfPop != 1 || selfPop != 2 || selfPop != 4 || selfPop != 8) );

				if( isSelectedAdditionalAny )
				{
					Selection.TrySelect( beacon );
				}
				// assign the new, "joined" population.
				beacon.population = (PopulationSize)selfPop;
				beacon.health = healthTotal;
			}
		}

		/// <summary>
		/// Splits the unit so that one of the results has population size of specified value.
		/// </summary>
		/// <param name="beacon">The unit to split.</param>
		/// <param name="desiredPopulation">Beacon unit after splitting will be this size.</param>
		public static List<Unit> Split( Unit beacon, PopulationSize? desiredPopulation = null )
		{
			// splits in half if desiredPopulation == null.

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
				beacon.population = (PopulationSize)populationTarget;
			}

			float healthPercentSrc = beacon.healthPercent;

			List<Unit> ret = new List<Unit>();
			ret.Add( beacon );
			UnitDefinition beaconDef = DefinitionManager.GetUnit( beacon.definitionId );

			bool isSelected = Selection.IsSelected( beacon );

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


				Vector3 pos = beacon.transform.position + new Vector3( UnityEngine.Random.Range( -0.01f, 0.01f ), UnityEngine.Random.Range( -0.01f, 0.01f ), UnityEngine.Random.Range( -0.01f, 0.01f ) );
				Quaternion rot = beacon.transform.rotation;

				Unit u = UnitCreator.Create( beaconDef, Guid.NewGuid(), pos, rot, beacon.factionId ).GetComponent<Unit>();
				u.population = newSize;
				u.healthPercent = healthPercentSrc;

				ret.Add( u );

				if( isSelected )
				{
					Selection.TrySelect( u );
				}
			}
			return ret;
		}
	}
}