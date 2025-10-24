using UnityEngine;

namespace Com.KherusEmporium.DragNDrop {
	public class SingleContainer : Container {
		[SerializeField] private bool canReplace = true;
		[SerializeField] private Draggable contained = null;

		public override Draggable Add(Draggable draggable) {
			if (draggable == contained) return null;
			if (!isOpen) return draggable;
			if (!canReplace && contained != null) return draggable; // Has a card and cannot be replaced, gives back draggable

			Draggable oldContained = contained;

			draggable.transform.SetParent(transform);
			draggable.transform.localPosition = Vector3.zero;
			contained = draggable;

			return oldContained;
		}

		public override bool IsEmpty() {
			return contained == null;
		}

		public override void Clear() {
			Destroy(contained);
			contained = null;
		}

		public override Draggable Remove() {
			if (contained == null) return null;

			Draggable toReturn = contained;
			contained.previousContainer = this;

			contained = null;
			return toReturn;
		}

	}
}
