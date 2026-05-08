using UnityEngine;

[System.Serializable]
public class MedicineRule
{
    public Sprite paramIcon;
    public string conditionText;
    public string resultFormula;
}

[CreateAssetMenu(fileName = "NewMedicineData", menuName = "Pharmath/Medicine Data")]
public class MedicineData : ScriptableObject
{
    [Header("Header Area")]
    public string medicineTitle;
    public string medicineCategory;
    public Sprite medicineIcon;
    
    [TextArea(3, 5)]
    public string medicineDescription;

    [Header("Base Dose Section")]
    public string baseDoseFormula;

    [Header("Logic Rules Section")]
    public MedicineRule[] logicRules = new MedicineRule[3];
    public string defaultRuleLabel;
}