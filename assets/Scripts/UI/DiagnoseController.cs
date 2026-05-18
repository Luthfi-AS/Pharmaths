using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Extensions;

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
        // agar layar Apoteker bisa tersinkronisasi. (Ongoing)
        
        if (FirebaseManager.Instance == null || FirebaseManager.Instance.DBReference == null)
    {
        Debug.LogError("Firebase belum siap!");
        return;
    }

    string roomId = GameSession.RoomID;
    if (string.IsNullOrEmpty(roomId))
    {
        Debug.LogError("RoomID belum diset!");
        return;
    }

    DatabaseReference diagnosisRef = FirebaseManager.Instance.DBReference
        .Child("rooms")
        .Child(roomId)
        .Child("doctorDiagnosis");

    diagnosisRef.SetValueAsync(diseaseName).ContinueWithOnMainThread(task =>
    {
        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Gagal kirim diagnosa ke Firebase: " + task.Exception);
        }
        else
        {
            Debug.Log("Diagnosa terkirim ke Firebase.");
            CloseDiagnosis();
        }
    });
    }
}