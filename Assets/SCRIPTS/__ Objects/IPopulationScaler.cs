namespace SS.Objects
{
	interface IPopulationScaler
	{
		/// <summary>
		/// Gets or sets the population, and recalculate fields scaled by it to the new value.
		/// </summary>
		PopulationSize population { get; set; }
	}

	public static class IPopulationScalerExtensions
	{
		public static float GetLinearScale( PopulationSize fromSize, PopulationSize toSize, float value )
		{
			return value * ((float)toSize / (float)fromSize);
		}
	}
}