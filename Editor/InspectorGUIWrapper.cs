﻿namespace GenericScriptableArchitecture.Editor
{
    using System;
    using UnityEditor;

    internal readonly struct InspectorGUIWrapper : IDisposable
    {
        private readonly Editor _editor;
        public readonly bool HasMissingScript;

        public InspectorGUIWrapper(Editor editor)
        {
            _editor = editor;

            HasMissingScript = _editor.target == null;

            if (HasMissingScript)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(_editor.serializedObject.FindProperty("m_Script"));
                }
            }
            else
            {
                _editor.serializedObject.UpdateIfRequiredOrScript();
            }
        }

        public void Dispose()
        {
            if ( ! HasMissingScript)
                _editor.serializedObject.ApplyModifiedProperties();
        }
    }
}