using UnityEngine;
using UnityEngine.UI; // Wajib ditambahkan untuk memanggil UI Button
using Firebase.Database;
using Firebase.Extensions;

public class DoctorGameplay : MonoBehaviour
{
    [Header("Diagnose Buttons")]
    [Tooltip("Masukkan semua tombol penyakit dari Canvas di sini")]
    [SerializeField] private Button[] diagnoseButtons;

    private DatabaseReference dbRef;
    private string roomID;

    void Start()
    {
        // Gunakan FirebaseManager jika ada, atau DefaultInstance jika tidak
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.DBReference != null)
        {
            dbRef = FirebaseManager.Instance.DBReference;
        }
        else
        {
            dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        }

        // --- AMBIL ROOM ID DINAMIS ---
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(GameSession.RoomID)) GameSession.RoomID = "TEST01";
#endif
        roomID = GameSession.RoomID;

        if (string.IsNullOrEmpty(roomID))
        {
            Debug.LogError("[DoctorGameplay] Room ID kosong! Pastikan login lewat Main Menu.");
        }

        // Jalankan fungsi untuk memasang listener otomatis ke tombol-tombol
        SetupDiagnoseButtons();
    }

    // --- SETUP TOMBOL OTOMATIS ---
    private void SetupDiagnoseButtons()
    {
        if (diagnoseButtons == null || diagnoseButtons.Length == 0)
        {
            Debug.LogWarning("[DoctorGameplay] Array diagnoseButtons kosong! Periksa Inspector.");
            return;
        }

        foreach (Button btn in diagnoseButtons)
        {
            if (btn != null)
            {
                // Ambil nama tombol (contoh: "Demam_Btn") lalu buang bagian "_Btn"-nya
                string diagnosisName = btn.gameObject.name.Replace("_Btn", "");

                // Daftarkan aksi ketika tombol ini diklik
                btn.onClick.AddListener(() => SubmitDiagnosis(diagnosisName));
                Debug.Log("[DoctorGameplay] Tombol terdaftar: " + diagnosisName);
            }
        }
    }

    // 1. PANGGIL FUNGSI INI DARI EVENT VUFORIA (Saat target terdeteksi)
    public void OnMarkerScanned(string caseID)
    {
        if (string.IsNullOrEmpty(roomID)) return;

        dbRef.Child("rooms").Child(roomID).Child("gameplay").Child("current_case")
             .SetValueAsync(caseID);
             
        Debug.Log("Dokter men-scan: " + caseID);
    }

    // 2. FUNGSI UNTUK MENGIRIM DIAGNOSIS KE FIREBASE
    public void SubmitDiagnosis(string diagnosisName)
    {
        if (dbRef == null)
        {
            Debug.LogError("[DoctorGameplay] Firebase belum siap!");
            return;
        }

        if (string.IsNullOrEmpty(roomID))
        {
            Debug.LogError("[DoctorGameplay] RoomID belum diset!");
            return;
        }

        dbRef.Child("rooms").Child(roomID).Child("gameplay").Child("doctor_diagnosis")
             .SetValueAsync(diagnosisName).ContinueWithOnMainThread(task =>
             {
                 if (task.IsFaulted || task.IsCanceled)
                 {
                     Debug.LogError("[DoctorGameplay] Gagal kirim diagnosa: " + task.Exception);
                 }
                 else
                 {
                     Debug.Log("[DoctorGameplay] Diagnosa terkirim: " + diagnosisName);
                 }
             });
    }
}