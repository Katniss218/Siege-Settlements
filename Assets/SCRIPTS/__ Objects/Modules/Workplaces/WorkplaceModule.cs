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
		
		/// <summary>
		/// Checks if the workplace can employ a specified civilian. Returns true if the civilian can get employed in the workplace.
		/// </summary>
		/// <param name="civilian">The civilian to check.</param>
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

		/// <summary>
		/// Employs the civilian in this workplace.
		/// </summary>
		/// <param name="civilian">The civilian to employ.</param>
		public void Employ( CivilianUnitExtension civilian )
		{
			if( civilian.isEmployed )
			{
				throw new System.Exception( "Can't employ an already employed civilian." );
			}

			for( int i = 0; i < this.interior.workerSlots.Length; i++ )
			{
				if( this.interior.workerSlots[i].worker == null )
				{
					SetWorking( this, civilian, i );
					return;
				}
			}
		}

		/// <summary>
		/// Fires the civilian (makes it no longer employed).
		/// </summary>
		/// <param name="civilian">The civilian to fire.</param>
		public void UnEmploy( CivilianUnitExtension civilian )
		{
			if( !civilian.isEmployed )
			{
				throw new System.Exception( "Can't fire an already fired civilian." );
			}

			for( int i = 0; i < this.interior.workerSlots.Length; i++ )
			{
				if( this.interior.workerSlots[i].worker == civilian )
				{
					ClearWorking( this, civilian, i );
					return;
				}
			}
		}
		
		/// <summary>
		/// An abstract method to control the worker (depending on the implementation, specific to each workplace).
		/// </summary>
		public abstract void MakeDoWork( Unit worker );


		//

		//
		//
		//
	
		//

		
		internal static void SetWorking( WorkplaceModule workplace, CivilianUnitExtension cue, int slotIndex )
		{
			cue.SetAutomaticDuty( false );
			cue.workplace = workplace;
			cue.workplaceSlotIndex = slotIndex;
			cue.obj.navMeshAgent.avoidancePriority = CivilianUnitExtension.NextAvoidancePriority( true );
			cue.obj.controller.SetGoals( TacticalGoalController.DEFAULT_GOAL_TAG, TacticalGoalController.GetDefaultGoal() );

			workplace.interior.workerSlots[slotIndex].worker = cue;
			workplace.interior.hudInterior.workerSlots[slotIndex].SetSprite( cue.obj.icon );
			workplace.interior.hudInterior.workerSlots[slotIndex].SetVisible( false );

			cue.onEmploy?.Invoke();
		}

		internal static void ClearWorking( WorkplaceModule workplace, CivilianUnitExtension cue, int slotIndex )
		{
			cue.isWorking = false;
			cue.workplace = null;
			cue.workplaceSlotIndex = 0;
			cue.obj.navMeshAgent.avoidancePriority = CivilianUnitExtension.NextAvoidancePriority( false );
			cue.obj.controller.SetGoals( TacticalGoalController.DEFAULT_GOAL_TAG, TacticalGoalController.GetDefaultGoal() );

			workplace.interior.workerSlots[slotIndex].worker = null;
			workplace.interior.hudInterior.workerSlots[slotIndex].ClearSprite();

			cue.onUnemploy?.Invoke();
		}
	}
}
