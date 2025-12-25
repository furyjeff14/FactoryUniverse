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
        // Subscribe to new graph
        if(graphSO != null)
            graphSO.OnGraphChanged += OnGraphChanged;
    }

    private void OnDisable()
    {
        if (graphView != null)
        {
            rootVisualElement.Remove(graphView);
        }
        if(graphSO != null)
            graphSO.OnGraphChanged -= OnGraphChanged;
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

    private void OnGraphChanged()
    {
        if (graphView != null && graphSO != null)
        {
            // DelayCall avoids repaint conflicts
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                    graphView.ReloadGraph(graphSO);
            };
        }
    }

    private void OnSelectionChange()
    {
        if (Selection.activeObject is FactoryGraphSO graph)
        {
            LoadGraph(graph);
            Repaint();
        }
    }

    public void LoadGraph(FactoryGraphSO graph)
    {
        if (graph == null)
            return;

        graphSO = graph;
        graphView.ReloadGraph(graph);

        // Save path to EditorPrefs
        string path = AssetDatabase.GetAssetPath(graphSO);
        EditorPrefs.SetString("FactoryGraph_LastGraphPath", path);
    }

    // ────────────── Load all nodes from a folder ──────────────
    private void LoadAllNodesFromFolder()
    {
        if (graphSO == null)
        {
            Debug.LogWarning("No graph assigned!");
            return;
        }

        string folderPath = EditorUtility.OpenFolderPanel("Select Folder with GraphNodeSO Assets", "Assets", "");
        if (string.IsNullOrEmpty(folderPath))
            return;

        if (folderPath.StartsWith(Application.dataPath))
            folderPath = "Assets" + folderPath.Substring(Application.dataPath.Length);

        string[] guids = AssetDatabase.FindAssets("t:GraphNodeSO", new[] { folderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var nodeSO = AssetDatabase.LoadAssetAtPath<GraphNodeSO>(path);
            if (nodeSO != null && !graphSO.nodes.Contains(nodeSO))
            {
                graphView.AddNode(nodeSO);
                EditorUtility.SetDirty(graphSO);
            }
        }

        AssetDatabase.SaveAssets();
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        // Load previous graph button
        var loadPrevBtn = new ToolbarButton(() =>
        {
            string path = EditorPrefs.GetString("FactoryGraph_LastGraphPath", "");
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("No previous Factory Graph found.");
                return;
            }

            var graph = AssetDatabase.LoadAssetAtPath<FactoryGraphSO>(path);
            if (graph == null)
            {
                Debug.LogWarning("Previous Factory Graph could not be loaded. It might have been deleted or moved.");
                return;
            }

            graphView.ReloadGraph(graph);
        })
        {
            text = "Load Previous Graph"
        };

        var loadButton = new Button(() =>
        {
            string path = EditorUtility.OpenFilePanel("Load Factory Graph", "Assets", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
                graphSO = AssetDatabase.LoadAssetAtPath<FactoryGraphSO>(path);
                graphView.ReloadGraph(graphSO);
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

        var loadAllBtn = new Toolbar();

        var loadBtn = new ToolbarButton(() =>
        {
            LoadAllNodesFromFolder();
        })
        { text = "Load Nodes From Folder" };

        toolbar.Add(loadAllBtn);
        toolbar.Add(loadBtn);
        toolbar.Add(loadPrevBtn);
        toolbar.Add(loadButton);
        toolbar.Add(saveButton);
        rootVisualElement.Add(toolbar);
    }

    public FactoryGraphSO GetGraphSO() => graphSO;
}
#endif
