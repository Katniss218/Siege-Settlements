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
			if( civilian.workplace != null )
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
			w.interior.workerSlots[slotIndex].worker = c;
			c.workplace = w;
			c.workplaceSlotId = slotIndex;
			c.unit.navMeshAgent.avoidancePriority = Unit.GetNextAvPriority( true);
		}

		public static void ClearWorker( WorkplaceModule w, CivilianUnitExtension c, int slotIndex )
		{
			w.interior.workerSlots[slotIndex].worker = null;
			c.workplace = null;
			c.workplaceSlotId = 0;
			c.unit.navMeshAgent.avoidancePriority = Unit.GetNextAvPriority( false );
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
