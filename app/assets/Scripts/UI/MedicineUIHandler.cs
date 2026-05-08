using UnityEngine;
using UnityEngine.EventSystems;

public class MedicineUIHandler : MonoBehaviour, IDragHandler, IScrollHandler
{
    [Header("Target References")]
    [SerializeField] private Transform medicineStudio; 
    [SerializeField] private Camera medicineCamera;    

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 0.5f;
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minFOV = 15f;
    [SerializeField] private float maxFOV = 60f;

    // Fungsi ini terpanggil otomatis saat user melakukan drag di area UI ini
    public void OnDrag(PointerEventData eventData)
    {
        if (medicineStudio != null)
        {
            // Menggunakan delta dari eventData (pergerakan mouse/jari)
            float rotX = eventData.delta.x * rotationSpeed;
            float rotY = eventData.delta.y * rotationSpeed;

            medicineStudio.Rotate(Vector3.up, -rotX, Space.World);
            medicineStudio.Rotate(Vector3.right, rotY, Space.World);
        }
    }

    // Fungsi ini untuk scroll mouse (Zoom) di area UI ini
    public void OnScroll(PointerEventData eventData)
    {
        if (medicineCamera != null)
        {
            float scroll = eventData.scrollDelta.y;
            ZoomCamera(scroll);
        }
    }

    void Update()
    {
        // Logika Pinch-to-Zoom khusus Mobile (Multi-touch)
        if (Input.touchCount == 2)
        {
            HandlePinchZoom();
        }
    }

    private void HandlePinchZoom()
    {
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

        float difference = currentMagnitude - prevMagnitude;
        ZoomCamera(difference * 0.01f);
    }

    private void ZoomCamera(float increment)
    {
        medicineCamera.fieldOfView -= increment * zoomSpeed;
        medicineCamera.fieldOfView = Mathf.Clamp(medicineCamera.fieldOfView, minFOV, maxFOV);
    }
}