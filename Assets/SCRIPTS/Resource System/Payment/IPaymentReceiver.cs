using SS.Objects;
using System.Collections.Generic;
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
}