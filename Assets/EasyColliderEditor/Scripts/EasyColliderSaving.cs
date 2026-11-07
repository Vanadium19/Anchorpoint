#if(UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;

namespace ECE
{
  public static class EasyColliderSaving
  {

    static UnityEngine.Object TryFindPrefabObject(GameObject go)
    {
      UnityEngine.Object foundObject = null;
#if UNITY_2018_2_OR_NEWER
      // 2018_2+
      foundObject = PrefabUtility.GetCorrespondingObjectFromSource(go);
#if (UNITY_2021_2_OR_NEWER)
      // can get prefab stage in 2021+
      if (foundObject == null)
      {
        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null)
        {
          GameObject stagedObject = stage.openedFromInstanceObject;
          if (stagedObject!=null) {
                    foundObject = PrefabUtility.GetCorrespondingObjectFromSource(stagedObject);
          }
        }
      }
#endif
#else
      foundObject = PrefabUtility.GetPrefabParent(go);
      if (foundObject == null)
      {
        foundObject = PrefabUtility.FindPrefabRoot(go);
      }
#endif
      return foundObject;
    }

    static UnityEngine.Object TryFindMeshOrSkinnedMeshObject(GameObject go)
    {
      UnityEngine.Object foundObject = null;
      MeshFilter mf = go.GetComponent<MeshFilter>();
      if (mf != null)
      {
        foundObject = mf.sharedMesh;
      }
      else
      {
        SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
        if (smr != null)
        {
          foundObject = smr.sharedMesh;
        }
      }
      return foundObject;
    }

    /// <summary>
    /// Static preferences asset that is currently loaded.
    /// </summary>
    /// <value></value>
    static EasyColliderPreferences ECEPreferences { get { return EasyColliderPreferences.Preferences; } }

