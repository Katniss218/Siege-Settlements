
using KFF;

namespace SS.Content
{
	/// <summary>
	/// An abstract class for storing data, of objects, that doesn't change.
	/// </summary>
	public abstract class Definition : IKFFSerializable
	{
		/// <summary>
		/// The id of this definition (Read Only).
		/// </summary>
		public string id { get; protected set; }

		protected Definition( string id )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new System.Exception( "Definition can't have null or empty id." );
			}
			this.id = id;
		}

		public abstract void SerializeKFF( KFFSerializer serializer );
		public abstract void DeserializeKFF( KFFSerializer serializer );
	}
}