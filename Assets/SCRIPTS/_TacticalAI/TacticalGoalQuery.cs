using SS.AI.Goals;
using SS.Content;
using SS.Levels;
using SS.Objects;
using SS.Objects.Modules;
using SS.ResourceSystem.Payment;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace SS.AI
{
	public static class TacticalGoalQuery
	{
		/// <summary>
		/// Added to everything user-input-related.
		/// </summary>
		public const int TAG_CUSTOM = -5;


		public static void QueryAt( Ray viewRay )
		{
			if( EventSystem.current.IsPointerOverGameObject() )
			{
				return;
			}

			RaycastHit[] raycastHits = Physics.RaycastAll( viewRay, float.MaxValue, ObjectLayer.ALL_MASK );

			Vector3? terrainHitPos = null;
			
			SSObjectDFS hitDamageable = null;

			SSObjectDFS hitInteriorDFS = null;
			InteriorModule hitInterior = null;

			for( int i = 0; i < raycastHits.Length; i++ )
			{
				if( raycastHits[i].collider.gameObject.layer == ObjectLayer.TERRAIN )
				{
					terrainHitPos = raycastHits[i].point;
				}
				else
				{
					SSObjectDFS damageable = raycastHits[i].collider.GetComponent<SSObjectDFS>();
					if( damageable != null && hitDamageable == null )
					{
						hitDamageable = damageable;
					}

					InteriorModule interior = raycastHits[i].collider.GetComponent<InteriorModule>();
					if( interior != null && hitInterior == null )
					{
						hitInteriorDFS = interior.ssObject as SSObjectDFS;
						hitInterior = interior;
					}
				}
			}

			if( hitDamageable == null && terrainHitPos.HasValue )
			{
				AssignMoveToGoal( terrainHitPos.Value, Selection.GetSelectedObjects() );
				return;
			}
			
			if( hitInterior != null && (hitInteriorDFS.factionId == LevelDataManager.PLAYER_FAC) )
			{
				AssignMoveToInteriorOrObjGoal( null, hitInterior, Selection.GetSelectedObjects() );
				return;
			}
			if( hitDamageable != null && (hitDamageable.factionId != LevelDataManager.PLAYER_FAC) )
			{
				AssignAttackGoal( hitDamageable, Selection.GetSelectedObjects() );
				return;
			}
			if( hitDamageable != null && (hitDamageable.factionId == LevelDataManager.PLAYER_FAC) )
			{
				AssignMoveToInteriorOrObjGoal( hitDamageable, null, Selection.GetSelectedObjects() );
				return;
			}
		}




		//
		//
		//


		private static void AssignAttackGoal( SSObjectDFS target, SSObjectDFS[] selected )
		{
			List<SSObjectDFS> filteredObjects = new List<SSObjectDFS>();

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			for( int i = 0; i < selected.Length; i++ )
			{
				if( selected[i].factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}
				IAttackModule[] targeters = selected[i].GetComponents<IAttackModule>();
				if( targeters == null || targeters.Length == 0 )
				{
					continue;
				}

				bool canTarget = false;
				for( int j = 0; j < targeters.Length; j++ )
				{
					if( target.CanTargetAnother( selected[i] ) )
					{
						canTarget = true;
						break;
					}
				}

				if( canTarget )
				{
					filteredObjects.Add( selected[i] );
				}
			}

			if( filteredObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), Main.cameraPivot.position );
			}
			for( int i = 0; i < filteredObjects.Count; i++ )
			{
				TacticalGoalController goalController = filteredObjects[i].GetComponent<TacticalGoalController>();
				TacticalTargetGoal goal = new TacticalTargetGoal();
				goal.target = target;
				goal.targetForced = true;
				goalController.SetGoals( TAG_CUSTOM, goal );
			}
		}

		

		private static void AssignMoveToGoal( Vector3 terrainHitPos, SSObjectDFS[] selected )
		{
			const float GRID_MARGIN = 0.125f;

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<SSObject> movableGameObjects = new List<SSObject>();

			float biggestRadius = float.MinValue;

			for( int i = 0; i < selected.Length; i++ )
			{
				if( selected[i].factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}
				NavMeshAgent navMeshAgent = selected[i].GetComponent<NavMeshAgent>();
				if( navMeshAgent == null )
				{
					continue;
				}

				// Calculate how big is the biggest unit/hero/etc. to be used when specifying movement grid size.
				movableGameObjects.Add( selected[i] );
				if( navMeshAgent.radius > biggestRadius )
				{
					biggestRadius = navMeshAgent.radius;
				}
			}

			//Calculate the grid position.

			MovementGridInfo gridInfo = new MovementGridInfo( movableGameObjects );


			if( gridInfo.positions.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), Main.cameraPivot.position );
			}
			Quaternion gridRotation = Quaternion.Euler( 0, Main.cameraPivot.rotation.eulerAngles.y, 0 );
			foreach( var kvp in gridInfo.positions )
			{
				Vector3 gridPositionWorld = gridInfo.GridPosToWorld( kvp.Value, gridRotation, terrainHitPos, biggestRadius * 2 + GRID_MARGIN );

				RaycastHit gridHit;
				Ray r = new Ray( gridPositionWorld + new Vector3( 0.0f, 50.0f, 0.0f ), Vector3.down );
				if( Physics.Raycast( r, out gridHit, 100.0f, ObjectLayer.TERRAIN_MASK ) )
				{
					TacticalGoalController goalController = kvp.Key.GetComponent<TacticalGoalController>();
					TacticalMoveToGoal goal = new TacticalMoveToGoal();
					goal.isHostile = false;
					goal.SetDestination( gridPositionWorld );
					goalController.SetGoals( TAG_CUSTOM, goal );
				}
				else
				{
					Debug.LogWarning( "Movement Grid position " + gridPositionWorld + " was outside of the map." );
				}
			}
		}

		private static void AssignMoveToInteriorOrObjGoal( SSObject obj, InteriorModule interior, SSObjectDFS[] selected )
		{

			// Extract only the objects that can have the goal assigned to them from the selected objects.
			List<SSObject> movableGameObjects = new List<SSObject>();

			float biggestRadius = float.MinValue;

			for( int i = 0; i < selected.Length; i++ )
			{
				if( selected[i].factionId != LevelDataManager.PLAYER_FAC )
				{
					continue;
				}
				NavMeshAgent navMeshAgent = selected[i].GetComponent<NavMeshAgent>();
				if( navMeshAgent == null )
				{
					continue;
				}

				// Calculate how big is the biggest unit/hero/etc. to be used when specifying movement grid size.
				movableGameObjects.Add( selected[i] );
				if( navMeshAgent.radius > biggestRadius )
				{
					biggestRadius = navMeshAgent.radius;
				}
			}

			if( movableGameObjects.Count > 0 )
			{
				AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), Main.cameraPivot.position );
			}
			for( int i = 0; i < movableGameObjects.Count; i++ )
			{
				TacticalGoalController goalController = movableGameObjects[i].GetComponent<TacticalGoalController>();
				TacticalMoveToGoal goal = new TacticalMoveToGoal();
				goal.isHostile = false;
				if( interior == null )
				{
					goal.SetDestination( obj );
				}
				else
				{
					goal.SetDestination( interior, InteriorModule.SlotType.Generic );
				}
				goalController.SetGoals( TAG_CUSTOM, goal );
			}
		}

		
	}
}