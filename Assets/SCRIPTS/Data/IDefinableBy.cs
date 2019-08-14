
namespace SS.Data
{
	/// <summary>
	/// Represents any object that can be defined in terms of a definition class.
	/// </summary>
	/// <typeparam name="T">The type of definition that is defining this type of object.</typeparam>
	public interface IDefinableBy<T> where T : Definition
	{
		/// <summary>
		/// Returns the definition ID of this object (Read Only).
		/// </summary>
		string id { get; }

		/// <summary>
		/// Assigns a specified definition to this object. Sets all of the relevant fields to appropriate values.
		/// </summary>
		/// <param name="def">The assigned definition.</param>
		void AssignDefinition( T def );
	}
}