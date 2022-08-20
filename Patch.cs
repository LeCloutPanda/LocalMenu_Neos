using System;
using System.Security.Policy;
using FrooxEngine;
using HarmonyLib;
using NeosModLoader;

namespace LocalMenu
{
    public class Patch : NeosMod
    {
        public override string Name => "Local-Menu";
        public override string Author => "LeCloutPanda";
        public override string Version => "1.0.2";

        public static ModConfiguration config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> CONTEXT_MENU_VISIBLE = new ModConfigurationKey<bool>("Allow others to see Context menu", "", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> INTERACTION_LASER_VISIBLE = new ModConfigurationKey<bool>("Allow others to see Interaction Laser", "", () => true);

        static ValueUserOverride<bool> contextMenuVUO;
        static ValueUserOverride<bool> interactionLaserVUOL;
        static ValueUserOverride<bool> interactionLaserVUOR;

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
            contextMenuVUO.Default.Value = config.GetValue(CONTEXT_MENU_VISIBLE);
            interactionLaserVUOL.Default.Value = config.GetValue(INTERACTION_LASER_VISIBLE);
            interactionLaserVUOR.Default.Value = config.GetValue(INTERACTION_LASER_VISIBLE);
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
                    contextMenuVUO = slot.AttachComponent<ValueUserOverride<bool>>();
                    contextMenuVUO.Target.Value = slot.ActiveSelf_Field.ReferenceID;
                    contextMenuVUO.Default.Value = config.GetValue(CONTEXT_MENU_VISIBLE);
                    contextMenuVUO.SetOverride(__instance.LocalUser, true);
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
                    ValueUserOverride<bool> current = slot.AttachComponent<ValueUserOverride<bool>>();
                    current.Target.Value = slot.ActiveSelf_Field.ReferenceID;
                    current.Default.Value = config.GetValue(INTERACTION_LASER_VISIBLE);
                    current.SetOverride(__instance.LocalUser, true);

                    if (__instance.Side == Chirality.Left)
                        interactionLaserVUOL = current; 
                    else if (__instance.Side == Chirality.Right)
                        interactionLaserVUOR = current;
                });
            }
        }
    }
}
