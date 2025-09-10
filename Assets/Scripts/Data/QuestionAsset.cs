using System;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

public enum LegalLabel
{
    [InspectorName("Hợp pháp")] HopPhap,
    [InspectorName("Lừa đảo")] LuaDao
}

[Serializable]
public class QuestionAsset
{
    // ---- Nội dung chính ----
    
    [TextArea(2, 6)]
    [XmlElement("questionText")]
    public string questionText;

    [XmlElement("label")]
    public LegalLabel label;              // sẽ ghi "HopPhap" hoặc "LuaDao"

    // Đảm bảo ra đúng <reasons><string>...</string></reasons>
    [TextArea(2, 8)]
    [XmlArray("reasons")]
    [XmlArrayItem("string")]
    public string[] reasons = Array.Empty<string>();

    [XmlElement("category")]
    public string category;

    // ---- Helper (không ghi vào XML) ----
    [XmlIgnore]
    public bool IsLegal => label == LegalLabel.HopPhap;

    [XmlIgnore]
    public string LabelText => IsLegal ? "HỢP PHÁP" : "LỪA ĐẢO";

    // Cho code cũ nếu còn dùng: true = Hợp pháp, false = Lừa đảo (không serialize)
    [XmlIgnore]
    public bool correctAnswer
    {
        get => IsLegal;
        set => label = value ? LegalLabel.HopPhap : LegalLabel.LuaDao;
    }

    // ---- Ghi/đọc IsLegal ra XML mới ----
    // Sẽ tạo <IsLegal>true/false</IsLegal>
    [XmlElement("IsLegal")]
    public bool IsLegalXml
    {
        get => IsLegal;
        set => label = value ? LegalLabel.HopPhap : LegalLabel.LuaDao; // cho phép DESERIALIZE
    }

    // ---- Dữ liệu legacy cho Unity/XML cũ ----
    [FormerlySerializedAs("correctAnswer")]
    [SerializeField, HideInInspector] private bool _isLegalLegacy = true;

    // Map phần tử <correctAnswer> của XML cũ
    [XmlElement("correctAnswer")]
    public bool LegacyCorrectAnswerXml
    {
        get => correctAnswer;                 // nếu bị ép serialize vẫn đúng
        set { _isLegalLegacy = value; _hasLegacyFromXml = true; }
    }
    // Không ghi lại trong XML mới
    public bool ShouldSerializeLegacyCorrectAnswerXml() => false;

    [XmlIgnore] private bool _hasLegacyFromXml = false;
    [XmlIgnore] private bool _migrated = false;

    // Gọi sau khi load (từ Unity hoặc từ XmlSerializer)
    public void MigrateIfNeeded()
    {
        if (_migrated) return;

        // Chỉ override khi đọc từ XML/serialize cũ có <correctAnswer>
        if (_hasLegacyFromXml)
            label = _isLegalLegacy ? LegalLabel.HopPhap : LegalLabel.LuaDao;

        _migrated = true;
    }
}
