using SS.AI;
using SS.Objects.Units;

namespace SS.Objects.Modules
{
	/// <summary>
	/// Every workplace module needs to inherit from this.
	/// </summary>
	public abstract class WorkplaceModule : SSModule
	{
		private InteriorModule __interior = null;
		/// <summary>
		/// Contains the interior (which contains the slots) used by this workplace.
		/// </summary>
		public InteriorModule interior
		{
			get
			{
				if( this.__interior == null )
				{
					this.__interior = this.ssObject.GetModules<InteriorModule>()[0];
				}
				return this.__interior;
			}
		}
		
		public bool CanEmploy( CivilianUnitExtension civilian )
		{
			// Returns true if there is space left in the interior's worker slots. Returns false if the civilian is already employed.
			if( civilian.isEmployed )
			{
				return false;
			}
			
			for( int i = 0; i < this.interior.workerSlots.Length; i++ )
			{
				if( this.interior.workerSlots[i].worker == null )
				{
					return true;
				}
			}
			return false;
		}

		public void Employ( CivilianUnitExtension civilian )
		{
			for( int i = 0; i < this.interior.workerSlots.Length; i++ )
			{
				if( this.interior.workerSlots[i].worker == null )
				{
					SetWorker( this, civilian, i );
					return;
				}
			}
		}

		public void UnEmploy( CivilianUnitExtension civilian )
		{
			for( int i = 0; i < this.interior.workerSlots.Length; i++ )
			{
				if( this.interior.workerSlots[i].worker == civilian )
				{
					ClearWorker( this, civilian, i );
					return;
				}
			}
		}
		
		/// <summary>
		/// Controls the AI of the civilian while he's working.
		/// </summary>
		public abstract void MakeDoWork( Unit worker );


		//

		//
		//
		//
	
		//


		public static void SetWorker( WorkplaceModule workplace, CivilianUnitExtension cue, int slotIndex )
		{
			cue.workplace = workplace;
			cue.workplaceSlotId = slotIndex;
			cue.unit.navMeshAgent.avoidancePriority = CivilianUnitExtension.GetNextAvPriority( true );
			cue.GetComponent<TacticalGoalController>().SetGoals( TacticalGoalController.DEFAULT_GOAL_TAG, TacticalGoalController.GetDefaultGoal() );
			cue.isOnAutomaticDuty = false;
			cue.onEmploy?.Invoke();

			workplace.interior.workerSlots[slotIndex].worker = cue;
			workplace.interior.hudInterior.workerSlots[slotIndex].SetSprite( cue.unit.icon );
			workplace.interior.hudInterior.workerSlots[slotIndex].SetVisible( false );
		}

		public static void ClearWorker( WorkplaceModule workplace, CivilianUnitExtension cue, int slotIndex )
		{
			cue.workplace = null;
			cue.workplaceSlotId = 0;
			cue.isWorking = false;
			cue.unit.navMeshAgent.avoidancePriority = CivilianUnitExtension.GetNextAvPriority( false );
			// clear the workplace goal (nothing else is trying to set it so..)
			cue.GetComponent<TacticalGoalController>().SetGoals( TacticalGoalController.DEFAULT_GOAL_TAG, TacticalGoalController.GetDefaultGoal() );
			cue.onUnemploy?.Invoke();

			workplace.ClearWorker( slotIndex );
		}

		public void ClearWorker( int slotIndex )
		{
			this.interior.workerSlots[slotIndex].worker = null;
			this.interior.hudInterior.workerSlots[slotIndex].ClearSprite();
		}
	}
}
