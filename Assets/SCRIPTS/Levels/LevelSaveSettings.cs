namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Contains additional information that can be used to manipulate what gets saved.
	/// </summary>
	public struct LevelSaveSettings
	{
		/// <summary>
		/// Should the information about what is selected and highlighted be saved?
		/// </summary>
		public bool saveSelection { get; set; }
	}
}