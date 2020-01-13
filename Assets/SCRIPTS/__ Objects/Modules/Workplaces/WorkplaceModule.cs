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
			bool foundEmpty = false;
			for( int i = 0; i < this.interior.workerSlots.Length; i++ )
			{
				if( this.interior.workerSlots[i].worker == null )
				{
					foundEmpty = true;
				}
			}
			return foundEmpty;
		}

		public static void SetWorker( WorkplaceModule w, CivilianUnitExtension c, int slotIndex )
		{
			c.workplace = w;
			c.workplaceSlotId = slotIndex;
			c.unit.navMeshAgent.avoidancePriority = Unit.GetNextAvPriority( true );
			c.GetComponent<TacticalGoalController>().SetGoals( TacticalGoalController.DEFAULT_GOAL_TAG, TacticalGoalController.GetDefaultGoal() );
			c.isOnAutomaticDuty = false;
			c.onEmploy?.Invoke();

			w.interior.workerSlots[slotIndex].worker = c;
			w.interior.hudInterior.workerSlots[slotIndex].SetSprite( c.unit.icon );
			w.interior.hudInterior.workerSlots[slotIndex].SetVisible( false );
		}

		public static void ClearWorker( WorkplaceModule w, CivilianUnitExtension c, int slotIndex )
		{
			c.workplace = null;
			c.workplaceSlotId = 0;
			c.isWorking = false;
			c.unit.navMeshAgent.avoidancePriority = Unit.GetNextAvPriority( false );
			// clear the workplace goal (nothing else is trying to set it so..)
			c.GetComponent<TacticalGoalController>().SetGoals( TacticalGoalController.DEFAULT_GOAL_TAG, TacticalGoalController.GetDefaultGoal() );
			c.onUnemploy?.Invoke();

			ClearWorker( w, slotIndex );
		}

		public static void ClearWorker( WorkplaceModule w, int slotIndex )
		{
			w.interior.workerSlots[slotIndex].worker = null;
			w.interior.hudInterior.workerSlots[slotIndex].ClearSprite();
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

		protected const string UI_WORKER_LIST_ID = "workplace.workerlist";

		protected virtual void Start()
		{

		}

		/// <summary>
		/// Controls the AI of the civilian while he's working.
		/// </summary>
		public abstract void MakeDoWork( Unit worker );
	}
}
