using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GorillaCamera.Patches
{
    [HarmonyPatch(typeof(UnityEngine.Debug), "Log", 0)]
    public class WriteLinePatch : MonoBehaviour
    {
        private static bool Prefix(object message)
        {
            Console.WriteLine(message);
            return false;
        }
    }
}
