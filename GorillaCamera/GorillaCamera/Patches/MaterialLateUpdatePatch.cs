using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace GorillaCamera.Patches
{
    [HarmonyPatch(typeof(VRRig), "LateUpdate")]
    public class MaterialLateUpdatePatch
    {
        private static void Postfix(VRRig __instance)
        {
            if (!__instance.isOfflineVRRig)
            {
                if (__instance.materialsToChangeTo != null && __instance.setMatIndex >= 0 && __instance.setMatIndex < __instance.materialsToChangeTo.Length)
                {
                    Renderer skinRenderer = __instance.mainSkin as Renderer;
                    if (skinRenderer != null)
                    {
                        skinRenderer.material = __instance.materialsToChangeTo[__instance.setMatIndex];
                    }
                }
            }
        }
    }

}
