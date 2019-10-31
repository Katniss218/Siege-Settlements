namespace SS
{
	/// <summary>
	/// Represents an asset that can be accessed by a path.
	/// </summary>
	/// <typeparam name="T">The type of asset.</typeparam>
	public class AddressableAsset<T>
	{
		private readonly string address;

		private readonly T asset;

		public AddressableAsset( string address, T obj )
		{
			this.address = address;
			this.asset = obj;
		}


		public static explicit operator string( AddressableAsset<T> other )
		{
			return other.address;
		}

		public static implicit operator T( AddressableAsset<T> other )
		{
			return other.asset;
		}
	}
}