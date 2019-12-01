using SS.InputSystem;
using UnityEngine;

namespace SS.DevConsole
{
	[DisallowMultipleComponent]
	public class Console : MonoBehaviour
	{
		[SerializeField] private GameObject consoleGameObject = null;
		
		[SerializeField] private TMPro.TextMeshProUGUI output = null;

		public void Print( string message )
		{
			output.text += message;
		}

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
					PrintLine( "<color=#dddddd>[" + System.DateTime.Now.ToLongTimeString() + "] - " + message + "</color>" );
					break;

				case LogType.Warning:
					PrintLine( "<color=#dddd11>[" + System.DateTime.Now.ToLongTimeString() + "] -(*) " + message + "</color>" );
					break;

				case LogType.Error:
					PrintLine( "<color=#dd1111>[" + System.DateTime.Now.ToLongTimeString() + "] -(!) " + message + "</color>" );
					consoleGameObject.SetActive( true ); // If an error is thrown - display the console.
					break;

				case LogType.Exception:
					PrintLine( "<color=#ff2111>[" + System.DateTime.Now.ToLongTimeString() + "] -(!) " + message + "</color>\n<color=#dd3333>" + stackTrace + "</color>" );
					consoleGameObject.SetActive( true ); // If an exception is thrown - display the console.
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