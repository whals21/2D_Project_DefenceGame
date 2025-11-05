using UnityEngine;

/// <summary>
/// RTS 스타일 카메라 컨트롤러
/// 마우스를 화면 가장자리로 이동하면 카메라가 이동
/// WASD 키보드 이동, 마우스 휠 줌 지원
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Edge Scrolling Settings")]
    [SerializeField] private bool enableEdgeScrolling = true;
    [SerializeField] private float edgeScrollSpeed = 10f; // 가장자리 스크롤 속도
    [SerializeField] private float edgeThreshold = 20f; // 화면 가장자리 감지 범위 (픽셀)

    [Header("Keyboard Movement Settings")]
    [SerializeField] private bool enableKeyboardMovement = true;
    [SerializeField] private float keyboardMoveSpeed = 15f; // WASD 이동 속도

    [Header("Mouse Drag Settings")]
    [SerializeField] private bool enableMouseDrag = true;
    [SerializeField] private float dragSpeed = 2f; // 마우스 드래그 이동 속도
    [SerializeField] private KeyCode dragButton = KeyCode.Mouse2; // 마우스 중간 버튼 (휠 클릭)

    [Header("Zoom Settings")]
    [SerializeField] private bool enableZoom = true;
    [SerializeField] private float zoomSpeed = 5f; // 줌 속도
    [SerializeField] private float minZoom = 3f; // 최소 줌 (가까이)
    [SerializeField] private float maxZoom = 20f; // 최대 줌 (멀리)

    [Header("Camera Bounds")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minBounds = new Vector2(-20, -20); // 카메라 이동 최소 범위
    [SerializeField] private Vector2 maxBounds = new Vector2(20, 20); // 카메라 이동 최대 범위

    [Header("Smooth Movement")]
    [SerializeField] private bool useSmoothMovement = true;
    [SerializeField] private float smoothSpeed = 10f; // 부드러운 이동 속도

    private Camera cam;
    private Vector3 targetPosition;
    private Vector3 lastMousePosition;
    private bool isDragging = false;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }

        targetPosition = transform.position;
    }

    void Update()
    {
        Vector3 movement = Vector3.zero;

        // 1. 화면 가장자리 스크롤
        if (enableEdgeScrolling)
        {
            movement += GetEdgeScrollMovement();
        }

        // 2. 키보드 이동 (WASD)
        if (enableKeyboardMovement)
        {
            movement += GetKeyboardMovement();
        }

        // 3. 마우스 드래그 이동
        if (enableMouseDrag)
        {
            movement += GetMouseDragMovement();
        }

        // 4. 이동 적용
        if (movement != Vector3.zero)
        {
            targetPosition += movement * Time.deltaTime;

            // 카메라 범위 제한
            if (useBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            }
        }

        // 5. 줌 (마우스 휠)
        if (enableZoom)
        {
            HandleZoom();
        }

        // 6. 부드러운 이동
        if (useSmoothMovement)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
        }
    }

    /// <summary>
    /// 화면 가장자리 마우스 위치 감지
    /// </summary>
    Vector3 GetEdgeScrollMovement()
    {
        Vector3 movement = Vector3.zero;
        Vector3 mousePos = Input.mousePosition;

        // 왼쪽 가장자리
        if (mousePos.x < edgeThreshold)
        {
            movement.x = -edgeScrollSpeed;
        }
        // 오른쪽 가장자리
        else if (mousePos.x > Screen.width - edgeThreshold)
        {
            movement.x = edgeScrollSpeed;
        }

        // 아래쪽 가장자리
        if (mousePos.y < edgeThreshold)
        {
            movement.y = -edgeScrollSpeed;
        }
        // 위쪽 가장자리
        else if (mousePos.y > Screen.height - edgeThreshold)
        {
            movement.y = edgeScrollSpeed;
        }

        return movement;
    }

    /// <summary>
    /// WASD 키보드 입력
    /// </summary>
    Vector3 GetKeyboardMovement()
    {
        Vector3 movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            movement.y = keyboardMoveSpeed;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            movement.y = -keyboardMoveSpeed;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            movement.x = -keyboardMoveSpeed;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            movement.x = keyboardMoveSpeed;
        }

        return movement;
    }

    /// <summary>
    /// 마우스 드래그 이동 (중간 버튼)
    /// </summary>
    Vector3 GetMouseDragMovement()
    {
        Vector3 movement = Vector3.zero;

        // 드래그 시작
        if (Input.GetKeyDown(dragButton))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }

        // 드래그 중
        if (isDragging && Input.GetKey(dragButton))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            movement = new Vector3(-delta.x * dragSpeed, -delta.y * dragSpeed, 0);
            lastMousePosition = Input.mousePosition;
        }

        // 드래그 종료
        if (Input.GetKeyUp(dragButton))
        {
            isDragging = false;
        }

        return movement;
    }

    /// <summary>
    /// 마우스 휠 줌
    /// </summary>
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            // Orthographic 카메라 (2D)
            if (cam.orthographic)
            {
                cam.orthographicSize -= scroll * zoomSpeed;
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
            }
            // Perspective 카메라 (3D)
            else
            {
                Vector3 pos = transform.position;
                pos.z += scroll * zoomSpeed;
                pos.z = Mathf.Clamp(pos.z, -maxZoom, -minZoom);
                targetPosition = pos;
            }
        }
    }

    /// <summary>
    /// 카메라를 특정 위치로 즉시 이동
    /// </summary>
    public void SetPosition(Vector3 position)
    {
        targetPosition = position;
        targetPosition.z = transform.position.z; // Z축 유지
        transform.position = targetPosition;
    }

    /// <summary>
    /// 카메라를 특정 위치로 부드럽게 이동
    /// </summary>
    public void MoveToPosition(Vector3 position)
    {
        targetPosition = position;
        targetPosition.z = transform.position.z; // Z축 유지
    }

    /// <summary>
    /// 카메라 범위 설정
    /// </summary>
    public void SetBounds(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;
        useBounds = true;
    }

    /// <summary>
    /// 특정 기능 토글
    /// </summary>
    public void ToggleEdgeScrolling(bool enabled)
    {
        enableEdgeScrolling = enabled;
    }

    public void ToggleKeyboardMovement(bool enabled)
    {
        enableKeyboardMovement = enabled;
    }

    public void ToggleMouseDrag(bool enabled)
    {
        enableMouseDrag = enabled;
    }

    public void ToggleZoom(bool enabled)
    {
        enableZoom = enabled;
    }

    /// <summary>
    /// Gizmos로 카메라 범위 시각화
    /// </summary>
    void OnDrawGizmos()
    {
        if (!useBounds) return;

        Gizmos.color = Color.yellow;

        // 카메라 범위 사각형 그리기
        Vector3 bottomLeft = new Vector3(minBounds.x, minBounds.y, 0);
        Vector3 bottomRight = new Vector3(maxBounds.x, minBounds.y, 0);
        Vector3 topLeft = new Vector3(minBounds.x, maxBounds.y, 0);
        Vector3 topRight = new Vector3(maxBounds.x, maxBounds.y, 0);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }

    // OnGUI는 GameUIManager에서 통합 관리됨
}
