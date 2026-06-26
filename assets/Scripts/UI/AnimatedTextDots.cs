using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class AnimatedTextDots : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    
    [Tooltip("Teks dasar sebelum titik-titik, misalnya: 'Arahkan kamera ke Buku'")]
    [SerializeField] private string baseText = "Arahkan kamera ke Buku Pasien";
    
    [Tooltip("Kecepatan ganti titik dalam detik")]
    [SerializeField] private float animationSpeed = 0.5f;

    private void Awake()
    {
        // Otomatis mengambil komponen TextMeshPro di objek ini
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        // Menggunakan OnEnable agar animasi langsung jalan setiap kali teks dimunculkan
        StartCoroutine(AnimateDots());
    }

    private void OnDisable()
    {
        // Hentikan animasi jika teks disembunyikan agar hemat memori
        StopAllCoroutines();
    }

    private IEnumerator AnimateDots()
    {
        int dotCount = 0;
        while (true) // Looping terus menerus
        {
            // Reset jumlah titik kalau sudah lebih dari 3
            if (dotCount > 3) dotCount = 0;

            // Buat string titik sebanyak dotCount
            string dots = new string('.', dotCount);
            
            // Gabungkan teks dasar dengan titik-titik
            textMesh.text = baseText + dots;

            dotCount++;
            
            // Tunggu beberapa saat sebelum lanjut ke frame berikutnya
            yield return new WaitForSeconds(animationSpeed);
        }
    }
}