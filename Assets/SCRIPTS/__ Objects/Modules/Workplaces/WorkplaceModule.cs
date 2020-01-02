using SS.Levels;
using SS.Objects.Units;
using SS.UI;
using UnityEngine;

namespace SS.Objects.Modules
{
	/// <summary>
	/// Every workplace module needs to inherit from this.
	/// </summary>
	public abstract class WorkplaceModule : SSModule
	{
		/// <summary>
		/// Contains the interior (which contains the slots) used by this workplace.
		/// </summary>
		public InteriorModule interior { get; set; }
				
		public bool CanEmploy( Unit civilian )
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

		public void Employ( Unit civilian )
		{
			for( int i = 0; i < this.interior.workerSlots.Length; i++ )
			{
				if( this.interior.workerSlots[i].worker == null )
				{
					this.interior.workerSlots[i].worker = civilian;

					civilian.workplace = this;
				}
			}
		}

		protected const string UI_WORKER_LIST_ID = "workplace.workerlist";

		protected virtual void Start()
		{
			this.interior = this.ssObject.GetModules<InteriorModule>()[0];
		}

		/// <summary>
		/// The workplace-dependent work of the employed civilian.
		/// </summary>
		public abstract void MakeDoWork( Unit worker );
	}
}
