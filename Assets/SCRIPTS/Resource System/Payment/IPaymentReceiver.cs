using SS.Content;
using SS.Objects;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;

namespace SS.ResourceSystem.Payment
{
	/// <summary>
	/// An interface that allows receiving payments of resources. Must be added to a MonoBehaviour, otherwise can cause exceptions with casting.
	/// </summary>
	public interface IPaymentReceiver
	{
		SSObject ssObject { get; }

		UnityEvent onPaymentReceived { get; }

		/// <summary>
		/// Makes this payment receiver receive a payment.
		/// </summary>
		/// <param name="resources">Every resource that the payment consists of.</param>
		void ReceivePayment( string id, int amount );


		/// <summary>
		/// Returns a dictionary of every currently wanted resource.
		/// </summary>
		Dictionary<string, int> GetWantedResources();
	}

	public static class ResourceUtils
	{
		public static string ToResourceString( Dictionary<string, int> resources )
		{
			if( resources == null )
			{
				return "";
			}

			StringBuilder sb = new StringBuilder();

			int i = 0;
			foreach( var kvp in resources )
			{
				if( i > 0 )
				{
					sb.Append( ", " );
				}

				if( kvp.Value != 0 )
				{
					ResourceDefinition resDef = DefinitionManager.GetResource( kvp.Key );
					sb.Append( kvp.Value + "x " + resDef.displayName );
				}

				i++;
			}

			return sb.ToString();
		}
	}
}