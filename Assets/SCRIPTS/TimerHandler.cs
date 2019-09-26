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

		void Start()
		{
			this.StartCoroutine( this._Timer() );
		}

		public void ResetTimer()
		{
			this.StopCoroutine( this._Timer() );
			this.StartCoroutine( this._Timer() );
		}

		/// <summary>
		/// Invokes the event early, stops the timer from firing after the set amount of time.
		/// </summary>
		public void OverrideTrigger()
		{
			this.onTimerEnd?.Invoke();
			this.StopAllCoroutines();
		}
		
		IEnumerator _Timer()
		{
			yield return new WaitForSeconds( this.duration );

			this.onTimerEnd?.Invoke();
		}
	}
}