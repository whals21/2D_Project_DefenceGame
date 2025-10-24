using Com.KherusEmporium.DragNDrop;
using UnityEditor;
using UnityEngine;

namespace Com.KherusEmporium.DragNDrop {

	[CustomEditor(typeof(SingleContainer))]
	public class SingleContainerEditor : Editor
    {
		private SerializedProperty canReplaceProp;
		private SerializedProperty containedProp;

		private void OnEnable() {
			canReplaceProp = serializedObject.FindProperty("canReplace");
			containedProp = serializedObject.FindProperty("contained");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();


			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Settings");
			EditorGUILayout.PropertyField(canReplaceProp, new GUIContent("Can replace", "Defines if a new object replaces the old contained, or is rejected."));

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Debug Info");
			EditorGUILayout.PropertyField(containedProp, new GUIContent("Contained object"));


			serializedObject.ApplyModifiedProperties();
		}
	}
}
