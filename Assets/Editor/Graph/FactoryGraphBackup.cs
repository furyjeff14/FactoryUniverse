using UnityEngine;
using UnityEditor;
using System.IO;

public static class FactoryGraphBackup
{
    private const string BackupFolder = "Assets/TempGraphBackups";

    public static FactoryGraphSO CreateBackup(FactoryGraphSO originalGraph)
    {
        if (originalGraph == null)
        {
            Debug.LogWarning("Cannot create backup, graph is null!");
            return null;
        }

        if (!Directory.Exists(BackupFolder))
            Directory.CreateDirectory(BackupFolder);

        string backupPath = $"{BackupFolder}/{originalGraph.name}_Backup_{System.DateTime.Now:yyyyMMdd_HHmmss}.asset";

        // Duplicate original graph
        FactoryGraphSO backup = ScriptableObject.Instantiate(originalGraph);
        backup.name = originalGraph.name + "_Backup";

        // Save the asset
        AssetDatabase.CreateAsset(backup, backupPath);

        // Duplicate nodes
        foreach (var node in originalGraph.nodes)
        {
            var nodeCopy = ScriptableObject.Instantiate(node);
            nodeCopy.name = node.name;
            AssetDatabase.AddObjectToAsset(nodeCopy, backup);
        }

        // Duplicate edges
        foreach (var edge in originalGraph.edges)
        {
            var edgeCopy = ScriptableObject.Instantiate(edge);
            AssetDatabase.AddObjectToAsset(edgeCopy, backup);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Backup created: {backupPath}");
        return backup;
    }

    public static void ClearBackups()
    {
        if (!Directory.Exists(BackupFolder)) return;

        var files = Directory.GetFiles(BackupFolder, "*.asset");
        foreach (var f in files)
        {
            AssetDatabase.DeleteAsset(f);
        }

        AssetDatabase.Refresh();
        Debug.Log("All temporary graph backups deleted.");
    }
}
