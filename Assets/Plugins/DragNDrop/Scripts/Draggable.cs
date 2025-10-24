using System;
using UnityEngine;

namespace Com.KherusEmporium.DragNDrop {

	/// <summary>
	/// Class for the objects dragged and contained
	/// </summary>
	[Serializable]
	public class Draggable : MonoBehaviour {
		/// <summary>
		/// Container the object was previously in
		/// </summary>
		public Container previousContainer = null;
		private int defaultLayer;
		private Transform defaultParent;
		private Vector3 defaultPos;

		private void Start() {
			defaultLayer = gameObject.layer;
			defaultPos = transform.localPosition;
			defaultParent = transform.parent;
		}

		/// <summary>
		/// Sends object back to the container it was previously in.
		/// </summary>
		public void ReturnToPreviousContainer() {
			if (previousContainer != null) {
				previousContainer.Add(this);
			}
			else ReturnToStartPos();

		}

		/// <summary>
		/// Sends object back to it's starting position (not in a container)
		/// </summary>
		public void ReturnToStartPos() {
			transform.SetParent(defaultParent);
			transform.localPosition = defaultPos;
			gameObject.layer = defaultLayer;
		}

		/// <summary>
		/// Ensure object no longer interferes with raycast once grabbed
		/// </summary>
		public void Grab() {
			gameObject.layer = 2;
		}

		/// <summary>
		/// Handles the resetting of object properties once dropped.
		/// </summary>
		/// <param name="canDropAnywhere">Can the object be dropped outside of a container?</param>
		/// <param name="inContainer">Is the object currently dropped inside a container?</param>
		public void Drop(bool canDropAnywhere, bool inContainer) {
			if (!canDropAnywhere && !inContainer) ReturnToPreviousContainer();
			else if (canDropAnywhere) {
				gameObject.layer = defaultLayer;
				previousContainer = null;
			}
		}
	}
}
