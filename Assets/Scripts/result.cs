using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;
using MidiJack;
using System.Security.Cryptography;
using System;



public class result : MonoBehaviour
{

    private string CalculateMD5(string filename)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filename))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    private GUIStyle fStyle;

    private GUIStyle sStyle;

    private Texture2D white_tex;

    private void Awake()
    {
    }

    private int prev_score = 0;

    private void Start()
    {
        // Set fStyle.
        fStyle = new GUIStyle();

        // Set sStyle : Style for selector.
        sStyle = new GUIStyle();

        white_tex = MakeTex(Screen.width, Screen.height, Color.white);
        prev_score = PlayerPrefs.GetInt(CalculateMD5(PlayerPrefs.GetString("path")), 0);

    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Escape) || MidiMaster.GetKey(36)>0f && (input_tick == 0f || Time.time - input_tick > 0.1f))
        {
            input_tick = Time.time;
            if (prev_score < PlayerPrefs.GetInt("score"))
            {
                PlayerPrefs.SetInt(CalculateMD5(PlayerPrefs.GetString("path")), PlayerPrefs.GetInt("score"));
            }
            SceneManager.LoadSceneAsync(sceneName: "Menu");
        }
    }

    private float input_tick = 0f;

    private void OnGUI()
    {
        fStyle.normal.textColor = Color.white;
        fStyle.wordWrap = false;
        fStyle.alignment = TextAnchor.UpperLeft;
        fStyle.fontSize = 60;
        sStyle.fontSize = 100;
        sStyle.normal.textColor = Color.black;
        sStyle.normal.background = white_tex;
        sStyle.alignment = TextAnchor.UpperLeft;
        sStyle.wordWrap = false;
        GUI.Label(new Rect(50, 50, Screen.width - 100, 100), PlayerPrefs.GetString("title"), sStyle);
        GUI.Label(new Rect(50, 180, Screen.width - 100, 100), PlayerPrefs.GetString("artist") +" / "+PlayerPrefs.GetString("diff")+" "+PlayerPrefs.GetInt("level").ToString(), fStyle);
        sStyle.fontSize = 190;
        GUI.Label(new Rect(50, Screen.height / 2 - 110, Screen.width - 100, 270), PlayerPrefs.GetInt("score").ToString(), sStyle);
        sStyle.fontSize = 50;
        GUI.Label(new Rect(50, Screen.height / 2 + 90, Screen.width - 100, 50), "BEST SCORE "+prev_score.ToString(), sStyle);
        GUI.Label(new Rect(50, Screen.height - 200, Screen.width - 100, 100), "Just* "+PlayerPrefs.GetInt("pjust").ToString() + " Just " + PlayerPrefs.GetInt("just").ToString() + " Good " + PlayerPrefs.GetInt("good").ToString() + " Miss " + PlayerPrefs.GetInt("miss").ToString()  , fStyle);
        GUI.Label(new Rect(50, Screen.height - 100, Screen.width - 100, 100), "Max Combo " + PlayerPrefs.GetInt("max_combo").ToString() + "(Fast " + PlayerPrefs.GetInt("fast").ToString() + ", Slow " + PlayerPrefs.GetInt("slow").ToString() +")", fStyle);

    }
}
