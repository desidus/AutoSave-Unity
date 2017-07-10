/*
 * AutoSave
 *
 * AutoSave system integration for the Unity Editor.
 *
 * Version:
 *   1.0
 *
 * Authors:
 *   GameBang <info@gamebang.it>
 */
namespace Com.GameBang.Editor
{
    using System.Globalization;
    using System.IO;
    using System;
    using UnityEditor.SceneManagement;
    using UnityEditor;
    using UnityEngine;

	/// <summary>
	/// Add the AutoSave function to the Unity Editor, stores and manages scenes backups.
	/// The class executes when the Unity Editor is launched.
	/// </summary>
    [InitializeOnLoad]
    public static class AutoSave
    {

#region Fields

        /// <summary>
        /// The current AutoSave version.
        /// </summary>
        private const string AUTOSAVE_VERSION = "1.0";

        /// <summary>
        /// The format used to store DateTime values in the EditorPrefs.
        /// </summary>
        private const string DATE_FORMAT = "dd/MM/yyyy HH:mm:ss.f";

        /// <summary>
        /// The default folder name for the AutoSaves folder.
        /// </summary>
        private const string DEFAULT_SAVE_FOLDER = "/AutoSaves/";

        /// <summary>
        /// The default number of minutes between each AutoSave. 
        /// </summary>
        private const int DEFAULT_SAVE_MINUTES = 10;

        /// <summary>
        /// The default number of minutes between each scene backup.
        /// </summary>
        private const int DEFAULT_BACKUP_MINUTES = 20;

        /// <summary>
        /// The default number of minutes that have to pass before an old backup gets deleted.
        /// </summary>
        private const int DEFAULT_DELETE_MINUTES = 60;

		// //////////////////////////////////////////////////////////////////////////////////// //
		// If you want to augment the time ranges in the AutoSave preferences tab, modify the 	//
		// following values.																	//
		// //////////////////////////////////////////////////////////////////////////////////// //

        /// <summary>
        /// The maximum number of minutes between each AutoSave.
        /// </summary>
        private const int MAX_SAVE_MINUTES = 15;

        /// <summary>
        /// The maximum number of minutes between each scene backup.
        /// </summary>
        private const int MAX_BACKUP_MINUTES = 30;

        /// <summary>
        /// The maximum number of minutes that have to pass before an old backup gets deleted.
        /// </summary>
        private const int MAX_DELETE_MINUTES = 90;

#endregion

#region Properties

        /// <summary>
        /// The path of the folder used to store scene backups.
        /// The path is stored in the AutoSave_Folder EditorPrefs key.
        /// If the EditorPrefs key is an empty string, the value is set to the DEFAULT_SAVE_FOLDER path.
        /// </summary>
        public static string SaveFolder
        {
            get
            {
                string current = EditorPrefs.GetString("AutoSave_Folder", "");
                if (current == "")
                {
                    current = "." + DEFAULT_SAVE_FOLDER;
                    EditorPrefs.SetString("AutoSave_Folder", current);
                }
                return current;
            }
            set
            {
                EditorPrefs.SetString("AutoSave_Folder", value);
            }
        }

        /// <summary>
        /// The number of minutes between each AutoSave.
        /// The value is stored into the AutoSave_SaveMinutes EditoPrefs key.
        /// If the EditorPrefs key has a value of 0, the value is set to DEFAULT_SAVE_MINUTES.
        /// </summary>
        public static int SaveMinutes
        {
            get
            {
                int current = EditorPrefs.GetInt("AutoSave_SaveMinutes", 0);
                if (current == 0)
                {
                    current = DEFAULT_SAVE_MINUTES;
                    EditorPrefs.SetInt("AutoSave_SaveMinutes", current);
                }
                return current;
            }
            private set
            {
                EditorPrefs.SetInt("AutoSave_SaveMinutes", value);
            }
        }

        /// <summary>
        /// The number of minutes between each scene backup.
        /// The value is stored into the AutoSave_BackupMinutes EditorPrefs key.
        /// If the EditorPrefs key has a negative value, the value is set to DEFAULT_BACKUP_MINUTES.
        /// </summary>
        public static int BackupMinutes
        {
            get
            {
                int current = EditorPrefs.GetInt("AutoSave_BackupMinutes", -1);
                if (current < -1)
                {
                    current = DEFAULT_BACKUP_MINUTES;
                    EditorPrefs.SetInt("AutoSave_BackupMinutes", current);
                }
                return current;
            }
            private set
            {
                EditorPrefs.SetInt("AutoSave_BackupMinutes", value);
            }
        }

