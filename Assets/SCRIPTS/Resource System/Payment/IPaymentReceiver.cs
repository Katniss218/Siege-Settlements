using System.Collections.Generic;

namespace SS.ResourceSystem.Payment
{
	/// <summary>
	/// An interface that allows receiving payments of resources.
	/// </summary>
	public interface IPaymentReceiver
	{
#warning We might have to deal with multiple payment receivers per object. Interact with the first available. GetComponents<> and get the first that would accept current resources.

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