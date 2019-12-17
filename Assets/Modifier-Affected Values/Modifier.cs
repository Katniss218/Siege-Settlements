namespace Katniss.ModifierAffectedValues
{
	public struct Modifier
	{
		public string id;
		public float value;

		public void Set( float value )
		{
			this.value = value;
		}

		public Modifier( string id, float value )
		{
			this.id = id;
			this.value = value;
		}
	}
}
