using UnityEngine;
using UnityEngine.UI; 
using TMPro; 
using Firebase.Database;
using Firebase.Extensions;
using System.Collections; // Wajib untuk IEnumerator

public class DoctorGameplay : MonoBehaviour
{
    [Header("UI Canvas References")]
    [SerializeField] private GameObject canvasDiagnose; 

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

    private DatabaseReference dbRef;
    private string roomID;
    private bool isListening = false;

    void Start()
    {
        // 1. Setup UI Lokal terlebih dahulu agar aman
        SetupDiagnoseButtons();

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

        // 2. Tunggu Firebase siap menggunakan Coroutine (Mencegah kegagalan listener)
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

        // Mulai mendengarkan hasil dari Apoteker
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
        if (string.IsNullOrEmpty(roomID) || dbRef == null) return;
        
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
        // LOG DIAGNOSA: Ini akan membuktikan apakah Firebase mengirim data ke Dokter atau tidak
        Debug.Log("[DoctorGameplay] Mendapatkan update data dari 'match_result'!");

        if (args.DatabaseError != null)
        {
            Debug.LogError("[DoctorGameplay] Error Database: " + args.DatabaseError.Message);
            return;
        }

        if (!args.Snapshot.Exists)
        {
            Debug.Log("[DoctorGameplay] Snapshot kosong (mungkin di-reset).");
            return;
        }

        string status = "";
        string message = "";

        if (args.Snapshot.HasChild("status")) status = args.Snapshot.Child("status").Value.ToString();
        if (args.Snapshot.HasChild("message")) message = args.Snapshot.Child("message").Value.ToString();

        Debug.Log($"[DoctorGameplay] Menerima Hasil - Status: {status}, Pesan: {message}");

        if (!string.IsNullOrEmpty(status))
        {
            ShowResultModal(status, message);
        }
    }

    private void ShowResultModal(string status, string message)
    {
        if (resultModalCanvas == null)
        {
            Debug.LogError("[DoctorGameplay] GAGAL: resultModalCanvas belum dimasukkan ke Inspector!");
            return;
        }

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
                txtResultStatus.text = "MALAPRAKTIK!";
                txtResultStatus.color = colorLose;
            }
            if (imgResultIcon != null && spriteFailIcon != null) imgResultIcon.sprite = spriteFailIcon;
        }
    }

    private void CloseLocalModal()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.clickGeneral);
        if (resultModalCanvas != null) resultModalCanvas.SetActive(false);
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