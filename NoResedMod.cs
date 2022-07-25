using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;

namespace NoResetMod
{

    static public class ModState
    {
        // Allows the user to reset the car using the D-Pad or the menu
        static public bool manualOverride = false;
    }

    static class Main
    {
        public static UnityModManager.ModEntry mod;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            mod = modEntry;
            modEntry.Logger.Log("NoResetMod is up and running!");
            return true;
        }
    }

    // I don't think this does anything. I think the class "OutOfBoundsManager" handles underwater
    // checks. However, I'll leave it in case there is some weird edge case that uses it. 
    [HarmonyPatch(typeof(global::ResetCar), "CheckUnderwater")]
    public class CarResetPatch
    {
        // __result seems to be a flag to pass function return values. Not sure if it needs to be in order
        // of the function call. I think Harmony looks for "__result" for a return type and assumes other
        // arguments input parameters, in order, not caring about names.
        //
        // i.e: To capture  bool Function(int i, string str), you would do:
        //
        // void Prefix(ref bool __result, int index, string a_string)
        //
        // Maybe you could make the input args refernces too.
        public static void Postfix(ref bool __result)
        {
            __result = false;
        }
    }

    // This is called outside of ResetCarOutOfBoundsAnimation() in a few places so we should always hijack it.
    [HarmonyPatch(typeof(global::OutOfBoundsManager), nameof(global::OutOfBoundsManager.SetResettingInProgress))]
    public class SetResettingInProgressPatch
    {
        public static bool Prefix(ref bool value)
        {
            if (ModState.manualOverride)
            {
                return true;
            }
            else
            {
                value = false;
                return true;
            }
        }
    }

    // This begins the reset procedure.
    [HarmonyPatch(typeof(global::OutOfBoundsManager), "ResetCarOutOfBoundsAnimation")]
    public class ResetCarOutOfBoundsAnimationPatch
    {
        public static bool Prefix()
        {
            return ModState.manualOverride;
        }

        public static void Postfix()
        {
            ModState.manualOverride = false;
        }
    }

    // Should be able to reset manually from pause screen
    [HarmonyPatch(typeof(global::PauseScreen), "ResetCar")]
    public class HardResettPatch
    {
        public static bool Prefix()
        {
            ModState.manualOverride = true;
            return true;
        }
    }

    // Player reset button/key. This function actually handles all player inputs, including other
    // inputs like "Repair puncture" and QuickRestart". I want all of those to work too, so there
    // is no point in chekcing for which button is pressed.
    [HarmonyPatch(typeof(global::ShortcutManager), "DoAction")]
    public class ShortcutManagerPatch
    {
        public static bool Prefix()
        {
            ModState.manualOverride = true;
            return true;
        }

        public static void Postfix()
        {
            ModState.manualOverride = false;
        }
    }

    // I stole this idea from /u/wulkanat on Nexus mods. I don't want anyone cheating on the
    // leaderboard using this mod.
    // https://www.nexusmods.com/artofrally/mods/3
    // https://github.com/Theaninova/Art-Of-Rally-Reset-Visualizer
    [HarmonyPatch(typeof(global::StageSceneManager), nameof(global::StageSceneManager.OnEventOver))]
    public class SceneManagerOnEventOverPatch
    {
        public static void Prefix(ref bool __0)
        {
            // apply terminal damage
            __0 = true;
        }
    }
}