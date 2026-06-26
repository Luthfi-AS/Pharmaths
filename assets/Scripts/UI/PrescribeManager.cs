using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database; 
using Firebase.Extensions; 
using UnityEngine.SceneManagement; // <-- BARU: Wajib untuk pindah scene

public class PrescribeManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject canvasPrescribe;
    [SerializeField] private TMP_Text medicineTitle;
    [SerializeField] private TMP_InputField doseInput;
    [SerializeField] private Button prescribeBtn;
    [SerializeField] private TextMeshProUGUI txtDiagnosis; 

    [Header("Result Modal UI (Logic)")]
    [SerializeField] private GameObject resultModalCanvas;
    [SerializeField] private TextMeshProUGUI txtResultStatus;
    [SerializeField] private TextMeshProUGUI txtResultMessage;
    [SerializeField] private Button btnResultAction;
    [SerializeField] private Color colorWin = new Color(0.1f, 0.8f, 0.1f); 
    [SerializeField] private Color colorLose = Color.red;

    [Header("Result Modal UI (Sprites)")]
    [SerializeField] private Image imgResultIcon;     
    [SerializeField] private Sprite spriteSuccessIcon; 
    [SerializeField] private Sprite spriteFailIcon;    
    
    [SerializeField] private Image imgBtnAction;      
    [SerializeField] private Sprite spriteBtnNext;     
    [SerializeField] private Sprite spriteBtnRetry;    

    [Header("3D Model References")]
    [SerializeField] private Transform medicineStudio; 
    [SerializeField] private List<GameObject> medicineModels;
    [SerializeField] private float rotationSpeed = 20f; 

    [Header("Patient Database (Local)")]
    [SerializeField] private PatientCaseData[] patientDatabase;
    
    // --- BARU: Variabel untuk Session Statistics & End Game ---
    [Header("Session Statistics & End Game")]
    [SerializeField] private string summarySceneName = "SummaryScene"; // Pastikan nama ini sama dengan di Build Settings
    [SerializeField] private string finalCaseID = "case_05"; 
    
    private int sessionSaved = 0;
    private int sessionMalpractice = 0;
    private float sessionStartTime;
    private string lastResultStatus = "";
    // ----------------------------------------------------------

    // Status Game & Jaringan
    private PatientCaseData activePatient;
    private int currentIndex = 0; 
    private string currentRoomID = ""; 
    private string currentActiveCase = "";
    private string currentDoctorDiagnosis = ""; 

    // Variabel Firebase
    private DatabaseReference caseRef;
    private DatabaseReference diagnosisRef;
    private DatabaseReference matchResultRef;
    private bool isListening = false;

    // Nama obat disesuaikan dengan urutan di Inspector
    private string[] medicineNames = { 
        "Amoxicillin", "Antasida", "Cetirizine", "Ibuprofen", 
        "Insulin", "Paracetamol", "Salbutamol", "Sirup Ekspektoran" 
    };

    void Start()
    {
        // --- BARU: Mulai catat waktu saat shift dimulai ---
        sessionStartTime = Time.time; 

        UpdateDisplay();
        
        if (prescribeBtn != null) prescribeBtn.onClick.AddListener(OnPrescribeClicked);
        if (txtDiagnosis != null) txtDiagnosis.text = "Menunggu diagnosis dokter...";

        if (resultModalCanvas != null) resultModalCanvas.SetActive(false);
        if (btnResultAction != null) btnResultAction.onClick.AddListener(OnResultActionClicked);

        InitializeRoomSession();

        if (!string.IsNullOrEmpty(currentRoomID))
        {
            StartCoroutine(InitializeFirebaseListeners());
        }
    }

    private void InitializeRoomSession()
    {
        if (!string.IsNullOrEmpty(GameSession.RoomID))
        {
            currentRoomID = GameSession.RoomID;
        }
        else
        {
#if UNITY_EDITOR
            currentRoomID = "TEST01";
            Debug.LogWarning("[Pharmath-Dev] Menggunakan Dummy Room ID (TEST01).");
#else
            Debug.LogError("[Pharmath-Error] Akses ilegal! Room ID kosong.");
#endif
        }
    }

    void Update()
    {
        if (Input.touchCount == 0 && medicineStudio != null)
        {
            medicineStudio.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    private IEnumerator InitializeFirebaseListeners()
    {
        while (FirebaseManager.Instance == null || FirebaseManager.Instance.DBReference == null)
        {
            yield return null;
        }

        DatabaseReference rootRef = FirebaseManager.Instance.DBReference;

        caseRef = rootRef.Child("rooms").Child(currentRoomID).Child("gameplay").Child("current_case");
        diagnosisRef = rootRef.Child("rooms").Child(currentRoomID).Child("gameplay").Child("doctor_diagnosis");
        matchResultRef = rootRef.Child("rooms").Child(currentRoomID).Child("gameplay").Child("match_result");

        caseRef.ValueChanged += HandleCaseChanged;
        diagnosisRef.ValueChanged += HandleDiagnosisChanged;
        matchResultRef.ValueChanged += HandleMatchResultChanged;

        isListening = true;
    }

    private void HandleCaseChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null) return;

        if (args.Snapshot != null && args.Snapshot.Value != null)
        {
            currentActiveCase = args.Snapshot.Value.ToString();
            
            int caseIndex = int.Parse(currentActiveCase.Replace("case_", "")) - 1;
            if (caseIndex >= 0 && caseIndex < patientDatabase.Length)
            {
                activePatient = patientDatabase[caseIndex];
            }
        }
    }

    private void HandleDiagnosisChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null) return;

        if (args.Snapshot != null && args.Snapshot.Exists)
        {
            currentDoctorDiagnosis = args.Snapshot.Value.ToString();
            if (txtDiagnosis != null) txtDiagnosis.text = "Diagnosis: " + currentDoctorDiagnosis;
        }
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

        // --- BARU: Simpan status untuk direkap ---
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
            if (imgBtnAction != null && spriteBtnNext != null) imgBtnAction.sprite = spriteBtnNext;
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
            if (imgBtnAction != null && spriteBtnRetry != null) imgBtnAction.sprite = spriteBtnRetry;
        }
    }

    // --- BARU: Modifikasi total fungsi OnResultActionClicked ---
    private void OnResultActionClicked()
    {
        if (resultModalCanvas != null) resultModalCanvas.SetActive(false);
        if (doseInput != null) doseInput.text = "";

        // 1. Catat Skor ke dalam memori sementara
        if (lastResultStatus == "win") sessionSaved++;
        else if (lastResultStatus == "lose") sessionMalpractice++;

        // 2. Cek apakah ini adalah kasus terakhir dan berhasil disembuhkan
        if (currentActiveCase == finalCaseID && lastResultStatus == "win")
        {
            TriggerEndGameSequence();
        }
        else
        {
            // Jika belum selesai, buka lagi Canvas Prescribe
            if (canvasPrescribe != null) canvasPrescribe.SetActive(true);
        }
    }

    // --- BARU: Fungsi Trigger End Game ---
    private void TriggerEndGameSequence()
    {
        Debug.Log("[Pharmath] Seluruh Kasus Selesai! Mengkalkulasi Skor...");

        // 1. Hitung Rata-rata Waktu
        float totalTime = Time.time - sessionStartTime;
        float avgTime = totalTime / 5f; 

        // 2. Simpan Data ke PlayerPrefs
        PlayerPrefs.SetInt("TotalSaved", sessionSaved);
        PlayerPrefs.SetInt("TotalMalpractice", sessionMalpractice);
        PlayerPrefs.SetFloat("AverageTime", avgTime);
        PlayerPrefs.Save();

        // 3. Bersihkan Sesi Firebase
        if (GameSession.IsHost && !string.IsNullOrEmpty(currentRoomID) && FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.DBReference.Child("rooms").Child(currentRoomID).RemoveValueAsync();
        }

        // 4. Pindah ke Scene Summary
        SceneManager.LoadScene(summarySceneName);
    }

    public void NextMedicine()
    {
        if (medicineModels.Count == 0) return;
        currentIndex = (currentIndex + 1) % medicineModels.Count;
        UpdateDisplay();
    }

    public void PrevMedicine()
    {
        if (medicineModels.Count == 0) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = medicineModels.Count - 1;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (medicineModels.Count == 0) return;

        foreach (Transform child in medicineStudio)
        {
            if (child != null) child.gameObject.SetActive(false);
        }

        if (currentIndex >= 0 && currentIndex < medicineModels.Count)
        {
            GameObject modelTarget = medicineModels[currentIndex];
            if (modelTarget != null)
            {
                modelTarget.SetActive(true);
                // modelTarget.transform.localRotation = Quaternion.identity;
                if (medicineTitle != null) medicineTitle.text = modelTarget.name; 
            }
        }
        
        if (doseInput != null) doseInput.text = "";
    }

    private void OnPrescribeClicked()
    {
        string selectedMedicine = medicineNames[currentIndex];
        string inputDose = doseInput.text;

        if (string.IsNullOrEmpty(inputDose)) return;

        if (canvasPrescribe != null) canvasPrescribe.SetActive(false);

        CheckValidation(selectedMedicine, inputDose);
    }

    private void CheckValidation(string selectedMedicine, string inputDoseStr)
    {
        if (activePatient == null || !float.TryParse(inputDoseStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float playerDose))
        {
            return;
        }

        string correctDisease = "";
        string correctMedicine = "";

        switch (currentActiveCase)
        {
            case "case_01": correctDisease = "Demam"; correctMedicine = "Paracetamol"; break;
            case "case_02": correctDisease = "Diabetes"; correctMedicine = "Insulin"; break;
            case "case_03": correctDisease = "Asma"; correctMedicine = "Salbutamol"; break;
            case "case_04": correctDisease = "Infeksi"; correctMedicine = "Amoxicillin"; break;
            case "case_05": correctDisease = "Maag"; correctMedicine = "Antasida"; break;
            case "case_06": correctDisease = "Alergi"; correctMedicine = "Cetirizine"; break;
            case "case_07": correctDisease = "Batuk"; correctMedicine = "Sirup Ekspektoran"; break;
            case "case_08": correctDisease = "Inflamasi"; correctMedicine = "Ibuprofen"; break;
        }

        float targetSystemDose = CalculateTargetDose(activePatient, currentActiveCase);

        if (currentDoctorDiagnosis != correctDisease)
        {0
            SendResultToFirebase("lose", $"Dokter Salah Diagnosis! Pasien mengidap {correctDisease}, tapi didiagnosis {currentDoctorDiagnosis}.", correctDisease, correctMedicine, targetSystemDose);
            return;
        }

        if (selectedMedicine != correctMedicine)
        {
            SendResultToFirebase("lose", $"Apoteker Salah Obat! Harusnya diberikan {correctMedicine}, tapi diberikan {selectedMedicine}.", correctDisease, correctMedicine, targetSystemDose);
            return;
        }

        float difference = Mathf.Abs(playerDose - targetSystemDose);
        
        if (difference <= 0.5f)
        {
            SendResultToFirebase("win", $"Pasien Sembuh! Diagnosis {correctDisease} tepat, obat dan dosis {playerDose} mg akurat.", correctDisease, correctMedicine, targetSystemDose);
        }
        else
        {
            SendResultToFirebase("lose", $"Dosis Salah! Input apoteker: {playerDose} mg | Dosis aman seharusnya: {targetSystemDose} mg.", correctDisease, correctMedicine, targetSystemDose);
        }
    }

    private float CalculateTargetDose(PatientCaseData p, string caseID)
    {
        float baseDose = 0f;
        float finalDose = 0f;

        switch (caseID)
        {
            case "case_01":
                baseDose = (p.bmi * 1.5f) + (p.temperature * 2f) - 65f;
                if (p.temperature >= 38.5f && p.vas == 4) finalDose = (baseDose * 1.3f) + 2.5f;
                else if (p.temperature >= 38.5f && p.vas < 4) finalDose = baseDose + (p.hr * 0.15f);
                else if (p.temperature < 38.5f && p.bmi <= 20f) finalDose = baseDose * 0.85f;
                else finalDose = baseDose;
                break;
            case "case_02":
                baseDose = ((p.gds - 200f) / 10f) + (p.bmi * 0.5f) + (p.sbp / 100f);
                if (p.gds > 350) finalDose = (baseDose * 1.5f) + (p.bmi * 0.2f);
                else if (p.gds >= 300 && p.gds <= 350 && p.bmi > 32f) finalDose = (baseDose * 1.25f) + 5.5f;
                else if (p.gds < 300 && p.sbp > 150) finalDose = baseDose + (p.sbp / 50f);
                else finalDose = baseDose;
                break;
            case "case_03":
                baseDose = ((100f - p.spo2) * 2.5f) + (p.rr * 0.8f) + (p.bmi / 5f);
                if (p.spo2 < 90 && p.rr > 28) finalDose = (baseDose * 1.5f) + (p.hr * 0.1f);
                else if (p.spo2 < 90 && p.rr <= 28) finalDose = (baseDose * 1.3f) + 5.0f;
                else if (p.spo2 >= 90 && p.hr > 120) finalDose = baseDose + (p.vas * 1.2f);
                else finalDose = baseDose;
                break;
            case "case_04":
                baseDose = (p.wbc / 1000f) + (p.bmi * 1.5f) + (p.temperature * 2f) - 105f;
                if (p.wbc >= 17000 && p.temperature >= 39.5f) finalDose = (baseDose * 1.6f) + (p.rr * 0.5f);
                else if (p.wbc >= 17000 && p.temperature < 39.5f) finalDose = (baseDose * 1.4f) + 12.5f;
                else if (p.wbc < 17000 && p.vas >= 5) finalDose = baseDose + (p.hr / 10f);
                else finalDose = baseDose;
                break;
            case "case_05":
                baseDose = (p.vas * 4f) + (p.bmi * 0.8f) + (p.sbp / 20f);
                if (p.vas >= 9 && p.hr >= 105) finalDose = (baseDose * 1.4f) + 6.2f;
                else if (p.vas >= 9 && p.hr < 105) finalDose = (baseDose * 1.25f) + (p.rr * 0.2f);
                else if (p.vas < 9 && p.bmi <= 20f) finalDose = baseDose * 0.85f;
                else finalDose = baseDose;
                break;
            case "case_06":
                baseDose = (p.wbc / 1000f) + (p.bmi * 1.2f) + (p.temperature * 0.5f);
                if (p.wbc > 10000 && p.hr > 105) finalDose = (baseDose * 1.35f) + 3.8f;
                else if (p.wbc > 10000 && p.hr <= 105) finalDose = (baseDose * 1.2f) + (p.vas * 0.9f);
                else if (p.wbc <= 10000 && p.bmi > 23f) finalDose = baseDose + (p.sbp / 50f);
                else finalDose = baseDose;
                break;
            case "case_07":
                baseDose = (p.wbc / 1000f) + (p.rr * 1.5f) + (p.temperature * 2f) - 85f;
                if (p.wbc >= 11000 && p.spo2 < 95) finalDose = (baseDose * 1.4f) + (p.vas * 0.6f);
                else if (p.wbc >= 11000 && p.spo2 >= 95) finalDose = (baseDose * 1.2f) + 4.5f;
                else if (p.wbc < 11000 && p.rr > 20) finalDose = baseDose + (p.bmi / 10f);
                else finalDose = baseDose;
                break;
            case "case_08":
                baseDose = (p.vas * 5f) + (p.bmi * 0.7f) + (p.sbp / 25f);
                if (p.vas == 8 && p.bmi >= 28f) finalDose = (baseDose * 1.5f) + (p.bmi * 0.3f);
                else if (p.vas == 8 && p.bmi < 28f) finalDose = (baseDose * 1.25f) + 3.5f;
                else if (p.vas < 8 && p.sbp > 130) finalDose = baseDose + (p.vas * 0.8f);
                else finalDose = baseDose;
                break;
        }

        return (float)System.Math.Round(finalDose, 1);
    }

    private void SendResultToFirebase(string status, string message, string expectedDisease, string expectedMedicine, float expectedDose)
    {
        if (string.IsNullOrEmpty(currentRoomID) || FirebaseManager.Instance == null) return;

        DatabaseReference rootRef = FirebaseManager.Instance.DBReference;
        DatabaseReference matchRef = rootRef.Child("rooms").Child(currentRoomID).Child("gameplay").Child("match_result");
        
        matchRef.Child("status").SetValueAsync(status);
        matchRef.Child("message").SetValueAsync(message);
        
        matchRef.Child("expected_disease").SetValueAsync(expectedDisease);
        matchRef.Child("expected_medicine").SetValueAsync(expectedMedicine);
        matchRef.Child("expected_dose").SetValueAsync(expectedDose);
    }

    private void OnDestroy()
    {
        if (isListening)
        {
            if (caseRef != null) caseRef.ValueChanged -= HandleCaseChanged;
            if (diagnosisRef != null) diagnosisRef.ValueChanged -= HandleDiagnosisChanged;
            if (matchResultRef != null) matchResultRef.ValueChanged -= HandleMatchResultChanged;
            isListening = false;
        }
    }
}