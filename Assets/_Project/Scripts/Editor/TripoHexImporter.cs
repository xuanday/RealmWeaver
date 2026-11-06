using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RealmWeaver.Editor
{
    [InitializeOnLoad]
    public static class TripoHexImporter
    {
        private const string PrefKeyTiles = "RealmWeaver_TerrainTilesImported_v1";
        private const string PrefKeyBuildings = "RealmWeaver_BuildingsImported_v1";
        private const string PrefKeyDecoration = "RealmWeaver_DecorationImported_v1";

        private const string TexturePath = "Assets/_Project/Art/Materials/TripoHex/hexagons_medieval.png";
        private const string MaterialPath = "Assets/_Project/Art/Materials/TripoHex/M_TripoHex.mat";

        private const string TilesModelsPath = "Assets/_Project/Art/Models/TripoHex/Tiles";
        private const string TilesPrefabsPath = "Assets/_Project/Prefabs/Tiles";

        private const string BuildingsModelsPath = "Assets/_Project/Art/Models/TripoHex/Buildings";
        private const string BuildingsPrefabsPath = "Assets/_Project/Prefabs/Buildings";

        private const string DecorationModelsPath = "Assets/_Project/Art/Models/TripoHex/Decoration";
        private const string DecorationPrefabsPath = "Assets/_Project/Prefabs/Decoration";

        static TripoHexImporter()
        {
            EditorApplication.delayCall += CheckAndAutoImport;
        }

        private static void CheckAndAutoImport()
        {
            bool needTiles = !EditorPrefs.GetBool(PrefKeyTiles, false) ||
                             !File.Exists(Path.Combine(Application.dataPath, "_Project/Prefabs/Tiles/hex_grass.prefab"));

            bool needBuildings = !EditorPrefs.GetBool(PrefKeyBuildings, false) ||
                                 !File.Exists(Path.Combine(Application.dataPath, "_Project/Prefabs/Buildings/neutral/building_castle_neutral.prefab"));

            bool needDecoration = !EditorPrefs.GetBool(PrefKeyDecoration, false) ||
                                  !File.Exists(Path.Combine(Application.dataPath, "_Project/Prefabs/Decoration/nature/tree_pine_A.prefab"));

            if (needTiles || needBuildings || needDecoration)
            {
                RunImportAll();
            }
        }

        [MenuItem("RealmWeaver/Import All (Tiles, Buildings, Decoration)")]
        public static void RunImportAllMenu()
        {
            RunImportAll();
        }

        [MenuItem("RealmWeaver/Import Buildings Prefabs")]
        public static void RunImportBuildingsMenu()
        {
            Material mat = SetupMaterial();
            if (mat != null)
            {
                int count = ImportBuildings(mat);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorPrefs.SetBool(PrefKeyBuildings, true);
                Debug.Log($"<color=green><b>[RealmWeaver]</b> Đã tạo thành công {count} Building Prefabs tại '{BuildingsPrefabsPath}'.</color>");
            }
        }

        [MenuItem("RealmWeaver/Import Decoration Prefabs")]
        public static void RunImportDecorationMenu()
        {
            Material mat = SetupMaterial();
            if (mat != null)
            {
                int count = ImportDecoration(mat);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorPrefs.SetBool(PrefKeyDecoration, true);
                Debug.Log($"<color=green><b>[RealmWeaver]</b> Đã tạo thành công {count} Decoration Prefabs tại '{DecorationPrefabsPath}'.</color>");
            }
        }

        [MenuItem("RealmWeaver/Import 3D Terrain Assets")]
        public static void RunImportTilesMenu()
        {
            Material mat = SetupMaterial();
            if (mat != null)
            {
                int count = ImportTiles(mat);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorPrefs.SetBool(PrefKeyTiles, true);
                Debug.Log($"<color=green><b>[RealmWeaver]</b> Đã tạo thành công {count} Tile Prefabs tại '{TilesPrefabsPath}'.</color>");
            }
        }

        public static void RunImportAll()
        {
            Debug.Log("<color=cyan><b>[RealmWeaver]</b> Bắt đầu quá trình Import Prefabs (Tiles, Buildings, Decoration)...</color>");

            EnsureFolderExists("Assets/_Project/Art/Models/TripoHex");
            EnsureFolderExists("Assets/_Project/Art/Materials/TripoHex");
            EnsureFolderExists(TilesPrefabsPath);
            EnsureFolderExists(BuildingsPrefabsPath);
            EnsureFolderExists(DecorationPrefabsPath);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            SetupTexture();

            Material mat = SetupMaterial();
            if (mat == null)
            {
                Debug.LogError("[RealmWeaver] Không thể tạo Material URP!");
                return;
            }

            int tileCount = ImportTiles(mat);
            int buildingCount = ImportBuildings(mat);
            int decorCount = ImportDecoration(mat);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorPrefs.SetBool(PrefKeyTiles, true);
            EditorPrefs.SetBool(PrefKeyBuildings, true);
            EditorPrefs.SetBool(PrefKeyDecoration, true);

            Debug.Log($"<color=green><b>[RealmWeaver]</b> Hoàn thành! Đã tạo: {tileCount} Tiles, {buildingCount} Buildings, {decorCount} Decoration Prefabs.</color>");
        }

        public static int ImportTiles(Material sharedMat)
        {
            return GeneratePrefabsFlat(TilesModelsPath, TilesPrefabsPath, sharedMat);
        }

        public static int ImportBuildings(Material sharedMat)
        {
            return GeneratePrefabsPreserveSubfolders(BuildingsModelsPath, BuildingsPrefabsPath, sharedMat);
        }

        public static int ImportDecoration(Material sharedMat)
        {
            return GeneratePrefabsPreserveSubfolders(DecorationModelsPath, DecorationPrefabsPath, sharedMat);
        }

        private static void EnsureFolderExists(string relativePath)
        {
            relativePath = relativePath.Replace("\\", "/").TrimEnd('/');
            if (!AssetDatabase.IsValidFolder(relativePath))
            {
                int lastSlash = relativePath.LastIndexOf('/');
                if (lastSlash > 0)
                {
                    string parent = relativePath.Substring(0, lastSlash);
                    string folderName = relativePath.Substring(lastSlash + 1);
                    if (!AssetDatabase.IsValidFolder(parent))
                    {
                        EnsureFolderExists(parent);
                    }
                    AssetDatabase.CreateFolder(parent, folderName);
                }
            }
        }

        private static void SetupTexture()
        {
            if (!File.Exists(TexturePath))
            {
                Debug.LogWarning($"[RealmWeaver] Không tìm thấy texture tại '{TexturePath}'.");
                return;
            }

            TextureImporter importer = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
            if (importer != null)
            {
                bool modified = false;
                if (importer.textureType != TextureImporterType.Default)
                {
                    importer.textureType = TextureImporterType.Default;
                    modified = true;
                }
                if (importer.sRGBTexture != true)
                {
                    importer.sRGBTexture = true;
                    modified = true;
                }
                if (importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    modified = true;
                }
                if (importer.wrapMode != TextureWrapMode.Clamp)
                {
                    importer.wrapMode = TextureWrapMode.Clamp;
                    modified = true;
                }

                if (modified)
                {
                    importer.SaveAndReimport();
                    Debug.Log($"[RealmWeaver] Đã cấu hình TextureImporter cho '{TexturePath}'.");
                }
            }
        }

        private static Material SetupMaterial()
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);

            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null)
            {
                urpShader = Shader.Find("Universal Render Pipeline/Simple Lit");
            }
            if (urpShader == null)
            {
                urpShader = Shader.Find("Standard");
                Debug.LogWarning("[RealmWeaver] Không tìm thấy URP Lit Shader, dùng Standard Shader tạm thời.");
            }

            if (mat == null)
            {
                mat = new Material(urpShader);
                mat.name = "M_TripoHex";
                AssetDatabase.CreateAsset(mat, MaterialPath);
                Debug.Log($"[RealmWeaver] Đã tạo Material mới tại '{MaterialPath}' với Shader: {urpShader.name}");
            }
            else
            {
                if (mat.shader != urpShader)
                {
                    mat.shader = urpShader;
                }
            }

            // Assign Texture
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            if (tex != null)
            {
                if (mat.HasProperty("_BaseMap"))
                {
                    mat.SetTexture("_BaseMap", tex);
                }
                if (mat.HasProperty("_MainTex"))
                {
                    mat.SetTexture("_MainTex", tex);
                }
                mat.mainTexture = tex;

                if (mat.HasProperty("_Smoothness"))
                {
                    mat.SetFloat("_Smoothness", 0.0f);
                }
                if (mat.HasProperty("_Metallic"))
                {
                    mat.SetFloat("_Metallic", 0.0f);
                }

                EditorUtility.SetDirty(mat);
            }
            else
            {
                Debug.LogWarning($"[RealmWeaver] Chưa load được Texture2D tại '{TexturePath}'.");
            }

            return mat;
        }

        private static int GeneratePrefabsFlat(string sourceModelsFolder, string targetPrefabsFolder, Material sharedMat)
        {
            if (!AssetDatabase.IsValidFolder(sourceModelsFolder))
            {
                Debug.LogWarning($"[RealmWeaver] Thư mục mô hình '{sourceModelsFolder}' không tồn tại!");
                return 0;
            }

            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { sourceModelsFolder });
            int createdCount = 0;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (modelAsset == null) continue;

                string modelName = Path.GetFileNameWithoutExtension(assetPath);
                string prefabPath = $"{targetPrefabsFolder}/{modelName}.prefab";

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
                if (instance != null)
                {
                    Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in renderers)
                    {
                        r.sharedMaterial = sharedMat;
                    }

                    PrefabUtility.SaveAsPrefabAsset(instance, prefabPath, out bool success);
                    Object.DestroyImmediate(instance);

                    if (success)
                    {
                        createdCount++;
                    }
                }
            }

            return createdCount;
        }

        private static int GeneratePrefabsPreserveSubfolders(string sourceModelsFolder, string targetPrefabsFolder, Material sharedMat)
        {
            if (!AssetDatabase.IsValidFolder(sourceModelsFolder))
            {
                Debug.LogWarning($"[RealmWeaver] Thư mục mô hình '{sourceModelsFolder}' không tồn tại!");
                return 0;
            }

            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { sourceModelsFolder });
            int createdCount = 0;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (modelAsset == null) continue;

                string relativeSubDir = "";
                string assetDir = Path.GetDirectoryName(assetPath).Replace("\\", "/");
                if (assetDir.Length > sourceModelsFolder.Length)
                {
                    relativeSubDir = assetDir.Substring(sourceModelsFolder.Length).TrimStart('/');
                }

                string destFolder = string.IsNullOrEmpty(relativeSubDir)
                    ? targetPrefabsFolder
                    : $"{targetPrefabsFolder}/{relativeSubDir}";

                EnsureFolderExists(destFolder);

                string modelName = Path.GetFileNameWithoutExtension(assetPath);
                string prefabPath = $"{destFolder}/{modelName}.prefab";

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
                if (instance != null)
                {
                    Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in renderers)
                    {
                        r.sharedMaterial = sharedMat;
                    }

                    PrefabUtility.SaveAsPrefabAsset(instance, prefabPath, out bool success);
                    Object.DestroyImmediate(instance);

                    if (success)
                    {
                        createdCount++;
                    }
                }
            }

            return createdCount;
        }
    }
}
