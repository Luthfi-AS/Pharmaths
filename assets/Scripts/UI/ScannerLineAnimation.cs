using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ScannerLineAnimation : MonoBehaviour
{
    [Header("Scanner Settings")]
    [Tooltip("Seberapa cepat garis naik dan turun?")]
    [SerializeField] private float speed = 250f;
    
    [Tooltip("Seberapa jauh (pixel) garis boleh bergerak dari titik tengah?")]
    [SerializeField] private float travelDistance = 150f;

    private RectTransform rectTransform;
    private float startY;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // Simpan titik tengah awal dari garis
        startY = rectTransform.anchoredPosition.y;
    }

    private void Update()
    {
        // Mathf.PingPong membuat nilai bolak-balik dari 0 hingga batas maksimal secara otomatis
        float pingPong = Mathf.PingPong(Time.time * speed, travelDistance * 2);
        
        // Geser rentang nilainya agar pergerakannya seimbang ke atas dan ke bawah dari startY
        float newY = startY + pingPong - travelDistance;
        
        // Terapkan posisi baru ke garis
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newY);
    }
}