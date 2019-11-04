using KFF;

namespace SS.Content
{
	public abstract class AddressableDefinition : IKFFSerializable
	{
		/// <summary>
		/// The id (address) of this definition (Read Only).
		/// </summary>
		public string id { get; protected set; }

		protected AddressableDefinition( string id )
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