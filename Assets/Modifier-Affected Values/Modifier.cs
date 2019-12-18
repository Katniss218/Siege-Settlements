using KFF;

namespace Katniss.ModifierAffectedValues
{
	public struct Modifier : IKFFSerializable
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


		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.value = serializer.ReadFloat( "Value" );
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteFloat( "", "Value", this.value );
		}
	}
}
