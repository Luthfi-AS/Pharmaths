using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

    private int currentIndex = 0;

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
        // Mengaktifkan model yang dipilih dan mematikan sisanya
        for (int i = 0; i < medicineModels.Count; i++)
        {
            if (medicineModels[i] != null)
            {
                medicineModels[i].SetActive(i == currentIndex);
            }
        }

        // Update teks judul obat
        if (medicineTitle != null && currentIndex < medicineNames.Length)
        {
            medicineTitle.text = medicineNames[currentIndex];
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
        // Jika benar, lanjut ke pasien berikutnya.
        Debug.Log("Memvalidasi tindakan medis...");
    }
}