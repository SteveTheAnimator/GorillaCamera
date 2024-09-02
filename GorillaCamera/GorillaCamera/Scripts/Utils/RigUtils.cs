using BepInEx;
using Cinemachine;
using GorillaCamera.Scripts.Utils;
using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GorillaCamera.Scripts.Utils
{
    internal class RigUtils
    {
        public static VRRig GetRandomRig(bool includeMyRig, bool HasToBeTagged = false, bool HasToBeSurvivor = false)
        {
            var players = includeMyRig ? PhotonNetwork.PlayerList : PhotonNetwork.PlayerListOthers;
            var randomPlayer = players[UnityEngine.Random.Range(0, players.Length)];
            VRRig rig = GorillaGameManager.instance.FindPlayerVRRig(randomPlayer);
            if(HasToBeTagged)
            {
                if (isThisPlayerTagged(rig))
                {
                    return rig;
                }
                else
                {
                    return GetRandomRig(includeMyRig, HasToBeTagged);
                }
            }
            else if (HasToBeSurvivor)
            {
                if (!isThisPlayerTagged(rig))
                {
                    return rig;
                }
                else
                {
                    return GetRandomRig(includeMyRig, HasToBeTagged, HasToBeSurvivor);
                }
            }
            else
            {
                return rig;
            }
        }
        public static bool isThisPlayerTagged(VRRig player)
        {
            if (player.mainSkin.material.name.Contains("fected"))
            {
                return true;
            }
            else
            {
                if (player.mainSkin.material.name.Contains("It"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
