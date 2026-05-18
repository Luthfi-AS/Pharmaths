using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;

public class PrescribeManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text medicineTitle;
    [SerializeField] private TMP_InputField doseInput;
    [SerializeField] private Button prescribeBtn;

    [Header("3D Model References")]
    [SerializeField] private Transform medicineStudio; 
    [SerializeField] private List<GameObject> medicineModels;
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private TextMeshProUGUI txtDiagnosis; // Menampilkan diagnosis dokter

    private int currentIndex = 0;
    private DatabaseReference diagnosisRef;
    private bool isListening = false;

    // Nama obat sesuai urutan model di list
    private string[] medicineNames = { 
        "Paracetamol", "Amoxicillin", "Salbutamol", 
        "Insulin", "Antasida", "Cetirizine", 
        "Sirup Ekspektoran", "Ibuprofen" 
    };

    void Start()
    {
        UpdateDisplay();
        if (prescribeBtn != null)
        {
            prescribeBtn.onClick.AddListener(OnPrescribeClicked);
        }

        StartCoroutine(WaitForFirebaseAndListen());
    }

    void Update()
    {
        // Membuat model obat berputar otomatis sesuai arah GDD
        if (medicineStudio != null)
        {
            medicineStudio.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
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

        // Mematikan SEMUA child di bawah medicineStudio
        foreach (Transform child in medicineStudio)
        {
            if (child != null) 
            {
                child.gameObject.SetActive(false);
            }
        }

        // Mengaktifkan objek yang berada di index yang sedang dipilih
        if (currentIndex >= 0 && currentIndex < medicineModels.Count)
    {
        GameObject modelTarget = medicineModels[currentIndex];
        
        if (modelTarget != null)
        {
            modelTarget.SetActive(true);
            
            // Mengambil teks langsung dari nama objek di Hierarchy!
            if (medicineTitle != null)
            {
                medicineTitle.text = modelTarget.name; 
            }
            
            Debug.Log("[PrescribeManager] Menampilkan model: " + modelTarget.name);
        }
    }
        
        // Reset input dosis setiap ganti obat
        if (doseInput != null)
        {
            doseInput.text = "";
        }
    }

    private void OnPrescribeClicked()
    {
        string selectedMedicine = medicineNames[currentIndex];
        string inputDose = doseInput.text;

        if (string.IsNullOrEmpty(inputDose))
        {
            Debug.Log("Dosis tidak boleh kosong!");
            return;
        }

        Debug.Log("Apoteker mengirim: " + selectedMedicine + " dengan dosis " + inputDose + " mg");
        
        CheckValidation(selectedMedicine, inputDose);
    }

    private void CheckValidation(string medicine, string dose)
    {
        // Logika Game: Jika salah pilih atau dosis tidak tepat, status Malapraktik.
        // Jika benar, lanjut ke pasien berikutnya. (Ongoing)
        Debug.Log("Memvalidasi tindakan medis...");
    }

    // Fungsi untuk menunggu Firebase siap dan mulai listening perubahan diagnosa dokter
    private IEnumerator WaitForFirebaseAndListen()
    {
        while (FirebaseManager.Instance == null || FirebaseManager.Instance.DBReference == null)
        {
            yield return null;
        }

        if (string.IsNullOrEmpty(GameSession.RoomID))
        {
            Debug.LogError("RoomID belum tersedia, tidak dapat mendengarkan diagnosa.");
            yield break;
        }

        diagnosisRef = FirebaseManager.Instance.DBReference
            .Child("rooms")
            .Child(GameSession.RoomID)
            .Child("doctorDiagnosis");

        diagnosisRef.ValueChanged += HandleDiagnosisChanged;
        isListening = true;

        yield break;
    }

    private void HandleDiagnosisChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null) return;

        if (args.Snapshot.Exists)
        {
            string diagnosis = args.Snapshot.Value.ToString();
            // tampilkan diagnosis di UI
            if (txtDiagnosis != null)
            {
                txtDiagnosis.text = "Diagnosis: " + diagnosis;
            }
            Debug.Log("Diagnosa diterima: " + diagnosis);
        }
        else if (txtDiagnosis != null)
        {
            txtDiagnosis.text = "Diagnosis belum diterima";
        }
    }

    private void OnDestroy()
    {
        if (diagnosisRef != null && isListening)
        {
            diagnosisRef.ValueChanged -= HandleDiagnosisChanged;
            isListening = false;
        }
    }
}