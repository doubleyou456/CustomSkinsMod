using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace UnbeatableSkinsMod
{
    [BepInPlugin("doubleyou.skinsMod", "Skins Mod", "1.1.0")]
    public class CustomSkinsPlugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("doubleyou.skinsMod");
        private static CustomSkinsPlugin Instance;
        internal static ManualLogSource mls;

        public static readonly string BeatSkinsFolder = "BeatCustomSkins";
        public static readonly string QuavSkinsFolder = "QuaverCustomSkins";
        public static readonly List<string> BeatCustomSkins = new List<string>();
        public static readonly List<string> QuavCustomSkins = new List<string>();

        private static void FindSkins()
        {
            if (!Directory.Exists(BeatSkinsFolder))
            {
                if (Directory.Exists("CustomSkins"))
                    Directory.Move("CustomSkins", BeatSkinsFolder);
                else
                    Directory.CreateDirectory(BeatSkinsFolder);
            }

            if (!Directory.Exists(QuavSkinsFolder))
                Directory.CreateDirectory(QuavSkinsFolder);
        }//end FindSkins

        private void LoadSkins()
        {
            mls.LogInfo("Loading Beat Skins");
            ReloadSkins(BeatCustomSkins, "beat");
            mls.LogInfo("Loading Quaver Skins");
            ReloadSkins(QuavCustomSkins, "quav");
        }//end LoadSkins

        public static bool SkinInFolder(string folder)
        {
            string[] files = Directory.GetFiles(folder);
            foreach (string skin in files)
                if (skin.EndsWith(".png"))
                    return true;
            return false;
        }//end SkinInFolder

        public static void FoundSkins(List<string> skins)
        {
            foreach (string text in skins)
                if (SkinInFolder(text))
                    mls.LogInfo("Found Skin in: " + text);
        }//end FoundSkins

        public static int ReloadSkins(List<string> skins, string character)
        {
            List<string> reload = new List<string>();
            switch (character.ToLower())
            {
                case "beat":
                    string[] Bdirs = Directory.GetDirectories(BeatSkinsFolder);
                    foreach (string text in Bdirs)
                        reload.Add(text);; 
                    break;

                case "quav":
                case "quaver":
                    string[] Qdirs = Directory.GetDirectories(QuavSkinsFolder);
                    foreach (string text in Qdirs)
                        reload.Add(text);
                    break;

                default:
                    return -1;
            }//end switch

            FoundSkins(reload);
            skins.Clear();
            foreach (string text in reload)
                skins.Add(text);
            return -1;
        }//end ReloadSkins

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource("doubleyou.skinsMod");
            mls.LogInfo("Looking For Skins Folder");

            FindSkins();

            LoadSkins();

            if (BeatCustomSkins.Count == 0 && QuavCustomSkins.Count == 0)
            {
                mls.LogInfo("No Skins Found, Stopping Mod Load");
                return;
            }
            
            CustomSkinsPatches.Init(mls);
            harmony.PatchAll(typeof(CustomSkinsPatches));

            mls.LogInfo("Finished");
        }//end Awake
    }
}//end namespace