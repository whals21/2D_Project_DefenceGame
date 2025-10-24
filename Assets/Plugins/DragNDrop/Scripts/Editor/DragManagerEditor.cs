using Com.KherusEmporium.DragNDrop;
using UnityEditor;
using UnityEngine;

namespace Com.KherusEmporium.DragNDrop {

	[CustomEditor(typeof(DragManager))]
	public class DragManagerEditor : Editor
    {
		private SerializedProperty dragOnProp;
		private SerializedProperty camProp;
		private SerializedProperty canDropAnywhereProp;
		private SerializedProperty distanceFromCamProp;

		private void OnEnable() {
			dragOnProp = serializedObject.FindProperty("dragOn");
			camProp = serializedObject.FindProperty("cam");
			canDropAnywhereProp = serializedObject.FindProperty("canDropAnywhere");
			distanceFromCamProp = serializedObject.FindProperty("distanceFromCam");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.LabelField("References");
			EditorGUILayout.PropertyField(camProp);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Settings");
			EditorGUILayout.PropertyField(dragOnProp, new GUIContent("Drag on Mouse Button"));
			EditorGUILayout.PropertyField(canDropAnywhereProp, new GUIContent("Can Drop Anywhere", "If true, objects won't be returned to previous containers when dropped"));

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(distanceFromCamProp, new GUIContent("Distance from camera", "The distance at which objects sit when dragged and the mouse is not hovering any collider"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}
