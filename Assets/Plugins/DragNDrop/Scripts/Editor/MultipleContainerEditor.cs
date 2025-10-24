using Com.KherusEmporium.DragNDrop;
using UnityEditor;
using UnityEngine;

namespace Com.KherusEmporium.DragNDrop {

	[CustomEditor(typeof(MultipleContainer))]
	public class MultipleContainerEditor : Editor
    {
		private SerializedProperty sortingMethodProp;
		private SerializedProperty containedProp;
		private SerializedProperty maxContainedProp;
		private SerializedProperty containerDimensionsProp;
		private SerializedProperty sortingAxisProp;

		private void OnEnable() {
			sortingMethodProp = serializedObject.FindProperty("sortingMethod");
			containedProp = serializedObject.FindProperty("contained");
			maxContainedProp = serializedObject.FindProperty("maxContained");
			containerDimensionsProp = serializedObject.FindProperty("containerDimensions");
			sortingAxisProp = serializedObject.FindProperty("sortingAxis");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();


			EditorGUILayout.LabelField("Settings");
			EditorGUILayout.PropertyField(sortingMethodProp, new GUIContent("Sorting method", "Used when retrieving item from the container: First In First Out, First In Last Out or Random"));
			EditorGUILayout.PropertyField(maxContainedProp, new GUIContent("Max amount contained", "Set to negative for unlimited container"));
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Visual Settings");
			EditorGUILayout.PropertyField(sortingAxisProp, new GUIContent("Sorting axis", "Used to choose the axis to visually spread the objects in the containers"));
			EditorGUILayout.PropertyField(containerDimensionsProp, new GUIContent("Container dimensions", "Used when visually spreading the objects in the container"));

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Debug Info");
			EditorGUILayout.PropertyField(containedProp, new GUIContent("Contained object"));


			serializedObject.ApplyModifiedProperties();
		}
	}
}
