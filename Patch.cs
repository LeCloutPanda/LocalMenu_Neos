using System;
using FrooxEngine;
using HarmonyLib;
using NeosModLoader;

namespace LocalMenu
{
    public class Patch : NeosMod
    {
        public override string Name => "Local-Menu";
        public override string Author => "LeCloutPanda";
        public override string Version => "1.0.1";

        public static ModConfiguration config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> CONTEXT_MENU_VISIBLE = new ModConfigurationKey<bool>("Use ValueUserOverride on Context Menu", "", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> INTERACTION_LASER_VISIBLE = new ModConfigurationKey<bool>("Use ValueUserOverride on Interaction Laser", "", () => true);

        public override void OnEngineInit()
        {
            config = GetConfiguration();
            config.Save(true);

            Harmony harmony = new Harmony($"dev.{Author}.{Name}");
            harmony.PatchAll();
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
                    if (!config.GetValue(CONTEXT_MENU_VISIBLE))
                        return;

                    if (__instance.Slot.ActiveUserRoot.ActiveUser != __instance.LocalUser)
                        return;

                    Slot slot = __instance.Slot;
                    ValueUserOverride<bool> value = slot.AttachComponent<ValueUserOverride<bool>>();
                    value.Target.Value = slot.ActiveSelf_Field.ReferenceID;
                    value.Default.Value = false;
                    value.SetOverride(__instance.LocalUser, true);
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
                    if (!config.GetValue(INTERACTION_LASER_VISIBLE))
                        return;

                    if (__instance.Slot.ActiveUserRoot.ActiveUser != __instance.LocalUser)
                        return;

                    Slot slot = __instance.Slot;
                    ValueUserOverride<bool> value = slot.AttachComponent<ValueUserOverride<bool>>();
                    value.Target.Value = slot.ActiveSelf_Field.ReferenceID;
                    value.Default.Value = false;
                    value.SetOverride(__instance.LocalUser, true);
                });
            }
        }
    }
}
