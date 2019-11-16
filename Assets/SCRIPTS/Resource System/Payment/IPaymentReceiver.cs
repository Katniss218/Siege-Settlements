using System.Collections.Generic;
using UnityEngine.Events;

namespace SS.ResourceSystem.Payment
{
	/// <summary>
	/// An interface that allows receiving payments of resources.
	/// </summary>
	public interface IPaymentReceiver
	{
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