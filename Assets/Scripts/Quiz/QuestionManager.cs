using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System;
using UnityEngine;
using System.Linq;

public class QuestionManager : MonoBehaviour
{
    [SerializeField] private int totalRounds = 60;   // Số câu muốn chơi

    private List<QuestionAsset> _deck;
    private int _index = -1;

    public int CurrentIndex => _index + 1;
    public int TotalRounds => Mathf.Min(totalRounds, _deck?.Count ?? 0);

    private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(Data));

    void Awake()
    {
        _deck = new List<QuestionAsset>();

        // Load tất cả file XML như TextAsset trong Assets/Resources/Questions
        TextAsset[] files = Resources.LoadAll<TextAsset>("Questions");
        if (files == null || files.Length == 0)
        {
            Debug.LogError("Không tìm thấy file XML nào trong Resources/Questions");
            return;
        }

        // Đọc tất cả các file XML chỉ 1 lần
        foreach (var ta in files)
        {
            try
            {
                using (var reader = new StringReader(ta.text))
                {
                    var data = (Data)_serializer.Deserialize(reader);
                    if (data?.Questions != null)
                    {
                        foreach (var q in data.Questions)
                        {
                            q?.MigrateIfNeeded(); // Nếu có logic migrate bool -> enum
                            _deck.Add(q);
                        }
                    }
                    Debug.Log($"Đã tải {data?.Questions.Length ?? 0} câu hỏi từ {ta.name}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Lỗi khi đọc XML '{ta.name}': {ex.Message}");
            }
        }

        // Trộn bộ câu hỏi (Fisher–Yates) và cắt đúng số lượng cần
        for (int i = 0; i < _deck.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, _deck.Count);
            (_deck[i], _deck[j]) = (_deck[j], _deck[i]);
        }

        // Cắt bộ câu hỏi nếu quá số lượng câu hỏi yêu cầu
        if (_deck.Count > totalRounds)
        {
            _deck = _deck.Take(totalRounds).ToList();  // Cập nhật lại với ToList()
        }

        _index = -1;  // Đảm bảo bắt đầu từ câu hỏi đầu tiên
    }

    public bool HasNext() => (_index + 1) < (_deck?.Count ?? 0);

    public QuestionAsset NextQuestion()
    {
        if (!HasNext()) return null;
        _index++;
        return _deck[_index];
    }
}
