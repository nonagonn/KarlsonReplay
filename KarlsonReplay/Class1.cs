using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using MelonLoader;
using HarmonyLib;
using System.Reflection;
[assembly: MelonInfo(typeof(KarlsonReplay.Class1), "KarlsonReplay", "1.0", "nonagon")]
[assembly: MelonGame("Dani", "Karlson")]

namespace KarlsonReplay
{
    public class Class1 : MelonMod
    {
        private List<Vector3> storedPosition;
        private List<Quaternion> storedRotation;
        private List<Vector3> storedScale;
        private List<Vector3> storedCameraPos = new List<Vector3>();
        private List<Quaternion> storedCameraRot = new List<Quaternion>();
        private List<bool> storedShots;
        private List<bool> storedPick;
        private List<bool> storedDrop;
        private bool isRecording, isReplaying, isFinishReplay, isShooting, isPick, isFreecam, isDrop, isPlayingReplay, isUiHidden;
        private int index, freeCamNumber;
        private int selectedCam;
        private GameObject timerUI, crosshair;


        public override void OnGUI()
        {
            GUIStyle myStyle = new GUIStyle();
            myStyle.fontSize = 50;
            myStyle.normal.textColor = Color.white;

            GUIStyle replayStyle = new GUIStyle();
            replayStyle.fontSize = 72;
            replayStyle.normal.textColor = Color.white;
            Scene scene = SceneManager.GetActiveScene();
            if (!Game.Instance.playing & scene.name != "MainMenu" & scene.name != "Initialize")
            {
                if (GUI.Button(new Rect(15, 15, 200, 60), "Replay", replayStyle))
                {
                    Game.Instance.RestartGame();
                    index = 0;
                    isRecording = false;
                    isPlayingReplay = false;
                    isReplaying = true;
                    isFinishReplay = true;
                    MelonLogger.Msg("Finished Recording!");
                    MelonLogger.Msg("Player Positions Stored: " + storedPosition.Count);
                    MelonLogger.Msg("Player Rotations Stored: " + storedRotation.Count);
                }

            }
            else if (Game.Instance.playing & scene.name != "MainMenu" & scene.name != "Initialize")
            {
                if (isUiHidden)
                {
                    return;
                }

                if (isReplaying)
                {
                    if (GUI.Button(new Rect(15, 950, 300, 120), "Play", myStyle))
                    {
                        isPlayingReplay = true;
                        Timer.Instance.StartTimer();
                    }
                    if (GUI.Button(new Rect(15, 890, 300, 120), "Pause", myStyle))
                    {
                        isPlayingReplay = false;
                        Timer.Instance.Stop();
                    }
                    GUI.Label(new Rect(15, 1015, 300, 120), "[" + index + "/" + storedPosition.Count + "]", myStyle);
                    if (isFreecam)
                    {
                        GUI.Label(new Rect(1810, 1015, 300, 120), "[" + selectedCam + "/" + storedCameraPos.Count + "]", myStyle);
                    }
                }
                if (isRecording)
                {
                    GUI.Label(new Rect(15, 1015, 300, 120), "Recording... " + "[" + storedPosition.Count + "/" + storedPosition.Count + "]", myStyle);
                }
            }
        }

