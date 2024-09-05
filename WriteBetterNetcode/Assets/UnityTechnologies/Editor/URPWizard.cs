using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;
#if USE_URP
using UnityEngine.Rendering.Universal;
#endif

public class URPWizard : AssetPostprocessor
{
    [InitializeOnLoadMethod]
    static void OnInitialize()
    {
        URPCheck();
    }

    static void URPCheck()
    {
        if (GraphicsSettings.currentRenderPipeline != null) 
            return;
        
        var request = Client.List();
        while (!request.IsCompleted) { }

        if (request.Status != StatusCode.Success) 
            return;
        
        if (request.Result.All(info => info.name != "com.unity.render-pipelines.universal"))
        {
            var addRequest = Client.Add("com.unity.render-pipelines.universal");
            
            while (!addRequest.IsCompleted) { }
                    
            Client.Resolve();
        }
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (GraphicsSettings.currentRenderPipeline != null) 
            return;
        
        FindAndAssignPipeline();
    }

#if USE_URP
    static void FindAndAssignPipeline()
    {
        var existingPipelines = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");

        if (existingPipelines.Length == 0)
        {
            Debug.LogError($"Universal Render Pipeline Asset was not found.\n" +
                           $"Please create one and assign under the Project Settings > Graphics > Scriptable Render Pipeline Settings.");

            return;
        }
        
        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(AssetDatabase.GUIDToAssetPath(existingPipelines[0]));
        GraphicsSettings.defaultRenderPipeline = pipeline;
    }
#else
    static void FindAndAssignPipeline(){}
#endif
}
