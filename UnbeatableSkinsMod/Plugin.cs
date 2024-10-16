using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace UnbeatableSkinsMod
{
    [BepInPlugin("doubleyou.skinsMod", "Skins Mod", "1.0.0")]
    public class CustomSkinsPlugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("doubleyou.skinsMod");
        private static CustomSkinsPlugin Instance;
        internal ManualLogSource mls;

        public static readonly string skinsFolder = "CustomSkins";
        public static readonly List<string> customSkins = new List<string>();

        private static void FindSkins()
        {
            if (!Directory.Exists(skinsFolder))
            {
                Directory.CreateDirectory(skinsFolder);
            }
        }

        private static void LoadSkins()
        {
            string[] dirs = Directory.GetDirectories(skinsFolder);
            foreach (string text in dirs)
                customSkins.Add(text);
            string[] skins = Directory.GetFiles(skinsFolder,"*.png");
            foreach (string text in skins)
                customSkins.Add(text);
        }

        public static bool SkinInFolder(string folder)
        {
            string[] files = Directory.GetFiles(folder);
            foreach (string skin in files)
                if (skin.EndsWith(".png"))
                    return true;
            return false;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource("doubleyou.skinsMod");
            mls.LogInfo("Looking For Skins Folder");

            FindSkins();

            mls.LogInfo("Loading Skins");

            LoadSkins();

            if (customSkins.Count == 0)
            {
                mls.LogInfo("No Skins Found, Stopping Mod Load");
                return;
            }

            foreach (string text in customSkins)
                if (SkinInFolder(text))
                    mls.LogInfo("Found Skin in: " + text);
            
            CustomSkinsPatches.Init(mls);
            harmony.PatchAll(typeof(CustomSkinsPatches));

            mls.LogInfo("Finished");
        }
    }
}