        public Vector3 HitPoint()
        {
            GameObject playerCam = GameObject.Find("Camera");
            RaycastHit[] array = Physics.RaycastAll(playerCam.transform.position, playerCam.transform.forward, (float)PlayerMovement.Instance.whatIsHittable);
            if (array.Length < 1)
            {
                return playerCam.transform.position + playerCam.transform.forward * 100f;
            }
            if (array.Length > 1)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                    {
                        return array[i].point;
                    }
                }
            }
            return array[0].point;
        }


        /*public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex != 0)
            {
                timerUI = GameObject.Find("Timer");
                crosshair = GameObject.Find("Crosshair");
                MelonLogger.Msg(timerUI);
                MelonLogger.Msg(crosshair);
            }
        */


        public override void OnUpdate()
        {
            GameObject camera = GameObject.Find("Camera");
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name != "MainMenu" & scene.name != "Initialize")
            {
                GameObject player = GameObject.Find("Player");

                if (isReplaying)
                {
                    if (Input.GetKeyDown(KeyCode.H))
                    {
                        isUiHidden = !isUiHidden;
                    }
                    /*if (isUiHidden)
                    {
                        timerUI.SetActive(false);
                        crosshair.SetActive(false);
                    }
                    else
                    {
                        timerUI.SetActive(true);
                        crosshair.SetActive(true);
                    }*/

                    if (Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        if (freeCamNumber == 0)
                        {
                            isFreecam = true;
                        }
                        else if (freeCamNumber == 1)
                        {
                            isFreecam = false;
                        }
                    }
                }

                if (isFreecam)
                {
                    if (freeCamNumber != 1)
                    {
                        freeCamNumber = 1;
                        GameObject gun = GameObject.Find("GunCam");
                        var freecam = new GameObject("freecam");
                        freecam.transform.position = player.transform.position;
                        freecam.transform.rotation = player.transform.rotation;
                        freecam.AddComponent<Camera>();
                        freecam.AddComponent<FreeCam>();
                        freecam.GetComponent<Camera>().fieldOfView = 110;
                        gun.SetActive(false);
                        player.GetComponent<MeshRenderer>().enabled = true;
                        camera.GetComponent<Camera>().enabled = false;
                    }
                }
                else
                {
                    if (freeCamNumber != 0)
                    {
                        freeCamNumber = 0;
                        GameObject gun = GameObject.Find("GunCam");
                        var freecam = GameObject.Find("freecam");
                        GameObject.Destroy(freecam);
                        gun.SetActive(true);
                        player.GetComponent<MeshRenderer>().enabled = false;
                        camera.GetComponent<Camera>().enabled = true;
                    }
                }

                if (!Game.Instance.playing)
                {
                    isRecording = false;
                }

                if (Input.GetButton("Fire1"))
                {
                    var weaponScript = GameObject.Find("DetectWeapons").GetComponent<DetectWeapons>();
                    if (weaponScript.HasGun())
                    {
                        isShooting = true;
                    }
                }
                if (Input.GetButtonUp("Fire1"))
                {
                    var weaponScript = GameObject.Find("DetectWeapons").GetComponent<DetectWeapons>();
                    if (weaponScript.HasGun())
                    {
                        isShooting = false;
                    }
                }
                if (Input.GetButtonDown("Pickup"))
                {
                    isPick = true;
                }
                if (Input.GetButtonUp("Pickup"))
                {
                    isPick = false;
                }
                if (Input.GetButtonDown("Drop"))
                {
                    isDrop = true;
                }
                if (Input.GetButtonUp("Drop"))
                {
                    isDrop = false;
                }

                if (Input.GetKeyDown(KeyCode.L))
                {
                    Game.Instance.RestartGame();
                    isRecording = false;
                    isReplaying = true;
                    isFinishReplay = false;
                    MelonLogger.Msg("Finished Recording!");
                    MelonLogger.Msg("Player Positions Stored: " + storedPosition.Count);
                    MelonLogger.Msg("Player Rotations Stored: " + storedRotation.Count);
                }

                if (isReplaying)
                {
                    var weaponScript = GameObject.Find("DetectWeapons").GetComponent<DetectWeapons>();
                    var playerMovement = player.GetComponent<PlayerMovement>();
                    if (index >= storedPosition.Count)
                    {
                        isReplaying = false;
                        isRecording = false;
                        if (isFinishReplay)
                        {
                            Game.Instance.Win();
                        }
                        playerMovement.playerCam = camera.transform;
                        isFinishReplay = false;
                        index = 0;
                        return;
                    }

                    if (player.GetComponent<PlayerMovement>().paused == true)
                    {
                        return;
                    }

                    player.transform.position = storedPosition[index];
                    player.transform.localScale = storedScale[index];
                    playerMovement.playerCam = null;
                    camera.transform.localRotation = storedRotation[index];

                    if (storedShots[index] == true)
                    {
                        weaponScript.Fire(HitPoint());
                    }
                    if (storedPick[index] == true)
                    {
                        weaponScript.Pickup();
                    }
                    if (storedDrop[index] == true)
                    {
                        weaponScript.Throw((HitPoint() - weaponScript.weaponPos.position).normalized);
                    }

                    if (!isPlayingReplay)
                    {
                        Timer.Instance.StartTimer();
                        Timer.Instance.Stop();
                        if (isFreecam)
                        {
                            if (Input.GetKeyDown(KeyCode.C))
                            {
                                var freecam = GameObject.Find("freecam");
                                storedCameraPos.Add(freecam.transform.position);
                                storedCameraRot.Add(freecam.transform.rotation);
                            }
                        }
                    }

                    if (isFreecam)
                    {
                        if (Input.GetKeyDown(KeyCode.RightArrow))
                        {
                            selectedCam += 1;
                            var freecam = GameObject.Find("freecam");
                            freecam.transform.position = storedCameraPos[selectedCam];
                            freecam.transform.localRotation = storedCameraRot[selectedCam];
                        }
                        if (Input.GetKeyDown(KeyCode.LeftArrow))
                        {
                            selectedCam -= 1;
                            var freecam = GameObject.Find("freecam");
                            freecam.transform.position = storedCameraPos[selectedCam];
                            freecam.transform.localRotation = storedCameraRot[selectedCam];
                        }
                    }

                    if (!Game.Instance.playing)
                    {
                        return;
                    }
                    if (isPlayingReplay)
                    {
                        index += 1;
                    }
                }

                var timer = Timer.Instance.GetTimer();

                if (timer == 0f)
                {
                    if (isReplaying)
                    {
                        return;
                    }

                    if (isRecording)
                    {
                        isRecording = false;
                    }

                    MelonLogger.Msg("Recording...");
                    storedPosition = new List<Vector3>();
                    storedPosition.Clear();
                    storedScale = new List<Vector3>();
                    storedScale.Clear();
                    storedRotation = new List<Quaternion>();
                    storedRotation.Clear();
                    storedShots = new List<bool>();
                    storedShots.Clear();
                    storedPick = new List<bool>();
                    storedPick.Clear();
                    storedDrop = new List<bool>();
                    storedDrop.Clear();
                    storedCameraPos = new List<Vector3>();
                    storedCameraPos.Clear();
                    storedCameraRot = new List<Quaternion>();
                    storedCameraRot.Clear();
                    isRecording = true;
                    isReplaying = false;
                    isFreecam = false;
                    index = 0;
                    freeCamNumber = 0;
                    selectedCam = 0;
                }

                if (isRecording)
                {
                    if (player.GetComponent<PlayerMovement>().paused == true)
                    {
                        return;
                    }

                    if (player.GetComponent<PlayerMovement>().IsDead())
                    {
                        return;
                    }

                    storedPosition.Add(player.transform.position);
                    storedScale.Add(player.transform.localScale);
                    storedRotation.Add(camera.transform.localRotation);
                    storedShots.Add(isShooting);
                    storedPick.Add(isPick);
                    storedDrop.Add(isDrop);
                }
            }
            else if (scene.name == "MainMenu")
            {
                isReplaying = false;
                isRecording = false;
                isFinishReplay = false;
                index = 0;
            }
        }
    }

    public class FreeCam : MonoBehaviour
    {
        /// <summary>
        /// Normal speed of camera movement.
        /// </summary>
        public float movementSpeed = 25f;

        /// <summary>
        /// Speed of camera movement when shift is held down,
        /// </summary>
        public float fastMovementSpeed = 75f;

        /// <summary>
        /// Sensitivity for free look.
        /// </summary>
        public float freeLookSensitivity = 1.5f;

        /// <summary>
        /// Set to true when free looking (on right mouse button).
        /// </summary>
        private bool looking = false;

        void Update()
        {
            var fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (fastMode)
            {
                movementSpeed = fastMovementSpeed;
            }
            var slowMode = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (slowMode)
            {
                movementSpeed = 10f;
            }
            if (!fastMode & !slowMode)
            {
                movementSpeed = 25f;
            }

            if (Input.GetKey(KeyCode.A))
            {
                transform.position = transform.position + (-transform.right * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.D))
            {
                transform.position = transform.position + (transform.right * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.W))
            {
                transform.position = transform.position + (transform.forward * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.S))
            {
                transform.position = transform.position + (-transform.forward * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.Q))
            {
                transform.position = transform.position + (transform.up * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.E))
            {
                transform.position = transform.position + (-transform.up * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.PageUp))
            {
                transform.position = transform.position + (Vector3.up * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.PageDown))
            {
                transform.position = transform.position + (-Vector3.up * movementSpeed * Time.deltaTime);
            }

            if (looking)
            {
                float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * freeLookSensitivity;
                float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * freeLookSensitivity;
                transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
            }

            StartLooking();
        }
        public void StartLooking()
        {
            looking = true;
        }
    }
}
