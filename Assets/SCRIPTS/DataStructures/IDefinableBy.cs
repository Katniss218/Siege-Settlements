
namespace SS.DataStructures
{
	public interface IDefinableBy<T> where T : Definition
	{
		// TODO! ----- get definition id and the data for saving with the level.

		// Assigns definition to this object.
		void AssignDefinition( T def );
	}
}