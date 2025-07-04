﻿using BepInEx.Logging;
using HarmonyLib;
using modtest1;
using Rhythm;
using Arcade.UI.SongSelect;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UnbeatableSkinsMod
{
    internal class CustomSkinsPatches
    {
        private static ManualLogSource Logger { get; set; }
        public static int BeatCustomSkinIndex = -1;
        public static int QuavCustomSkinIndex = -1;
        public static bool loadCustomSkins = true;

        public static void Init(ManualLogSource logger)
        {
            Logger = logger;
        }

        [HarmonyPatch(typeof(ArcadeSongDatabase), "Awake")]
        [HarmonyPostfix]
        private static async void MainMenuStart()
        {
            await Task.Delay(1500);
            Logger.LogInfo("Press '/' to toggle skins mod");
            Logger.LogInfo($"Press '<' to switch selected Beat skin. Current Beat Skin Set To '{GetSkinName(CustomSkinsPlugin.BeatCustomSkins, BeatCustomSkinIndex)}'");
            Logger.LogInfo($"Press '>' to switch selected Quaver skin. Current Quaver Skin Set To '{GetSkinName(CustomSkinsPlugin.QuavCustomSkins, QuavCustomSkinIndex)}'");
        }

        [HarmonyPatch(typeof(ArcadeSongDatabase), "Update")]
        [HarmonyPostfix]
        private static void MainMenuUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Slash))
            {
                if (!loadCustomSkins)
                {
                    loadCustomSkins = true;
                    Logger.LogInfo("Toggled Skins Mod ON");
                    BeatCustomSkinIndex = CustomSkinsPlugin.ReloadSkins(CustomSkinsPlugin.BeatCustomSkins, "beat");
                    Logger.LogInfo($"Current Beat Skin Set To '{GetSkinName(CustomSkinsPlugin.BeatCustomSkins, BeatCustomSkinIndex)}'");
                    QuavCustomSkinIndex = CustomSkinsPlugin.ReloadSkins(CustomSkinsPlugin.QuavCustomSkins, "quav");
                    Logger.LogInfo($"Current Quaver Skin Set To '{GetSkinName(CustomSkinsPlugin.QuavCustomSkins, QuavCustomSkinIndex)}'");
                }
                else
                {
                    loadCustomSkins = false;
                    Logger.LogInfo("Toggled Skins Mod OFF");
                }
            }

            if ((Input.GetKeyDown(KeyCode.Comma) || Input.GetKeyDown(KeyCode.Less)) && loadCustomSkins)
            {
                BeatCustomSkinIndex++;
                if (BeatCustomSkinIndex >= CustomSkinsPlugin.BeatCustomSkins.Count)
                    BeatCustomSkinIndex = -1;

                Logger.LogInfo($"Current Beat Skin Changed To '{GetSkinName(CustomSkinsPlugin.BeatCustomSkins, BeatCustomSkinIndex)}'");
            }
            if ((Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.Greater)) && loadCustomSkins)
            {
                QuavCustomSkinIndex++;
                if (QuavCustomSkinIndex >= CustomSkinsPlugin.QuavCustomSkins.Count)
                    QuavCustomSkinIndex = -1;

                Logger.LogInfo($"Current Quaver Skin Changed To '{GetSkinName(CustomSkinsPlugin.QuavCustomSkins, QuavCustomSkinIndex)}'");
            }
        }

        public static string GetSkinName(IReadOnlyList<string> skins, int index)
        {
            if (index == -1)
                return "Default";
            else
                return skins[index];
        }

        [HarmonyPatch(typeof(RhythmPlayer), "Awake")]
        [HarmonyPostfix]
        private static void AwakePostPatchBeat(ref SpriteRenderer ___sprite, ref Animator ___animator)
        {
            if (loadCustomSkins && BeatCustomSkinIndex >= 0)
                UpdateSkin(CustomSkinsPlugin.BeatCustomSkins, ___sprite, ___animator, BeatCustomSkinIndex);
            //GetSpritesFromAnimator(___animator, ___sprite);
        }

        [HarmonyPatch(typeof(RhythmAssist), "Awake")]
        [HarmonyPostfix]
        private static void AwakePostPatchQuav(ref SpriteRenderer ___sprite, ref Animator ___animator)
        {
            if (loadCustomSkins && QuavCustomSkinIndex >= 0)
                UpdateSkin(CustomSkinsPlugin.QuavCustomSkins, ___sprite, ___animator, QuavCustomSkinIndex);
            //GetSpritesFromAnimator(___animator, ___sprite);
        }

        public static string GetSkinInFolder(IReadOnlyList<string> skinFolders, int index)
        {
            string[] files = Directory.GetFiles(skinFolders[index]);
            foreach (string skin in files)
                if (skin.EndsWith(".png"))
                    return skin;
            return "";
        }

        public static string GetSkinBounds(IReadOnlyList<string> skinFolders, int index)
        {
            string[] files = Directory.GetFiles(skinFolders[index]);
            foreach (string file in files)
                if (file.EndsWith(".csv"))
                    return file;
            return "";
        }

        private static void UpdateSkin(IReadOnlyList<string> skinFolders, SpriteRenderer spriteR, Animator anim, int index)
        {
            string skin = GetSkinInFolder(skinFolders, index);
            if (skin == "") {
                Logger.LogInfo((object)("No skin found in folder " + skinFolders[index]));
                return;
            }

            Logger.LogInfo((object)("Patching " + spriteR.sprite.texture.name + " with " + skin));
            ImageConversion.LoadImage(spriteR.sprite.texture, File.ReadAllBytes(skin));

            string skinBounds = GetSkinBounds(skinFolders, index);
            if (skinBounds != "")
            {
                var spriteBounds = LoadCustomSkinBounds(skinBounds);
                //var spriteBounds = new List<SpriteBound>();
                ReplaceSpritesInAnimator(anim, spriteR, spriteBounds);
            }
        }

        private static List<SpriteBound> LoadCustomSkinBounds(string skinBounds)
        {
            var sr = new StreamReader(skinBounds);
            var fileContents = sr.ReadToEnd();
            sr.Close();
            List<SpriteBound> bounds = new List<SpriteBound>();

            var lines = fileContents.Split(""[0]);
            for (int i = 0; i < lines.Length; i = i+3)
            {
                SpriteBound bound = new SpriteBound(lines[i], lines[i + 1], lines[i + 2]);
                bounds.Add(bound);
            }
            return bounds;
        }

        public static async Task ReplaceSpritesInAnimator(Animator anim, SpriteRenderer spriteR, List<SpriteBound> sbs)
        {
            bool BCJ2U = false;
            foreach (AnimationClip ac in anim.runtimeAnimatorController.animationClips)
            {
                var replace = false;
                var f = new List<float>();
                
                switch (ac.name)
                {
                    case "BeatCombatIntro":
                        await Task.Delay(100);
                        f = new List<float> {0.08f, 0.1f, 0.25f, 0.3f, 0.45f, 0.5f, 0.6f, 0.7f, 0.8f, 0.99f};
                        replace = true;
                        break;

                    case "BeatCombatIdle01":
                        f = new List<float> {0f, 1/4f, 2/4f, 3/4f};
                        replace = true;
                        break;

                    case "BeatCombatJump01Up":
                        f = new List<float> {0f, 0.15f, 0.2f, 0.5f};
                        replace = true;
                        break;

                    case "BeatCombatJump01Land":
                    case "BeatCombatJump02Land":
                    case "BeatCombatHurt01":
                    case "BeatCombatHurt02":
                    case "BeatCombatHurt03":
                        f = new List<float> {0f};
                        replace = true;
                        break;

                    case "BeatCombatJump02Up":
                        if (BCJ2U)
                            break;
                        f = new List<float> {0f, 0.15f, 0.3f, 0.5f};
                        replace = true;
                        BCJ2U = true;
                        break;

                    case "BeatCombatGroundBlockIntro":
                    case "BeatCombatAirBlockIntro":
                    case "QuaverCombatAirBlockLoop":
                        f = new List<float> {0f, 0.5f};
                        replace = true;
                        break;

                    case "BeatCombatGroundBlockLoop":
                    case "BeatCombatAirBlockLoop":
                        f = new List<float> {0f, 1/3f, 2/3f};
                        replace = true;
                        break;

                    case "BeatCombatAttack01":
                    case "BeatCombatAttack02":
                    case "BeatCombatAttack03":
                    case "BeatCombatAttack04":
                    case "BeatCombatAttack05":
                    case "BeatCombatAttack06":
                    case "BeatCombatAttack07":
                    case "BeatCombatAttack08":
                    case "BeatCombatAttack09":
                    case "BeatCombatAttack10":
                    case "QuaverCombatAttack01":
                    case "QuaverCombatAttack02":
                        f = new List<float> {0f, 0.25f, 0.75f};
                        replace = true;
                        break;

                    case "BeatCombatRun01":
                        f = new List<float> {0f, 1/8f, 2/8f, 3/8f, 4/8f, 5/8f, 6/8f, 7/8f};
                        replace = true;
                        break;

                    case "QuaverCombatBackgroundIdle":
                        f = new List<float> { };
                        for (int i = 1; i < 55; i++)
                            f.Add(i / 55f);
                        break;

                    default:
                        break;
                }//end switch

                if (replace)
                {
                    foreach (var f2 in f)
                    {
                        anim.Play(ac.name, -1, f2);
                        await Task.Delay(1);//1
                        Logger.LogInfo("Replacing sprite: " + ac.name + " - " + spriteR.sprite.name);
                        SpriteBound.ReplaceSprite(sbs, spriteR.sprite);
                    }
                    replace = false;
                }//end if
            }//end foreach
        }//end ReplaceSpritesInAnimator

        public static async void GetSpritesFromAnimator(Animator anim, SpriteRenderer spriteR) //testing
        {
            foreach (AnimationClip ac in anim.runtimeAnimatorController.animationClips)
            {

                /*
                string lastSprite = "";
                anim.Play(ac.name);
                for (int i = 0; i < 500; i++)
                {
                    await Task.Delay(1);
                    if (lastSprite != spriteR.sprite.name)
                    {
                        Logger.LogInfo(ac.name + " - " + ((i + 1) * 1).ToString() + "ms: " + spriteR.sprite.name);
                        lastSprite = spriteR.sprite.name;
                    }
                }
                */
                /*
                var f = new List<float>();

                switch (ac.name)
                {
                    case "QuaverCombatAirBlockLoop":
                        f = new List<float> { 0f, 0.5f };
                        break;
                    case "QuaverCombatAttack01":
                    case "QuaverCombatAttack02":
                        f = new List<float> { 0f, 0.25f, 0.75f };
                        break;
                    case "QuaverCombatBackgroundIdle":
                        f = new List<float> { };
                        for (int i = 1; i < 55; i++)
                            f.Add(i / 55f);
                        break;
                }

                foreach (var f2 in f)
                {
                    anim.Play(ac.name, -1, f2);
                    await Task.Delay(1);//1
                    Logger.LogInfo("Replacing sprite: " + ac.name + " - " + spriteR.sprite.name);
                }
                */
            }
        }//end GetSpritesFromAnimaton

    }//end CustomSkinPatches
}//end namespace
