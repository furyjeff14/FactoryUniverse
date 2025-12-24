#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class FactoryGraphWindow : EditorWindow
{
    private FactoryGraphView graphView;
    private FactoryGraphSO graphSO;

    [MenuItem("Factory/Graph Editor")]
    public static void OpenWindow()
    {
        FactoryGraphWindow window = GetWindow<FactoryGraphWindow>();
        window.titleContent = new GUIContent("Factory Graph Editor");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }

    private void ConstructGraphView()
    {
        graphView = new FactoryGraphView(this)
        {
            name = "Factory Graph"
        };
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        var loadButton = new Button(() =>
        {
            string path = EditorUtility.OpenFilePanel("Load Factory Graph", "Assets", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
                graphSO = AssetDatabase.LoadAssetAtPath<FactoryGraphSO>(path);
                graphView.LoadGraph(graphSO);
            }
        })
        { text = "Load Graph" };

        var saveButton = new Button(() =>
        {
            if (graphSO != null)
            {
                EditorUtility.SetDirty(graphSO);
                AssetDatabase.SaveAssets();
            }
        })
        { text = "Save Graph" };

        toolbar.Add(loadButton);
        toolbar.Add(saveButton);
        rootVisualElement.Add(toolbar);
    }

    public FactoryGraphSO GetGraphSO() => graphSO;
}
#endif