        /// <summary>
        /// The number of minutes that have to pass before an old backup gets deleted.
        /// The value is stored into the AutoSave_DeleteMinutes EditorPrefs key.
        /// If the EditorPrefs key has a negative value, the value is set to DEFAULT_DELETE_MINUTES.
        /// </summary>
        public static int DeleteMinutes
        {
            get
            {
                int current = EditorPrefs.GetInt("AutoSave_DeleteMinutes", -1);
                if (current < 0)
                {
                    current = DEFAULT_DELETE_MINUTES;
                    EditorPrefs.SetInt("AutoSave_DeleteMinutes", current);
                }
                return current;
            }
            private set
            {
                EditorPrefs.SetInt("AutoSave_DeleteMinutes", value);
            }
        }

        /// <summary>
        /// The flag that enables the AutoSave integration.
        /// The value is stored into the AutoSave_Enabled EditorPrefs key.
        /// </summary>
        public static bool AutoSaveEnabled
        {
            get
            {
                return EditorPrefs.GetBool("AutoSave_Enabled", true);
            }
            private set
            {
                EditorPrefs.SetBool("AutoSave_Enabled", value);
            }
        }

        /// <summary>
        /// The flag that enables to AutoSave scenes when play is pressed.
        /// The value is stored into the AutoSave_SaveAfterPlay EditorPrefs key.
        /// </summary>
        public static bool AutoSaveAfterPlay
        {
            get
            {
                return EditorPrefs.GetBool("AutoSave_SaveAfterPlay", true);
            }
            private set
            {
                EditorPrefs.SetBool("AutoSave_SaveAfterPlay", value);
            }
        }

		/// <summary>
        /// The flag that enables the scene backups.
        /// The value is stored into the AutoSave_BackupEnabled EditorPrefs key.
        /// </summary>
		public static bool BackupEnabled
		{
			get
			{
				return EditorPrefs.GetBool("AutoSave_BackupEnabled", true);
			}
			private set 
			{
				EditorPrefs.SetBool("AutoSave_BackupEnabled", value);
			}
		}

		/// <summary>
        /// The flag that enables the backup deletion.
        /// The value is stored into the AutoSave_DeleteEnabled EditorPrefs key.
        /// </summary>
		public static bool DeleteEnabled
		{
			get
			{
				return EditorPrefs.GetBool("AutoSave_DeleteEnabled", true);
			}
			private set
			{
				EditorPrefs.SetBool("AutoSave_DeleteEnabled", value);
			}
		}

        /// <summary>
        /// The flag that enables AutoSave to run while the Editor is running in background.
        /// The value is stored into the AutoSave_EnabledInBackground EditorPrefs key.
        /// </summary>
        public static bool EnabledInBackground
        {
            get
            {
                return EditorPrefs.GetBool("AutoSave_EnabledInBackground", true);
            }
            private set
            {
                EditorPrefs.SetBool("AutoSave_EnabledInBackground", value);
            }
        }

        /// <summary>
        /// The flag that enables AutoSave to log info on the integration activities.
        /// The value is stored into the AutoSave_LogInfo EditorPrefs key.
        /// </summary>
        public static bool LogInfo
        {
            get
            {
                return EditorPrefs.GetBool("AutoSave_LogInfo", true);
            }
            private set 
            {
                EditorPrefs.SetBool("AutoSave_LogInfo", value);
            }
        }

        /// <summary>
        /// The last time an AutoSave occurred.
        /// The value is stored into the AutoSave_LastSave EditorPrefs key as a string formatted according
        /// to DATE_FORMAT.
        /// If the EditorPrefs key is equal to DateTime.MinValue, the value is set to DateTime.Now. 
        /// </summary>
        public static DateTime LastSaveTime
        {
            get
            {
                string date = EditorPrefs.GetString("AutoSave_LastSave", DateTime.MinValue.ToString(DATE_FORMAT));
                DateTime current = DateTime.ParseExact(date, DATE_FORMAT, null);
                if (current.Equals(DateTime.MinValue))
                {
                    current = DateTime.Now;
                    EditorPrefs.SetString("AutoSave_LastSave", current.ToString(DATE_FORMAT));
                }
                return current;
            }
            set
            {
                EditorPrefs.SetString("AutoSave_LastSave", value.ToString(DATE_FORMAT));
            }
        }

