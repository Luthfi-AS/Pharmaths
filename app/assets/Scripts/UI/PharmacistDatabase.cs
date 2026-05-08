using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class RuleUI
{
    public Image paramIcon;
    public TextMeshProUGUI paramVal;
    public TextMeshProUGUI resultVal;
}

public class PharmacistDatabase : MonoBehaviour
{
    [Header("Data Source")]
    public List<MedicineData> medicineList;
    private int currentIndex = 0;

    [Header("UI Navigation (Overlays)")]
    public GameObject canvasGuidebook; 
    public GameObject canvasPrescribe; 

    [Header("Header UI")]
    public Image imgIcon;
    public TextMeshProUGUI txtTitle;
    public TextMeshProUGUI txtCategory;
    public TextMeshProUGUI txtDescription;

    [Header("Base Dose UI")]
    public TextMeshProUGUI valBaseDose;

    [Header("Logic Rules UI")]
    public RuleUI[] uiRules = new RuleUI[3];
    public TextMeshProUGUI txtDefaultRule;

    private void Start()
    {
        // Menutup semua overlay saat game dimulai sesuai alur GDD [cite: 91]
        CloseGuidebook();
        ClosePrescribe();
    }

    // --- Overlay Management ---

    public void OpenGuidebook()
    {
        if (canvasGuidebook != null) canvasGuidebook.SetActive(true);
        if (canvasPrescribe != null) canvasPrescribe.SetActive(false);
        UpdatePharmacistUI();
    }

    public void CloseGuidebook()
    {
        if (canvasGuidebook != null) canvasGuidebook.SetActive(false);
    }

    public void OpenPrescribe()
    {
        if (canvasPrescribe != null) canvasPrescribe.SetActive(true);
        if (canvasGuidebook != null) canvasGuidebook.SetActive(false);
    }

    public void ClosePrescribe()
    {
        if (canvasPrescribe != null) canvasPrescribe.SetActive(false);
    }

    // --- Navigation Logic ---

    public void NextMedicine()
    {
        if (medicineList.Count == 0) return;
        currentIndex = (currentIndex + 1) % medicineList.Count;
        UpdatePharmacistUI();
    }

    public void PrevMedicine()
    {
        if (medicineList.Count == 0) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = medicineList.Count - 1;
        UpdatePharmacistUI();
    }

    private void UpdatePharmacistUI()
    {
        if (medicineList.Count == 0) return;

        MedicineData data = medicineList[currentIndex];

        if (imgIcon != null) imgIcon.sprite = data.medicineIcon;
        if (txtTitle != null) txtTitle.text = data.medicineTitle;
        if (txtCategory != null) txtCategory.text = data.medicineCategory;
        if (txtDescription != null) txtDescription.text = data.medicineDescription;
        if (valBaseDose != null) valBaseDose.text = data.baseDoseFormula;

        for (int i = 0; i < uiRules.Length; i++)
        {
            if (i < data.logicRules.Length)
            {
                if (uiRules[i].paramIcon != null)
                {
                    uiRules[i].paramIcon.gameObject.SetActive(true);
                    uiRules[i].paramIcon.sprite = data.logicRules[i].paramIcon;
                }
                if (uiRules[i].paramVal != null) uiRules[i].paramVal.text = data.logicRules[i].conditionText;
                if (uiRules[i].resultVal != null) uiRules[i].resultVal.text = data.logicRules[i].resultFormula;
            }
            else
            {
                if (uiRules[i].paramIcon != null) uiRules[i].paramIcon.gameObject.SetActive(false);
            }
        }

        if (txtDefaultRule != null) txtDefaultRule.text = data.defaultRuleLabel;
    }
}