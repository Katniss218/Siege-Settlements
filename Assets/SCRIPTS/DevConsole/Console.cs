using SS.InputSystem;
using UnityEngine;

namespace SS.DevConsole
{
	[DisallowMultipleComponent]
	public class Console : MonoBehaviour
	{
		const string LOG_COLOR_INFO = "#dddddd";
		const string LOG_COLOR_WARN = "#dddd55";
		const string LOG_COLOR_ERROR = "#dd5555";

		const string LOG_COLOR_EXCEPTION = "#dd5555";
		const string LOG_COLOR_EXCEPTION_STACK = "#c55555";

		[SerializeField] private GameObject consoleGameObject = null;
		
		[SerializeField] private TMPro.TextMeshProUGUI output = null;

		/// <summary>
		/// Prints a string out to the console.
		/// </summary>
		public void Print( string message )
		{
			output.text += message;
		}

		/// <summary>
		/// Prints a string terminated with a newline character.
		/// </summary>
		public void PrintLine( string message )
		{
			output.text += message;
			output.text += "\n";
		}

		private void HandleLog( string message, string stackTrace, LogType logType )
		{
			switch( logType )
			{
				case LogType.Log:
					PrintLine( $"<color={LOG_COLOR_INFO}>[{System.DateTime.Now.ToLongTimeString()}](___) - {message}</color>" );
					break;

				case LogType.Warning:
					PrintLine( $"<color={LOG_COLOR_WARN}>[{System.DateTime.Now.ToLongTimeString()}](WRN) - {message}</color>" );
					break;

				case LogType.Error:
					PrintLine( $"<color={LOG_COLOR_ERROR}>[{System.DateTime.Now.ToLongTimeString()}](ERR) - {message}</color>" );
					consoleGameObject.SetActive( true );
					break;

				case LogType.Exception:
					PrintLine( $"<color={LOG_COLOR_EXCEPTION}>[{System.DateTime.Now.ToLongTimeString()}](EXC) - {message}</color>\n  at\n<color={LOG_COLOR_EXCEPTION_STACK}>" + stackTrace + "</color>" );
					consoleGameObject.SetActive( true );
					break;
			}
		}

		private void Inp_Tilde( InputQueue self )
		{
			consoleGameObject.SetActive( !consoleGameObject.activeSelf );
		}

		void Awake()
		{
			if( !output.richText )
			{
				Debug.LogWarning( "The console isn't set to Rich Rext, setting to Rich Text now." );
				output.richText = true;
			}
			output.text = "Console:\n\n"; // This is required to fix glitch requiring reenabling the gameObject after adding some text to the output (if it's set to blank).
			consoleGameObject.SetActive( false );
		}

		void OnEnable()
		{
			if( Main.keyboardInput != null )
			{
				Main.keyboardInput.RegisterOnPress( KeyCode.BackQuote, -666.69f, this.Inp_Tilde, true );
			}
			Application.logMessageReceived += this.HandleLog;
		}
		
		void OnDisable()
		{
			if( Main.keyboardInput != null )
			{
				Main.keyboardInput.ClearOnPress( KeyCode.BackQuote, this.Inp_Tilde );
			}
			Application.logMessageReceived -= this.HandleLog;
		}
	}
}