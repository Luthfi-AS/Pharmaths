using UnityEngine;
using UnityEngine.UI; 
using TMPro; 
using Firebase.Database;
using Firebase.Extensions;
using System.Collections; 
using UnityEngine.SceneManagement;

public class DoctorGameplay : MonoBehaviour
{
    [Header("UI Canvas References")]
    [SerializeField] private GameObject canvasDiagnose; 

    // --- BARU: Referensi elemen UI secara individu (Flat Hierarchy) ---
    [Header("Scanning UI Elements")]
    [SerializeField] private GameObject scanFrame;
    [SerializeField] private GameObject txtInstruction;
    [SerializeField] private GameObject btnManualBook;
    [SerializeField] private GameObject btnDiagnoseHud;

    [Header("Diagnose Buttons")]
    [Tooltip("Masukkan semua tombol penyakit dari Canvas di sini")]
    [SerializeField] private Button[] diagnoseButtons;

    [Header("Result Modal UI (Logic)")]
    [SerializeField] private GameObject resultModalCanvas;
    [SerializeField] private TextMeshProUGUI txtResultStatus;
    [SerializeField] private TextMeshProUGUI txtResultMessage;
    [SerializeField] private Button btnCloseModal; 
    [SerializeField] private Color colorWin = new Color(0.1f, 0.8f, 0.1f);
    [SerializeField] private Color colorLose = Color.red;

    [Header("Result Modal UI (Sprites)")]
    [SerializeField] private Image imgResultIcon;     
    [SerializeField] private Sprite spriteSuccessIcon; 
    [SerializeField] private Sprite spriteFailIcon;    

    // --- BARU: Variabel untuk Session Statistics & End Game ---
    [Header("Session Statistics & End Game")]
    [SerializeField] private string summarySceneName = "SummaryScene";
    [SerializeField] private string finalCaseID = "case_05"; 
    
    private int sessionSaved = 0;
    private int sessionMalpractice = 0;
    private float sessionStartTime;
    private string lastResultStatus = "";
    private string currentActiveCase = ""; // Untuk mengingat pasien mana yang sedang di-scan Dokter
    // ----------------------------------------------------------

    private DatabaseReference dbRef;
    private string roomID;
    private bool isListening = false;
    private bool hasScannedAnyMarker = false; // Penanda agar UI Scan hanya hilang sekali

    void Start()
    {
        // Mulai catat waktu saat shift Dokter dimulai
        sessionStartTime = Time.time;

        SetupDiagnoseButtons();

        // Di awal, nyalakan elemen instruksi scan, matikan tombol HUD
        if (scanFrame != null) scanFrame.SetActive(true);
        if (txtInstruction != null) txtInstruction.SetActive(true);
        if (btnManualBook != null) btnManualBook.SetActive(false);
        if (btnDiagnoseHud != null) btnDiagnoseHud.SetActive(false);

        if (resultModalCanvas != null) resultModalCanvas.SetActive(false);
        if (btnCloseModal != null) btnCloseModal.onClick.AddListener(CloseLocalModal);

        #if UNITY_EDITOR
        if (string.IsNullOrEmpty(GameSession.RoomID)) GameSession.RoomID = "TEST01";
        #endif
        
        roomID = GameSession.RoomID;

        if (string.IsNullOrEmpty(roomID))
        {
            Debug.LogError("[DoctorGameplay] Room ID kosong! Pastikan login lewat Main Menu.");
            return;
        }

        StartCoroutine(WaitForFirebaseAndListen());
    }

    private IEnumerator WaitForFirebaseAndListen()
    {
        Debug.Log("[DoctorGameplay] Menunggu Firebase siap...");
        while (FirebaseManager.Instance == null || FirebaseManager.Instance.DBReference == null)
        {
            yield return null;
        }

        dbRef = FirebaseManager.Instance.DBReference;
        Debug.Log("[DoctorGameplay] Firebase siap. Menempelkan listener ke Room: " + roomID);

        DatabaseReference matchResultRef = dbRef.Child("rooms").Child(roomID).Child("gameplay").Child("match_result");
        matchResultRef.ValueChanged += HandleMatchResultChanged;
        isListening = true;
    }

