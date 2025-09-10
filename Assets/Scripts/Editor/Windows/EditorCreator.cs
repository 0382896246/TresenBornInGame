using System.IO;
using UnityEditor;
using UnityEngine;

public class EditorCreator : EditorWindow
{
    [SerializeField] private Data data = new Data();

    private SerializedObject _so;
    private SerializedProperty _questionsProp;
    private Vector2 _scroll = Vector2.zero;

    // Đường dẫn dạng "Assets/Resources/Questions/questions.xml"
    private string _assetPath;

    [MenuItem("Game/Data Creator")]
    public static void OpenWindow()
    {
        var w = GetWindow<EditorCreator>("Creator");
        w.minSize = new Vector2(510f, 344f);
        w.Show();
    }

    private void OnEnable()
    {
        if (data == null) data = new Data();
        if (data.Questions == null) data.Questions = System.Array.Empty<QuestionAsset>();
        RebindSO();
    }

    private void RebindSO()
    {
        _so = new SerializedObject(this);
        _questionsProp = _so.FindProperty("data").FindPropertyRelative("Questions");
    }

    // --- helper: tạo đường dẫn duy nhất trong Assets ---
    private static string GetUniquePathInProject(string wantedPath)
    {
        if (string.IsNullOrEmpty(wantedPath)) return wantedPath;
        return AssetDatabase.GenerateUniqueAssetPath(wantedPath); // "x.xml" -> "x 1.xml" nếu trùng
    }

    private void OnGUI()
    {
        // ALWAYS update trước khi vẽ
        _so.Update();

        #region header section
        Rect headerRect = new Rect(15, 15, position.width - 30, 65);
        GUI.Box(headerRect, GUIContent.none);
        GUIStyle headerStyle = new GUIStyle(EditorStyles.largeLabel) { fontSize = 26, alignment = TextAnchor.UpperLeft };
        var hr = new Rect(headerRect.x + 5, headerRect.y + 5, headerRect.width - 10, headerRect.height - 10);
        GUI.Label(hr, "Data to XML Creator", headerStyle);
        Rect summaryRect = new Rect(hr.x + 25, (hr.y + hr.height) - 20, hr.width - 50, 15);
        GUI.Label(summaryRect, "Create the data that needs to be included into the XML file");
        #endregion

        #region body section
        Rect bodyRect = new Rect(15, (headerRect.y + headerRect.height) + 20,
                                 position.width - 30,
                                 position.height - (headerRect.y + headerRect.height) - 80);
        GUI.Box(bodyRect, GUIContent.none);

        float propHeight = EditorGUI.GetPropertyHeight(_questionsProp, includeChildren: true);
        Rect viewRect = new Rect(bodyRect.x + 10, bodyRect.y + 10, bodyRect.width - 20, propHeight);
        Rect scrollRect = new Rect(viewRect) { height = bodyRect.height - 20 };

        _scroll = GUI.BeginScrollView(scrollRect, _scroll, viewRect, false, true, GUIStyle.none, GUI.skin.verticalScrollbar);

        bool drawSlider = viewRect.height > scrollRect.height;
        Rect propertyRect = new Rect(bodyRect.x + 10, bodyRect.y + 10, bodyRect.width - (drawSlider ? 40 : 20), 17);
        EditorGUI.PropertyField(propertyRect, _questionsProp, includeChildren: true);

        GUI.EndScrollView();
        #endregion

        // APPLY sau khi vẽ xong
        _so.ApplyModifiedProperties();

        #region navigation (Create / Fetch)
        Rect buttonRect = new Rect(bodyRect.x + bodyRect.width - 85, bodyRect.y + bodyRect.height + 15, 85, 30);

        // CREATE
        if (GUI.Button(buttonRect, "Create", EditorStyles.miniButtonRight))
        {
            // luôn gợi ý lưu trong Assets/Resources/Questions
            string defaultDir = "Assets/Resources/Questions";
            if (!Directory.Exists(defaultDir)) Directory.CreateDirectory(defaultDir);

            // nếu chưa có đường dẫn, hỏi người dùng
            if (string.IsNullOrEmpty(_assetPath))
            {
                _assetPath = EditorUtility.SaveFilePanelInProject(
                    "Save XML",
                    string.IsNullOrEmpty(GameUtility.FileName) ? "questions" : GameUtility.FileName,
                    "xml",
                    "Chọn nơi lưu trong Assets",
                    defaultDir
                );
                if (string.IsNullOrEmpty(_assetPath)) return; // user cancel
            }

            // Bắt buộc tạo đường dẫn duy nhất (không overwrite)
            _assetPath = GetUniquePathInProject(_assetPath);

            // Ghi file theo đường dẫn tuyệt đối
            string abs = Path.GetFullPath(_assetPath);
            Data.Write(data, abs);

            // Import vào Project + highlight
            AssetDatabase.ImportAsset(_assetPath);
            AssetDatabase.SaveAssets();
            var ta = AssetDatabase.LoadAssetAtPath<TextAsset>(_assetPath);
            if (ta) EditorGUIUtility.PingObject(ta);

            // chuẩn bị cho lần create tiếp theo: tiếp tục sinh tên mới thay vì ghi đè
            _assetPath = GetUniquePathInProject(_assetPath);
        }


        // FETCH
        buttonRect.x -= buttonRect.width;
        if (GUI.Button(buttonRect, "Fetch", EditorStyles.miniButtonLeft))
        {
            string pick = EditorUtility.OpenFilePanel("Select XML", Application.dataPath, "xml"); // absolute path
            if (!string.IsNullOrEmpty(pick))
            {
                var d = Data.Fetch(out bool ok, pick);
                if (ok && d != null)
                {
                    data = d;           // thay ref -> cần rebind
                    RebindSO();
                    Repaint();
                }
            }
        }
        #endregion
    }
}
