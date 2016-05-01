#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_5
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace TextFx
{
	[CustomEditor(typeof(TextFxUGUI))]
	public class TextFxUGUI_Inspector : UnityEditor.UI.TextEditor
	{
		TextFxUGUI uguiEffect;
		TextFxAnimationManager animationManager;

		new void OnEnable()
		{
			uguiEffect = (TextFxUGUI) target;
			animationManager = uguiEffect.AnimationManager;

			EditorApplication.update += UpdateManager;

			base.OnEnable ();
		}

		new void OnDisable()
		{
			EditorApplication.update -= UpdateManager;

			base.OnEnable ();
		}

		void UpdateManager()
		{
			TextFxBaseInspector.UpdateManager (animationManager);
		}

		public override void OnInspectorGUI ()
		{
			// Draw TextFx inspector section
			TextFxBaseInspector.DrawTextFxInspectorSection(this, animationManager);

			// Draw the default UI Text inspector
            base.OnInspectorGUI ();

			// Draw the extra Mesh Effects section
			GUILayout.Space(10);

            GUILayout.Label("Mesh Effect - [TextFX Feature]", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            bool guiChanged = GUI.changed;

            uguiEffect.m_effect_type = (TextFxUGUI.UGUI_MESH_EFFECT_TYPE)EditorGUILayout.EnumPopup("Type", uguiEffect.m_effect_type);

            GUI.enabled = uguiEffect.m_effect_type != TextFxUGUI.UGUI_MESH_EFFECT_TYPE.None;

            uguiEffect.m_effect_offset = EditorGUILayout.Vector2Field("Offset", uguiEffect.m_effect_offset);

            uguiEffect.m_effect_colour = EditorGUILayout.ColorField("Colour", uguiEffect.m_effect_colour);

            if (!guiChanged && GUI.changed)
            {
                uguiEffect.ForceUpdateGeometry();
            }

            GUI.enabled = true;
            EditorGUI.indentLevel--;
		}
	}
}
#endif