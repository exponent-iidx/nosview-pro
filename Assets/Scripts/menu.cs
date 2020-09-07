using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;
using MidiJack;
using System.Security.Cryptography;
using System;

public class Chart
{
    public string title;
    public string artist;
    public string diff;
    public int level;
    public string checksum;
    public int best_score;
}

public class menu : MonoBehaviour
{
    //A List of strings that holds the file names with their respective extensions  
    private List<string> charts = new List<string>();
    //A string that stores the selected file or an error message  
    private string outputMessage = "";

    // scroller for list.
    private int cursor;

    // starting index for list.
    private int start_idx;

    // max length of list.
    private int max_list = 1000;

    private Texture2D white_tex;

    private Texture2D gray_tex;

    private Texture2D black_tex;

    private GUIStyle fStyle;

    private GUIStyle sStyle;

    private List<Chart> charts_data = new List<Chart>();

    private void Awake()
    {
        
        //Append the '@' verbatim to the directory path string  
        string directoryPath = "C:\\Users\\KiJoungJang\\Desktop\\NOSTALGIA\\Charts";
        try
        {
            seek_subfolder(directoryPath);
        }
        //Catch any of the following exceptions and store the error message at the outputMessage string  
        catch (System.UnauthorizedAccessException UAEx)
        {
            this.outputMessage = "ERROR: " + UAEx.Message;
        }
        catch (System.IO.PathTooLongException PathEx)
        {
            this.outputMessage = "ERROR: " + PathEx.Message;
        }
        catch (System.IO.DirectoryNotFoundException DirNfEx)
        {
            this.outputMessage = "ERROR: " + DirNfEx.Message;
        }
        catch (System.ArgumentException aEX)
        {
            this.outputMessage = "ERROR: " + aEX.Message;
        }
        for (int i = 0; i < charts.Count; i++)
        {
            Chart tmp_chart = new Chart();
            tmp_chart.checksum = CalculateMD5(charts[i]);
            using (StreamReader sr = File.OpenText(charts[i]))
            {
                tmp_chart.title = sr.ReadLine().Substring(6);
                tmp_chart.artist = sr.ReadLine().Substring(7);
                tmp_chart.diff = sr.ReadLine().Substring(5);
                tmp_chart.level = int.Parse(sr.ReadLine().Substring(6));
            }
            tmp_chart.best_score = PlayerPrefs.GetInt(tmp_chart.checksum, 0);
            charts_data.Add(tmp_chart);
            
        }
    }

    private void seek_subfolder(string dir)
    {
        //Get the path of all files inside the directory and save them on a List  
        List<string> fileNames = new List<string>(Directory.GetFiles(dir));
        //For each string in the fileNames List   
        for (int i = 0; i < fileNames.Count; i++)
        {
            //Append each file name to the outputString at a new line 
            if(Path.GetExtension(fileNames[i])==".nv")
            {
                charts.Add(fileNames[i]);
            }
        }
        //Get the path of all subfolders inside the directory and save them on a List 
        List<string> dirNames = new List<string>(Directory.GetDirectories(dir));
        //For each string in the dirNames List   
        for (int i = 0; i<dirNames.Count; i++)
        {
            //Append each file name to the outputString at a new line  
            seek_subfolder(dirNames[i]);
        }
    }

    private int chart_size = 30;

