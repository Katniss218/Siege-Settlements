﻿using SS.ResourceSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SS
{
	[DisallowMultipleComponent]
	public class PaymentReceiver : MonoBehaviour
	{
		public class _UnityEvent_string_int : UnityEvent<string, int> { }
		
		public IPaymentProgress paymentProgress { get; set; }

		/// <summary>
		/// Returns the maximum amount of specific resource, that the PaymentReceiver wants.
		/// </summary>
		public int GetWantedAmount( string id )
		{
			return this.paymentProgress.GetWantedAmount( id );
		}

		/// <summary>
		/// Checks if the list of resources contains any wanted ones.
		/// </summary>
		/// <param name="resources">The list of resources to check.</param>
		public bool IsSuitable( Dictionary<string, int> resources )
		{
			if( resources == null )
			{
				return false;
			}
			foreach( var stack in resources )
			{
				if( this.GetWantedAmount( stack.Key ) == 0 )
				{
					continue;
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gives certain amount of resources to the PaymentReceiver.
		/// </summary>
		/// <param name="resources">The resources to give to the PaymentReceiver.</param>
		public void ReceivePayment( string id, int amount )
		{
			if( GetWantedAmount( id ) < amount )
			{
				throw new System.ArgumentOutOfRangeException( "The PaymentReceiver doesn't want that many resources." );
			}

			this.onPaymentMade?.Invoke( id, amount );

			if( this.paymentProgress.IsDone() )
			{
				this.onProgressComplete?.Invoke();
				Object.Destroy( this );
			}
		}

		/// <summary>
		/// Fired when the payment is made.
		/// </summary>
		public _UnityEvent_string_int onPaymentMade = new _UnityEvent_string_int();

		/// <summary>
		/// Fired when the payment is fully completed.
		/// </summary>
		public UnityEvent onProgressComplete = new UnityEvent();
	}
}