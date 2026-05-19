using UnityEngine;
using UnityEngine.UI; // Wajib ditambahkan untuk memanggil UI Button
using Firebase.Database;

public class DoctorGameplay : MonoBehaviour
{
    [Header("Diagnose Buttons")]
    [Tooltip("Masukkan semua tombol penyakit dari Canvas di sini")]
    [SerializeField] private Button[] diagnoseButtons;

    private DatabaseReference dbRef;
    private string roomID;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

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
        foreach (Button btn in diagnoseButtons)
        {
            if (btn != null)
            {
                // Ambil nama tombol (contoh: "Demam_Btn") lalu buang bagian "_Btn"-nya
                string diagnosisName = btn.gameObject.name.Replace("_Btn", "");

                // Daftarkan aksi ketika tombol ini diklik
                btn.onClick.AddListener(() => SubmitDiagnosis(diagnosisName));
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
        if (string.IsNullOrEmpty(roomID)) return;

        dbRef.Child("rooms").Child(roomID).Child("gameplay").Child("doctor_diagnosis")
             .SetValueAsync(diagnosisName);
             
        Debug.Log("Dokter mendiagnosis: " + diagnosisName);
    }
}