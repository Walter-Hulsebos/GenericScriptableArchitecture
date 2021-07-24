﻿namespace GenericScriptableArchitecture.Editor
{
    using EasyButtons.Editor;
    using GenericUnityObjects.UnityEditorInternals;
    using UnityEditor;

    [CustomEditor(typeof(BaseScriptableEvent), true)]
    internal class ScriptableEventEditor : GenericHeaderEditor, IRepaintable
    {
        private ButtonsDrawer _buttonsDrawer;
        private FoldoutList<BaseScriptableEventListener> _listenersList;
        private FoldoutList<UnityEngine.Object> _responseTargetsList;
        private SerializedProperty _description;
        private StackTraceDrawer _stackTrace;

        private void OnEnable()
        {
            _buttonsDrawer = new ButtonsDrawer(target);
            var typedTarget = (BaseScriptableEvent) target;

            var listenersExpanded = serializedObject.FindProperty(nameof(BaseScriptableEvent.ListenersExpanded));
            _listenersList = new FoldoutList<BaseScriptableEventListener>(typedTarget.ScriptableListeners, listenersExpanded, "Scriptable Listeners");

            var responseTargetsExpanded = serializedObject.FindProperty(nameof(BaseScriptableEvent.ResponseTargetsExpanded));
            _responseTargetsList = new FoldoutList<UnityEngine.Object>(typedTarget.OtherListeners, responseTargetsExpanded, "Other Listeners");

            _description = serializedObject.FindProperty("_description");

            _stackTrace = new StackTraceDrawer(typedTarget, this);
        }

        public override void OnInspectorGUI()
        {
            using var guiWrapper = new InspectorGUIWrapper(this);

            if (guiWrapper.HasMissingScript)
                return;

            EditorGUILayout.PropertyField(_description);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight / 2);
            _buttonsDrawer.DrawButtons(targets);

            _stackTrace.Draw();

            if (ApplicationUtil.InEditMode)
                return;

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            _listenersList.DoLayoutList();
            _responseTargetsList.DoLayoutList();
        }
    }
}