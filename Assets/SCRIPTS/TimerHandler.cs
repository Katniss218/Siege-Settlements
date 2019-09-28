using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace SS
{
	/// <summary>
	/// Fires an event after a set amount of time. Can be triggered early, which will cancel the timer.
	/// </summary>
	public class TimerHandler : MonoBehaviour
	{
		/// <summary>
		/// The length of time after which the event will trigger.
		/// </summary>
		public float duration = 1.0f;

		/// <summary>
		/// The event that will fire when the timer reaches 'duration' seconds, or when 'TriggerEarly()' is called.
		/// </summary>
		public UnityEvent onTimerEnd = new UnityEvent();
		


		/// <summary>
		/// Starts the timer.
		/// </summary>
		public void StartTimer()
		{
			this.StartCoroutine( this._Timer() );
		}

		/// <summary>
		/// Stops the timer, without triggering the onTimerEnd event.
		/// </summary>
		public void StopTimer()
		{
			this.StopCoroutine( this._Timer() );
		}

		/// <summary>
		/// Restarts the timer.
		/// </summary>
		public void RestartTimer()
		{
			this.StopTimer();
			this.StartTimer();
		}

		/// <summary>
		/// Invokes the onTimerEnd event early, stops the timer.
		/// </summary>
		public void OverrideTrigger()
		{
			this.onTimerEnd?.Invoke();
			this.StopTimer();
		}
		
		IEnumerator _Timer()
		{
			yield return new WaitForSeconds( this.duration );

			this.onTimerEnd?.Invoke();
		}
	}
}