using KFF;
using UnityEngine;

namespace SS
{
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
			this.displayName = serializer.ReadString( "Name" );
			this.color = serializer.ReadColor( "Color" );
#warning incomplete.
			throw new System.NotImplementedException();
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Name", this.displayName );
			serializer.WriteColor( "", "Color", this.color );
#warning incomplete.
			throw new System.NotImplementedException();
		}
	}
}