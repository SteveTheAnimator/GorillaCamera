using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace GorillaCamera.Scripts.Utils
{
    public class V3Utils
    {
        public static bool IsThisNearThat(Vector3 A, Vector3 B, float Distance)
        {
            if(Vector3.Distance(A, B) < Distance) return true;
            else return false;
        }
    }
}
