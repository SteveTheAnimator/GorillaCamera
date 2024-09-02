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

namespace GorillaCamera
{
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public GameObject ShoulderCamera;
        public Camera ActualCamera;
        public float SmoothAmount = 0.1f;
        public CinemachineBrain CameraBrain;
        public CameraModes CurrentCameraMode = CameraModes.ThirdPerson;
        public GameObject LocalPlayerObject;
        public GameObject LocalPlayerCameraObject;
        public bool isGUIEnabled = false;
        public float RandomRigTime = 0f;
        public VRRig FollowingRig;
        public float RandomRigTimeChangeDelay = 7f;
        public bool TweenFirstPerson = false;
        public float RotationTime = 0.1f;

        public void Start()
        {
            Utilla.Events.GameInitialized += OnGameInitialized;
        }

        public void OnEnable()
        {
            HarmonyPatches.ApplyHarmonyPatches();
        }

        public void OnDisable()
        {
            HarmonyPatches.RemoveHarmonyPatches();
        }

        public void OnGameInitialized(object sender, EventArgs e)
        {
            // Setup

            ShoulderCamera = GorillaTagger.Instance.thirdPersonCamera.transform.Find("Shoulder Camera").gameObject;
            ActualCamera = GorillaTagger.Instance.thirdPersonCamera.transform.Find("Shoulder Camera").gameObject.GetComponent<Camera>();
            CameraBrain = GorillaTagger.Instance.thirdPersonCamera.GetComponentInChildren<CinemachineBrain>();
            LocalPlayerObject = GorillaLocomotion.Player.Instance.gameObject;
            LocalPlayerCameraObject = GorillaTagger.Instance.mainCamera.gameObject;
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
                ShoulderCamera.transform.LookAt(LocalPlayerCameraObject.transform.position);
            }
            if(CurrentCameraMode == CameraModes.FirstPerson)
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
                ShoulderCamera.transform.rotation = Quaternion.LerpUnclamped(ShoulderCamera.transform.rotation, LocalPlayerCameraObject.transform.rotation, RotationTime);
            }
            if (CurrentCameraMode == CameraModes.SecondPerson)
            {
                Vector3 offset = new Vector3(0f, 0f, 2f);
                Vector3 targetPosition = LocalPlayerCameraObject.transform.position + LocalPlayerCameraObject.transform.TransformDirection(offset);
                CameraBrain.enabled = false;
                ShoulderCamera.transform.position = Vector3.SmoothDamp(ShoulderCamera.transform.position, targetPosition, ref Velocity, SmoothAmount);
                ShoulderCamera.transform.LookAt(LocalPlayerCameraObject.transform.position);
            }
            if(CurrentCameraMode == CameraModes.Following)
            {
                Vector3 offset = new Vector3(0f, 0f, 0f);
                Vector3 targetPosition = LocalPlayerCameraObject.transform.position + LocalPlayerCameraObject.transform.TransformDirection(offset);
                CameraBrain.enabled = false;
                if(!IsThisNearThat(ShoulderCamera.transform.position, LocalPlayerObject.transform.position, 1f))
                {
                    ShoulderCamera.transform.position = Vector3.SmoothDamp(ShoulderCamera.transform.position, targetPosition, ref Velocity, SmoothAmount + 0.2f);
                }
                ShoulderCamera.transform.LookAt(LocalPlayerCameraObject.transform.position);
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
                        ShoulderCamera.transform.LookAt(FollowingRig.headMesh.transform.position);
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
                        ShoulderCamera.transform.LookAt(FollowingRig.headMesh.transform.position);
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
                        ShoulderCamera.transform.LookAt(FollowingRig.headMesh.transform.position);
                    }
                    else
                    {
                        FollowingRig = GetRandomRig(true, false, true);
                        RandomRigTime = Time.time + RandomRigTimeChangeDelay;
                    }
                }
            }

            if (Keyboard.current.rightBracketKey.wasPressedThisFrame)
            {
                isGUIEnabled = !isGUIEnabled;
            }
        }

        public void OnGUI()
        {
            if (isGUIEnabled)
            {
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 20,
                    normal = { textColor = Color.grey },
                    active = { textColor = Color.white },
                    alignment = TextAnchor.MiddleCenter,
                    fixedWidth = 80,
                    fixedHeight = 40
                };

                GUIStyle buttonStyleNext = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 4,
                    normal = { textColor = Color.grey },
                    active = { textColor = Color.white },
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
                    normal = { textColor = Color.yellow },
                    alignment = TextAnchor.MiddleCenter
                };

                float screenWidth = Screen.width;
                float screenHeight = Screen.height;

                float panelWidth = 300;
                float panelHeight = 200;

                Rect panelRect = new Rect((screenWidth - panelWidth) / 2, screenHeight - panelHeight - 20, panelWidth, panelHeight);

                GUI.Box(panelRect, "", GUI.skin.box);

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
                    if(GUI.Button(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 30, panelWidth - 10, 20), $"Tween First Person Position {TweenFirstPerson}", buttonStyleNext))
                    {
                        TweenFirstPerson = !TweenFirstPerson;
                    }
                    GUI.Label(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 150, panelWidth - 20, 20), "Rotation Time", labelStylesmall);
                    RotationTime = GUI.HorizontalSlider(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 130, panelWidth - 20, 20), RotationTime, 0f, 1f);
                }

                GUI.Label(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 120, panelWidth - 20, 20), "FOV", labelStyle);
                ActualCamera.fieldOfView = GUI.HorizontalSlider(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 100, panelWidth - 20, 20), ActualCamera.fieldOfView, 1f, 180f);

                GUI.Label(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 80, panelWidth - 20, 20), "Smooth Amount", labelStyle);
                SmoothAmount = GUI.HorizontalSlider(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 60, panelWidth - 20, 20), SmoothAmount, 0f, 1f);

                if (CurrentCameraMode == CameraModes.RandomView || CurrentCameraMode == CameraModes.RandomSurvivorView || CurrentCameraMode == CameraModes.RandomTaggedView)
                {
                    GUI.Label(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 30, panelWidth - 20, 20), "Rig Time Delay", labelStyle);
                    RandomRigTimeChangeDelay = GUI.HorizontalSlider(new Rect(panelRect.x + 10, panelRect.y + panelHeight - 10, panelWidth - 20, 20), RandomRigTimeChangeDelay, 1f, 20f);
                }
            }
        }

        private void SwitchCameraMode(int direction)
        {
            CameraModes[] modes = (CameraModes[])Enum.GetValues(typeof(CameraModes));
            int currentIndex = Array.IndexOf(modes, CurrentCameraMode);

            int newIndex = (currentIndex + direction + modes.Length) % modes.Length;
            CurrentCameraMode = modes[newIndex];
            if(CurrentCameraMode == CameraModes.RandomView || CurrentCameraMode == CameraModes.RandomSurvivorView || CurrentCameraMode == CameraModes.RandomTaggedView) { FollowingRig = null; RandomRigTime = 0; }
            if(CurrentCameraMode == CameraModes.RandomTaggedView || CurrentCameraMode == CameraModes.RandomSurvivorView)
            {
                if(!isThisGameMode("INFECTION"))
                {
                    CurrentCameraMode = CameraModes.ThirdPerson;
                }
            }
            if (CurrentCameraMode == CameraModes.RandomView)
            {
                if (!PhotonNetwork.InRoom)
                {
                    CurrentCameraMode = CameraModes.ThirdPerson;
                }
            }
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