    private void SetupDiagnoseButtons()
    {
        if (diagnoseButtons == null || diagnoseButtons.Length == 0) return;

        foreach (Button btn in diagnoseButtons)
        {
            if (btn != null)
            {
                string diagnosisName = btn.gameObject.name.Replace("_Btn", "");
                btn.onClick.AddListener(() => SubmitDiagnosis(diagnosisName));
            }
        }
    }

    public void OnMarkerScanned(string caseID)
    {
        // Jika ini scan pertama kali, matikan frame scan dan nyalakan tombol HUD permanen
        if (!hasScannedAnyMarker)
        {
            if (scanFrame != null) scanFrame.SetActive(false);
            if (txtInstruction != null) txtInstruction.SetActive(false);
            
            if (btnManualBook != null) btnManualBook.SetActive(true);
            if (btnDiagnoseHud != null) btnDiagnoseHud.SetActive(true);
            
            hasScannedAnyMarker = true; 
        }

        if (string.IsNullOrEmpty(roomID) || dbRef == null) return;
        
        currentActiveCase = caseID;

        dbRef.Child("rooms").Child(roomID).Child("gameplay").Child("current_case").SetValueAsync(caseID);
        Debug.Log("Dokter men-scan: " + caseID);
    }

    public void SubmitDiagnosis(string diagnosisName)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.clickGeneral);

        if (dbRef == null || string.IsNullOrEmpty(roomID)) return;

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
        Debug.Log("[DoctorGameplay] Mendapatkan update data dari 'match_result'!");

        if (args.DatabaseError != null) return;
        if (!args.Snapshot.Exists) return;

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

        lastResultStatus = status;

        resultModalCanvas.SetActive(true);
        if (txtResultMessage != null) txtResultMessage.text = message;

        if (status == "win")
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.succeedStatus);
            if (txtResultStatus != null)
            {
                txtResultStatus.text = "PASIEN SEMBUH!";
                txtResultStatus.color = colorWin;
            }
            if (imgResultIcon != null && spriteSuccessIcon != null) imgResultIcon.sprite = spriteSuccessIcon;
        }
        else if (status == "lose")
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.failedStatus);
            if (txtResultStatus != null)
            {
                txtResultStatus.text = "MALAPRAKTIK";
                txtResultStatus.color = colorLose;
            }
            if (imgResultIcon != null && spriteFailIcon != null) imgResultIcon.sprite = spriteFailIcon;
        }
    }

    private void CloseLocalModal()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.clickGeneral);
        
        // 1. Catat Skor ke dalam memori sementara
        if (lastResultStatus == "win") sessionSaved++;
        else if (lastResultStatus == "lose") sessionMalpractice++;

        // 2. Cek apakah ini adalah kasus terakhir dan berhasil disembuhkan
        if (currentActiveCase == finalCaseID && lastResultStatus == "win")
        {
            Debug.Log("[DoctorGameplay] Shift Selesai! Mengkalkulasi Skor Dokter...");

            float totalTime = Time.time - sessionStartTime;
            float avgTime = totalTime / 5f; 

            // Simpan Data ke PlayerPrefs untuk dibaca oleh SummaryManager di HP Dokter
            PlayerPrefs.SetInt("TotalSaved", sessionSaved);
            PlayerPrefs.SetInt("TotalMalpractice", sessionMalpractice);
            PlayerPrefs.SetFloat("AverageTime", avgTime);
            PlayerPrefs.Save();

            // Pindah ke Scene Summary
            SceneManager.LoadScene(summarySceneName);
        }
        else
        {
            // Jika belum selesai, tutup modal dan buka lagi panel diagnosa
            if (resultModalCanvas != null) resultModalCanvas.SetActive(false);
            if (canvasDiagnose != null) canvasDiagnose.SetActive(true);
        }
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