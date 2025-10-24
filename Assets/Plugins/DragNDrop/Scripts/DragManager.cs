using UnityEngine;

namespace Com.KherusEmporium.DragNDrop {
	/// <summary>
	/// Handler for all drag and drop mouse interactions, singleton.
	/// </summary>
	public class DragManager : MonoBehaviour {
		/// <summary>
		/// Button the tool uses to handle the dragging mechanics.
		/// </summary>
		[SerializeField] private MouseButton dragOn = MouseButton.Left;
		/// <summary>
		/// Camera used to handle the dragging mechanics.
		/// </summary>
		[SerializeField] private Camera cam = null;
		/// <summary>
		/// Can objects be dropped everywhere, or only in containers?
		/// </summary>
		[SerializeField] private bool canDropAnywhere = false;

		/// <summary>
		/// The distance from the camera at which objects sit when not dragged on top of a Collider.
		/// </summary>
		[Space]
		[SerializeField] private float distanceFromCam = 5;

		private Draggable drag = null;

		private void Start() {
			SetupSingleton();
		}

		private void Update() {
			if (Input.GetMouseButtonDown(((int)dragOn))) StartDrag();
			if (Input.GetMouseButtonUp(((int)dragOn)) && drag != null) StopDrag();

			if (drag != null) Drag();
		}

		/// <summary>
		/// Get usable mouse position for raycasts
		/// </summary>
		private Vector3 MousePosition {
			get {
				Vector3 startPos = Input.mousePosition;
				startPos.z = distanceFromCam;

				return startPos;
			}
		}

		/// <summary>
		/// Moves the currently dragged object around
		/// </summary>
		private void Drag() {
			if (Physics.Raycast(cam.ScreenPointToRay(MousePosition), out RaycastHit hit)) {
				Debug.DrawLine(cam.transform.position, hit.point, Color.green);

				drag.transform.position = hit.point;
			}
			else {

				drag.transform.position = cam.ScreenToWorldPoint(MousePosition);

			}
		}

		/// <summary>
		/// Attempts to grab an object to start dragging
		/// </summary>
		private void StartDrag() {

			if (Physics.Raycast(cam.ScreenPointToRay(MousePosition), out RaycastHit hit)) {
				Debug.DrawLine(cam.transform.position, hit.point, Color.red, 5);

				drag = hit.collider.gameObject.GetComponent<Draggable>();
				if (CheckIfDragging()) return;

				Container container = hit.collider.GetComponentInChildren<Container>();
				drag = container?.Remove();
				CheckIfDragging();
			}
		}

		/// <summary>
		/// Self explanatory
		/// </summary>
		/// <returns></returns>
		private bool CheckIfDragging() {
			if (drag != null) {
				drag?.Grab();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Drops currently dragged item, attempting to place it in a container if possible.
		/// </summary>
		private void StopDrag() {
			Container container = null;

			if (Physics.Raycast(cam.ScreenPointToRay(MousePosition), out RaycastHit hit)) {
				Debug.DrawLine(cam.transform.position, hit.point, Color.blue, 5);

				container = hit.collider.gameObject.GetComponentInChildren<Container>();

				if (container != null) {
					Draggable removedDraggable = container.Add(drag);

					removedDraggable?.ReturnToStartPos();
				}
			}

			drag.Drop(canDropAnywhere, container != null);
			drag = null;
		}


		// Singleton code
		static private DragManager instance;
		static public DragManager Instance => instance;
		private void SetupSingleton() {
			instance = this;
		}
	}

	public enum MouseButton {
		Left,
		Right,
		Middle
	}
}
