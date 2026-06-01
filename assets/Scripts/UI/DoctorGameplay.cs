using UnityEngine;
using UnityEngine.UI; 
using TMPro; // Wajib untuk TextMeshPro
using Firebase.Database;
using Firebase.Extensions;

public class DoctorGameplay : MonoBehaviour
{
    [Header("UI Canvas References")]
    [SerializeField] private GameObject canvasDiagnose; // <-- BARU: Referensi untuk menutup/membuka Canvas Diagnose

    [Header("Diagnose Buttons")]
    [Tooltip("Masukkan semua tombol penyakit dari Canvas di sini")]
    [SerializeField] private Button[] diagnoseButtons;

    [Header("Result Modal UI (Logic)")]
    [SerializeField] private GameObject resultModalCanvas;
    [SerializeField] private TextMeshProUGUI txtResultStatus;
    [SerializeField] private TextMeshProUGUI txtResultMessage;
    [SerializeField] private Button btnCloseModal; // Tombol untuk menutup modal lokal
    [SerializeField] private Color colorWin = new Color(0.1f, 0.8f, 0.1f);
    [SerializeField] private Color colorLose = Color.red;

    [Header("Result Modal UI (Sprites)")]
    [SerializeField] private Image imgResultIcon;     
    [SerializeField] private Sprite spriteSuccessIcon; 
    [SerializeField] private Sprite spriteFailIcon;    

    private DatabaseReference dbRef;
    private string roomID;
    private bool isListening = false;

    void Start()
    {
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.DBReference != null)
        {
            dbRef = FirebaseManager.Instance.DBReference;
        }
        else
        {
            dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        }

#if UNITY_EDITOR
        if (string.IsNullOrEmpty(GameSession.RoomID)) GameSession.RoomID = "TEST01";
#endif
        roomID = GameSession.RoomID;

        if (string.IsNullOrEmpty(roomID))
        {
            Debug.LogError("[DoctorGameplay] Room ID kosong! Pastikan login lewat Main Menu.");
        }

        SetupDiagnoseButtons();

        // --- SETUP MODAL LOKAL ---
        if (resultModalCanvas != null) resultModalCanvas.SetActive(false);
        if (btnCloseModal != null) btnCloseModal.onClick.AddListener(CloseLocalModal);

        // --- MULAI MENDENGARKAN HASIL DARI APOTEKER ---
        if (!string.IsNullOrEmpty(roomID) && dbRef != null)
        {
            DatabaseReference matchResultRef = dbRef.Child("rooms").Child(roomID).Child("gameplay").Child("match_result");
            matchResultRef.ValueChanged += HandleMatchResultChanged;
            isListening = true;
        }
    }

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
                string diagnosisName = btn.gameObject.name.Replace("_Btn", "");
                btn.onClick.AddListener(() => SubmitDiagnosis(diagnosisName));
                Debug.Log("[DoctorGameplay] Tombol terdaftar: " + diagnosisName);
            }
        }
    }

    public void OnMarkerScanned(string caseID)
    {
        if (string.IsNullOrEmpty(roomID) || dbRef == null) return;

        dbRef.Child("rooms").Child(roomID).Child("gameplay").Child("current_case")
             .SetValueAsync(caseID);
             
        Debug.Log("Dokter men-scan: " + caseID);
    }

    public void SubmitDiagnosis(string diagnosisName)
    {
        if (dbRef == null || string.IsNullOrEmpty(roomID))
        {
            Debug.LogError("[DoctorGameplay] Firebase/RoomID belum siap!");
            return;
        }

        // --- BARU: Tutup Canvas Diagnose otomatis setelah tombol ditekan ---
        if (canvasDiagnose != null) canvasDiagnose.SetActive(false);

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

    private void HandleMatchResultChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null || !args.Snapshot.Exists) return;

        string status = "";
        string message = "";

        if (args.Snapshot.HasChild("status")) status = args.Snapshot.Child("status").Value.ToString();
        if (args.Snapshot.HasChild("message")) message = args.Snapshot.Child("message").Value.ToString();

        if (!string.IsNullOrEmpty(status))
        {
            ShowResultModal(status, message);
        }
    }

    private void ShowResultModal(string status, string message)
    {
        if (resultModalCanvas == null) return;

        resultModalCanvas.SetActive(true);
        if (txtResultMessage != null) txtResultMessage.text = message;

        if (status == "win")
        {
            if (txtResultStatus != null)
            {
                txtResultStatus.text = "PASIEN SEMBUH!";
                txtResultStatus.color = colorWin;
            }
            if (imgResultIcon != null && spriteSuccessIcon != null) imgResultIcon.sprite = spriteSuccessIcon;
        }
        else if (status == "lose")
        {
            if (txtResultStatus != null)
            {
                txtResultStatus.text = "MALAPRAKTIK!";
                txtResultStatus.color = colorLose;
            }
            if (imgResultIcon != null && spriteFailIcon != null) imgResultIcon.sprite = spriteFailIcon;
        }
    }

    private void CloseLocalModal()
    {
        if (resultModalCanvas != null) resultModalCanvas.SetActive(false);

        // --- BARU: Membuka kembali Canvas Diagnose agar Dokter bisa siap mendiagnosis rekam medis berikutnya atau merevisi jawaban salah ---
        if (canvasDiagnose != null) canvasDiagnose.SetActive(true);
    }

    private void OnDestroy()
    {
        if (isListening && dbRef != null && !string.IsNullOrEmpty(roomID))
        {
            DatabaseReference matchResultRef = dbRef.Child("rooms").Child(roomID).Child("gameplay").Child("match_result");
            matchResultRef.ValueChanged -= HandleMatchResultChanged;
            isListening = false;
        }
    }
}