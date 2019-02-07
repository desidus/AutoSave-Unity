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
 * ============= DesidusEditorUtility =============
 *
 * This class provides useful tools for Editor scripts.
 *
 * Version:
 *   1.0
 *
 * Authors:
 *   Ettore Passaro <ettore3091@gmail.com>
 *	 Desidus <info@desidus.it>
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace It.Desidus.EditorExtensions
{
	public static class DesidusEditorUtility
	{
		static GUIStyle editButtonStyle;

		/// <summary>
		/// Draws the Edit button that Unity uses for editing a collider's shape.
		/// </summary>
		/// <param name="value">If the button is pressed and the Editor is in edit mode.</param>
		/// <returns>Returns the button state.</returns>
		public static bool EditBoundsButton(bool value)
		{
			return EditBoundsButton(value, new GUIContent("Edit Bounds"));
		}

		/// <summary>
		/// Draws the Edit button that Unity uses for editing a collider's shape.
		/// </summary>
		/// <param name="value">If the button is pressed and the Editor is in edit mode.</param>
		/// <param name="label">Label for the button</param>
		/// <returns>Returns the button state.</returns>
		public static bool EditBoundsButton(bool value, string label)
		{
			return EditBoundsButton(value, new GUIContent(label));
		}

		/// <summary>
		/// Draws the Edit button that Unity uses for editing a collider's shape.
		/// </summary>
		/// <param name="value">If the button is pressed and the Editor is in edit mode.</param>
		/// <param name="label">Label for the button.</param>
		/// <returns>Returns the button state.</returns>
		public static bool EditBoundsButton(bool value, GUIContent label)
		{
			if(editButtonStyle == null)
            {
                editButtonStyle = new GUIStyle(GUI.skin.GetStyle("LargeButton"));
                editButtonStyle.margin = new RectOffset(0, 0, 0, 0);
                editButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            }

            GUILayout.Space(4);
		
            Rect rect = EditorGUILayout.GetControlRect(true, 23);
            Rect buttonRect = new Rect(rect.xMin + EditorGUIUtility.labelWidth, rect.yMin, 33, 23);
            Vector2 labelSize = GUI.skin.label.CalcSize(label);

            Rect labelRect = new Rect(
                buttonRect.xMax + 5,
                rect.yMin + (rect.height - labelSize.y) * .5f,
                labelSize.x,
                rect.height);

            value = GUI.Toggle(
                buttonRect, 
                value, 
                EditorGUIUtility.IconContent("EditCollider"), 
                editButtonStyle);
            GUI.Label(labelRect, "Edit Volume");
			
            GUILayout.Space(2);

			return value;
		}

		/// <summary>
		/// Draws the standard field with the script's name that Unity usually puts at
		/// the beginning of MonoBehaviour inspectors.
		/// </summary>
		/// <param name="behaviour"></param>
		/// <param name="type"></param>
		public static void DisabledScriptField(MonoBehaviour behaviour, Type type)
		{
			GUI.enabled = false;
			EditorGUILayout.ObjectField(
				"Script", 
				MonoScript.FromMonoBehaviour(behaviour), 
				type, 
				false);
			GUI.enabled = true; 
		}

		/// <summary>
		/// Draws the standard field with the script's name that Unity usually puts at
		/// the beginning of MonoBehaviour inspectors.
		/// </summary>
		/// <param name="behaviour"></param>
		/// <param name="type"></param>
		public static void DisabledScriptField(ScriptableObject sObject, Type type)
		{
			GUI.enabled = false;
			EditorGUILayout.ObjectField(
				"Script", 
				MonoScript.FromScriptableObject(sObject), 
				type, 
				false);
			GUI.enabled = true; 
		}

		/*
		 * The following two methods are thanks to the awesome Bunny83 and 
		 * their answer on this question http://answers.unity.com/answers/960709/view.html
		 */
		/// <summary>
		/// Get all derived types of a class in the calling AppDomain.
		/// </summary>
		/// <param name="aType">The Type of the parent class.</param>
		/// <returns>Returns an array of Type containing all the derived types.</returns>
		internal static Type[] GetAllDerivedTypes(this AppDomain aAppDomain, Type aType)
		{
			var result = new List<Type>();
			var assemblies = aAppDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				var types = assembly.GetTypes();
				foreach (var type in types)
				{
					if (type.IsSubclassOf(aType))
						result.Add(type);
				}
			}
			return result.ToArray();
		}

		/// <summary>
		/// Get the Editor main window's position.
		/// </summary>
		public static Rect GetEditorMainWindowPos()
		{
			var containerWinType = 
				System.AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(ScriptableObject)).
					Where(t => t.Name == "ContainerWindow").FirstOrDefault();
			if (containerWinType == null)
				throw new System.MissingMemberException(
					"Can't find internal type ContainerWindow. " + 
					"Maybe something has changed inside Unity");

			var showModeField = 
				containerWinType.GetField(
					"m_ShowMode", 
					System.Reflection.BindingFlags.NonPublic | 
						System.Reflection.BindingFlags.Instance);

			var positionProperty = 
				containerWinType.GetProperty(
					"position", 
					System.Reflection.BindingFlags.Public | 
						System.Reflection.BindingFlags.Instance);

			if (showModeField == null || positionProperty == null)
				throw new System.MissingFieldException(
					"Can't find internal fields 'm_ShowMode' or 'position'. " + 
					"Maybe something has changed inside Unity");

			var windows = Resources.FindObjectsOfTypeAll(containerWinType);
			foreach (var win in windows)
			{
				var showmode = (int)showModeField.GetValue(win);
				if (showmode == 4) // main window
				{
					var pos = (Rect)positionProperty.GetValue(win, null);
					return pos;
				}
			}
			throw new System.NotSupportedException(
				"Can't find internal main window. Maybe something has changed inside Unity");
		}

        /// <summary>
        /// Draws a NavMesh Area Mask selection menu.
        /// </summary>
        /// <param name="navMeshAreaProperty">The NavMesh Area Mask property.</param>
        public static void NavMeshAreaMaskField(SerializedProperty navMeshAreaProperty)
        {
            NavMeshAreaMaskField(navMeshAreaProperty, new GUIContent(navMeshAreaProperty.displayName));
        }
        
        /// <summary>
        /// Draws a NavMesh Area Mask selection menu.
        /// </summary>
        /// <param name="navMeshAreaProperty">The NavMesh Area Mask property.</param>
        /// <param name="label">Label for the field.</param>
        public static void NavMeshAreaMaskField(SerializedProperty navMeshAreaProperty, string label)
        {
            NavMeshAreaMaskField(navMeshAreaProperty, new GUIContent(label));            
        }

        /// <summary>
        /// Draws a NavMesh Area Mask selection menu.
        /// </summary>
        /// <param name="navMeshAreaProperty">The NavMesh Area Mask property.</param>
        /// <param name="label">Label for the field.</param>
		public static void NavMeshAreaMaskField(SerializedProperty navMeshAreaProperty, GUIContent label)
        {
            string[] areaNames = GameObjectUtility.GetNavMeshAreaNames();
            int n = areaNames.Length;
            long currentMask = navMeshAreaProperty.longValue;
            int compressedMask = 0;

            for (int i = 0; i < n; i++)
            {
                int areaIndex = GameObjectUtility.GetNavMeshAreaFromName(areaNames[i]);
                if (((1 << areaIndex) & currentMask) != 0)
                {
                    compressedMask = compressedMask | (1 << i);
                }
            }

            float lineHeight = EditorGUIUtility.singleLineHeight;
            Rect position = 
                GUILayoutUtility.GetRect(
                    EditorGUIUtility.labelWidth, 
                    EditorGUIUtility.labelWidth, 
                    EditorGUIUtility.singleLineHeight, 
                    EditorGUIUtility.singleLineHeight, 
                    EditorStyles.layerMaskField);

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = navMeshAreaProperty.hasMultipleDifferentValues;
            int areaMask = 
                EditorGUI.MaskField(
                    position, 
                    label, 
                    compressedMask, 
                    areaNames, 
                    EditorStyles.layerMaskField);
            EditorGUI.showMixedValue = false;

            if (EditorGUI.EndChangeCheck())
            {
                if (areaMask == -1)
                {
                    navMeshAreaProperty.longValue = 0xffffffff;
                }
                else
                {
                    uint newMask = 0;
                    for (int i = 0; i < n; i++)
                    {
                        if (((areaMask >> i) & 1) != 0)
                        {
                            newMask = 
                                newMask | 
                                (uint)(1 << GameObjectUtility.GetNavMeshAreaFromName(areaNames[i]));
                        }
                    }
                    navMeshAreaProperty.longValue = newMask;
                }
            }
        }
	}
}
