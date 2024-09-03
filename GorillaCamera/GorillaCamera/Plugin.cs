using BepInEx;
using Cinemachine;
using GorillaCamera.Scripts.Utils;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilla;
using static GorillaCamera.Scripts.Important.CameraModesEnum; // For CameraModes Reference
using static GorillaCamera.Scripts.Utils.V3Utils; // Vector 3 Utils, all made by me!
using static GorillaCamera.Scripts.Utils.RigUtils;
using static GorillaCamera.Scripts.Utils.GameModeUtils;
using Photon.Pun;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace GorillaCamera
{
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        // Instance
        public static Plugin instance {  get; private set; }

        // References
        public GameObject ShoulderCamera;
        public Camera ActualCamera;
        public CinemachineBrain CameraBrain;
        public GameObject LocalPlayerObject;
        public GameObject LocalPlayerCameraObject;
        public VRRig FollowingRig;
        public GameObject VisibleCameraObject = null;
        private Texture2D boxTexture;
        private Texture2D buttonTexture;
        private Texture2D headerTexture;

        // Values
        public float SmoothAmount = 0.1f;
        public CameraModes CurrentCameraMode = CameraModes.ThirdPerson;
        public bool isGUIEnabled = false;
        public float RandomRigTime = 0f;
        public float RandomRigTimeChangeDelay = 7f;
        public bool TweenFirstPerson = true;
        public float RotationTime = 0.1f;
        public bool ShowCameraPositon = false;
        public bool ShowFollowingPlayerName = true;

        // Competitive
        public bool isCompetitiveTeam = false;
        public bool isCompetitiveTeamConfiguring = false;
        public string Team1Name = "Team 1";
        public string Team2Name = "Team 2";
        public int Team1Score = 0;
        public int Team2Score = 0;
        public bool isChangingTeam1Score = true;

        public void Start() { Utilla.Events.GameInitialized += OnGameInitialized; }

        public void OnEnable() { HarmonyPatches.ApplyHarmonyPatches(); }

        public void OnDisable() { HarmonyPatches.RemoveHarmonyPatches(); }

        public void OnGameInitialized(object sender, EventArgs e)
        {
            // Setup

            ShoulderCamera = GorillaTagger.Instance.thirdPersonCamera.transform.Find("Shoulder Camera").gameObject;
            ActualCamera = GorillaTagger.Instance.thirdPersonCamera.transform.Find("Shoulder Camera").gameObject.GetComponent<Camera>();
            CameraBrain = GorillaTagger.Instance.thirdPersonCamera.GetComponentInChildren<CinemachineBrain>();
            LocalPlayerObject = GorillaLocomotion.Player.Instance.gameObject;
            LocalPlayerCameraObject = GorillaTagger.Instance.mainCamera.gameObject;

            boxTexture = new Texture2D(1, 1);
            boxTexture.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f, 0.9f));
            boxTexture.Apply();

            buttonTexture = new Texture2D(1, 1);
            buttonTexture.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f, 0.9f));
            buttonTexture.Apply();

            headerTexture = new Texture2D(1, 1);
            headerTexture.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f, 0.9f));
            headerTexture.Apply();
            SetupVCO();
        }

        public void SetupVCO()
        {
            VisibleCameraObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            VisibleCameraObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            VisibleCameraObject.GetComponent<Renderer>().material = new Material(Shader.Find("GUI/Text Shader"));
            VisibleCameraObject.GetComponent<Renderer>().material.color = Color.grey;
            VisibleCameraObject.GetComponent<Renderer>().enabled = false;
            VisibleCameraObject.transform.SetParent(ShoulderCamera.transform, false); // mja ha ha
            VisibleCameraObject.transform.localPosition = new Vector3(0, 0, -0.1f);
            GameObject.Destroy(VisibleCameraObject.GetComponent<BoxCollider>());
        }

        public void Update()
        {
            Vector3 Velocity = GorillaTagger.Instance.rigidbody.velocity;
            if (CurrentCameraMode == CameraModes.ThirdPerson)
            {
                Vector3 offset = new Vector3(0.5f, 0.7f, -0.8f);
                Vector3 targetPosition = LocalPlayerCameraObject.transform.position + LocalPlayerCameraObject.transform.TransformDirection(offset);
                CameraBrain.enabled = false;
                ShoulderCamera.transform.position = Vector3.SmoothDamp(ShoulderCamera.transform.position, targetPosition, ref Velocity, SmoothAmount);
                Quaternion targetRotation = Quaternion.LookRotation(LocalPlayerCameraObject.transform.position - ShoulderCamera.transform.position);
                ShoulderCamera.transform.rotation = Quaternion.LerpUnclamped(ShoulderCamera.transform.rotation, targetRotation, RotationTime);
            }
            if (CurrentCameraMode == CameraModes.FirstPerson)
            {
                Vector3 offset = new Vector3(0f, 0f, 0f);
                Vector3 targetPosition = LocalPlayerCameraObject.transform.position + LocalPlayerCameraObject.transform.TransformDirection(offset);
                CameraBrain.enabled = false;
                if (TweenFirstPerson)
                {
                    ShoulderCamera.transform.position = Vector3.SmoothDamp(ShoulderCamera.transform.position, targetPosition, ref Velocity, SmoothAmount - 0.06f);
                }
                else
                {
                    ShoulderCamera.transform.position = LocalPlayerCameraObject.transform.position;
                }
                Quaternion targetRotation = LocalPlayerCameraObject.transform.rotation;
                ShoulderCamera.transform.rotation = Quaternion.LerpUnclamped(ShoulderCamera.transform.rotation, targetRotation, RotationTime);
            }
            if (CurrentCameraMode == CameraModes.SecondPerson)
            {
                Vector3 offset = new Vector3(0f, 0f, 2f);
                Vector3 targetPosition = LocalPlayerCameraObject.transform.position + LocalPlayerCameraObject.transform.TransformDirection(offset);
                CameraBrain.enabled = false;
                ShoulderCamera.transform.position = Vector3.SmoothDamp(ShoulderCamera.transform.position, targetPosition, ref Velocity, SmoothAmount);
                Quaternion targetRotation = Quaternion.LookRotation(LocalPlayerCameraObject.transform.position - ShoulderCamera.transform.position);
                ShoulderCamera.transform.rotation = Quaternion.LerpUnclamped(ShoulderCamera.transform.rotation, targetRotation, RotationTime);
            }
            if (CurrentCameraMode == CameraModes.Following)
            {
                Vector3 offset = new Vector3(0f, 0f, 0f);
                Vector3 targetPosition = LocalPlayerCameraObject.transform.position + LocalPlayerCameraObject.transform.TransformDirection(offset);
                CameraBrain.enabled = false;
                if (!IsThisNearThat(ShoulderCamera.transform.position, LocalPlayerObject.transform.position, 1f))
                {
                    ShoulderCamera.transform.position = Vector3.SmoothDamp(ShoulderCamera.transform.position, targetPosition, ref Velocity, SmoothAmount + 0.2f);
                }
                Quaternion targetRotation = Quaternion.LookRotation(LocalPlayerCameraObject.transform.position - ShoulderCamera.transform.position);
                ShoulderCamera.transform.rotation = Quaternion.LerpUnclamped(ShoulderCamera.transform.rotation, targetRotation, RotationTime);
            }
            if (PhotonNetwork.InRoom)
            {
                if (CurrentCameraMode == CameraModes.RandomView)
                {
                    if (RandomRigTime < Time.time)
                    {
                        FollowingRig = GetRandomRig(true);
                        RandomRigTime = Time.time + RandomRigTimeChangeDelay;
                    }
                    if (FollowingRig != null)
                    {
                        Vector3 offset = new Vector3(0.5f, 0.7f, -0.8f);
                        Vector3 targetPosition = FollowingRig.headMesh.transform.position + FollowingRig.headMesh.transform.TransformDirection(offset);
                        CameraBrain.enabled = false;
                        ShoulderCamera.transform.position = Vector3.SmoothDamp(ShoulderCamera.transform.position, targetPosition, ref Velocity, SmoothAmount);
                        Quaternion targetRotation = Quaternion.LookRotation(FollowingRig.headMesh.transform.position - ShoulderCamera.transform.position);
                        ShoulderCamera.transform.rotation = Quaternion.LerpUnclamped(ShoulderCamera.transform.rotation, targetRotation, RotationTime);
                    }
                    else
                    {
                        FollowingRig = GetRandomRig(true);
                        RandomRigTime = Time.time + RandomRigTimeChangeDelay;
                    }
                }
            }
            if (isThisGameMode("INFECTION"))
            {
                if (CurrentCameraMode == CameraModes.RandomTaggedView)
                {
                    if (RandomRigTime < Time.time)
                    {
                        FollowingRig = GetRandomRig(true, true, false);
                        RandomRigTime = Time.time + RandomRigTimeChangeDelay;
                    }
                    if (FollowingRig != null)
                    {
                        Vector3 offset = new Vector3(0.5f, 0.7f, -0.8f);
                        Vector3 targetPosition = FollowingRig.headMesh.transform.position + FollowingRig.headMesh.transform.TransformDirection(offset);
                        CameraBrain.enabled = false;
                        ShoulderCamera.transform.position = Vector3.SmoothDamp(ShoulderCamera.transform.position, targetPosition, ref Velocity, SmoothAmount);
                        Quaternion targetRotation = Quaternion.LookRotation(FollowingRig.headMesh.transform.position - ShoulderCamera.transform.position);
                        ShoulderCamera.transform.rotation = Quaternion.LerpUnclamped(ShoulderCamera.transform.rotation, targetRotation, RotationTime);
                    }
                    else
                    {
                        FollowingRig = GetRandomRig(true, true, false);
                        RandomRigTime = Time.time + RandomRigTimeChangeDelay;
                    }
                }
                if (CurrentCameraMode == CameraModes.RandomSurvivorView)
                {
                    if (RandomRigTime < Time.time)
                    {
                        FollowingRig = GetRandomRig(true, false, true);
                        RandomRigTime = Time.time + RandomRigTimeChangeDelay;
                    }
                    if (FollowingRig != null)
                    {
                        Vector3 offset = new Vector3(0.5f, 0.7f, -0.8f);
                        Vector3 targetPosition = FollowingRig.headMesh.transform.position + FollowingRig.headMesh.transform.TransformDirection(offset);
                        CameraBrain.enabled = false;
                        ShoulderCamera.transform.position = Vector3.SmoothDamp(ShoulderCamera.transform.position, targetPosition, ref Velocity, SmoothAmount);
                        Quaternion targetRotation = Quaternion.LookRotation(FollowingRig.headMesh.transform.position - ShoulderCamera.transform.position);
                        ShoulderCamera.transform.rotation = Quaternion.LerpUnclamped(ShoulderCamera.transform.rotation, targetRotation, RotationTime);
                    }
                    else
                    {
                        FollowingRig = GetRandomRig(true, false, true);
                        RandomRigTime = Time.time + RandomRigTimeChangeDelay;
                    }
                }
            }
            if(CurrentCameraMode == CameraModes.LeftHand)
            {
                Vector3 offset = new Vector3(0f, 0f, 0f);
                Vector3 targetPosition = GorillaTagger.Instance.offlineVRRig.leftHandTransform.position + GorillaTagger.Instance.offlineVRRig.leftHandTransform.TransformDirection(offset);
                CameraBrain.enabled = false;
                ShoulderCamera.transform.position = Vector3.SmoothDamp(ShoulderCamera.transform.position, targetPosition, ref Velocity, SmoothAmount - 0.06f);
                Quaternion targetRotation = Quaternion.LookRotation(LocalPlayerCameraObject.transform.position - ShoulderCamera.transform.position);
                ShoulderCamera.transform.rotation = Quaternion.LerpUnclamped(ShoulderCamera.transform.rotation, targetRotation, RotationTime);
            }
            if (CurrentCameraMode == CameraModes.RightHand)
            {
                Vector3 offset = new Vector3(0f, 0f, 0f);
                Vector3 targetPosition = GorillaTagger.Instance.offlineVRRig.rightHandTransform.position + GorillaTagger.Instance.offlineVRRig.rightHandTransform.TransformDirection(offset);
                CameraBrain.enabled = false;
                ShoulderCamera.transform.position = Vector3.SmoothDamp(ShoulderCamera.transform.position, targetPosition, ref Velocity, SmoothAmount - 0.06f);
                Quaternion targetRotation = Quaternion.LookRotation(LocalPlayerCameraObject.transform.position - ShoulderCamera.transform.position);
                ShoulderCamera.transform.rotation = Quaternion.LerpUnclamped(ShoulderCamera.transform.rotation, targetRotation, RotationTime);
            }
            if (CurrentCameraMode == CameraModes.InMiddle)
            {
                Vector3 offset = new Vector3(0f, 0f, 0f);
                Vector3 targetPosition = (GorillaTagger.Instance.offlineVRRig.rightHandTransform.position + GorillaTagger.Instance.offlineVRRig.leftHandTransform.position) / 2 + offset;
                CameraBrain.enabled = false;
                ShoulderCamera.transform.position = Vector3.SmoothDamp(ShoulderCamera.transform.position, targetPosition, ref Velocity, SmoothAmount - 0.06f);
                Quaternion targetRotation = Quaternion.LookRotation(LocalPlayerCameraObject.transform.position - ShoulderCamera.transform.position);
                ShoulderCamera.transform.rotation = Quaternion.LerpUnclamped(ShoulderCamera.transform.rotation, targetRotation, RotationTime);
            }
            if (Keyboard.current.rightBracketKey.wasPressedThisFrame)
            {
                isGUIEnabled = !isGUIEnabled;
            }
            if(Keyboard.current.leftBracketKey.wasPressedThisFrame) 
            { 
                if(isGUIEnabled)
                {
                    isCompetitiveTeam = !isCompetitiveTeam;
                }
            }
            if (Keyboard.current.semicolonKey.wasPressedThisFrame)
            {
                if (isGUIEnabled)
                {
                    isCompetitiveTeamConfiguring = !isCompetitiveTeamConfiguring;
                }
            }
            if (Keyboard.current.commaKey.wasPressedThisFrame)
            {
                if (isGUIEnabled)
                {
                   isChangingTeam1Score = !isChangingTeam1Score;
                }
            }
            if(Keyboard.current.endKey.wasPressedThisFrame)
            {
                ShowCameraPositon = !ShowCameraPositon;
                VisibleCameraObject.GetComponent<Renderer>().enabled = ShowCameraPositon;
            }

            // Competitive Adding Score
            if(isCompetitiveTeam)
            {
                if (isChangingTeam1Score)
                {
                    if (Keyboard.current.equalsKey.wasPressedThisFrame) Team1Score = Team1Score + 1;
                    if (Keyboard.current.minusKey.wasPressedThisFrame) Team1Score = Team1Score - 1;
                }
                else
                {
                    if (Keyboard.current.equalsKey.wasPressedThisFrame) Team2Score = Team2Score + 1;
                    if (Keyboard.current.minusKey.wasPressedThisFrame) Team2Score = Team2Score - 1;
                }
            }
        }

        public void OnGUI()
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 20,
                normal = { textColor = Color.grey, background = buttonTexture },
                active = { textColor = Color.white, background = buttonTexture },
                hover = { textColor = Color.white, background = buttonTexture },
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = 80,
                fixedHeight = 40
            };

            GUIStyle buttonStyleNext = new GUIStyle(GUI.skin.button)
            {
                fontSize = 9,
                normal = { textColor = Color.grey, background = buttonTexture },
                active = { textColor = Color.white, background = buttonTexture },
                hover = { textColor = Color.white, background = buttonTexture },
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = 80,
                fixedHeight = 20
            };

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter
            };
            GUIStyle labelStylesmall = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter
            };

            GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter
            };

            GUIStyle TeamScoreStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 30,
                normal = { textColor = Color.grey },
                alignment = TextAnchor.MiddleCenter
            };
            GUIStyle FollowStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 30,
                normal = { textColor = Color.grey },
                alignment = TextAnchor.MiddleRight
            };
            GUIStyle BoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = boxTexture }
            };

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            float panelWidth = 300;
            float panelHeight = 200;

            Rect panelRect = new Rect((screenWidth - panelWidth) / 2, screenHeight - panelHeight - 20, panelWidth, panelHeight);

            if (isGUIEnabled)
            {
                // Actual UI

                GUI.Box(panelRect, "", BoxStyle);

                float buttonWidth = 80;
                float buttonHeight = 40;
                Rect leftButtonRect = new Rect(panelRect.x + 10, panelRect.y + (panelHeight - buttonHeight) / 2 - 60, buttonWidth, buttonHeight);
                Rect rightButtonRect = new Rect(panelRect.x + panelWidth - buttonWidth - 10, panelRect.y + (panelHeight - buttonHeight) / 2 - 60, buttonWidth, buttonHeight);
                Rect labelRect = new Rect(panelRect.x + buttonWidth + 10, panelRect.y + (panelHeight - buttonHeight) / 2 - 60, panelWidth - 2 * (buttonWidth + 20), buttonHeight);

                Rect headerRect = new Rect(panelRect.x, panelRect.y - 30, panelWidth, 30);
                GUI.Label(headerRect, "Gorilla Camera - Made By Steve Monke", headerStyle);

                GUI.Label(labelRect, CurrentCameraMode.ToString(), labelStyle);

                if (GUI.Button(leftButtonRect, "<", buttonStyle))
                {
                    GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(66, true, 1f);
                    SwitchCameraMode(-1);
                }

                if (GUI.Button(rightButtonRect, ">", buttonStyle))
                {
                    GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(66, false, 1f);
                    SwitchCameraMode(1);
                }

                if (CurrentCameraMode == CameraModes.FirstPerson)
                {
                    if (GUI.Button(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 30, panelWidth - 10, 20), $"Tween First Person Position {TweenFirstPerson}", buttonStyleNext))
                    {
                        TweenFirstPerson = !TweenFirstPerson;
                    }
                }

                GUI.Label(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 150, panelWidth - 20, 20), "Rotation Time", labelStylesmall);
                RotationTime = GUI.HorizontalSlider(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 130, panelWidth - 20, 20), RotationTime, 0f, 1f);

                GUI.Label(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 120, panelWidth - 20, 20), "FOV", labelStyle);
                ActualCamera.fieldOfView = GUI.HorizontalSlider(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 100, panelWidth - 20, 20), ActualCamera.fieldOfView, 1f, 180f);

                GUI.Label(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 80, panelWidth - 20, 20), "Smooth Amount", labelStyle);
                SmoothAmount = GUI.HorizontalSlider(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 60, panelWidth - 20, 20), SmoothAmount, 0f, 1f);

                if (CurrentCameraMode == CameraModes.RandomView || CurrentCameraMode == CameraModes.RandomSurvivorView || CurrentCameraMode == CameraModes.RandomTaggedView)
                {
                    GUI.Label(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 30, panelWidth - 20, 20), "Rig Time Delay", labelStyle);
                    RandomRigTimeChangeDelay = GUI.HorizontalSlider(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 10, panelWidth - 20, 20), RandomRigTimeChangeDelay, 1f, 20f);
                    if (GUI.Button(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 30, panelWidth - 10, 20), $"Show Following Player Name {ShowFollowingPlayerName}", buttonStyleNext))
                    {
                        ShowFollowingPlayerName = !ShowFollowingPlayerName;
                    }
                }


                if (isCompetitiveTeamConfiguring)
                {
                    Team1Name = GUI.TextArea(new Rect(panelRect.x, panelRect.y - 120, panelWidth, 30), Team1Name);
                    Team2Name = GUI.TextArea(new Rect(panelRect.x, panelRect.y - 90, panelWidth, 30), Team2Name);
                }
            }
            // Competitive UI
            if (isCompetitiveTeam)
            {
                GUI.Label(new Rect((screenWidth - panelWidth) / 2, 10, panelWidth, 30), $"{Team1Name} Score: {Team1Score}", TeamScoreStyle);
                GUI.Label(new Rect((screenWidth - panelWidth) / 2, 40, panelWidth, 30), $"{Team2Name} Score: {Team2Score}", TeamScoreStyle);
                GUI.Label(new Rect((screenWidth - panelWidth) / 2, 80, panelWidth, 30), $"Currently Changing: {GetCurrentChangingTeam()}", labelStylesmall);
            }
            if (CurrentCameraMode == CameraModes.RandomView || CurrentCameraMode == CameraModes.RandomSurvivorView || CurrentCameraMode == CameraModes.RandomTaggedView)
            {
                if (FollowingRig != null)
                {
                    if (ShowFollowingPlayerName)
                    {
                        GUI.Label(new Rect(30, 120, panelWidth + 200, 90), "Currently Spectating: " + FollowingRig.playerText.text, FollowStyle);
                    }
                }
            }
        }

        public string GetCurrentChangingTeam()
        {
            return isChangingTeam1Score ? Team1Name : Team2Name;
        }

        private void SwitchCameraMode(int direction)
        {
            CameraModes[] modes = (CameraModes[])Enum.GetValues(typeof(CameraModes));
            int currentIndex = Array.IndexOf(modes, CurrentCameraMode);

            int newIndex = (currentIndex + direction + modes.Length) % modes.Length;
            if(modes[newIndex] == CameraModes.RandomView || modes[newIndex] == CameraModes.RandomSurvivorView || modes[newIndex] == CameraModes.RandomTaggedView) { FollowingRig = null; RandomRigTime = 0; }
            if(modes[newIndex] == CameraModes.RandomTaggedView || modes[newIndex] == CameraModes.RandomSurvivorView)
            {
                if(!isThisGameMode("INFECTION"))
                {
                    modes[newIndex] = direction == 1 ? CameraModes.LeftHand : CameraModes.Following;
                }
            }
            if (modes[newIndex] == CameraModes.RandomView)
            {
                if (!PhotonNetwork.InRoom)
                {
                    modes[newIndex] = direction == 1 ? CameraModes.LeftHand : CameraModes.Following;
                }
            }
            CurrentCameraMode = modes[newIndex];
        }
    }
    public class PluginInfo
    {
        internal const string
            GUID = "Steve.GorillaCamera",
            Name = "Gorilla Camera",
            Version = "1.0.0";
    }
}
