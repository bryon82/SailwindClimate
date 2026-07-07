using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static Climate.Cli_Plugin;

namespace Climate
{
    internal class AssetLoader
    {
        private static readonly List<string> assetPaths = new List<string>() {
            Path.Combine(Path.GetDirectoryName(Instance.Info.Location), "Assets"),
            Path.Combine(Path.GetDirectoryName(Instance.Info.Location))
        };

        public static string FindAssetPath(string fileName)
        {
            foreach (string basePath in assetPaths)
            {
                string fullPath = Path.Combine(basePath, fileName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
            return null;
        }

        internal static IEnumerator LoadAssets()
        {
            LogDebug("Loading bundle");
            var bundlePath = FindAssetPath("meteorology_tools");
            if (string.IsNullOrEmpty(bundlePath))
            {
                LogError("Asset bundle not found");
                yield break;
            }

            var assetBundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
            yield return assetBundleRequest;

            var assetBundle = assetBundleRequest.assetBundle;
            if (assetBundle == null)
                LogError($"Failed to load {bundlePath}");
            var request = assetBundle.LoadAllAssetsAsync();
            yield return request;

            Items.Barometer = request.allAssets.FirstOrDefault(a => a.name == "barometer") as GameObject;
            Items.Thermometer = request.allAssets.FirstOrDefault(a => a.name == "thermometer") as GameObject;
            Items.Hygrometer = request.allAssets.FirstOrDefault(a => a.name == "hygrometer") as GameObject;

            if (Items.Barometer == null || Items.Thermometer == null || Items.Hygrometer == null)
            {
                LogError("Failed to load all required assets from the bundle");
                yield break;
            }

            LogInfo("Assets loaded");

            Items.Initialize();
        }
    }
}
