using System;
using System.Collections.Generic;
using System.Security.Policy;
using FrooxEngine;
using HarmonyLib;
using NeosModLoader;

namespace LocalMenu
{
    public class Patch : NeosMod
    {
        public override string Name => "LocalMenu";
        public override string Author => "LeCloutPanda";
        public override string Version => "1.0.3";

        public static ModConfiguration config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> CONTEXT_MENU_VISIBLE = new ModConfigurationKey<bool>("Allow others to see Context menu", "", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> INTERACTION_LASER_VISIBLE = new ModConfigurationKey<bool>("Allow others to see Interaction Laser", "", () => true);

        static List<ValueUserOverride<bool>> contextMenuVUOS = new List<ValueUserOverride<bool>>();
        static List<ValueUserOverride<bool>> interactionLaserVUOS = new List<ValueUserOverride<bool>>();


        public override void OnEngineInit()
        {
            config = GetConfiguration();
            config.Save(true);

            Harmony harmony = new Harmony($"dev.{Author}.{Name}");
            harmony.PatchAll();

            config.OnThisConfigurationChanged += UpdateValues;
        }

        private void UpdateValues(ConfigurationChangedEvent @event)
        {
            for (int i = 0; i < contextMenuVUOS.Count; i++)
            {
                var menu = contextMenuVUOS[i];

                if (menu != null)
                {
                    menu.Default.Value = config.GetValue(CONTEXT_MENU_VISIBLE);
                }
            }

            for (int i = 0; i < interactionLaserVUOS.Count; i++)
            {
                var menu = interactionLaserVUOS[i];

                if (menu != null)
                {
                    menu.Default.Value = config.GetValue(INTERACTION_LASER_VISIBLE);
                }
            }
        }
        [HarmonyPatch(typeof(ContextMenu))]
        class PatchContextMenu
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnAwake")]
            static void Postfix(ContextMenu __instance)
            {
                __instance.RunInUpdates(3, () =>
                {
                    if (__instance.Slot.ActiveUserRoot.ActiveUser != __instance.LocalUser)
                        return;

                    Slot slot = __instance.Slot;
                    var temp = slot.AttachComponent<ValueUserOverride<bool>>();
                    contextMenuVUOS.Add(temp);
                    temp.Target.Value = slot.ActiveSelf_Field.ReferenceID;
                    temp.Default.Value = config.GetValue(CONTEXT_MENU_VISIBLE);
                    temp.SetOverride(__instance.LocalUser, true);
                });
            }
        }

        [HarmonyPatch(typeof(InteractionLaser))]
        class PatchInteractionLaser
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnAwake")]
            static void Postfix(InteractionLaser __instance)
            {
                __instance.RunInUpdates(3, () =>
                {
                    if (__instance.Slot.ActiveUserRoot.ActiveUser != __instance.LocalUser)
                        return;

                    Slot slot = __instance.Slot;
                    var temp = slot.AttachComponent<ValueUserOverride<bool>>();
                    interactionLaserVUOS.Add(temp);
                    temp.Target.Value = slot.ActiveSelf_Field.ReferenceID;
                    temp.Default.Value = config.GetValue(INTERACTION_LASER_VISIBLE);
                    temp.SetOverride(__instance.LocalUser, true);
                });
            }
        }
    }
}