        /// <summary>
        /// The last time a scene backup occurred.
        /// The value is stored into the AutoSave_LastBackup EditorPrefs key as a string formatted according
        /// to DATE_FORMAT.
        /// If the EditorPrefs key is equal to DateTime.MinValue, the value is set to DateTime.Now. 
        /// </summary>
        public static DateTime LastBackupTime
        {
            get
            {
                string date = EditorPrefs.GetString("AutoSave_LastBackup", DateTime.MinValue.ToString(DATE_FORMAT));
                DateTime current = DateTime.ParseExact(date, DATE_FORMAT, null);
                if (current.Equals(DateTime.MinValue))
                {
                    current = DateTime.Now;
                    EditorPrefs.SetString("AutoSave_LastBackup", current.ToString(DATE_FORMAT));
                }
                return current;
            }
            set
            {
                EditorPrefs.SetString("AutoSave_LastBackup", value.ToString(DATE_FORMAT));
            }
        }

#endregion

        /// <summary>
        /// The AutoSave static contructor.
        /// It enables AutoSave on Unity Editor startup if Enabled is true.
        /// It saves the scenes when Play is pressed if AutoSaveAfterPlay is true.
        /// </summary>
        static AutoSave()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (AutoSaveEnabled)
                {
                    EnableAutoSave();
                }

                Debug.Log("[AutoSaveBackup] " + GetInfo());
            }
            else if (AutoSaveAfterPlay)
            {
                SaveScene();
            }
        }

