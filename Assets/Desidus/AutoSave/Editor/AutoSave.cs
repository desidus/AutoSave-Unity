/*
 *
 *                                /IIIIIII 
 *                       /SSSSSS |__  II_/ /DDDDDDD 
 *            /EEEEEEEE /SS__  SS   | II  | DD__  DD /UU   /UU
 *  /DDDDDDD | EE_____/| SS  \__/   | II  | DD  \ DD| UU  | UU  /SSSSSS 
 * | DD__  DD| EE      |  SSSSSS    | II  | DD  | DD| UU  | UU /SS__  SS
 * | DD  \ DD| EEEEE    \____  SS   | II  | DD  | DD| UU  | UU| SS  \__/
 * | DD  | DD| EE__/    /SS  \ SS /IIIIIII| DD  | DD| UU  | UU|  SSSSSS 
 * | DD  | DD| EE      |  SSSSSS/|_______/| DDDDDDD/| UU  | UU \____  SS
 * | DD  | DD| EEEEEEEE \______/          |_______/ |  UUUUUU/ /SS  \ SS
 * | DDDDDDD/|________/                              \______/ |  SSSSSS/
 * |_______/                                                   \______/ 
 * 
 *
 *
 * ============= AutoSave =============
 *
 * AutoSave plugin for Unity.
 *
 * Version:
 *   2.0
 *
 * Authors:
 *   Ettore Passaro <ettore3091@gmail.com>
 *	 Desidus <info@desidus.it>
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using It.Desidus.EditorExtensions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace It.Desidus.EditorExtensions
{
	/// <summary>
	/// Add the AutoSave function to the Unity Editor, stores and manages scenes backups. 
	/// The class executes when the Unity Editor is launched.
	/// </summary>
	[InitializeOnLoad]
	public static class AutoSave
	{

#region Constants

		private const string AUTOSAVE_VERSION = "2.0";

		/// <summary>
		/// The format used to store DateTime values in the EditorPrefs.
		/// </summary>
		private const string DATE_FORMAT = "yyyy/MM/dd HH:mm:ss.f";

		private const int DEFAULT_SAVE_MINUTES = 10;

		private const string SAVE_MIN_PREF = "DAS_SaveMin";
		private const string ENABLED_PREF = "DAS_Enabled";
		private const string ONPLAY_PREF = "DAS_OnPlay";
		private const string BACKGROUND_PREF = "DAS_BgOn";
		private const string DEF_BACKGROUND_PREF = "DAS_DefRunBg";
		private const string POPUP_PREF = "DAS_PopOn";
		private const string TIMEOUT_PREF = "DAS_PopTime";
		private const string LOG_PREF = "DAS_LogOn";
		private const string LAST_SAVE_PREF = "DAS_LastSave";


		// /////////////////////////////////////////////////////////////////// //
		// If you want to increase the time ranges in the AutoSave preferences //
		// tab, modify the following values.								   //  
		// /////////////////////////////////////////////////////////////////// //
		private const int MAX_SAVE_MINUTES = 30;
		private const int MAX_POPUP_TIMEOUT = 30;

#endregion

#region Properties

		/// <summary>
		/// The flag that enables the AutoSave integration.
		/// The value is stored into the DAS_Enabled EditorPrefs key.
		/// </summary>
		public static bool AutoSaveEnabled
		{
			get
			{
				return EditorPrefs.GetBool(ENABLED_PREF, true);
			}
			private set
			{
				EditorPrefs.SetBool(ENABLED_PREF, value);
			}
		}

		/// <summary>
		/// The number of minutes between each AutoSave.
		/// The value is stored into the DAS_SaveMin EditorPrefs key.
		/// If the EditorPrefs key has a value of 0, the value is set to DEFAULT_SAVE_MINUTES.
		/// </summary>
		public static int SaveMinutes
		{
			get
			{
				int current = EditorPrefs.GetInt(SAVE_MIN_PREF, 0);
				if (current == 0)
				{
					current = DEFAULT_SAVE_MINUTES;
					EditorPrefs.SetInt(SAVE_MIN_PREF, current);
				}
				return current;
			}
			private set
			{
				EditorPrefs.SetInt(SAVE_MIN_PREF, value);
			}
		}

		/// <summary>
		/// The flag that enables to AutoSave scenes when play is pressed.
		/// The value is stored into the DAS_OnPlay EditorPrefs key.
		/// </summary>
		public static bool AutoSaveAfterPlay
		{
			get
			{
				return EditorPrefs.GetBool(ONPLAY_PREF, true);
			}
			private set
			{
				EditorPrefs.SetBool(ONPLAY_PREF, value);
			}
		}

		/// <summary>
		/// The flag that enables AutoSave to run while the Editor is running in background. 
		/// The value is stored into the DAS_BgOn EditorPrefs key.
		/// </summary>
		public static bool EnabledInBackground
		{
			get
			{
				return EditorPrefs.GetBool(BACKGROUND_PREF, true);
			}
			private set
			{
				EditorPrefs.SetBool(BACKGROUND_PREF, value);
			}
		}

		/// <summary>
		/// Default Application.runInBackgroud value.
		/// The value is stored into the DAS_DefRunBg EditorPrefs key.
		/// </summary>
		public static bool DefaultRunInBackground
		{
			get
			{
				return EditorPrefs.GetBool(DEF_BACKGROUND_PREF, false);
			}
			private set
			{
				EditorPrefs.SetBool(DEF_BACKGROUND_PREF, value);
			}
		}

		/// <summary>
		/// Show a popup before saving the scenes, so AutoSave can be canceled if unwanted. 
		/// The value is stored into the DAS_PopOn EditorPrefs key.
		/// </summary>
		public static bool ShowPopup
		{
			get
			{
				return EditorPrefs.GetBool(POPUP_PREF, true);
			}
			private set
			{
				EditorPrefs.SetBool(POPUP_PREF, value);
			}
		}

		/// <summary>
		/// How long before the popup is closed and AutoSave is performed.
		/// The value is stored into the DAS_PopTime EditorPrefs key.
		/// </summary>
		public static int PopupTimeout
		{
			get
			{
				return EditorPrefs.GetInt(TIMEOUT_PREF, 10);
			}
			private set
			{
				EditorPrefs.SetInt(TIMEOUT_PREF, Mathf.Clamp(value, 0, 30));
			}
		}

		/// <summary>
		/// The flag that enables AutoSave to log info on the plugin's 
		/// activities. The value is stored into the DAS_LogOn 
		/// EditorPrefs key.
		/// </summary>
		public static bool LogInfo
		{
			get
			{
				return EditorPrefs.GetBool(LOG_PREF, true);
			}
			private set
			{
				EditorPrefs.SetBool(LOG_PREF, value);
			}
		}

		/// <summary>
		/// The last time an AutoSave occurred.
		/// The value is stored into the DAS_LastSave EditorPrefs key as a 
		/// string formatted according to DATE_FORMAT.
		/// If the EditorPrefs key is equal to DateTime.MinValue, the value is 
		/// set to DateTime.Now. 
		/// </summary>
		public static DateTime LastSaveTime
		{
			get
			{
				string date = EditorPrefs.GetString(
					LAST_SAVE_PREF,
					DateTime.MinValue.ToString(DATE_FORMAT));

				DateTime current = DateTime.ParseExact(date, DATE_FORMAT, null);

				if (current.Equals(DateTime.MinValue))
				{
					current = DateTime.Now;

					EditorPrefs.SetString(
						LAST_SAVE_PREF,
						current.ToString(DATE_FORMAT));
				}
				return current;
			}
			set
			{
				EditorPrefs.SetString(
					LAST_SAVE_PREF,
					value.ToString(DATE_FORMAT));
			}
		}

#endregion

		static AutoSave()
		{
			if(!EditorPrefs.HasKey(ENABLED_PREF))
			{
				DefaultRunInBackground = Application.runInBackground;
			}

			if (AutoSaveEnabled)
			{
				EnableAutoSave();
			}
			#if !UNITY_2017_2_OR_NEWER
			else if (AutoSaveAfterPlay)
			{
				SaveScenes();
			}
			#endif          
		}

#region FrontEnd

		internal static class SettingsLabels
		{
			public static readonly GUIContent enable = 
				EditorGUIUtility.TrTextContent("Enable AutoSave");
			public static readonly GUIContent onPlay = 
				EditorGUIUtility.TrTextContent("Save on Play");
			public static readonly GUIContent background = 
				EditorGUIUtility.TrTextContent("Run while Editor in background");
			public static readonly GUIContent popup = 
				EditorGUIUtility.TrTextContent("Show AutoSave popup");
			public static readonly GUIContent timeout = 
				EditorGUIUtility.TrTextContent(
					"Popup timeout", 
					"Time before the popup is closed and the scenes are saved.");
			public static readonly GUIContent frequency = 
				EditorGUIUtility.TrTextContent(
					"AutoSave every", 
					"Time span between AutoSaves.");
			public static readonly GUIContent seconds = 
				EditorGUIUtility.TrTextContent("sec.");
			public static readonly GUIContent minutes = 
				EditorGUIUtility.TrTextContent("min.");
			public static readonly GUIContent log = 
				EditorGUIUtility.TrTextContent("Show AutoSave log");
			public static readonly GUIContent version = 
				EditorGUIUtility.TrTextContent("Version " + AUTOSAVE_VERSION);

			public const string providerLabel = "AutoSave";
			public const string providerPath = "Preferences/AutoSave";
			public static readonly HashSet<string> keywords = 
				new HashSet<string>(
					new[] { 
						"AutoSave", 
						"AutoSavePopup" 
					});
		}


		/// <summary>
		/// Generates the AutoSave preference tab.
		/// </summary>
		[SettingsProvider]
		public static SettingsProvider ShowAutoSavePreferences()
		{
			var provider = new SettingsProvider(SettingsLabels.providerPath, SettingsScope.User)
			{
				label = SettingsLabels.providerLabel,
				keywords = SettingsLabels.keywords,
				guiHandler = (searchContext) =>
				{
					DrawGUI();
				}				
			};			

			return provider;
		}

		private static void DrawGUI()
		{
			float defaultLabelWidth = EditorGUIUtility.labelWidth; // save Unity's default value
			float autosaveLabelWidth = 200;
			int save = SaveMinutes;
			bool en = AutoSaveEnabled;
			bool splay = AutoSaveAfterPlay;
			bool bg = EnabledInBackground;
			bool pen = ShowPopup;
			float ptime = PopupTimeout;
			bool log = LogInfo;
			string info = GetInfo();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(10);
			EditorGUILayout.BeginVertical(GUILayout.MaxWidth(490));

			EditorGUIUtility.labelWidth = autosaveLabelWidth;
			en = EditorGUILayout.Toggle(SettingsLabels.enable, en);

			using (new EditorGUI.DisabledScope(!AutoSaveEnabled))
			{
				splay = EditorGUILayout.Toggle(SettingsLabels.onPlay, splay);
			
				bg = EditorGUILayout.Toggle(SettingsLabels.background, bg);

				GUILayout.Space(8);

				ShowPopup = EditorGUILayout.Toggle(SettingsLabels.popup, ShowPopup);

				using (new EditorGUI.DisabledScope(!ShowPopup))
				{
					EditorGUILayout.BeginHorizontal();
					PopupTimeout = EditorGUILayout.IntSlider(
						SettingsLabels.timeout,
						PopupTimeout,
						1,
						30);
					GUILayout.Label(SettingsLabels.seconds, GUILayout.MaxWidth(30));
					EditorGUILayout.EndHorizontal();
				}			

				EditorGUILayout.BeginHorizontal();
				save = EditorGUILayout.IntSlider(
					SettingsLabels.frequency,
					save,
					1,
					MAX_SAVE_MINUTES);
				GUILayout.Label(SettingsLabels.minutes, GUILayout.MaxWidth(30));
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(8);

				log = EditorGUILayout.Toggle(SettingsLabels.log, log);
			}


			if (GUI.changed)
			{
				if (save != SaveMinutes)
				{
					info = SetInterval(save);
				}

				if (en)
				{
					if (!AutoSaveEnabled)
					{
						info = EnableAutoSave();
					}
					else
					{
						info = GetInfo();
					}
				}
				else
				{
					if (AutoSaveEnabled)
					{
						info = DisableAutoSave();
					}
					else
					{
						info = GetInfo();
					}
				}

				if (splay != AutoSaveAfterPlay)
				{
					AutoSaveAfterPlay = splay;
				}

				if (bg != EnabledInBackground)
				{
					EnableInBackground(bg);
				}

				if (log != LogInfo)
				{
					LogInfo = log;
				}
			}

			GUILayout.Space(12);

			EditorGUILayout.HelpBox(info, MessageType.Info);

			GUILayout.Space(8);

			EditorGUILayout.LabelField(SettingsLabels.version, EditorStyles.miniBoldLabel);

			EditorGUIUtility.labelWidth = defaultLabelWidth;
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}

		private class AutoSavePopup : EditorWindow
		{
			private static Rect mainPos = Rect.zero;
			bool hasInput = false;
			double time = 10;
			float timeOut;
			Action callback;
			double startTime = 0;

			public static void ShowAutoSavePopup(Action saveFunction, float timeOut)
			{
				AutoSavePopup window =
					(AutoSavePopup) EditorWindow.GetWindow(
						typeof(AutoSavePopup),
						true,
						"Auto Save Popup");

				Vector2 size = new Vector2(200, 80);

				UpdateMainPos();						

				Vector2 pos = 
					new Vector2(
						(mainPos.x + mainPos.width) - 210, 
						(mainPos.y + mainPos.height) - 90);

				window.minSize = size;
				window.maxSize = size;
				window.position = new Rect(pos, size);

				window.Setup(saveFunction, timeOut);
			}

			static internal void UpdateMainPos()
			{
				try
				{
					mainPos = DesidusEditorUtility.GetEditorMainWindowPos();
				} 
				catch(Exception e)
				{
					Debug.LogError(
						"Can't find Editor main window. " + 
						"Popup will open on main screen's lower right corner. " +
						"Please contact the author if this error appears.");
					Debug.LogException(e);

					mainPos = new Rect(
						0, 
						0, 
						Screen.currentResolution.width, 
						Screen.currentResolution.height - 40); // -40 subtracts Windows bar max size
				}
			}

			public void Setup(Action callback, float timeOut)
			{
				this.callback = callback;
				this.timeOut = timeOut;
				hasInput = false;
				startTime = EditorApplication.timeSinceStartup;
				EditorApplication.update -= PopupUpdate;
				EditorApplication.update += PopupUpdate;
			}

			void PopupUpdate()
			{
				if (time > 0 && !hasInput)
				{
					int prev = (int) time;
					time = timeOut - (EditorApplication.timeSinceStartup - startTime);
					if (prev > time)
					{
						this.Repaint();
					}
				}
				else
				{
					if (!hasInput && callback != null)
					{
						callback();
						Close();
						ShowNotification(new GUIContent("Scene saved."));
					}
				}
			}

			void OnGUI()
			{
				GUILayout.Space(12);

				EditorGUILayout.LabelField("Auto Save in " + ((int) time + 1) + " seconds.");

				GUILayout.FlexibleSpace();

				GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
				{
					fixedWidth = 80,
					fixedHeight = 24
				};

				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(14);
				if (GUILayout.Button("Save now", buttonStyle))
				{
					hasInput = true;
					callback();
					Close();
				}
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Skip", buttonStyle))
				{
					hasInput = true;
					Close();
				}
				GUILayout.Space(10); // for some reasons, this has to be asymmetric.
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(12);
			}

			void OnDestroy()
			{
				EditorApplication.update -= PopupUpdate;		
			}
		}

#endregion

#region BackEnd

		/// <summary>
		/// Checks when it's time to AutoSave and make backups.
		/// </summary>
		static void OnUpdate()
		{
			if ((DateTime.Now - LastSaveTime).TotalMinutes >= SaveMinutes)
			{
				int sc = EditorSceneManager.sceneCount;
				for(int i = 0; i < sc; i++)
				{
					// Show popup or save scenes only if there is at least one dirty.
					if(EditorSceneManager.GetSceneAt(i).isDirty)
					{
						if(ShowPopup)
						{
							AutoSavePopup.ShowAutoSavePopup(SaveScenes, PopupTimeout);
						}
						else
						{
							SaveScenes();
						}
						break;
					}
				}		
				LastSaveTime = DateTime.Now;		
			}
		}

		static string EnableAutoSave()
		{
			EditorApplication.update -= OnUpdate;
			EditorApplication.update += OnUpdate;

			EnableInBackground(EnabledInBackground);

			#if UNITY_2017_2_OR_NEWER
			if (AutoSaveAfterPlay)
			{
				EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
				EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			}
			#endif

			AutoSaveEnabled = true;

			return GetInfo();
		}

		#if UNITY_2017_2_OR_NEWER
		static void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.ExitingEditMode)
			{
				SaveScenes();
			}
		}
		#endif

		static string DisableAutoSave()
		{
			if (AutoSaveEnabled)
			{
				EditorApplication.update -= OnUpdate;
				EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
				AutoSaveEnabled = false;
			}

			return GetInfo();
		}

		static void EnableInBackground(bool _enabled)
		{
			if (Application.isEditor)
			{
				Application.runInBackground = _enabled || DefaultRunInBackground;
				EnabledInBackground = _enabled;
			}
		}

		/// <summary>
		/// Sets time intervals between AutoSave calls.
		/// </summary>
		/// <param name="_save"> Minutes between each AutoSave. </param>
		/// <returns> Returns the info message. </returns>
		static string SetInterval(int _save)
		{
			SaveMinutes = _save;

			return GetInfo();
		}

		static string GetInfo()
		{
			string message;

			if (!AutoSaveEnabled)
			{
				return "AutoSave disabled.\n";
			}

			if (AutoSaveAfterPlay)
			{
				message = "AutoSave enabled. Saves scenes on Play.\n";
			}
			else
			{
				message = "AutoSave enabled. Doesn't save scenes on Play.\n";
			}

			message += "AutoSave every " + SaveMinutes + " minutes.\n";

			if (EnabledInBackground)
			{
				message += "AutoSave enabled when Editor is in background.\n";
			}
			else
			{
				message += "AutoSave disabled when Editor is in background.\n";
			}

			if(ShowPopup)
			{
				message += "Popup enabled with " + PopupTimeout + " seconds timeout.\n";
			}
			else
			{
				message += "Popup disabled.\n";
			}

			if (LogInfo)
			{
				message += "Log enabled.";
			}
			else
			{
				message += "Log disabled.";
			}

			return message;

		}

		static void SaveScenes()
		{
			EditorSceneManager.SaveOpenScenes();
			AssetDatabase.SaveAssets();
			if (LogInfo)
			{
				Debug.Log("[AutoSave] AutoSaved scenes.\n" + DateTime.Now);
			}
		}

#endregion
	}
}