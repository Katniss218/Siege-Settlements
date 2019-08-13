
namespace SS
{
	/// <summary>
	/// Represents an object that belongs to a faction.
	/// </summary>
	public interface IFactionMember
	{
		/// <summary>
		/// Gets the ID of the faction that this object belongs to.
		/// </summary>
		int factionId { get; }

		/// <summary>
		/// Sets the ID of the faction that this object belongs to.
		/// </summary>
		void SetFaction( int id );
	}
}