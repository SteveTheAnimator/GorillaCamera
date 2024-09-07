using BepInEx;
using Cinemachine;
using GorillaTag;
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
            if (HasToBeTagged)
            {
                if (isThisPlayerTagged(rig.OwningNetPlayer))
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
                if (!isThisPlayerTagged(rig.OwningNetPlayer))
                {
                    return rig;
                }
                else
                {
                    if (!IsEveryoneTagged())
                    {
                        return GetRandomRig(includeMyRig, HasToBeTagged, HasToBeSurvivor);
                    }
                    else
                    {
                        return rig; /* Prevent Game Crashes */
                    }
                }
            }
            else
            {
                return rig;
            }
        }
        public static bool isThisPlayerTagged(NetPlayer player)
        {
            if (GetGorillaTagManager().currentInfected.Contains(player))
            {
                return true;
            }
            else
            {
                if (GetGorillaTagManager().currentIt == player)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public static bool IsEveryoneTagged()
        {
            bool notpossible = false;
            foreach(VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (!isThisPlayerTagged(rig.OwningNetPlayer)) { notpossible = true; break; }
            }
            return !notpossible;
        }

        public static GorillaTagManager GetGorillaTagManager()
        {
            return GameObject.Find("GT Systems/GameModeSystem/Gorilla Tag Manager").GetComponent<GorillaTagManager>();
        }
    }
}
