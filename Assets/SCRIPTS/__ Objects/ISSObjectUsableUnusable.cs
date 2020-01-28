using SS.ResourceSystem.Payment;
using UnityEngine.Events;

namespace SS.Objects
{
	interface ISSObjectUsableUnusable
	{
		IPaymentReceiver paymentReceiver { get; }

		UnityEvent onUsableStateChanged { get; }

		bool isUsable { get; set; }
	}
}