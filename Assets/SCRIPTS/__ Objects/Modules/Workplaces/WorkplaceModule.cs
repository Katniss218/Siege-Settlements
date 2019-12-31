using SS.Objects.Units;

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

#warning If the total amt of civilians employed by all workplaces on a specific building is equal to num of worker slots, block ability to assign more.

		// somehow get all civilians employed at the particular building (interior). Technically, units with an interior could also employ civilians.

		/// <summary>
		/// The workplace-dependent work of the employed civilian.
		/// </summary>
		public abstract void MakeDoWork( Unit worker );
	}
}
