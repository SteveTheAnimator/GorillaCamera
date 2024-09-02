using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;

namespace GorillaCamera.Scripts.Utils
{
    public class GameModeUtils
    {
        public static bool isThisGameMode(string gameMode)
        {
            if(!PhotonNetwork.InRoom)
            {
                return false;
            }
            else
            {
                if(PhotonNetwork.CurrentRoom.CustomProperties.ToString().Contains(gameMode))
                {
                    return true;
                }
                else
                return false;
            }
        }
    }
}