    /// <summary>
    /// Gets a valid path to save a convex hull at to feed into save convex hull meshes function.
    /// </summary>
    /// <param name="go">selected gameobject</param>
    /// <param name="ECEPreferences">preferences object</param>
    /// <returns>full save path (ie C:/UnityProjects/ProjectName/Assets/Folder/baseObject)</returns>
    public static string GetValidConvexHullPath(GameObject go)
    {
      // use default specified path
      // remove invalid characters from file name, just in case (user reported error, thanks!)
      string goName = string.Join("_", go.name.Split(Path.GetInvalidFileNameChars()));
      // start with the default path specified in preferences.
      string path = ECEPreferences.SaveConvexHullPath;
      // get path to gameobject
      if (ECEPreferences.ConvexHullSaveMethod != CONVEX_HULL_SAVE_METHOD.Folder)
      {
        // bandaid for scaled temporary skinned mesh:
        // as the scaled mesh filter is added during setup with the name Scaled Mesh Filter (Temporary)
        if (go.name.Contains("Scaled Mesh Filter"))
        {
          go = go.transform.parent.gameObject; // set the gameobject to the temp's parent (as that will be a part of the prefab if it is one and thus should work.)
        }

        UnityEngine.Object foundObject = null;

        if (ECEPreferences.ConvexHullSaveMethod == CONVEX_HULL_SAVE_METHOD.Prefab)
        {
          foundObject = TryFindPrefabObject(go);
        }
        else if (ECEPreferences.ConvexHullSaveMethod == CONVEX_HULL_SAVE_METHOD.Mesh)
        {
          foundObject = TryFindMeshOrSkinnedMeshObject(go);
        }
        else if (ECEPreferences.ConvexHullSaveMethod == CONVEX_HULL_SAVE_METHOD.PrefabMesh)
        {
          foundObject = TryFindPrefabObject(go);
          if (foundObject == null)
          {
            foundObject = TryFindMeshOrSkinnedMeshObject(go);
          }
        }
        else if (ECEPreferences.ConvexHullSaveMethod == CONVEX_HULL_SAVE_METHOD.MeshPrefab)
        {
          foundObject = TryFindMeshOrSkinnedMeshObject(go);
          if (foundObject == null)
          {
            foundObject = TryFindPrefabObject(go);
          }
        }
        string altPath = AssetDatabase.GetAssetPath(foundObject);
        // but only use the path it if it exists.
        if (altPath != null && altPath != "" && foundObject != null)
        {
          int index = altPath.LastIndexOf("/");
          if (index >= 0)
          {
            string foundObjectName = string.Join("_", foundObject.name.Split(Path.GetInvalidFileNameChars()));
            // removes object name.
            path = altPath.Remove(index + 1);
            // if they want a subfolder, create it if needed and use it.
            if (!string.IsNullOrEmpty(ECEPreferences.SaveConvexHullSubFolder))
            {
              if (!AssetDatabase.IsValidFolder(path + ECEPreferences.SaveConvexHullSubFolder))
              {
                // path still has trailing '/' which needs to be removed for create folder.
                AssetDatabase.CreateFolder(path.Remove(path.Length - 1), ECEPreferences.SaveConvexHullSubFolder);
              }
              path += ECEPreferences.SaveConvexHullSubFolder + "/";
            }
          }
        }
      }
      string originalPath = path;
      string pathFallback = path;
      //remove any invalid path characters.
      path = string.Join("", path.Split(Path.GetInvalidPathChars()));

      // prefab/mesh searched for a folder to save in and failed to find a valid path, default to the save convex hull path specified in preferences.
      if (!AssetDatabase.IsValidFolder(path) && originalPath != ECEPreferences.SaveConvexHullPath)
      {
        pathFallback = ECEPreferences.SaveConvexHullPath;
        path = pathFallback;
        path = string.Join("", path.Split(Path.GetInvalidPathChars()));
        // could not find a valid mesh/prefab location, but the fallback save convex hull folder DOES work.
        if (AssetDatabase.IsValidFolder(path))
        {
          Debug.LogWarning("Easy Collider Editor: Could not find a valid location to save the collider. Saving in the folder specified in preferences: " + pathFallback + ".\n\nPath originally found was:" + originalPath);
        }
      }
      // saving in a non-asset path and does not have allow saving convex hulls in packages enabled
      if ((!path.StartsWith("Assets") && !ECEPreferences.AllowSavingConvexHullsInPackages))
      {
        path = ECEPreferences.SaveConvexHullPath;
        pathFallback = path;
      }
      // folder path is saved with / at the end, so remove if we have to.
      if (path[path.Length - 1] == '/')
      {
        path = path.Remove(path.Length - 1);
      }
      // this will automatically be true if we're using a folder (and it failed) OR the fallback on prefab/mesh fails (which also uses SaveConvexHullPath)
      if ((!AssetDatabase.IsValidFolder(path) && pathFallback == ECEPreferences.SaveConvexHullPath))
      {
        // path to ece preferences (in scripts typically)
        path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(ECEPreferences));
        // because the above is the full path to the file including the files name.
        path = path.Remove(path.LastIndexOf("/"));
        path = path.Replace("/Scripts", "");
        path = string.Join("", path.Split(Path.GetInvalidPathChars()));
        //  asset path for preferences file is invalid, not in assets folder a
        if (path.StartsWith("Assets") && !ECEPreferences.AllowSavingConvexHullsInPackages)
        {
          if (!AssetDatabase.IsValidFolder(path + "/Convex Hulls"))
          {
            AssetDatabase.CreateFolder(path, "Convex Hulls");
            AssetDatabase.Refresh();
          }
          path = path + "/Convex Hulls";
          if (originalPath == ECEPreferences.SaveConvexHullPath)
          {
            // original path was saving to a folder.
            ECEPreferences.SaveConvexHullPath = path + "/";
            Debug.LogWarning("Easy Collider Editor: Convex Hull save path specified in Easy Collider Editor's preferences could not be found, or it is an invalid asset folder. Saving in: " + path + " as a fallback and updating preferences. If the folder has been moved or deleted, update to a different folder in the edit preferences foldout.\n\n Original path: " + originalPath);
          }
          // so that if we already have a valid 
          else if (!ECEPreferences.SaveConvexHullPath.StartsWith("Assets"))
          {
            // used an alternative mesh/prefab/prefabmesh save method.
            ECEPreferences.SaveConvexHullPath = path + "/";
            Debug.LogWarning("Easy Collider Editor: Could not find a valid location to save the collider and the save path specified in preferences could not be found, or it is an invalid asset folder. Saving in: " + path + " and updated preferences to this folder. If the folder has been moved or deleted, update to a different folder in the edit preferences foldout.\n\n Original path: " + originalPath);
          }
        }
        else // in a packages folder probably without AllowSavingConvexHullsInPackages enabled.
        {
          // final fallback.
          pathFallback = "Assets/Convex Hulls";
          path = pathFallback;
          if (originalPath == ECEPreferences.SaveConvexHullPath)
          {
            // change default path to a valid path.
            ECEPreferences.SaveConvexHullPath = pathFallback + "/";
#if (UNITY_2020_3_OR_NEWER)
            AssetDatabase.SaveAssetIfDirty(ECEPreferences);
#endif
          }
          if (!AssetDatabase.IsValidFolder(path))
          {
            AssetDatabase.CreateFolder("Assets", "Convex Hulls");
            AssetDatabase.Refresh();
            if (originalPath == ECEPreferences.SaveConvexHullPath)
            {
              // original path was saving to a folder.
              Debug.LogWarning("Easy Collider Editor: Convex Hull save path specified in Easy Collider Editor's preferences could not be found, or it is an invalid asset folder. A folder to save collider assets was created at: " + path + " and automatically set in preferences. A different folder in Easy Collider Editor's preferences foldout can be specified if desired.\n\n Original path: " + originalPath);
            }
            else
            {
              // used an alternative mesh/prefab/prefabmesh save method.
              Debug.LogWarning("Easy Collider Editor: Could not find a valid location to save the collider and the save path specified in preferences could not be found, or it is an invalid asset folder. A new folder has been created at " + path + " to save mesh collider assets and automatically set in preferences.  A different folder in Easy Collider Editor's preferences foldout can be specified if desired. \n\n Original path: " + originalPath);
            }
          }
        }
      }
      string fullPath = path + "/" + goName;
      return fullPath;
    }

    /// <summary>
    /// goes thorugh the path and finds the first non-existing path that can be used to save.
    /// </summary>
    /// <param name="path">Full path up to save location: ie C:/UnityProjects/ProjectName/Assets/Folder/baseObject</param>
    /// <param name="suffix">Suffix to add to save files ie _Suffix_</param>
    /// <returns>first valid path for AssetDatabase.CreateAsset ie baseObject_Suffix_0</returns>
    private static string GetFirstValidAssetPath(string path, string suffix)
    {

      string validPath = path;
      if (File.Exists(validPath + suffix + "0.asset"))
      {
        int i = 1;
        while (File.Exists(validPath + suffix + i + ".asset"))
        {
          i += 1;
        }
        validPath += suffix + i + ".asset";
      }
      else
      {
        validPath += suffix + "0.asset";
      }

      // replace application's data path  (Unity Editor: <path to project folder>/Assets) 
      // "Assets" earlier in the path should no longer cause issues.
      validPath = validPath.Replace(Application.dataPath, "Assets");
      return validPath;
    }

    /// <summary>
    /// Creates and saves a mesh asset in the asset database with appropriate path and suffix.
    /// </summary>
    /// <param name="mesh">mesh</param>
    /// <param name="attachTo">gameobject the mesh will be attached to, used to find asset path.</param>
    public static void CreateAndSaveMeshAsset(Mesh mesh, GameObject attachTo)
    {
      if (mesh != null && !DoesMeshAssetExists(mesh))
      {
        string savePath = GetValidConvexHullPath(attachTo);
        if (savePath != "")
        {
          string assetPath = GetFirstValidAssetPath(savePath, ECEPreferences.SaveConvexHullSuffix);
          AssetDatabase.CreateAsset(mesh, assetPath);
          AssetDatabase.SaveAssets();
        }
      }
    }

    /// <summary>
    /// Checks if the asset already exists (needed for rotate and duplicate, as the mesh is the same mesh repeated.)
    /// </summary>
    /// <param name="mesh"></param>
    /// <returns></returns>
    public static bool DoesMeshAssetExists(Mesh mesh)
    {
      string p = AssetDatabase.GetAssetPath(mesh);
      if (p == null || p.Length == 0)
      {
        return false;
      }
      return true;
    }



    /// <summary>
    /// Creates and saves an array of mesh assets in the assetdatabase at the path with the the format "savePath"+"suffix"+[0-n].asset
    /// </summary>
    /// <param name="savePath">Full path up to save location: ie C:/UnityProjects/ProjectName/Assets/Folder/baseObject</param>
    /// <param name="suffix">Suffix to add to save files ie _Suffix_</param>
    public static Mesh[] CreateAndSaveMeshAssets(Mesh[] meshes, string savePath, string suffix)
    {
      string firstAssetPath = null;
      int assetSuffixIndex = -1;
      for (int i = 0; i < meshes.Length; i++)
      {
        // get a new valid path for each mesh to save.
        string path = GetFirstValidAssetPath(savePath, suffix);
        try
        {
          if (ECEPreferences.CombinedVHACDColliders && firstAssetPath != null)
          {
            //adding a name to the mesh even though it isn't required to match the first assets name as it by default has the path's name.
            string name = firstAssetPath.Remove(assetSuffixIndex, firstAssetPath.Length - assetSuffixIndex);
            name = name.Remove(0, name.LastIndexOf("/") + 1);
            meshes[i].name = name + suffix + i.ToString();
            AssetDatabase.AddObjectToAsset(meshes[i], firstAssetPath);
          }
          else
          {
            AssetDatabase.CreateAsset(meshes[i], path);
          }
        }
        catch (System.Exception error)
        {
          Debug.LogError("Error saving at path:" + path + ". Try changing the save Convex Hull path in Easy Collider Editor's preferences to a different folder.\n" + error.ToString());
        }
        if (firstAssetPath == null)
        {
          firstAssetPath = path;
          assetSuffixIndex = firstAssetPath.IndexOf(suffix);
        }
      }
      AssetDatabase.SaveAssets();
      if (ECEPreferences.CombinedVHACDColliders)
      {
        // need to reload the assets and update the meshes array otherwise they don't point to the correct object
        // only the first one will without this as for whatever reason create asset will correctly link the objects but adding an object to an asset will not.
        var assets = AssetDatabase.LoadAllAssetsAtPath(firstAssetPath);
        for (int i = 0; i < assets.Length; i++)
        {
          meshes[i] = (Mesh)assets[i];
        }
      }
      return meshes;
    }

  }
}
#endif
