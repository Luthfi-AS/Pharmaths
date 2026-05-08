using UnityEngine;
using UnityEngine.UI;

public class DiagnoseController : MonoBehaviour
{
    [Header("UI Overlays")]
    [SerializeField] private GameObject diagnosisOverlay; // Tarik Canvas_Diagnos ke sini

    void Start()
    {
        // Pastikan overlay diagnosis tertutup saat aplikasi pertama kali dijalankan
        CloseDiagnosis();
    }

    // Fungsi untuk membuka modal pemilihan penyakit
    public void OpenDiagnosis()
    {
        if (diagnosisOverlay != null)
        {
            diagnosisOverlay.SetActive(true);
            Debug.Log("Membuka Modal Diagnosis...");
        }
    }

    // Fungsi untuk menutup modal (tombol Batal atau tombol X)
    public void CloseDiagnosis()
    {
        if (diagnosisOverlay != null)
        {
            diagnosisOverlay.SetActive(false);
        }
    }

    // Fungsi placeholder untuk tombol "Kirim Diagnosa" ke Apoteker
    public void SubmitDiagnosis(string diseaseName)
    {
        Debug.Log("Dokter mendiagnosa: " + diseaseName);
        
        // Logika selanjutnya: Mengirim data diseaseName ke Firebase 
        // agar layar Apoteker bisa tersinkronisasi.
        
        CloseDiagnosis();
    }
}