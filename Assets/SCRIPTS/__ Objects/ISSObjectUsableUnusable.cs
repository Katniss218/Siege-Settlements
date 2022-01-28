using SS.ResourceSystem.Payment;
using UnityEngine.Events;

namespace SS.Objects
{
	/// <summary>
	/// Represents an object that can either be usable or unusable at a given time.
	/// </summary>
	interface ISSObjectUsableUnusable
	{
		IPaymentReceiver paymentReceiver { get; }

		UnityEvent onUsableStateChanged { get; }

		bool isUsable { get; set; }
	}
}