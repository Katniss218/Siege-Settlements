using SS.Content;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Objects.Modules;
using SS.Objects.SubObjects;
using SS.UI;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SS.Objects.Units
{
	public static class UnitCreator
	{
		private const string GAMEOBJECT_NAME = "Unit";
		

		public static void SetData( Unit unit, UnitData data )
		{
			//
			//    CONTAINER GAMEOBJECT
			//

			// Set the position/movement information.
			unit.transform.SetPositionAndRotation( data.position, data.rotation );

			// Set the unit's movement parameters.
			NavMeshAgent navMeshAgent = unit.navMeshAgent;
			navMeshAgent.enabled = true; // Enable the NavMeshAgent since the position is set (data.position).

			// Set the unit's native parameters.
			if( unit.guid != data.guid )
			{
				throw new Exception( "Mismatched guid: '" + unit.guid + "'." );
			}
			unit.factionId = data.factionId;
			if( data.health != null )
			{
				unit.health = data.health.Value;
			}

			if( unit.isCivilian )
			{
				CivilianUnitExtension cue = unit.civilian;

				// Set the workplace (if unit is a civilian & workplace is present).
				if( data.workplace != null )
				{
					SSObject obj = SSObject.Find( data.workplace.Item1 );
					WorkplaceModule workplace = obj.GetModule<WorkplaceModule>( data.workplace.Item2 );

					WorkplaceModule.SetWorking( workplace, cue, data.workplace.Item3 );
					cue.isWorking = data.isWorking ?? false;
				}

				// Set the automatic duty (only for civilians).
				if( data.isOnAutomaticDuty != null )
				{
					cue.SetAutomaticDuty( data.isOnAutomaticDuty.Value );
				}
			}
			
			unit.population = data.population;

			//
			//    MODULES
			//

			SSObjectCreator.AssignModuleData( unit, data );
			
			if( data.tacticalGoalData != null )
			{
				unit.controller.SetGoalData( data.tacticalGoalData, data.tacticalGoalTag );
			}
#warning inventory can't block setting the population. It could block splitting & overall needs better handling of population changing.
#warning   data should set the population in a different way maybe?

		}




		private static Unit CreateUnit( UnitDefinition def, Guid guid )
		{
			GameObject gameObject = new GameObject( GAMEOBJECT_NAME + " - '" + def.id + "'" );
			gameObject.layer = ObjectLayer.UNITS;

			//
			//    CONTAINER GAMEOBJECT
			//
			
			BoxCollider collider = gameObject.AddComponent<BoxCollider>();

			// Add a kinematic rigidbody to the unit (required by the NavMeshAgent).
			Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
			rigidbody.isKinematic = true;

			// Add the NavMeshAgent to the unit, to make it movable.
			NavMeshAgent navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
			navMeshAgent.baseOffset = Main.DEFAULT_NAVMESH_BASE_OFFSET;
			navMeshAgent.acceleration = Main.DEFAULT_NAVMESH_ACCELERATION;
			navMeshAgent.stoppingDistance = Main.DEFAULT_NAVMESH_STOPPING_DIST;
			navMeshAgent.enabled = false; // Disable the NavMeshAgent for as long as the position is not set (data.position).

			Unit unit = gameObject.AddComponent<Unit>();
			unit.guid = guid;
			unit.definitionId = def.id;
			unit.displayName = def.displayName;
			unit.icon = def.icon;
			unit.movementSpeed = def.movementSpeed;
			unit.rotationSpeed = def.rotationSpeed;
			unit.sizePerPopulation = def.size;
			unit.hurtSound = def.hurtSoundEffect;
			unit.deathSound = def.deathSoundEffect;
			unit.isCivilian = def.isCivilian;
			unit.isPopulationLocked = def.isPopulationLocked;
			unit.populationSizeLimit = def.populationSizeLimit;

			unit.viewRange = def.viewRange;
			unit.healthMax = def.healthMax;
			unit.health = def.healthMax;
			unit.armor = def.armor;


			unit.onFactionChange.AddListener( ( int fromFac, int toFac ) =>
			{
				Color color = LevelDataManager.factions[toFac].color;

				unit.hud.SetColor( color );
				MeshSubObject[] meshes = unit.GetSubObjects<MeshSubObject>();
				for( int i = 0; i < meshes.Length; i++ )
				{
					meshes[i].GetMaterial().SetColor( "_FactionColor", color );
				}
				MeshPredicatedSubObject[] meshes2 = unit.GetSubObjects<MeshPredicatedSubObject>();
				for( int i = 0; i < meshes2.Length; i++ )
				{
					meshes2[i].GetMaterial().SetColor( "_FactionColor", color );
				}

				// only for "real" faction changes.
				if( fromFac != SSObjectDFSC.FACTIONID_INVALID )
				{
					LevelDataManager.factionData[fromFac].populationCache -= (int)unit.population;
					LevelDataManager.factionData[toFac].populationCache += (int)unit.population;

					if( toFac == LevelDataManager.PLAYER_FAC )
					{
						ResourcePanel.instance.UpdatePopulationDisplay( LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].populationCache );
					}
				}
				

				// Re-Display the object

				if( Selection.IsDisplayed( unit ) )
				{
					SSObjectHelper.ReDisplayDisplayed();
				}
			} );

			unit.onHealthPercentChanged.AddListener( () =>
			{
				unit.hud.SetHealthBarFill( unit.healthPercent );

				if( unit.isInside )
				{
					HUDInteriorSlot slotHud = null;
					if( unit.slotType == InteriorModule.SlotType.Generic )
					{
						slotHud = unit.interior.hudInterior.slots[unit.slotIndex];
					}
					else if( unit.slotType == InteriorModule.SlotType.Worker )
					{
						slotHud = unit.interior.hudInterior.workerSlots[unit.slotIndex];
					}
					slotHud.SetHealth( unit.healthPercent );
				}

				MeshSubObject[] meshes = unit.GetSubObjects<MeshSubObject>();
				for( int i = 0; i < meshes.Length; i++ )
				{
					meshes[i].GetMaterial().SetFloat( "_Dest", 1 - unit.healthPercent );
				}
				MeshPredicatedSubObject[] meshes2 = unit.GetSubObjects<MeshPredicatedSubObject>();
				for( int i = 0; i < meshes2.Length; i++ )
				{
					meshes2[i].GetMaterial().SetFloat( "_Dest", 1 - unit.healthPercent );
				}
			} );




			UnityAction<bool> onHudLockChangeListener = ( bool isLocked ) =>
			{
				if( unit.hud.isDisplayedDueToDamage )
				{
					return;
				}
				if( isLocked )
				{
					unit.hud.hudContainer.isVisible = true;
				}
				else
				{
					if( Selection.IsSelected( unit ) )
					{
						return;
					}
					if( (object)MouseOverHandler.currentObjectMousedOver == unit )
					{
						return;
					}
					unit.hud.hudContainer.isVisible = false;
				}
			};

			Main.onHudLockChange.AddListener( onHudLockChangeListener );

			unit.onSelect.AddListener( () =>
			{
				if( Main.isHudForcedVisible ) { return; }
				if( MouseOverHandler.currentObjectMousedOver == gameObject )
				{
					return;
				}
				unit.hud.hudContainer.isVisible = true;
			} );

			unit.onDeselect.AddListener( () =>
			{
				if( Main.isHudForcedVisible ) { return; }
				if( MouseOverHandler.currentObjectMousedOver == gameObject )
				{
					return;
				}
				unit.hud.hudContainer.isVisible = false;
			} );

			// Make the unit update it's healthbar and material when health changes.
			unit.onHealthChange.AddListener( ( float deltaHP ) =>
			{
				if( deltaHP < 0 )
				{
					unit.hud.hudContainer.isVisible = true;
					unit.hud.isDisplayedDueToDamage = true;
				}
				
				if( !Selection.IsDisplayed( unit ) )
				{
					return;
				}
				Transform healthUI = SelectionPanel.instance.obj.GetElement( "unit.health" );
				if( healthUI != null )
				{
					UIUtils.EditText( healthUI.gameObject, SSObjectDFSC.GetHealthString( unit.health, unit.healthMax ) );
				}
			} );
			
			// Make the unit deselect itself, and destroy it's UI when killed.
			unit.onDeath.AddListener( () =>
			{
				Object.Destroy( unit.hud.gameObject );

				if( unit.isInside )
				{
					unit.SetOutside();
				}

				if( unit.isCivilian )
				{
					if( unit.civilian.isEmployed )
					{
						WorkplaceModule.ClearWorking( unit.civilian.workplace, unit.civilian, unit.civilian.workplaceSlotIndex );
					}
				}

				if( Selection.IsSelected( unit ) )
				{
					Selection.Deselect( unit ); // We have all of the references of this unit here, so we can just simply pass it like this. Amazing, right?
				}
				// Remove the now unused listeners.
				Main.onHudLockChange.RemoveListener( onHudLockChangeListener );
			} );

			//
			//    SUB-OBJECTS
			//

			SSObjectCreator.AssignSubObjects( gameObject, def );

			//
			//    MODULES
			//

			SSObjectCreator.AssignModules( unit, def );
			
			return unit;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
		
		public static UnitData GetData( Unit unit )
		{
			if( unit.guid == null )
			{
				throw new Exception( "Guid not assigned." );
			}

			UnitData data = new UnitData();
			data.guid = unit.guid;

			data.position = unit.transform.position;
			data.rotation = unit.transform.rotation;

			data.factionId = unit.factionId;

			if( unit.health != unit.healthMax )
			{
				data.health = unit.health;
			}

			data.population = unit.population;

			if( unit.isCivilian )
			{
				if( unit.civilian.workplace != null )
				{
					data.workplace = new Tuple<Guid, Guid, int>(
							unit.civilian.workplace.ssObject.guid,
							unit.civilian.workplace.moduleId,
							unit.civilian.workplaceSlotIndex
						);
					data.isWorking = unit.civilian.isWorking;
				}

				data.isOnAutomaticDuty = unit.civilian.isOnAutomaticDuty;
			}

			//
			// MODULES
			//

			SSObjectCreator.ExtractModulesToData( unit, data );
			
			data.tacticalGoalData = unit.controller.GetGoalData();

			return data;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@


		public static Unit Create( UnitDefinition def, Guid guid )
		{
			return CreateUnit( def, guid );
		}

		public static Unit Create( UnitDefinition def, Guid guid, Vector3 position, Quaternion rotation, int factionId )
		{
			Unit unit = CreateUnit( def, guid );
			unit.transform.position = position;
			unit.transform.rotation = rotation;
			
			unit.factionId = factionId;

			NavMeshAgent navMeshAgent = unit.navMeshAgent;
			navMeshAgent.enabled = true;

			return unit;
		}
	}
}