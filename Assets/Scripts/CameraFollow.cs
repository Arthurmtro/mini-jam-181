using UnityEngine;

namespace BunnyCoffee
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField]
        [Header("Target Settings")]
        [Tooltip("The target that the camera will follow.")]
        private Transform player;

        [SerializeField]
        [Tooltip("Distance between the player and the camera")]
        private float baseDistance = 5f;

        [SerializeField]
        [Header("Follow Settings")]
        [Tooltip("How smoothly the camera follows the target.")]
        private float smoothSpeed = 0.125f;

        #region Zoom
        [SerializeField]
        [Header("Zoom Settings")]
        [Tooltip("Whether the camera should perform repetitive zoom in and out.")]
        private bool isZooming = false;

        [SerializeField]
        [Tooltip("How much the camera zooms in")]
        private float zoomFactor = 0.05f;

        [SerializeField]
        [Tooltip("How smoothly the camera zooms in and out.")]
        private float zoomSmoothSpeed = 0.1f;

        [SerializeField]
        [Tooltip("Speed at which the camera oscillates between zoom levels.")]
        public float zoomOscillationSpeed = 6f;
        #endregion

        #region Rotation
        [SerializeField]
        [Header("Rotation Settings")]
        [Tooltip("Whether the camera should perform repetitive left-right rotation.")]
        private bool isRotating = false;

        [SerializeField]
        [Tooltip("How much the camera rotates left and right.")]
        public float rotationFactor = 1f;

        [SerializeField]
        [Tooltip("Speed at which the camera oscillates between angles.")]
        public float oscillationSpeed = 3f;
        #endregion

        private Camera cam;

        private Vector3 velocity = Vector3.zero;
        private float currentRotationTime = 0f;
        private float currentZoomTime = 0f;

        void Start()
        {
            cam = GetComponent<Camera>();
        }

        void LateUpdate()
        {
            FollowPlayer();

            HandleZoomEffect();
            HandleRotationEffect();
        }

        private void FollowPlayer()
        {
            Vector3 desiredPosition = player.position + new Vector3(0, 0, -5);
            Vector3 smoothedPosition = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref velocity,
                smoothSpeed
            );
            transform.position = smoothedPosition;
        }

        private void HandleZoomEffect()
        {
            if (!cam || !isZooming)
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

        private void HandleRotationEffect()
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
    }
}
