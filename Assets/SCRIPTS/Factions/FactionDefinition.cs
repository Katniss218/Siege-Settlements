using KFF;
using UnityEngine;

namespace SS
{
	/// <summary>
	/// Represents faction's data, that doesn't change.
	/// </summary>
	public class FactionDefinition : IKFFSerializable
	{
		/// <summary>
		/// The name that is displayed on the diplomacy menu.
		/// </summary>
		public string displayName { get; private set; }

		/// <summary>
		/// The team color of the faction.
		/// </summary>
		public Color color { get; private set; }


		// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.displayName = serializer.ReadString( "DisplayName" );
			this.color = serializer.ReadColor( "Color" );
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "DisplayName", this.displayName );
			serializer.WriteColor( "", "Color", this.color );
		}
	}
}