#region FrontEnd

        /// <summary>
        /// Generates the AutoSave preference tab.
        /// </summary>
        [PreferenceItem("AutoSave")]
        static void AutoSavePreferences()
        {
            int save = SaveMinutes;
            int backup = BackupMinutes;
            int delete = DeleteMinutes;
            bool en = AutoSaveEnabled;
            bool splay = AutoSaveAfterPlay;
			bool ben = BackupEnabled;
			bool den = DeleteEnabled;
            bool bg = EnabledInBackground;
            bool log = LogInfo;
            string folder = SaveFolder;
            string info = GetInfo();

            EditorGUILayout.LabelField("Version: " + AUTOSAVE_VERSION, EditorStyles.boldLabel);

            GUILayout.Space(8);

            en = EditorGUILayout.Toggle(new GUIContent("Enable AutoSave"), en);
            splay = EditorGUILayout.Toggle(new GUIContent("Save when play is pressed"), splay);
			ben = EditorGUILayout.Toggle(new GUIContent("Save scene backups"), ben);
			den = EditorGUILayout.Toggle(new GUIContent("Auto-delete old backups"), den);
            bg = EditorGUILayout.Toggle(new GUIContent("Run when Editor in background"), bg);
            log = EditorGUILayout.Toggle(new GUIContent("Show AutoSave log"), log);

            GUILayout.Space(8);

            folder = EditorGUILayout.TextField(new GUIContent("AutoSave folder", 
												"Folder path. Must be inside the project folder."), folder);
            GUIStyle folderButtonStyle = new GUIStyle(GUI.skin.button);
            folderButtonStyle.fixedWidth = 100;

            // When the button is pressed, a FolderPanel is opened and the value is parsed to have
            // a valid path for the AutoSaves folder.
            if (GUILayout.Button("Choose folder", folderButtonStyle))
            {
                string f = EditorUtility.OpenFolderPanel("Select AutoSave folder",
                    Application.dataPath.Replace("/Assets", ""), "AutoSaves");
                if (f == "")
                {
                    f = folder;
                }
                else
                {
                    string[] p = Application.dataPath.Split('/');
                    string projectName = "/" + p[p.Length - 2]; // Retrieve the project folder name.
                    int index = f.IndexOf(projectName); // Look for the project name in the selected path.

                    if (index < 0)
                    {
                        string err = "AutoSave folder must be placed inside your project directory.";
                        EditorUtility.DisplayDialog("Wrong folder path", err, "OK");
                        f = folder;
                    }
                    else
                    {
                        // Create a path relative to the project folder.
                        f = "." + f.Remove(0, index + projectName.Length);
                        if (!f.EndsWith("/AutoSaves"))
                        {
                            f += "/AutoSaves";
                        }
                        folder = f;
                    }
                }
            }

			EditorGUI.BeginDisabledGroup(!AutoSaveEnabled);
            save = EditorGUILayout.IntSlider(new GUIContent("AutoSave interval (minutes)", 
												"Time span between AutoSaves."), save, 1, MAX_SAVE_MINUTES);

			EditorGUI.BeginDisabledGroup(!BackupEnabled);
            backup = EditorGUILayout.IntSlider(new GUIContent("Scene backup interval (minutes)", 
												"Time span between scene backups."), 
												backup, 1, MAX_BACKUP_MINUTES);
			EditorGUI.EndDisabledGroup(); // BackupEnabled

			EditorGUI.BeginDisabledGroup(!DeleteEnabled);
            delete = EditorGUILayout.IntSlider(new GUIContent("Delete backups interval (minutes)", 
												"Time span between old backups deletion."), 
												delete, 1, MAX_DELETE_MINUTES);
			EditorGUI.EndDisabledGroup(); // DeleteEnabled
			EditorGUI.EndDisabledGroup(); // AutoSaveEnabled

            GUILayout.Space(12);

            if (GUILayout.Button(new GUIContent("Clean backup folder", "Delete all the backups from AutoSaves folder.")))
            {
                string title = "Clean backup folder";
                string message = "Do you really want to cleanup the backup folder? This action cannot be undone.";
                if (EditorUtility.DisplayDialog(title, message, "Yes", "No"))
                {
                    message = CleanupFolder();
                    EditorUtility.DisplayDialog(title, message, "OK");
                }
            }

            if (GUI.changed)
            {
                if (!folder.Equals(SaveFolder))
                {
                    SetFolder(folder);
                }

                if (save != SaveMinutes || backup != BackupMinutes || delete != DeleteMinutes)
                {
                    info = SetIntervals(save, backup, delete);
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

				if (ben != BackupEnabled)
				{
					BackupEnabled = ben;
				}

				if (den != DeleteEnabled)
				{
					DeleteEnabled = den;
				}

                if (bg != EnabledInBackground)
                {
                    EnableBackground(bg);
                }

                if (log != LogInfo)
                {
                    LogInfo = log;
                }

            }

            GUILayout.Space(12);

            EditorGUILayout.HelpBox(info, MessageType.Info);
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
                SaveScene();
                LastSaveTime = DateTime.Now;
            }
            if (BackupEnabled)
            {
				if ((DateTime.Now - LastBackupTime).TotalMinutes >= BackupMinutes)
				{
					SaveBackupScene();
					LastBackupTime = DateTime.Now;
					if (DeleteEnabled)
					{
						DeleteOldBackups();
					}
				}
            }
        }

		/// <summary>
		/// Enables AutoSave.
		/// </summary>
		/// <returns> Info message. </returns>
        static string EnableAutoSave()
        {
            EnsureAutoSavePathExists();
            EditorApplication.update += OnUpdate;
            AutoSaveEnabled = true;

            EnableBackground(EnabledInBackground);

            return GetInfo();
        }

		/// <summary>
		/// Disables AutoSave.
		/// </summary>
		/// <returns> Info message. </returns>
        static string DisableAutoSave()
        {
            EditorApplication.update -= OnUpdate;
            AutoSaveEnabled = false;

            return GetInfo();
        }

        /// <summary>
        /// Enables the integration to work also when the Editor is running in background.
        /// </summary>
        /// <param name="_enabled"> If AutoSave has to be enabled in background. &#xD; </param>
        static void EnableBackground(bool _enabled)
        {
            if (Application.isEditor)
            {
                Application.runInBackground = _enabled;
                EnabledInBackground = _enabled;
            }
        }

		/// <summary>
		/// Sets time intervals between AutoSave calls.
		/// </summary>
		/// <param name="_save"> Minutes between each AutoSave. &#xD; </param>
		/// <param name="_backup"> Minutes between each scene backup. &#xD; </param>
		/// <param name="_delete"> Minutes that have to pass before a backup gets deleted. &#xD; </param>
		/// <returns> Info message. </returns>
        static string SetIntervals(int _save, int _backup, int _delete)
        {
            SaveMinutes = _save;
            BackupMinutes = _backup;
            DeleteMinutes = _delete;

            return GetInfo();
        }

		/// <summary>
		/// Sets the AutoSaves folder path.
		/// </summary>
		/// <param name="_folder"> New folder path. &#xD; </param>
		/// <returns> Info message. </returns>
        static string SetFolder(string _folder)
        {
            CleanupFolder();
            SaveFolder = _folder;
            EnsureAutoSavePathExists();

            return "AutoSave folder set to " + _folder;
        }

		/// <summary>
		/// Deletes all the scenes backups from the AutoSaves folder.
		/// </summary>
		/// <returns> Info message. </returns>
        static string CleanupFolder()
        {
            string message;

            EnsureAutoSavePathExists();

            Directory.Delete(SaveFolder, true);

            if (!Directory.Exists(SaveFolder))
            {
                message = "Backup folder cleaned up successfully.";
            }
            else
            {
                message = "Impossible to clean up backup folder.";
            }

            EnsureAutoSavePathExists();

            return message;
        }

		/// <summary>
		/// Retrieves informations about the AutoSave status.
		/// </summary>
		/// <returns> Info message. </returns>
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
			
            message += GetIntervalsInfo();

            if (EnabledInBackground)
            {
                message += "AutoSave enabled when Editor is in background.\n";
            }
            else
            {
                message += "AutoSave disabled when Editor is in background.\n";
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

		/// <summary>
		/// Retrieves informations about the intervals.
		/// </summary>
		/// <returns> Info message. </returns>
		static string GetIntervalsInfo()
		{
			string message = "AutoSave interval: " + SaveMinutes + " minutes.\n";

          	if (BackupEnabled)
			{
           		message += "AutoBackup interval: " + BackupMinutes + " minutes.\n";
			}
			else
			{
				message += "AutoBackup disabled.\n";
			}
			
			if (DeleteEnabled)
			{
				message += "Backups will be deleted after " + DeleteMinutes + " minutes.\n";
			}
			else
			{
				message += "Auto-delete backups disabled.\n";
			}

            return message;
		}

		/// <summary>
		/// Checks if the AutoSaves folder path exists. If it doesn't exist, it is created a new
		/// folder on that path.
		/// </summary>
        static void EnsureAutoSavePathExists()
        {
            if (!Directory.Exists(SaveFolder))
            {
                Directory.CreateDirectory(SaveFolder);
            }
        }

		/// <summary>
		/// Saves the scenes currently opened in the Unity Editor.
		/// </summary>
        static void SaveScene()
        {
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            if (LogInfo)
            {
                Debug.Log("[AutoSaveBackup] AutoSaved scenes.\n" + DateTime.Now);
            }
        }

		/// <summary>
		/// Makes backups of the scenes currently opened in the Unity Editor.
		/// </summary>
        static void SaveBackupScene()
        {
            EnsureAutoSavePathExists();
            AssetDatabase.SaveAssets();
            int n = EditorSceneManager.loadedSceneCount;

            for (int i = 0; i < n; i++)
            {
                UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetSceneAt(i);
                string newName = GetSceneBackupName(scene.path);
                EditorSceneManager.SaveScene(scene, Path.Combine(SaveFolder, newName), true);
            }

            if (LogInfo)
            {
                Debug.Log("[AutoSaveBackup] Scene backups saved in " + SaveFolder + ".\n" + DateTime.Now);
            }
        }

		/// <summary>
		/// Deletes old scene backups from the AutoSaves folder.
		/// </summary>
        static void DeleteOldBackups()
        {
            string[] files;
            files = Directory.GetFiles(SaveFolder, "*.unity");

            int n = files.Length;

			if (n > 0)
			{
				DateTime tempTime;

				for (int i = 0; i < n; i++)
				{
					tempTime = File.GetCreationTime(files[i]);
					if ((DateTime.Now - tempTime).TotalMinutes >= DeleteMinutes)
						File.Delete(files[i]);
				}
			}

            if (LogInfo)
            {
                Debug.Log("[AutoSaveBackup] Deleted " + n + " old scene backups. \n" + DateTime.Now);
            }
        }

		/// <summary>
		/// Adds timestamps to the name of the scene backup.
		/// </summary>
		/// <param name="_originalSceneName"> Name of the scene to format. &#xD; </param>
		/// <returns> Name of the scene backup file. </returns>
        static string GetSceneBackupName(string _originalSceneName)
        {
            string scene = Path.GetFileNameWithoutExtension(_originalSceneName);
            return string.Format("{0}_{1}.unity", scene, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture));
        }

#endregion
    }
}