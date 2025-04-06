using UnityEngine;

namespace BunnyCoffee
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Boundaries the camera cannot escape from.")]
        Rect boundaries;

        [SerializeField]
        [Header("Follow Settings")]
        [Tooltip("Whether the camera should follow a target.")]
        bool isFollowing = false;

        [SerializeField]
        [Tooltip("The target that the camera will follow.")]
        Transform followTarget;

        [SerializeField]
        [Tooltip("Distance between the player and the camera")]
        float baseDistance = 5f;

        [SerializeField]
        [Tooltip("How smoothly the camera follows the target.")]
        float smoothSpeed = 0.125f;

        [Header("Zoom")]
        [SerializeField]
        [Header("Zoom Settings")]
        [Tooltip("Whether the camera should perform repetitive zoom in and out.")]
        bool isZooming = false;

        [SerializeField]
        [Tooltip("How much the camera zooms in")]
        float zoomFactor = 0.05f;

        [SerializeField]
        [Tooltip("How smoothly the camera zooms in and out.")]
        float zoomSmoothSpeed = 0.1f;

        [SerializeField]
        [Tooltip("Speed at which the camera oscillates between zoom levels.")]
        public float zoomOscillationSpeed = 6f;

        [Header("Rotation")]
        [SerializeField]
        [Header("Rotation Settings")]
        [Tooltip("Whether the camera should perform repetitive left-right rotation.")]
        bool isRotating = false;

        [SerializeField]
        [Tooltip("How much the camera rotates left and right.")]
        public float rotationFactor = 1f;

        [SerializeField]
        [Tooltip("Speed at which the camera oscillates between angles.")]
        public float oscillationSpeed = 3f;

        Camera cam;

        Vector3 velocity = Vector3.zero;
        float currentRotationTime = 0f;
        float currentZoomTime = 0f;

        void Start()
        {
            cam = GetComponent<Camera>();
        }

        void LateUpdate()
        {

            FollowPlayer();
            Interact();
            HandleZoomEffect();
            HandleRotationEffect();
            ClampCamera();

        }

        public void SetRotationState(bool enable)
        {
            isRotating = enable;
            if (!enable)
            {
                currentRotationTime = 0f;
            }
        }

        public void SetZoomState(bool enable)
        {
            isZooming = enable;
            if (!enable)
            {
                currentZoomTime = 0f;
            }
        }

        public void SetBaseDistance(float newValue)
        {
            baseDistance = newValue;
        }

        void Interact()
        {
            if (Input.GetMouseButton(0))
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                isFollowing = false;
                Vector3 move = new Vector3(-mouseX, -mouseY, 0) * baseDistance * 0.1f;
                transform.Translate(move, Space.Self);
            }

            if (Input.GetMouseButtonUp(0))
            {
                Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);

                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider != null && hit.collider.CompareTag("Clickable"))
                    {
                        followTarget = hit.collider.gameObject.transform;
                        isFollowing = true;
                    }
                }
            }

            float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
            if (zoomDelta != 0f)
            {
                Vector3 mouseScreenPos = Input.mousePosition;
                Vector3 mouseWorldPos = cam.ScreenToWorldPoint(mouseScreenPos);

                float newZoom = Mathf.Clamp(cam.orthographicSize - zoomDelta, 4f, 16f);
                float zoomFactor = newZoom / cam.orthographicSize;

                baseDistance -= zoomDelta;
                baseDistance = Mathf.Clamp(baseDistance, 4f, 16f);

                cam.orthographicSize = newZoom;

                Vector3 cameraPosToMouse = mouseWorldPos - transform.position;
                Vector3 newCameraPos = mouseWorldPos - (cameraPosToMouse * zoomFactor);
                transform.position = new Vector3(newCameraPos.x, newCameraPos.y, transform.position.z);
            }
        }

        void FollowPlayer()
        {
            if (!isFollowing)
            {
                return;
            }

            Vector3 desiredPosition = followTarget.position + new Vector3(0, 0, -5);
            Vector3 smoothedPosition = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref velocity,
                smoothSpeed
            );
            transform.position = smoothedPosition;
        }

        void HandleZoomEffect()
        {
            if (!isZooming)
            {
                if (!Mathf.Approximately(cam.orthographicSize, baseDistance))
                {
                    cam.orthographicSize = baseDistance;
                }
                return;
            }

            currentZoomTime += Time.deltaTime * zoomOscillationSpeed;

            float t = (Mathf.Sin(currentZoomTime) + 1f) / 2f;
            float targetZoom = Mathf.Lerp(baseDistance, baseDistance - zoomFactor, t);

            float smoothZoom = Mathf.Lerp(cam.orthographicSize, targetZoom, zoomSmoothSpeed);
            cam.orthographicSize = smoothZoom;
        }

        void HandleRotationEffect()
        {
            if (!isRotating)
            {
                if (transform.rotation != Quaternion.identity)
                {
                    currentRotationTime = 0f;
                    transform.rotation = Quaternion.identity;
                }
                return;
            }

            currentRotationTime += Time.deltaTime * oscillationSpeed;
            float t = (Mathf.Sin(currentRotationTime) + 1f) / 2f;
            float angle = Mathf.Lerp(-rotationFactor, rotationFactor, t);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }


        void ClampCamera()
        {
            Vector3 cameraPosition = cam.transform.position;
            float cameraHeight = cam.orthographicSize;
            float cameraWidth = cam.aspect * cameraHeight;

            // Clamp the x position
            if (cameraPosition.x - cameraWidth < boundaries.xMin)
            {
                cameraPosition.x = boundaries.xMin + cameraWidth;
            }
            else if (cameraPosition.x + cameraWidth > boundaries.xMax)
            {
                cameraPosition.x = boundaries.xMax - cameraWidth;
            }

            // Clamp the y position
            if (cameraPosition.y - cameraHeight < boundaries.yMin)
            {
                cameraPosition.y = boundaries.yMin + cameraHeight;
            }
            else if (cameraPosition.y + cameraHeight > boundaries.yMax)
            {
                cameraPosition.y = boundaries.yMax - cameraHeight;
            }

            cam.transform.position = cameraPosition;
        }
    }
}
