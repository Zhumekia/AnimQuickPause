using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using Studio;
using UnityEngine;

namespace AnimPause
{
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInProcess("CharaStudio")]
    public class AnimPausePlugin : BaseUnityPlugin
    {
        public const string GUID = "kia.animpause";
        public const string PluginName = "Anim Pauser";
        public const string Version = "1.1.0";

        private ConfigEntry<KeyboardShortcut> _hotkeySelected;
        private ConfigEntry<KeyboardShortcut> _hotkeyAll;

        // Speed each character had before it was paused
        private readonly Dictionary<OCIChar, float> _saved = new Dictionary<OCIChar, float>();

        private void Awake()
        {
            // Default is Right Ctrl + P. The player can change it in F1 -> Plugin Settings.
            _hotkeySelected = Config.Bind(
                "General",
                "Pause selected hotkey",
                new KeyboardShortcut(KeyCode.P, KeyCode.RightControl),
                "Toggle animation pause on the selected characters.");

            // Default is no key. The player sets one in F1 -> Plugin Settings.
            _hotkeyAll = Config.Bind(
                "General",
                "Pause all hotkey",
                new KeyboardShortcut(KeyCode.None),
                "Toggle animation pause on ALL characters in the scene. Set a key to enable.");
        }

        private void Update()
        {
            var studio = Studio.Studio.Instance;
            if (studio == null) return;

            if (_hotkeySelected.Value.IsDown())
                PauseSelected(studio);

            if (_hotkeyAll.Value.IsDown())
                PauseAll(studio);
        }

        // Selected characters only: each one toggles on its own
        private void PauseSelected(Studio.Studio studio)
        {
            OCIChar firstChara = null;

            foreach (var node in studio.treeNodeCtrl.selectNodes)
            {
                ObjectCtrlInfo info;
                if (!studio.dicInfo.TryGetValue(node, out info)) continue;

                var chara = info as OCIChar;
                if (chara == null) continue;

                if (firstChara == null) firstChara = chara;

                if (_saved.ContainsKey(chara)) Resume(chara);
                else Pause(chara);
            }

            if (firstChara != null)
                RefreshAnimationPanel(firstChara);
        }

        // Every character in the scene.
        // If any character is still playing, pause all of them.
        // If every character is already paused, resume all of them.
        private void PauseAll(Studio.Studio studio)
        {
            var all = new List<OCIChar>();
            foreach (var info in studio.dicInfo.Values)
            {
                var chara = info as OCIChar;
                if (chara != null) all.Add(chara);
            }

            if (all.Count == 0) return;

            bool anyPlaying = false;
            foreach (var chara in all)
            {
                if (!_saved.ContainsKey(chara))
                {
                    anyPlaying = true;
                    break;
                }
            }

            foreach (var chara in all)
            {
                if (anyPlaying)
                {
                    if (!_saved.ContainsKey(chara)) Pause(chara);
                }
                else
                {
                    if (_saved.ContainsKey(chara)) Resume(chara);
                }
            }

            // The panel shows the selected character, so refresh for that one
            foreach (var node in studio.treeNodeCtrl.selectNodes)
            {
                ObjectCtrlInfo info;
                if (!studio.dicInfo.TryGetValue(node, out info)) continue;

                var selected = info as OCIChar;
                if (selected == null) continue;

                RefreshAnimationPanel(selected);
                break;
            }
        }

        private void Pause(OCIChar chara)
        {
            _saved[chara] = chara.animeSpeed;
            chara.animeSpeed = 0f;
        }

        private void Resume(OCIChar chara)
        {
            float oldSpeed;
            if (_saved.TryGetValue(chara, out oldSpeed))
            {
                chara.animeSpeed = oldSpeed;
                _saved.Remove(chara);
            }
        }

        private void RefreshAnimationPanel(OCIChar chara)
        {
            try
            {
                // Find the character panel in the scene (works even if the name of the
                // field that holds it differs between game versions)
                MPCharCtrl mpCharCtrl = null;
                foreach (var obj in Resources.FindObjectsOfTypeAll(typeof(MPCharCtrl)))
                {
                    var candidate = obj as MPCharCtrl;
                    if (candidate == null) continue;
                    if (mpCharCtrl == null) mpCharCtrl = candidate;
                    if (candidate.gameObject.activeInHierarchy)
                    {
                        mpCharCtrl = candidate;
                        break;
                    }
                }

                if (mpCharCtrl == null)
                {
                    Logger.LogWarning("MPCharCtrl panel not found.");
                    return;
                }

                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                var field = typeof(MPCharCtrl).GetField("animeInfo", flags);
                if (field == null)
                {
                    Logger.LogWarning("animeInfo field not found in MPCharCtrl. Fields with 'anim':");
                    foreach (var f in typeof(MPCharCtrl).GetFields(flags))
                        if (f.Name.ToLower().Contains("anim"))
                            Logger.LogWarning("  " + f.Name + " : " + f.FieldType.Name);
                    return;
                }

                var animeInfo = field.GetValue(mpCharCtrl);
                if (animeInfo == null) return;

                var method = animeInfo.GetType().GetMethod("UpdateInfo", flags);
                if (method == null)
                {
                    Logger.LogWarning("UpdateInfo method not found on " + animeInfo.GetType().Name + ". Methods:");
                    foreach (var m in animeInfo.GetType().GetMethods(flags))
                        Logger.LogWarning("  " + m.Name);
                    return;
                }

                method.Invoke(animeInfo, new object[] { chara });
            }
            catch (Exception e)
            {
                Logger.LogWarning("Could not refresh animation panel: " + e.Message);
            }
        }
    }
}