using UnityEngine;

namespace Com.KherusEmporium.DragNDrop {
	public abstract class Container : MonoBehaviour {
		/// <summary>
		/// Can the container receive draggable currently.
		/// </summary>
		[SerializeField] protected bool isOpen = true;

		public abstract bool IsEmpty();

		/// <summary>
		/// Adds a Draggable to a container.
		/// </summary>
		/// <param name="draggable"></param>
		/// <returns>The eventual Draggable removed from the container (replacing a previously contained draggable, or container is currently locked and returning the Draggable that we attempted to contain.</returns>
		public abstract Draggable Add(Draggable draggable);

		/// <summary>
		/// Removes a Draggable from a container
		/// </summary>
		/// <param name="draggable">If null, removes any Draggable</param>
		/// <returns>The Draggable removed</returns>
		public abstract Draggable Remove();

		/// <summary>
		/// Empty out container, deleting all
		/// </summary>
		public abstract void Clear();
	}
}
