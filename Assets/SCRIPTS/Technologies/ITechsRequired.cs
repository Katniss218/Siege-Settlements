namespace SS.Technologies
{
	/// <summary>
	/// Allows you to specify technologies required to unlock the object.
	/// </summary>
	public interface ITechsRequired
	{
		string[] techsRequired { get; }
	}
}