using Com.KherusEmporium.DragNDrop;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.KherusEmporium.DragNDrop {
    [CustomEditor(typeof(Draggable))]
    public class DraggableEditor : Editor
    {
		private SerializedProperty previousContainerProp;

		private void OnEnable() {
			previousContainerProp = serializedObject.FindProperty("previousContainer");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.PropertyField(previousContainerProp, new GUIContent("Previous container", "The container the draggable will return to if dropped somewhere invalid"));
		}
	}
}
