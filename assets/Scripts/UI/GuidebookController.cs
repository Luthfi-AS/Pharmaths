using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Pharmath.Core.Data; // Pastikan namespace sesuai dengan refactor sebelumnya

public class GuidebookController : MonoBehaviour
{
    [Header("Data Source")]
    public List<DiseaseData> diseaseList; 
    private int currentIndex = 0;

    [Header("Identity UI")]
    public TextMeshProUGUI titleTxt;
    public TextMeshProUGUI descTxt;
    public UnityEngine.UI.Image diseaseIcon; // TAMBAHAN: Referensi untuk Img_Icon
    public UnityEngine.UI.Image urgencyIndicator; 

    [Header("Vitals UI")]
    public TextMeshProUGUI dbpTxt;
    public TextMeshProUGUI sbpTxt;
    public TextMeshProUGUI tempTxt;
    public TextMeshProUGUI rrTxt;
    public TextMeshProUGUI spo2Txt;
    public TextMeshProUGUI hrTxt;

    [Header("Lab & Assessment UI")]
    public TextMeshProUGUI gdsTxt;
    public TextMeshProUGUI wbcTxt;
    public TextMeshProUGUI vasTxt;
    public TextMeshProUGUI bmiTxt;

    public void OpenGuidebook()
    {
        gameObject.SetActive(true); // Memunculkan overlay
        UpdateUI(); // Pastikan data ter-update saat dibuka
    }

    public void CloseGuidebook()
    {
        gameObject.SetActive(false); // Menyembunyikan overlay
    }

    void Start() 
    {
        if (diseaseList.Count > 0) UpdateUI();
    }

    public void NextPage()
    {
        if (diseaseList.Count == 0) return;
        currentIndex = (currentIndex + 1) % diseaseList.Count;
        UpdateUI();
    }

    public void PrevPage()
    {
        if (diseaseList.Count == 0) return;
        currentIndex = (currentIndex - 1 + diseaseList.Count) % diseaseList.Count;
        UpdateUI();
    }

    void UpdateUI()
    {
        DiseaseData data = diseaseList[currentIndex];
        
        // Identity
        titleTxt.text = data.diseaseName;
        descTxt.text = data.description;
        
        // Update Ikon (TAMBAHAN)
        if (diseaseIcon != null && data.icon != null)
        {
            diseaseIcon.sprite = data.icon;
        }
        
        // Update Warna Urgensi
        UpdateUrgencyVisual(data.urgency);
        
        // Vitals - Diakses melalui struct .vitals
        dbpTxt.text = data.vitals.dbp;
        sbpTxt.text = data.vitals.sbp;
        tempTxt.text = data.vitals.temperature;
        rrTxt.text = data.vitals.respRate;
        spo2Txt.text = data.vitals.spo2;
        hrTxt.text = data.vitals.heartRate;

        // Lab - Diakses melalui struct .labResults
        gdsTxt.text = data.labResults.gds;
        wbcTxt.text = data.labResults.wbc;
        vasTxt.text = data.labResults.vas;
        bmiTxt.text = data.labResults.bmi;
    }

    void UpdateUrgencyVisual(UrgencyLevel urgency)
    {
        if (urgencyIndicator == null) return;

        switch (urgency)
        {
            case UrgencyLevel.Low: urgencyIndicator.color = Color.green; break;
            case UrgencyLevel.Medium: urgencyIndicator.color = Color.yellow; break;
            case UrgencyLevel.High: urgencyIndicator.color = new Color(1f, 0.5f, 0f); break; // Orange
            case UrgencyLevel.Critical: 
            case UrgencyLevel.Emergency: urgencyIndicator.color = Color.red; break;
        }
    }
}