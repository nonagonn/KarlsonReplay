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
        private bool isRecording, isReplaying, isFinishReplay;
        private int index;

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
                    isRecording = false;
                    isReplaying = true;
                    isFinishReplay = true;
                    MelonLogger.Msg("Finished Recording!");
                    MelonLogger.Msg("Player Positions Stored: " + storedPosition.Count);
                    MelonLogger.Msg("Player Rotations Stored: " + storedRotation.Count);
                }
                
            }
            else if (Game.Instance.playing & scene.name != "MainMenu" & scene.name != "Initialize")
            {
                if (isReplaying)
                {
                    GUI.Label(new Rect(15, 1015, 300, 120), "Replaying...", myStyle);
                }
                if (isRecording)
                {
                    GUI.Label(new Rect(15, 1015, 300, 120), "Recording...", myStyle);
                }
            }
        }
        public override void OnUpdate()
        {  
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name != "MainMenu" & scene.name != "Initialize")
            {
                GameObject player = GameObject.Find("Player");
                GameObject camera = GameObject.Find("Camera");

                if (!Game.Instance.playing)
                {
                    isRecording = false;
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
                    if (index >= storedPosition.Count)
                    {
                        index = 0;
                        isReplaying = false;
                        isRecording = false;
                        if (isFinishReplay)
                        {
                            index = 0;
                            Game.Instance.Win();
                        }
                        isFinishReplay = false;
                        return;
                    }

                    
                    player.transform.position = storedPosition[index];
                    player.transform.localScale = storedScale[index];
                    camera.transform.localRotation = storedRotation[index];
                    if (!Game.Instance.playing)
                    {
                        return;
                    }
                    index += 1;
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
                    isRecording = true;
                    isReplaying = false;
                    index = 0;
                }

                if (isRecording)
                {
                    storedPosition.Add(player.transform.position);
                    storedScale.Add(player.transform.localScale);
                    storedRotation.Add(camera.transform.localRotation);
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
}
