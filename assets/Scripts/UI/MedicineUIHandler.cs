using UnityEngine;
using UnityEngine.EventSystems;

public class MedicineUIHandler : MonoBehaviour, IDragHandler, IScrollHandler
{
    [Header("Target References")]
    [SerializeField] private Transform medicineStudio; // Parent (Wadah obat)
    [SerializeField] private Camera medicineCamera;    

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 0.4f;
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minFOV = 15f;
    [SerializeField] private float maxFOV = 60f;

    // Fungsi utama untuk memutar anak yang sedang aktif
    public void OnDrag(PointerEventData eventData)
    {
        if (medicineStudio == null) return;

        // Mencari objek anak yang sedang aktif (Active Self)
        Transform activeChild = GetActiveChild();

        if (activeChild != null)
        {
            float rotX = eventData.delta.x * rotationSpeed;
            float rotY = eventData.delta.y * rotationSpeed;

            // Rotasi diterapkan langsung ke objek anak (ActiveChild)
            // Menggunakan Space.World agar arah putaran tetap mengikuti logika layar
            activeChild.Rotate(Vector3.up, -rotX, Space.World);
            activeChild.Rotate(Vector3.right, rotY, Space.World);
        }
    }

    // Helper function untuk menemukan anak yang sedang aktif di bawah Parent
    private Transform GetActiveChild()
    {
        foreach (Transform child in medicineStudio)
        {
            if (child.gameObject.activeSelf)
            {
                return child;
            }
        }
        return null;
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (medicineCamera != null)
        {
            ZoomCamera(eventData.scrollDelta.y);
        }
    }

    void Update()
    {
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