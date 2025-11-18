using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 20f;
    [SerializeField] private float panSpeed = 2f; // 新增平移速度参数
    private bool isPanning = false; // 新增平移状态标志

    private Camera cam;
    private Vector3 targetRotation;
    private bool isDragging = false;
    private Vector3 previousMousePosition;

    private void Start()
    {
        cam = Camera.main;
        targetRotation = transform.eulerAngles;
    }

    private void Update()
    {
        HandleRotation();
        HandlePan(); // 新增平移处理
        HandleZoom();
        
        // 平滑插值实现丝滑旋转
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(targetRotation),
            Time.deltaTime * smoothSpeed
        );
    }

    private void HandleRotation()
    {
        // 检测鼠标左键按下
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            previousMousePosition = Input.mousePosition;
        }
        
        // 检测鼠标左键松开
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // 处理拖拽旋转
        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - previousMousePosition;
            targetRotation.y += delta.x * rotationSpeed * Time.deltaTime;
            targetRotation.x -= delta.y * rotationSpeed * Time.deltaTime;
            
            // 限制垂直旋转角度
            targetRotation.x = Mathf.Clamp(targetRotation.x, -90f, 90f);
            
            previousMousePosition = Input.mousePosition;
        }
    }

    private void HandleZoom()
    {
        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0)
        {
            // 获取鼠标在世界空间中的射线
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Vector3 zoomTarget;

            // 如果射线击中物体，使用击中点作为缩放焦点
            if (Physics.Raycast(ray, out hit))
            {
                zoomTarget = hit.point;
            }
            else
            {
                // 如果没有击中物体，使用相机前方的点
                zoomTarget = transform.position + transform.forward * 10f;
            }

            // 计算新的相机位置
            Vector3 directionToTarget = transform.position - zoomTarget;
            float newDistance = directionToTarget.magnitude - scrollDelta * zoomSpeed;
            newDistance = Mathf.Clamp(newDistance, minZoom, maxZoom);

            // 更新相机位置
            transform.position = zoomTarget + directionToTarget.normalized * newDistance;
        }
        
    }
    // 新增平移处理方法
    private void HandlePan()
    {
        // 检测中键按下
        if (Input.GetMouseButtonDown(2))
        {
            isPanning = true;
            previousMousePosition = Input.mousePosition;
        }
    
        // 检测中键松开
        if (Input.GetMouseButtonUp(2))
        {
            isPanning = false;
        }
    
        // 处理拖拽平移
        if (isPanning)
        {
            Vector3 delta = Input.mousePosition - previousMousePosition;
            Vector3 panTranslation = new Vector3(
                -delta.x * panSpeed * Time.deltaTime,
                -delta.y * panSpeed * Time.deltaTime,
                0
            );
            
            // 基于相机朝向的平移
            transform.Translate(
                panTranslation.x * transform.right + 
                panTranslation.y * transform.up,
                Space.World
            );
            
            previousMousePosition = Input.mousePosition;
        }
    }
}