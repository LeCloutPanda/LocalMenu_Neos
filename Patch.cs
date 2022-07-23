using System;
using FrooxEngine;
using HarmonyLib;
using NeosModLoader;

namespace LocalMenu
{
    public class Patch : NeosMod
    {
        public override string Name => "LocalMenu";
        public override string Author => "LeCloutPanda";
        public override string Version => "1.0.0";

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
            [HarmonyPatch("OnAttach")]
            static void Postfix(ContextMenu __instance)
            {
                if (!config.GetValue(CONTEXT_MENU_VISIBLE))
                    return;

                Slot slot = __instance.Slot;

                if (slot.ActiveUser == slot.LocalUser)
                {
                    ValueUserOverride<bool> value = slot.AttachComponent<ValueUserOverride<bool>>();
                    value.Target.Value = slot.ActiveSelf_Field.ReferenceID;
                    value.Default.Value = false;
                    value.SetOverride(__instance.LocalUser, true);
                }
            }
        }

        [HarmonyPatch(typeof(InteractionLaser))]
        class PatchInteractionLaser
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnAttach")]
            static void Postfix(InteractionLaser __instance)
            {
                if (!config.GetValue(INTERACTION_LASER_VISIBLE))
                    return;

                Slot slot = __instance.Slot;

                if (slot.ActiveUser == slot.LocalUser)
                {
                    ValueUserOverride<bool> value = slot.AttachComponent<ValueUserOverride<bool>>();
                    value.Target.Value = slot.ActiveSelf_Field.ReferenceID;
                    value.Default.Value = false;
                    value.SetOverride(__instance.LocalUser, true);
                }
            }
        }
    }
}
