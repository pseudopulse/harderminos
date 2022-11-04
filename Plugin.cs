using BepInEx;
using UnityEngine;
using MonoMod;
using System;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using UnityEngine.AddressableAssets;
using Unity;
using UnityEditor;
using MonoMod.Cil;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;


namespace chaos
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public NewMovement player = null;
        public static AssetBundle assets = null;
        private void Awake()
        {

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            SceneManager.activeSceneChanged += (from, to) => {
                player = null;
                if (SceneManager.GetActiveScene().name.StartsWith("Level")) {
                    player = FindObjectOfType<NewMovement>();
                }
                if (assets == null) {
                    IEnumerable<AssetBundle> bundles = AssetBundle.GetAllLoadedAssetBundles();
                    foreach (AssetBundle bundle in bundles) {
                        if (bundle.name.Contains("common")) {
                            assets = bundle;
                        }
                    }
                }
            };

            On.FleshPrison.Start += (orig, self) => {
                orig(self);
                self.eid.InstaKill();
            };

            // faster animations, starts in phase 2
            On.MinosPrime.Start += (orig, self) => {
                orig(self);
                self.anim.speed = 1.6f;
                self.enraged = true;
            };

            // lower cooldowns
            On.MinosPrime.FixedUpdate += (orig, self) => {
                orig(self);
                if (self.cooldown >= 0.5f) {
                    self.cooldown = 0.5f;
                }
                self.enraged = true;
            }; 

            // faster explosion
            IL.MinosPrime.Explosion += (il) => {
                ILCursor c = new ILCursor(il);
                bool found = c.TryGotoNext(MoveType.After,
                    x => x.MatchLdloc(3),
                    x => x.MatchDup(),
                    x => x.MatchLdfld<Explosion>("speed"),
                    x => x.MatchLdcR4(0.6f)
                );

                if (found) {
                    c.Prev.Operand = 2.4f;
                }
                else {
                    Logger.LogError("Could not apply IL hook for minos explosion speed");
                }
            };

            // stronger combo knockback
            IL.MinosPrime.Combo += (il) => {
                ILCursor c = new ILCursor(il);
                bool found = c.TryGotoNext(MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<MinosPrime>("sc"),
                    x => x.MatchLdcR4(50)
                );

                if (found) {
                    c.Prev.Operand = 250f;
                }
                else {
                    Logger.LogError("Could not apply IL hook for minos combo knockback");
                }
            };

            On.MinosPrime.PickAttack += (orig, self, type) => {
                orig(self, type);
                self.ProjectileCharge();
            };
        }
    }

    public class DeathTimer : MonoBehaviour {
        float stopwatch = 0f;
        float delay = 5f;

        public void FixedUpdate() {
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= delay) {
                DestroyImmediate(gameObject);
            }
        }
    }
}