    private void Start()
    {
        max_list = (int)(Screen.height - 150) / chart_size - 1;
        cursor = PlayerPrefs.GetInt("cursor", 0);
        start_idx = PlayerPrefs.GetInt("start_idx", 0);
        if (cursor < start_idx || cursor >= start_idx + max_list)
        {
            cursor = 0;
            start_idx = 0;
        }
        if(start_idx + max_list - 1>= charts_data.Count-1)
        {
            start_idx -= start_idx + max_list - charts_data.Count;
            if (start_idx < 0)
                start_idx = 0;
        }
        fStyle = new GUIStyle();
        sStyle = new GUIStyle();
        white_tex = MakeTex(Screen.width - 50, 55, Color.white);
        gray_tex = MakeTex(Screen.width - 90, 40, Color.gray);
        black_tex = MakeTex(Screen.width - 50, 40, Color.black);
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for(int i = 0;i<pix.Length;i++)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private float input_tick = 0f;
    
    private bool getkey_delayed(int num)
    {
        
        if(MidiMaster.GetKey(num) > 0f && (input_tick == 0f || Time.time - input_tick > 0.1f))
        {
            input_tick = Time.time;
            return true;
        }
        else
        {
            return false;
        }

    }
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
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)|| getkey_delayed(81))
        {
            if (cursor > 0)
            {
                cursor -= 1;
                if (cursor == start_idx - 1)
                {
                    start_idx--;
                }
            }
            else
            {
                cursor = charts_data.Count - 1;
                start_idx = cursor - max_list + 1;
                if(start_idx<0)
                {
                    start_idx = 0;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || getkey_delayed(83))
        {
            if (cursor < charts.Count - 1)
            {
                cursor++;
                if (cursor == start_idx + max_list)
                {
                    start_idx++;
                }
            }
            else
            {
                cursor = 0;
                start_idx = 0;
            }
        }
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || getkey_delayed(84))
        {
            decide();
        }
        if(Input.GetAxis("Mouse ScrollWheel")<0f)
        {
            if (cursor > 0)
            {
                cursor -= 1;
                if (cursor == start_idx - 1)
                {
                    start_idx--;
                }
            }
            else
            {
                cursor = charts_data.Count - 1;
                start_idx = cursor - max_list + 1;
                if (start_idx < 0)
                {
                    start_idx = 0;
                }
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (cursor < charts.Count - 1)
            {
                cursor++;
                if (cursor == start_idx + max_list)
                {
                    start_idx++;
                }
            }
            else
            {
                cursor = 0;
                start_idx = 0;
            }
        }
    }

    private void decide()
    {
        PlayerPrefs.SetInt("cursor", cursor);
        PlayerPrefs.SetInt("start_idx", start_idx);
        PlayerPrefs.SetString("path", charts[cursor]);
        SceneManager.LoadSceneAsync(sceneName: "Player");
    }

    private void OnGUI()
    {
        // Set fStyle.
        
        fStyle.fontSize = 100;
        fStyle.normal.textColor = Color.white;
        fStyle.alignment = TextAnchor.MiddleCenter;
        fStyle.wordWrap = false;
        //GUI.Label(new Rect(Screen.width / 2, 100, 0, 0), "Nosview Pro", fStyle);
        fStyle.alignment = TextAnchor.UpperLeft;
        fStyle.fontSize = 20;
        // Set sStyle : Style for selector.
        sStyle.fontSize = 25;
        sStyle.normal.textColor = Color.black;
        sStyle.normal.background =white_tex;
        sStyle.alignment = TextAnchor.UpperLeft;
        sStyle.wordWrap = false;
        //If the outputMessage string contains the expression "ERROR: "  
        if (outputMessage.Contains("ERROR: "))
        {
            //Display an error message  
            GUI.Label(new Rect(25, Screen.height - 50, Screen.width, 100), this.outputMessage, fStyle);
            //Force an early out return of the OnGUI() method. No code below this line will get executed.  
            return;
        }
        
        int drawer_y = 50;
        string desc_string = "";
        for (int i = start_idx; i<charts_data.Count && i < start_idx + max_list; i++)
        {
            int idx = i % charts_data.Count;
            desc_string = charts_data[idx].title;

            if(i == cursor)
            {
                if(GUI.Button(new Rect(25, drawer_y, Screen.width, 30), desc_string, sStyle))
                {
                    decide();
                }
                drawer_y += chart_size;
                desc_string = charts_data[i].artist + " / " + charts_data[i].diff + " " + charts_data[i].level;
                fStyle.normal.background = gray_tex;
                GUI.Label(new Rect(65, drawer_y, Screen.width, 20), desc_string, fStyle);
            }
            else
            {
                fStyle.normal.background = black_tex;
                desc_string += " [" + charts_data[idx].diff +"]";
                if(GUI.Button(new Rect(25, drawer_y, Screen.width, 25), desc_string, fStyle))
                {
                    cursor = idx;
                }
            }
            drawer_y += chart_size;
        }
        fStyle.normal.background = black_tex;
        GUI.Button(new Rect(25, Screen.height - 50, 0, 40), "Best Score : "+charts_data[cursor].best_score, fStyle);
        desc_string = (cursor + 1).ToString() + " / " + charts_data.Count.ToString();
        fStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Button(new Rect(Screen.width /2 , Screen.height-50, 0, 40), desc_string, fStyle);
    }
}
