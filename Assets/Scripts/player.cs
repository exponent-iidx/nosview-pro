// player
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using MidiJack;
using UnityEngine.U2D;

public class Note
{
    public int note_id;

    public string type;

    public int x_pos;

    public float timing;

    public int x_pos2;

    public float timing2;

    public int width;

    public bool hand;

    public bool in_scene;
}

// Coordinate
public class Coordinate
{
    public float x;

    public float y;

    public float multiplier;

    public float opacity;
}

public class Effect
{
    public int x_pos;

    public float width;

    public float timing;

    public float score;

    public bool add_combo = false;

    public GameObject obj;

    public GameObject text;

    public GameObject fs;
}



public class Rendering_Note
{
    public Note note;

    public GameObject obj;

    public List<GameObject> obj2;

    public GameObject obj4;

    public float score;

    public float pushing_time;

    public float pushing_trill;
}

public class Bpm
{
    public int id;
    public float bpm;
    public float timing;
}

public class Line
{
    public float timing;
    public bool in_scene;
}

public class Rendering_Line
{
    public Line line;
    public GameObject obj;
}



public class player : MonoBehaviour
{
    private List<Rendering_Note> lock_check = new List<Rendering_Note>();

	private float pjust_time = 0.042f;

	private float just_time = 0.084f;

	private float good_time = 0.168f;

	private float effect_time = 0.2f;

	private float text_time = 0.5f;

	private List<Rendering_Note> rendering_Notes = new List<Rendering_Note>();

    private List<Rendering_Line> rendering_Lines = new List<Rendering_Line>();

	private KeyCode[] keys = new KeyCode[28];

    private int[] midi_keys = new int[28];

	private List<Note> notes = new List<Note>();

	private List<Effect> rendering_Effects = new List<Effect>();

    private List<Bpm> bpms = new List<Bpm>();

	public GameObject notpush;

	public GameObject push;

	public GameObject note_l;

	public GameObject note_r;

	public GameObject judge_bar;

	public GameObject pjust_effect;

	public GameObject just_effect;

	public GameObject good_effect;

	public GameObject pjust_text;

	public GameObject just_text;

	public GameObject good_text;

	public GameObject miss_text;

	public GameObject combo_title;

	public GameObject combo_text;

	public GameObject score_title;

	public GameObject score_text;

    public GameObject object_speaker;

    public GameObject fast_text;

    public GameObject slow_text;

    public GameObject line_120;

    public GameObject line_20;

    public GameObject line_2;

    public GameObject tenuto_left;

    public GameObject tenuto_right;

    public GameObject dark_l;

    public GameObject dark_r;

    public GameObject trill_l;
    public GameObject trill_r;
    public GameObject gli_l;
    public GameObject gli_r;

    private float hs = 5f;

	private float offset = 500f;

    private float trill_tolerance = 0.3f;

	private float float_PI = Convert.ToSingle(Math.PI);

	private string trajectory = "Splash";

	private float judge_offset = 200f;

	private float timing_offset;

	private float splash_top = 0.1f;

	private float bs = -300f;

	private int nid;

	private float y_top = -Screen.height;

	private float y_bottom = 2 * Screen.height;

	private GameObject[] push_effects = new GameObject[28];

	private float score;

	private float total_score;

	private int total_note;

	private int passed_note;

	private int pjust;

	private int just;

	private int good;

    private int bid = 0;
	private int miss;

    private bool autoplay = true;

	private int combo;

	private float off_timer;

	private bool ready_to_off;

	private int max_combo;

    private int fast = 0;
    private int slow = 0;

    private float sync_offset = 0f;

    private AudioClip clip;

    private float p_offset = 0f;

    private float start_time;

	private float get_y(float timing)
	{
		return (timing - offset) * hs + (float)Screen.height;
	}

	private bool is_object(float y)
	{
		if (y > y_top)
		{
			return y < y_bottom;
		}
		return false;
	}

	private bool is_object(float y, float y2)
	{
		if (y > y_top || y2 > y_top)
		{
			if (!(y < y_bottom))
			{
				return y2 < y_bottom;
			}
			return true;
		}
		return false;
	}

	private bool is_object(Note note)
	{
		if (note.type == "Note")
		{
			return is_object(get_y(note.timing));
		}
		return is_object(get_y(note.timing), get_y(note.timing2));
	}

	private float float_Sin(float x)
	{
		return Convert.ToSingle(Math.Sin(x));
	}

	private float float_Cos(float x)
	{
		return Convert.ToSingle(Math.Cos(x));
	}

	private Coordinate get_Coordinates(float x, float y)
	{
		Coordinate coordinate = new Coordinate();
		if (trajectory == "editor")
		{
			coordinate.x = x;
			coordinate.y = y;
			coordinate.multiplier = 1f;
			coordinate.opacity = 1f;
		}
		else if (trajectory == "Splash")
		{
			coordinate.multiplier = 2f / (3f - float_Sin(0.5f * float_PI * y / (float)Screen.height));
			coordinate.opacity = Math.Max(0f, Math.Min(1f, 1f + 2f * float_Sin(0.5f * float_PI * y / (float)Screen.height)));
			coordinate.y = (float)Screen.height - judge_offset + float_Cos(0.5f * float_PI * y / (float)Screen.height) * ((splash_top - 1f) * (float)Screen.height + judge_offset);
			coordinate.x = (x - (float)Screen.width / 2f) * coordinate.multiplier + (float)Screen.width / 2f;
		}
		else if (trajectory == "Approach")
		{
			coordinate.multiplier = 2f / 3f + 0.333333343f * (y / (float)Screen.height);
			coordinate.opacity = 1f;
			coordinate.y = ((float)Screen.height - judge_offset) * y / (float)Screen.height;
			coordinate.x = (x - (float)Screen.width / 2f) * coordinate.multiplier + (float)Screen.width / 2f;
		}
		else if (trajectory == "Slope")
		{
			coordinate.multiplier = 2f / (3f - y / (float)Screen.height);
			coordinate.opacity = 1f;
			coordinate.y = ((float)Screen.height - judge_offset) * y / (float)Screen.height;
			coordinate.x = (x - (float)Screen.width / 2f) * coordinate.multiplier + (float)Screen.width / 2f;
		}
		else if (trajectory == "Vertical")
		{
			coordinate.x = x;
			coordinate.y = y - judge_offset;
			coordinate.multiplier = 1f;
			coordinate.opacity = 1f;
		}
		return coordinate;
	}

    private float cur_bpm(float cur_time)
    {
        if(cur_time>0f)
        {
            return cur_bpm(-1f);
        }
        int opt_id = -1;
        float opt_timing = 0f;
        float value = 150.0f;
        foreach (Bpm bpm in bpms)
        {
            float tmp = bpm.timing;
            if (tmp > cur_time - 0.00001 && (opt_id == -1 || tmp < opt_timing))
            {
                opt_id = bpm.id;
                opt_timing = bpm.timing;
                value = bpm.bpm;
            }
        }
        return value;
    }

    

	private void Awake()
	{
		keys[0] = KeyCode.BackQuote;
		keys[1] = KeyCode.Alpha1;
		keys[2] = KeyCode.Alpha2;
		keys[3] = KeyCode.Alpha3;
		keys[4] = KeyCode.Alpha4;
		keys[5] = KeyCode.Alpha5;
		keys[6] = KeyCode.Alpha6;
		keys[7] = KeyCode.Alpha7;
		keys[8] = KeyCode.Alpha8;
		keys[9] = KeyCode.Alpha9;
		keys[10] = KeyCode.Alpha0;
		keys[11] = KeyCode.Minus;
		keys[12] = KeyCode.Equals;
		keys[13] = KeyCode.Backslash;
		keys[14] = KeyCode.Tab;
		keys[15] = KeyCode.Q;
		keys[16] = KeyCode.W;
		keys[17] = KeyCode.E;
		keys[18] = KeyCode.R;
		keys[19] = KeyCode.T;
		keys[20] = KeyCode.Y;
		keys[21] = KeyCode.U;
		keys[22] = KeyCode.I;
		keys[23] = KeyCode.O;
		keys[24] = KeyCode.P;
		keys[25] = KeyCode.LeftBracket;
		keys[26] = KeyCode.RightBracket;
		keys[27] = KeyCode.Return;
        midi_keys[0] = 36;
        midi_keys[1] = 38;
        midi_keys[2] = 40;
        midi_keys[3] = 41;
        midi_keys[4] = 43;
        midi_keys[5] = 45;
        midi_keys[6] = 47;
        midi_keys[7] = 48;
        midi_keys[8] = 50;
        midi_keys[9] = 52;
        midi_keys[10] = 53;
        midi_keys[11] = 55;
        midi_keys[12] = 57;
        midi_keys[13] = 59;
        midi_keys[14] = 60;
        midi_keys[15] = 62;
        midi_keys[16] = 64;
        midi_keys[17] = 65;
        midi_keys[18] = 67;
        midi_keys[19] = 69;
        midi_keys[20] = 71;
        midi_keys[21] = 72;
        midi_keys[22] = 74;
        midi_keys[23] = 76;
        midi_keys[24] = 77;
        midi_keys[25] = 79;
        midi_keys[26] = 81;
        midi_keys[27] = 83;

        StreamReader streamReader = File.OpenText(PlayerPrefs.GetString("path"));
		string text = "";
		while ((text = streamReader.ReadLine()) != null)
		{
			string[] array = text.Split(' ');
			if (array[0] == "TITLE")
			{
				PlayerPrefs.SetString("title", text.Substring(6));
			}
			if (array[0] == "ARTIST")
			{
				PlayerPrefs.SetString("artist", text.Substring(7));
			}
			if (array[0] == "DIFF")
			{
				PlayerPrefs.SetString("diff", text.Substring(5));
			}
			if (array[0] == "LEVEL")
			{
				PlayerPrefs.SetInt("level", int.Parse(text.Substring(6)));
			}
			if (array[0] == "N")
			{
				Note note = new Note();
				note.note_id = nid++;
				note.type = "Note";
				note.x_pos = int.Parse(array[4]);
				note.timing = float.Parse(array[2]) * bs;
				note.hand = (array[6] == "R");
				note.width = int.Parse(array[8]);
				note.in_scene = false;
				notes.Add(note);
				total_score += 1f;
				total_note++;
			}
            if (array[0] == "T")
            {
                Note note = new Note();
                note.note_id = nid++;
                note.type = "Tenuto";
                note.x_pos = int.Parse(array[4]);
                note.timing = float.Parse(array[2]) * bs;
                note.hand = (array[6] == "R");
                note.width = int.Parse(array[8]);
                note.timing2 = float.Parse(array[10]) * bs;
                note.in_scene = false;
                notes.Add(note);
                total_score += 4f;
                total_note++;
            }
            if (array[0] == "TR")
            {
                Note note = new Note();
                note.note_id = nid++;
                note.type = "Trill";
                note.x_pos = int.Parse(array[4]);
                note.timing = float.Parse(array[2]) * bs;
                note.hand = (array[6] == "R");
                note.width = int.Parse(array[8]);
                note.timing2 = float.Parse(array[10]) * bs;
                note.in_scene = false;
                notes.Add(note);
                total_score += 5f;
                total_note++;
            }
            if (array[0] == "G")
            {
                Note note = new Note();
                note.note_id = nid++;
                note.type = "Glissando";
                note.x_pos = int.Parse(array[4]);
                note.timing = float.Parse(array[2]) * bs;
                note.hand = (array[6] == "R");
                note.width = int.Parse(array[8]);
                note.timing2 = float.Parse(array[10]) * bs;
                note.x_pos2 = int.Parse(array[12]);
                note.in_scene = false;
                notes.Add(note);
                total_score += 0.5f;
                total_note++;
            }
            if (array[0] == "MUSIC")
            {
                string music_dir = Directory.GetParent(PlayerPrefs.GetString("path")).FullName + "\\" + text.Substring(6);
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(music_dir, AudioType.OGGVORBIS))
                {
                    AsyncOperation request = www.SendWebRequest();
                    while (!request.isDone)
                    {
                        float tmp = request.progress;
                    }
                     clip = DownloadHandlerAudioClip.GetContent(www);
                }
            }
            if (array[0] == "OFFSET")
            {
                p_offset = float.Parse(text.Substring(7)) / 1000f;
            }
            if (array[0] == "BPM")
            {
                Bpm bpmt = new Bpm();
                bpmt.id = bid++;
                bpmt.bpm = float.Parse(array[2]);
                bpmt.timing = float.Parse(array[1]) * bs;
                bpms.Add(bpmt);
            }
		}
        float cursor = 0.0f;
        int max_ind = -1;
        int min_ind = -1;
        float max_time = 0;
        float min_time = 0;
        foreach (Note note in notes)
        {
            if (min_ind == -1 || min_time < note.timing)
            {
                min_time = note.timing;
                min_ind = note.note_id;
            }
            if (min_ind == -1 || (note.type != "Note" && min_time < note.timing2))
            {
                min_time = note.timing;
                min_ind = note.note_id;
            }
            if (max_ind == -1 || max_time > note.timing)
            {
                max_time = note.timing;
                max_ind = note.note_id;
            }
            if (max_ind == -1 || (note.type != "Note" && max_time > note.timing2))
            {
                max_time = note.timing;
                max_ind = note.note_id;
            }
        }
        float old_bpm = cur_bpm(cursor + 1.0f);
        var chord_num = 0;
        cursor = -600f / old_bpm * bs;
        while (cursor > max_time + 240f / old_bpm * bs)
        {
            float bpm = cur_bpm(cursor);
            chord_num += 1;
            Line line = new Line();
            line.timing = cursor;
            line.in_scene = false;
            lines.Add(line);
            if (old_bpm!=bpm)
            {
                int opt_id = -1;
                float opt_time = 0f;
                foreach(Bpm bpmt in bpms)
                {
                    float tmp = bpmt.timing;
                    if(tmp > cursor - 0.00001 && (opt_id == -1 || tmp<opt_time))
                    {
                        opt_id = 0;
                        opt_time = tmp;
                    }
                }
                cursor = opt_time;
            }
            cursor += 60.0f / bpm * bs;
            old_bpm = bpm;
        }
        Cursor.visible = false;
    }

	private float get_x(int x)
	{
		return (float)(x * Screen.width) / 28f;
	}

	private float get_x(float x)
	{
		return x * (float)Screen.width / 28f;
	}

    private float music_start_time;

    private bool[] pushing = new bool[28];

    private bool[] locked = new bool[28];

    private List<Line> lines = new List<Line>();

    private bool midi_mode = true;

    private Texture2D MakeTex(int width, int height, Color col)
    { 
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            int x = i % width;
            float fx = (float)x / (float)width;
            pix[i] = col * fx + (1f-fx) * new Color(1f,1f,1f,col.a);
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }


    private void Start()
	{
		for (int i = 0; i < 28; i++)
		{
			float num = 100f * (float)Screen.width / (560f * (float)Screen.height);
			UnityEngine.Object.Instantiate(notpush, canvas_to_unity(new Vector2(get_x((float)i + 0.5f), (float)Screen.height - judge_offset + (float)Screen.width / 28f * 2.5f), 0.1f), Quaternion.identity).transform.localScale = new Vector3(num, num, 1f);
			push_effects[i] = UnityEngine.Object.Instantiate(push, canvas_to_unity(new Vector2(get_x((float)i + 0.5f), (float)Screen.height - judge_offset + (float)Screen.width / 28f * 2.5f)), Quaternion.identity);
			push_effects[i].transform.localScale = new Vector3(num, num, 1f);
			push_effects[i].SetActive(value: false);

        }
        int[] line_idx = { 0, 3, 6, 9, 12, 16, 19, 22, 25, 28 };
        foreach(int i in line_idx)
        {
            GameObject line_obj = UnityEngine.Object.Instantiate(line_120, new Vector2(0f, 0f), Quaternion.identity);
            for (int j = 0; j < 120; j++)
            {
                float xc = get_x(i);
                float yc = (float)Screen.height * (1f - 0.01f * j);
                Coordinate coord = get_Coordinates(xc, yc);
                line_obj.GetComponent<LineRenderer>().SetPosition(j, canvas_to_unity(new Vector2(coord.x, coord.y), 2f));
            }
            line_obj.GetComponent<LineRenderer>().material.color = new Color(1f, 1f, 1f, 0.4f);
            line_obj.SetActive(true);
        }
        Coordinate coordinate = get_Coordinates((float)Screen.width / 2f, Screen.height);
		GameObject gameObject = UnityEngine.Object.Instantiate(judge_bar, canvas_to_unity(new Vector2(coordinate.x, coordinate.y), -0.5f), Quaternion.identity);
		float num2 = 10f * (float)Screen.width / ((float)Screen.height * 11.2f);
		gameObject.transform.localScale = new Vector3(num2, num2, 1f);
		combo_text.SetActive(value: false);
		combo_title.SetActive(value: false);
        offset = 900f + p_offset * bs;
        start_time = Time.time;

        combo_text.GetComponent<Text>().fontSize = (int)Math.Round(180f * (float)Screen.height / 1440f);
        score_text.GetComponent<Text>().GetComponent<RectTransform>().anchoredPosition += new Vector2(0f,50f * (1f - (float)Screen.height / 1440f));
        combo_title.GetComponent<Text>().fontSize = (int)Math.Round(50f * (float)Screen.height / 1440f);
        score_text.GetComponent<Text>().fontSize = (int)Math.Round(100f * (float)Screen.height / 1440f);
        score_title.GetComponent<Text>().fontSize = (int)Math.Round(50f * (float)Screen.height / 1440f);
        combo_text.GetComponent<Text>().GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, - 300f * (1f - (float)Screen.height / 1440f));
        combo_title.GetComponent<Text>().GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, - 450f * (1f - (float)Screen.height / 1440f));

        for(int i=0;i<28;i++)
        {
            pushing[i] = false;
            locked[i] = false;
        }
        red_tex = MakeTex(Screen.width, Screen.height, new Color(255f / 255f, 83f / 255f, 83f / 255f, 0.8f));
        blue_tex = MakeTex(Screen.width, Screen.height, new Color(107f / 255f, 218f / 255f, 255f / 255f, 0.8f));
    }

	private Vector2 canvas_to_unity(Vector2 vec)
	{
		Vector2 result = default(Vector2);
		float num = Screen.width;
		float num2 = Screen.height;
		result.x = (vec.x * 10f / num - 5f) * num / num2;
		result.y = (num2 - vec.y) * 10f / num2 - 5f;
		return result;
	}

	private Vector3 canvas_to_unity(Vector2 vec, float z)
	{
		Vector3 result = default(Vector3);
		float num = Screen.width;
		float num2 = Screen.height;
		result.x = (vec.x * 10f / num - 5f) * num / num2;
		result.y = (num2 - vec.y) * 10f / num2 - 5f;
		result.z = z;
		return result;
	}

    private Texture2D red_tex;
    private Texture2D blue_tex;

    private int tenuto_N = 100;

    private bool playing = false;

    private int compare_Notes(Rendering_Note x, Rendering_Note y)
    {
        return -(x.note.timing).CompareTo(y.note.timing);
    }

	private void Update()
	{
        offset = 900f + (p_offset * bs) + (Time.time - start_time) * bs;
        if (clip!=null && !playing && offset< (p_offset - sync_offset) * bs)
        {
            AudioSource speaker = object_speaker.GetComponent<AudioSource>();
            speaker.PlayOneShot(clip);

            playing = true;
            music_start_time = Time.time;
        }
        
		if (!ready_to_off && total_note == passed_note && !(object_speaker.GetComponent<AudioSource>().isPlaying))
		{
            ready_to_off = true;
			off_timer = offset;
		}
		if (ready_to_off && offset < off_timer + 5f * bs)
		{
			PlayerPrefs.SetInt("pjust", pjust);
			PlayerPrefs.SetInt("just", just);
			PlayerPrefs.SetInt("good", good);
			PlayerPrefs.SetInt("miss", miss);
            PlayerPrefs.SetInt("fast", fast);
            PlayerPrefs.SetInt("slow", slow);
			PlayerPrefs.SetInt("score", (int)Math.Round(score / total_score * 1000000f, 0));
			PlayerPrefs.SetInt("max_combo", max_combo);
			SceneManager.LoadSceneAsync("Result");
		}
        for (int i=0;i<lines.Count;i++)
        {
            if(!lines[i].in_scene && is_object(get_y(lines[i].timing)))
            {
                Rendering_Line rl = new Rendering_Line();
                rl.line = lines[i];
                rl.obj = Instantiate(line_2);
                Coordinate coord = get_Coordinates(0f, get_y(lines[i].timing));
                rl.obj.GetComponent<LineRenderer>().SetPosition(0, canvas_to_unity(new Vector2(coord.x, coord.y),2f));
                rl.obj.GetComponent<LineRenderer>().SetPosition(1, canvas_to_unity(new Vector2(coord.x  + (float)Screen.width * coord.multiplier, coord.y), 2f));
                rl.obj.GetComponent<LineRenderer>().material.color = new Color(1f, 1f, 1f, 0.5f * coord.opacity);
                rl.obj.SetActive(true);
                rendering_Lines.Add(rl);
                lines[i].in_scene = true;
            }
        }
		for (int i = 0; i < notes.Count; i++)
		{
			if (!is_object(notes[i]) || notes[i].in_scene)
			{
				continue;
			}
            if (notes[i].type == "Note")
            {
                float num = 10f * (float)Screen.width / (float)Screen.height * 3f / 28f / 12f;
                bool num2 = is_object(notes[i]);
                Coordinate coordinate = get_Coordinates(get_x((float)notes[i].x_pos + 0.5f * (float)notes[i].width), get_y(notes[i].timing));
                if (num2 && !notes[i].in_scene)
                {
                    Rendering_Note rendering_Note = new Rendering_Note();
                    rendering_Note.note = notes[i];
                    if (!notes[i].hand)
                    {
                        rendering_Note.obj = UnityEngine.Object.Instantiate(note_l, canvas_to_unity(new Vector2(coordinate.x, coordinate.y), -1f), Quaternion.identity);
                        rendering_Note.obj.transform.localScale = new Vector3(num * coordinate.multiplier * (float)notes[i].width / 3f, num * coordinate.multiplier, 1f);
                        rendering_Note.obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate.opacity);
                    }
                    else
                    {
                        rendering_Note.obj = UnityEngine.Object.Instantiate(note_r, canvas_to_unity(new Vector2(coordinate.x, coordinate.y), -1f), Quaternion.identity);
                        rendering_Note.obj.transform.localScale = new Vector3(num * coordinate.multiplier * (float)notes[i].width / 3f, num * coordinate.multiplier, 1f);
                        rendering_Note.obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate.opacity);
                    }
                    rendering_Note.score = -1f;
                    rendering_Notes.Add(rendering_Note);
                }
                notes[i].in_scene = true;
            }
            if (notes[i].type == "Tenuto")
            {
                float num = 10f * (float)Screen.width / (float)Screen.height * 3f / 28f / 12f;
                bool num2 = is_object(notes[i]);
                Coordinate coordinate = get_Coordinates(get_x((float)notes[i].x_pos + 0.5f * (float)notes[i].width), get_y(notes[i].timing));
                Coordinate coordinate4 = get_Coordinates(get_x((float)notes[i].x_pos + 0.5f * (float)notes[i].width), get_y(notes[i].timing2));
                if (num2 && !notes[i].in_scene)
                {
                    Rendering_Note rendering_Note = new Rendering_Note();
                    rendering_Note.note = notes[i];
                    if (!notes[i].hand)
                    {
                        rendering_Note.obj = UnityEngine.Object.Instantiate(note_l, canvas_to_unity(new Vector2(coordinate.x, coordinate.y), -1f), Quaternion.identity);
                        rendering_Note.obj.transform.localScale = new Vector3(num * coordinate.multiplier * (float)notes[i].width / 3f, num * coordinate.multiplier, 1f);
                        rendering_Note.obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate.opacity);
                        rendering_Note.obj2 = new List<GameObject>();
                        GameObject obj = new GameObject();
                        obj.name = "Tenuto";
                        obj.AddComponent<MeshFilter>();
                        obj.AddComponent<MeshRenderer>();
                        Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
                        mesh.Clear();
                        int idx = 0;
                        List<Vector3> vertices = new List<Vector3>();
                        List<Vector2> uv = new List<Vector2>();
                        List<int> triangles = new List<int>();
                        for(int j=0;j<tenuto_N;j++)
                        {
                            float xc = get_x((float)notes[i].x_pos + 0.05f * (float)notes[i].width-1f);
                            float yc = get_y(notes[i].timing + (notes[i].timing2 - notes[i].timing) * j / (tenuto_N - 1));
                            Coordinate coord = get_Coordinates(xc, yc);
                            Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                            vertices.Add(canvas_to_unity(new Vector2(coord.x,coord.y), -0.5f));
                            uv.Add(vec);
                            if (j < tenuto_N - 1)
                            {
                                triangles.Add(j);
                                triangles.Add(2 * tenuto_N - 1 - j);
                                triangles.Add(j + 1);
                            }
                        }
                        for(int j=tenuto_N-1;j>=0;j--)
                        {
                            float xc = get_x((float)notes[i].x_pos + 0.95f * (float)notes[i].width -1f);
                            float yc = get_y(notes[i].timing + (notes[i].timing2 - notes[i].timing) * j / (tenuto_N - 1));
                            Coordinate coord = get_Coordinates(xc, yc);
                            Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                            vertices.Add(canvas_to_unity(new Vector2(coord.x, coord.y), -0.5f));
                            uv.Add(vec);
                            if (j>0)
                            {
                                triangles.Add(2 * tenuto_N - j);
                                triangles.Add(2 * tenuto_N - 1 - j);
                                triangles.Add(j);
                            }
                        }
                        mesh.vertices = vertices.ToArray();
                        mesh.uv = uv.ToArray();
                        mesh.triangles = triangles.ToArray();
                        obj.GetComponent<MeshRenderer>().material = new Material(Shader.Find("UI/Default"));
                        obj.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f);
                        rendering_Note.obj2.Add(obj);

                        rendering_Note.obj4 = UnityEngine.Object.Instantiate(dark_l, canvas_to_unity(new Vector2(coordinate4.x, coordinate4.y), -1f), Quaternion.identity);
                        rendering_Note.obj4.transform.localScale = new Vector3(num * coordinate4.multiplier * (float)notes[i].width / 3f, num * coordinate4.multiplier, 1f);
                        rendering_Note.obj4.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate4.opacity);
                    }
                    else
                    {
                        rendering_Note.obj = UnityEngine.Object.Instantiate(note_r, canvas_to_unity(new Vector2(coordinate.x, coordinate.y), -1f), Quaternion.identity);
                        rendering_Note.obj.transform.localScale = new Vector3(num * coordinate.multiplier * (float)notes[i].width / 3f, num * coordinate.multiplier, 1f);
                        rendering_Note.obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate.opacity);

                        rendering_Note.obj2 = new List<GameObject>();
                        GameObject obj = new GameObject();
                        obj.name = "Tenuto";
                        obj.AddComponent<MeshFilter>();
                        obj.AddComponent<MeshRenderer>();
                        Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
                        mesh.Clear();
                        int idx = 0;
                        List<Vector3> vertices = new List<Vector3>();
                        List<Vector2> uv = new List<Vector2>();
                        List<int> triangles = new List<int>();
                        for (int j = 0; j < tenuto_N; j++)
                        {
                            float xc = get_x((float)notes[i].x_pos + 0.05f * (float)notes[i].width);
                            float yc = get_y(notes[i].timing + (notes[i].timing2 - notes[i].timing) * j / (tenuto_N - 1));
                            Coordinate coord = get_Coordinates(xc, yc);
                            Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                            vertices.Add(canvas_to_unity(new Vector2(coord.x, coord.y), -0.5f));
                            uv.Add(vec);
                            if (j < tenuto_N - 1)
                            {
                                triangles.Add(j);
                                triangles.Add(2 * tenuto_N - 1 - j);
                                triangles.Add(j + 1);
                            }
                        }
                        for (int j = tenuto_N - 1; j >= 0; j--)
                        {
                            float xc = get_x((float)notes[i].x_pos + 0.95f * (float)notes[i].width);
                            float yc = get_y(notes[i].timing + (notes[i].timing2 - notes[i].timing) * j / (tenuto_N - 1));
                            Coordinate coord = get_Coordinates(xc, yc);
                            Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                            vertices.Add(canvas_to_unity(new Vector2(coord.x, coord.y), -0.5f));
                            uv.Add(vec);
                            if (j > 0)
                            {
                                triangles.Add(2 * tenuto_N - j);
                                triangles.Add(2 * tenuto_N - 1 - j);
                                triangles.Add(j);
                            }
                        }
                        mesh.vertices = vertices.ToArray();
                        mesh.uv = uv.ToArray();
                        mesh.triangles = triangles.ToArray();
                        obj.GetComponent<MeshRenderer>().material = new Material(Shader.Find("UI/Default"));
                        obj.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f);
                        rendering_Note.obj2.Add(obj);


                        rendering_Note.obj4 = UnityEngine.Object.Instantiate(dark_r, canvas_to_unity(new Vector2(coordinate4.x, coordinate4.y), -1f), Quaternion.identity);
                        rendering_Note.obj4.transform.localScale = new Vector3(num * coordinate4.multiplier * (float)notes[i].width / 3f, num * coordinate4.multiplier, 1f);
                        rendering_Note.obj4.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate4.opacity);

                    }
                    rendering_Note.score = -1f;
                    rendering_Notes.Add(rendering_Note);
                }
                notes[i].in_scene = true;
            }
            if (notes[i].type == "Trill")
            {
                float num = 10f * (float)Screen.width / (float)Screen.height * 3f / 28f / 12f;
                bool num2 = is_object(notes[i]);
                Coordinate coordinate = get_Coordinates(get_x((float)notes[i].x_pos + 0.5f * (float)notes[i].width), get_y(notes[i].timing));
                Coordinate coordinate4 = get_Coordinates(get_x((float)notes[i].x_pos + 0.5f * (float)notes[i].width), get_y(notes[i].timing2));
                if (num2 && !notes[i].in_scene)
                {
                    Rendering_Note rendering_Note = new Rendering_Note();
                    rendering_Note.note = notes[i];
                    if (!notes[i].hand)
                    {
                        rendering_Note.obj = UnityEngine.Object.Instantiate(trill_l, canvas_to_unity(new Vector2(coordinate.x, coordinate.y), -1f), Quaternion.identity);
                        rendering_Note.obj.transform.localScale = new Vector3(num * coordinate.multiplier * (float)notes[i].width / 3f, num * coordinate.multiplier, 1f);
                        rendering_Note.obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate.opacity);
                        rendering_Note.obj2 = new List<GameObject>();
                        GameObject obj = new GameObject();
                        obj.name = "Trill";
                        obj.AddComponent<MeshFilter>();
                        obj.AddComponent<MeshRenderer>();
                        Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
                        mesh.Clear();
                        int idx = 0;
                        List<Vector3> vertices = new List<Vector3>();
                        List<Vector2> uv = new List<Vector2>();
                        List<int> triangles = new List<int>();
                        for (int j = 0; j < tenuto_N; j++)
                        {
                            float xc = get_x((float)notes[i].x_pos + 0.05f * (float)notes[i].width - 1f);
                            float yc = get_y(notes[i].timing + (notes[i].timing2 - notes[i].timing) * j / (tenuto_N - 1));
                            Coordinate coord = get_Coordinates(xc, yc);
                            Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                            vertices.Add(canvas_to_unity(new Vector2(coord.x, coord.y), -0.5f));
                            uv.Add(vec);
                            if (j < tenuto_N - 1)
                            {
                                triangles.Add(j);
                                triangles.Add(2 * tenuto_N - 1 - j);
                                triangles.Add(j + 1);
                            }
                        }
                        for (int j = tenuto_N - 1; j >= 0; j--)
                        {
                            float xc = get_x((float)notes[i].x_pos + 0.95f * (float)notes[i].width - 1f);
                            float yc = get_y(notes[i].timing + (notes[i].timing2 - notes[i].timing) * j / (tenuto_N - 1));
                            Coordinate coord = get_Coordinates(xc, yc);
                            Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                            vertices.Add(canvas_to_unity(new Vector2(coord.x, coord.y), -0.5f));
                            uv.Add(vec);
                            if (j > 0)
                            {
                                triangles.Add(2 * tenuto_N - j);
                                triangles.Add(2 * tenuto_N - 1 - j);
                                triangles.Add(j);
                            }
                        }
                        mesh.vertices = vertices.ToArray();
                        mesh.uv = uv.ToArray();
                        mesh.triangles = triangles.ToArray();
                        obj.GetComponent<MeshRenderer>().material = new Material(Shader.Find("UI/Default"));
                        obj.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f);
                        rendering_Note.obj2.Add(obj);

                        rendering_Note.obj4 = UnityEngine.Object.Instantiate(dark_l, canvas_to_unity(new Vector2(coordinate4.x, coordinate4.y), -1f), Quaternion.identity);
                        rendering_Note.obj4.transform.localScale = new Vector3(num * coordinate4.multiplier * (float)notes[i].width / 3f, num * coordinate4.multiplier, 1f);
                        rendering_Note.obj4.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate4.opacity);
                    }
                    else
                    {
                        rendering_Note.obj = UnityEngine.Object.Instantiate(trill_r, canvas_to_unity(new Vector2(coordinate.x, coordinate.y), -1f), Quaternion.identity);
                        rendering_Note.obj.transform.localScale = new Vector3(num * coordinate.multiplier * (float)notes[i].width / 3f, num * coordinate.multiplier, 1f);
                        rendering_Note.obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate.opacity);

                        rendering_Note.obj2 = new List<GameObject>();
                        GameObject obj = new GameObject();
                        obj.name = "Tenuto";
                        obj.AddComponent<MeshFilter>();
                        obj.AddComponent<MeshRenderer>();
                        Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
                        mesh.Clear();
                        int idx = 0;
                        List<Vector3> vertices = new List<Vector3>();
                        List<Vector2> uv = new List<Vector2>();
                        List<int> triangles = new List<int>();
                        for (int j = 0; j < tenuto_N; j++)
                        {
                            float xc = get_x((float)notes[i].x_pos + 0.05f * (float)notes[i].width);
                            float yc = get_y(notes[i].timing + (notes[i].timing2 - notes[i].timing) * j / (tenuto_N - 1));
                            Coordinate coord = get_Coordinates(xc, yc);
                            Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                            vertices.Add(canvas_to_unity(new Vector2(coord.x, coord.y), -0.5f));
                            uv.Add(vec);
                            if (j < tenuto_N - 1)
                            {
                                triangles.Add(j);
                                triangles.Add(2 * tenuto_N - 1 - j);
                                triangles.Add(j + 1);
                            }
                        }
                        for (int j = tenuto_N - 1; j >= 0; j--)
                        {
                            float xc = get_x((float)notes[i].x_pos + 0.95f * (float)notes[i].width);
                            float yc = get_y(notes[i].timing + (notes[i].timing2 - notes[i].timing) * j / (tenuto_N - 1));
                            Coordinate coord = get_Coordinates(xc, yc);
                            Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                            vertices.Add(canvas_to_unity(new Vector2(coord.x, coord.y), -0.5f));
                            uv.Add(vec);
                            if (j > 0)
                            {
                                triangles.Add(2 * tenuto_N - j);
                                triangles.Add(2 * tenuto_N - 1 - j);
                                triangles.Add(j);
                            }
                        }
                        mesh.vertices = vertices.ToArray();
                        mesh.uv = uv.ToArray();
                        mesh.triangles = triangles.ToArray();
                        obj.GetComponent<MeshRenderer>().material = new Material(Shader.Find("UI/Default"));
                        obj.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f);
                        rendering_Note.obj2.Add(obj);


                        rendering_Note.obj4 = UnityEngine.Object.Instantiate(dark_r, canvas_to_unity(new Vector2(coordinate4.x, coordinate4.y), -1f), Quaternion.identity);
                        rendering_Note.obj4.transform.localScale = new Vector3(num * coordinate4.multiplier * (float)notes[i].width / 3f, num * coordinate4.multiplier, 1f);
                        rendering_Note.obj4.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate4.opacity);

                    }
                    rendering_Note.score = -1f;
                    rendering_Notes.Add(rendering_Note);
                }
                notes[i].in_scene = true;
            }
            if (notes[i].type == "Glissando")
            {
                float num = 10f * (float)Screen.width / (float)Screen.height * 3f / 28f / 12f;
                bool num2 = is_object(notes[i]);
                Coordinate coordinate = get_Coordinates(get_x((float)notes[i].x_pos + 0.5f * (float)notes[i].width), get_y(notes[i].timing));
                if (num2 && !notes[i].in_scene)
                {
                    Rendering_Note rendering_Note = new Rendering_Note();
                    rendering_Note.note = notes[i];
                    if (!notes[i].hand)
                    {
                        rendering_Note.obj = UnityEngine.Object.Instantiate(gli_l, canvas_to_unity(new Vector2(coordinate.x, coordinate.y), -1f), Quaternion.identity);
                        rendering_Note.obj.transform.localScale = new Vector3(num * coordinate.multiplier * (float)notes[i].width / 3f, num * coordinate.multiplier, 1f);
                        rendering_Note.obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate.opacity);
                        rendering_Note.obj2 = new List<GameObject>();
                        GameObject obj = new GameObject();
                        obj.name = "Glissando";
                        obj.AddComponent<MeshFilter>();
                        obj.AddComponent<MeshRenderer>();
                        Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
                        mesh.Clear();
                        int idx = 0;
                        List<Vector3> vertices = new List<Vector3>();
                        List<Vector2> uv = new List<Vector2>();
                        List<int> triangles = new List<int>();
                        for (int j = 0; j < tenuto_N; j++)
                        {
                            float xc = get_x((float)notes[i].x_pos + 0.05f * (float)notes[i].width - 1f);
                            float yc = get_y(notes[i].timing + (notes[i].timing2 - notes[i].timing) * j / (tenuto_N - 1));
                            Coordinate coord = get_Coordinates(xc, yc);
                            Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                            vertices.Add(canvas_to_unity(new Vector2(coord.x, coord.y), -0.5f));
                            uv.Add(vec);
                            if (j < tenuto_N - 1)
                            {
                                triangles.Add(j);
                                triangles.Add(2 * tenuto_N - 1 - j);
                                triangles.Add(j + 1);
                            }
                        }
                        for (int j = tenuto_N - 1; j >= 0; j--)
                        {
                            float xc = get_x((float)notes[i].x_pos + 0.95f * (float)notes[i].width - 1f);
                            float yc = get_y(notes[i].timing + (notes[i].timing2 - notes[i].timing) * j / (tenuto_N - 1));
                            Coordinate coord = get_Coordinates(xc, yc);
                            Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                            vertices.Add(canvas_to_unity(new Vector2(coord.x, coord.y), -0.5f));
                            uv.Add(vec);
                            if (j > 0)
                            {
                                triangles.Add(2 * tenuto_N - j);
                                triangles.Add(2 * tenuto_N - 1 - j);
                                triangles.Add(j);
                            }
                        }
                        mesh.vertices = vertices.ToArray();
                        mesh.uv = uv.ToArray();
                        mesh.triangles = triangles.ToArray();
                        obj.GetComponent<MeshRenderer>().material = new Material(Shader.Find("UI/Default"));
                        obj.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f);
                        rendering_Note.obj2.Add(obj);
                    }
                    else
                    {
                        rendering_Note.obj = UnityEngine.Object.Instantiate(gli_r, canvas_to_unity(new Vector2(coordinate.x, coordinate.y), -1f), Quaternion.identity);
                        rendering_Note.obj.transform.localScale = new Vector3(num * coordinate.multiplier * (float)notes[i].width / 3f, num * coordinate.multiplier, 1f);
                        rendering_Note.obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate.opacity);

                        rendering_Note.obj2 = new List<GameObject>();
                        GameObject obj = new GameObject();
                        obj.name = "Tenuto";
                        obj.AddComponent<MeshFilter>();
                        obj.AddComponent<MeshRenderer>();
                        Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
                        mesh.Clear();
                        int idx = 0;
                        List<Vector3> vertices = new List<Vector3>();
                        List<Vector2> uv = new List<Vector2>();
                        List<int> triangles = new List<int>();
                        for (int j = 0; j < tenuto_N; j++)
                        {
                            float xc = get_x((float)notes[i].x_pos + 0.05f * (float)notes[i].width);
                            float yc = get_y(notes[i].timing + (notes[i].timing2 - notes[i].timing) * j / (tenuto_N - 1));
                            Coordinate coord = get_Coordinates(xc, yc);
                            Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                            vertices.Add(canvas_to_unity(new Vector2(coord.x, coord.y), -0.5f));
                            uv.Add(vec);
                            if (j < tenuto_N - 1)
                            {
                                triangles.Add(j);
                                triangles.Add(2 * tenuto_N - 1 - j);
                                triangles.Add(j + 1);
                            }
                        }
                        for (int j = tenuto_N - 1; j >= 0; j--)
                        {
                            float xc = get_x((float)notes[i].x_pos + 0.95f * (float)notes[i].width);
                            float yc = get_y(notes[i].timing + (notes[i].timing2 - notes[i].timing) * j / (tenuto_N - 1));
                            Coordinate coord = get_Coordinates(xc, yc);
                            Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                            vertices.Add(canvas_to_unity(new Vector2(coord.x, coord.y), -0.5f));
                            uv.Add(vec);
                            if (j > 0)
                            {
                                triangles.Add(2 * tenuto_N - j);
                                triangles.Add(2 * tenuto_N - 1 - j);
                                triangles.Add(j);
                            }
                        }
                        mesh.vertices = vertices.ToArray();
                        mesh.uv = uv.ToArray();
                        mesh.triangles = triangles.ToArray();
                        obj.GetComponent<MeshRenderer>().material = new Material(Shader.Find("UI/Default"));
                        obj.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f);
                        rendering_Note.obj2.Add(obj);

                    }
                    rendering_Note.score = -1f;
                    rendering_Notes.Add(rendering_Note);
                }
                notes[i].in_scene = true;
            }
        }
        for (int i= rendering_Lines.Count - 1; i>= 0; i--)
        {
            if(!is_object(get_y(rendering_Lines[i].line.timing)) || (rendering_Lines[i].line.timing - offset) /bs < 0f )
            {
                Destroy(rendering_Lines[i].obj);
                rendering_Lines.Remove(rendering_Lines[i]);
            }
            else
            {
                Coordinate coord = get_Coordinates(0f, get_y(rendering_Lines[i].line.timing));
                rendering_Lines[i].obj.GetComponent<LineRenderer>().SetPosition(0, canvas_to_unity(new Vector2(coord.x, coord.y), 2f));
                rendering_Lines[i].obj.GetComponent<LineRenderer>().SetPosition(1, canvas_to_unity(new Vector2(coord.x + (float)Screen.width * coord.multiplier, coord.y), 2f));
                rendering_Lines[i].obj.GetComponent<LineRenderer>().material.color = new Color(1f, 1f, 1f, 0.5f * coord.opacity);
            }
        }
		for (int num3 = rendering_Notes.Count - 1; num3 >= 0; num3--)
		{
            if (rendering_Notes[num3].note.type == "Glissando" && rendering_Notes[num3].score > 0f && rendering_Notes[num3].note.timing2 > offset)
            {
                UnityEngine.Object.Destroy(rendering_Notes[num3].obj);
                foreach (GameObject obj in rendering_Notes[num3].obj2)
                    UnityEngine.Object.Destroy(obj);
                rendering_Notes.Remove(rendering_Notes[num3]);
                passed_note++;
        }
            else if ((!is_object(rendering_Notes[num3].note) || (rendering_Notes[num3].score < 0f && (rendering_Notes[num3].note.timing - offset - timing_offset) / bs < 0f - good_time) || (rendering_Notes[num3].score > 0f && (rendering_Notes[num3].note.type!= "Note") &&(rendering_Notes[num3].note.timing2-offset-timing_offset)/bs < 0f - good_time)))
			{
				if (rendering_Notes[num3].score < 0f)
				{
					Effect effect = new Effect();
					effect.x_pos = rendering_Notes[num3].note.x_pos;
					effect.width = rendering_Notes[num3].note.width;
					effect.timing = offset;
					effect.score = 0f;
					effect.text = UnityEngine.Object.Instantiate(miss_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos - 1f + 0.5f * effect.width), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);
					miss++;
					effect.text.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
					float num4 = 50f * (float)Screen.width / (560f * (float)Screen.height);
					effect.text.transform.localScale = new Vector3(num4, num4, 1f);
					rendering_Effects.Add(effect);
					combo = 0;
				}

				UnityEngine.Object.Destroy(rendering_Notes[num3].obj);
                if(rendering_Notes[num3].note.type=="Tenuto" || rendering_Notes[num3].note.type=="Trill")
                {
                    foreach(GameObject obj in rendering_Notes[num3].obj2)
                        UnityEngine.Object.Destroy(obj);
                    UnityEngine.Object.Destroy(rendering_Notes[num3].obj4);
                }
                if(rendering_Notes[num3].note.type=="Glissando")
                {
                    foreach (GameObject obj in rendering_Notes[num3].obj2)
                        UnityEngine.Object.Destroy(obj);
                }
				rendering_Notes.Remove(rendering_Notes[num3]);
				passed_note++;
			}
			else if (rendering_Notes[num3].note.type=="Note")
			{
				float num5 = 10f * (float)Screen.width / (float)Screen.height * 3f / 28f / 12f;
				Coordinate coordinate1 = get_Coordinates(get_x((float)rendering_Notes[num3].note.x_pos + 0.5f * (float)rendering_Notes[num3].note.width - 1f), get_y(rendering_Notes[num3].note.timing));
				rendering_Notes[num3].obj.transform.localScale = new Vector3(num5 * coordinate1.multiplier * (float)rendering_Notes[num3].note.width / 3f, num5 * coordinate1.multiplier, 1f);
				rendering_Notes[num3].obj.transform.position = canvas_to_unity(new Vector2(coordinate1.x, coordinate1.y), -1f - rendering_Notes[num3].note.timing / 1E+07f);
				rendering_Notes[num3].obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate1.opacity);
			}
            else if (rendering_Notes[num3].note.type=="Tenuto")
            {
                float num5 = 10f * (float)Screen.width / (float)Screen.height * 3f / 28f / 12f;

                Coordinate coordinate4 = get_Coordinates(get_x((float)rendering_Notes[num3].note.x_pos + 0.5f * (float)rendering_Notes[num3].note.width - 1f), get_y(rendering_Notes[num3].note.timing2));
                rendering_Notes[num3].obj4.transform.localScale = new Vector3(num5 * coordinate4.multiplier * (float)rendering_Notes[num3].note.width / 3f, num5 * coordinate4.multiplier, 1f);
                rendering_Notes[num3].obj4.transform.position = canvas_to_unity(new Vector2(coordinate4.x, coordinate4.y), -1f - rendering_Notes[num3].note.timing / 1E+07f);
                rendering_Notes[num3].obj4.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate4.opacity);
                rendering_Notes[num3].obj4.SetActive(is_object(get_y(rendering_Notes[num3].note.timing2)));
                Mesh mesh = rendering_Notes[num3].obj2[0].GetComponent<MeshFilter>().mesh;
                List<Vector3> vertices = new List<Vector3>();
                List<Vector2> uv = new List<Vector2>();
                List<int> triangles = new List<int>();
                float value1 = get_y(rendering_Notes[num3].note.timing);
                if(rendering_Notes[num3].score>0f)
                {
                    value1 = Screen.height;
                }
                Coordinate coordinate1 = get_Coordinates(get_x((float)rendering_Notes[num3].note.x_pos + 0.5f * (float)rendering_Notes[num3].note.width - 1f), value1);
                rendering_Notes[num3].obj.transform.localScale = new Vector3(num5 * coordinate1.multiplier * (float)rendering_Notes[num3].note.width / 3f, num5 * coordinate1.multiplier, 1f);
                rendering_Notes[num3].obj.transform.position = canvas_to_unity(new Vector2(coordinate1.x, coordinate1.y), -1f - rendering_Notes[num3].note.timing / 1E+07f);
                rendering_Notes[num3].obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate1.opacity);
                float value2 = get_y(rendering_Notes[num3].note.timing2);
                if (value1 >= 0f && value2 < 0f)
                {
                    value2 = 0f;
                }
                for (int j = 0; j < tenuto_N; j++)
                {
                    float xc = get_x((float)rendering_Notes[num3].note.x_pos + 0.05f * (float)rendering_Notes[num3].note.width-1f);
                    float yc = value1 + (value2-value1) * j / (tenuto_N - 1);
                    Coordinate coord = get_Coordinates(xc, yc);
                    Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                    vertices.Add(canvas_to_unity(new Vector2(coord.x, coord.y), -0.5f));
                    uv.Add(vec);
                    if (j < tenuto_N - 1)
                    {
                        triangles.Add(j);
                        triangles.Add(2 * tenuto_N - 1 - j);
                        triangles.Add(j + 1);
                    }
                }
                for (int j = tenuto_N - 1; j >= 0; j--)
                {
                    float xc = get_x((float)rendering_Notes[num3].note.x_pos + 0.95f * (float)rendering_Notes[num3].note.width-1f);
                    float yc = value1 + (value2 - value1) * j / (tenuto_N - 1);
                    Coordinate coord = get_Coordinates(xc, yc);
                    Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                    vertices.Add(canvas_to_unity(new Vector2(coord.x, coord.y), -0.5f));
                    uv.Add(vec);
                    if (j > 0)
                    {
                        triangles.Add(2 * tenuto_N - j);
                        triangles.Add(2 * tenuto_N - 1 - j);
                        triangles.Add(j);
                    }
                }
                mesh.vertices = vertices.ToArray();
                mesh.uv = uv.ToArray();
                mesh.triangles = triangles.ToArray();
                Coordinate coordinate_op = get_Coordinates(get_x((float)rendering_Notes[num3].note.x_pos + 0.95f * (float)rendering_Notes[num3].note.width - 1f), value1);
                float opacity = 0.8f * coordinate_op.opacity;
                if (value1 < 0f)
                    opacity *= 0f;
                else if (value2 < 0f)
                    value2 = 0f;
                if (rendering_Notes[num3].note.hand)
                {
                    rendering_Notes[num3].obj2[0].GetComponent<MeshRenderer>().material.color = new Color(255f / 255f, 83f / 255f, 83f / 255f, opacity);
                }
                else
                {
                    rendering_Notes[num3].obj2[0].GetComponent<MeshRenderer>().material.color = new Color(107f/255f, 218f/255f, 255f/255f, opacity);
                }
            }
            else if (rendering_Notes[num3].note.type == "Trill")
            {
                float num5 = 10f * (float)Screen.width / (float)Screen.height * 3f / 28f / 12f;

                Coordinate coordinate4 = get_Coordinates(get_x((float)rendering_Notes[num3].note.x_pos + 0.5f * (float)rendering_Notes[num3].note.width - 1f), get_y(rendering_Notes[num3].note.timing2));
                rendering_Notes[num3].obj4.transform.localScale = new Vector3(num5 * coordinate4.multiplier * (float)rendering_Notes[num3].note.width / 3f, num5 * coordinate4.multiplier, 1f);
                rendering_Notes[num3].obj4.transform.position = canvas_to_unity(new Vector2(coordinate4.x, coordinate4.y), -1f - rendering_Notes[num3].note.timing / 1E+07f);
                rendering_Notes[num3].obj4.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate4.opacity);
                rendering_Notes[num3].obj4.SetActive(is_object(get_y(rendering_Notes[num3].note.timing2)));
                Mesh mesh = rendering_Notes[num3].obj2[0].GetComponent<MeshFilter>().mesh;
                List<Vector3> vertices = new List<Vector3>();
                List<Vector2> uv = new List<Vector2>();
                List<int> triangles = new List<int>();
                float value1 = get_y(rendering_Notes[num3].note.timing);
                if (rendering_Notes[num3].score > 0f)
                {
                    value1 = Screen.height;
                }
                Coordinate coordinate1 = get_Coordinates(get_x((float)rendering_Notes[num3].note.x_pos + 0.5f * (float)rendering_Notes[num3].note.width - 1f), value1);
                rendering_Notes[num3].obj.transform.localScale = new Vector3(num5 * coordinate1.multiplier * (float)rendering_Notes[num3].note.width / 3f, num5 * coordinate1.multiplier, 1f);
                rendering_Notes[num3].obj.transform.position = canvas_to_unity(new Vector2(coordinate1.x, coordinate1.y), -1f - rendering_Notes[num3].note.timing / 1E+07f);
                rendering_Notes[num3].obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate1.opacity);
                float value2 = get_y(rendering_Notes[num3].note.timing2);
                if (value1 >= 0f && value2 < 0f)
                {
                    value2 = 0f;
                }
                for (int j = 0; j < tenuto_N; j++)
                {
                    float xc = get_x((float)rendering_Notes[num3].note.x_pos + 0.05f * (float)rendering_Notes[num3].note.width - 1f);
                    float yc = value1 + (value2 - value1) * j / (tenuto_N - 1);
                    Coordinate coord = get_Coordinates(xc, yc);
                    Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                    vertices.Add(canvas_to_unity(new Vector2(coord.x, coord.y), -0.5f));
                    uv.Add(new Vector2(0f, 0f));
                    if (j < tenuto_N - 1)
                    {
                        triangles.Add(j);
                        triangles.Add(2 * tenuto_N - 1 - j);
                        triangles.Add(j + 1);
                    }
                }
                for (int j = tenuto_N - 1; j >= 0; j--)
                {
                    float xc = get_x((float)rendering_Notes[num3].note.x_pos + 0.95f * (float)rendering_Notes[num3].note.width - 1f);
                    float yc = value1 + (value2 - value1) * j / (tenuto_N - 1);
                    Coordinate coord = get_Coordinates(xc, yc);
                    Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                    vertices.Add(canvas_to_unity(new Vector2(coord.x, coord.y), -0.5f));
                    uv.Add(new Vector2(1f,0f));
                    if (j > 0)
                    {
                        triangles.Add(2 * tenuto_N - j);
                        triangles.Add(2 * tenuto_N - 1 - j);
                        triangles.Add(j);
                    }
                }
                mesh.vertices = vertices.ToArray();
                mesh.uv = uv.ToArray();
                mesh.triangles = triangles.ToArray();
                Coordinate coordinate_op = get_Coordinates(get_x((float)rendering_Notes[num3].note.x_pos + 0.95f * (float)rendering_Notes[num3].note.width - 1f), value1);
                float opacity = 0.8f * coordinate_op.opacity;
                if (value1 < 0f)
                    opacity *= 0f;
                else if (value2 < 0f)
                    value2 = 0f;
                if (rendering_Notes[num3].note.hand)
                {
                    rendering_Notes[num3].obj2[0].GetComponent<MeshRenderer>().material.mainTexture = red_tex;
                    rendering_Notes[num3].obj2[0].GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f, opacity);
                }
                else
                {
                    rendering_Notes[num3].obj2[0].GetComponent<MeshRenderer>().material.mainTexture = blue_tex;
                    rendering_Notes[num3].obj2[0].GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f, opacity);
                    //rendering_Notes[num3].obj2[0].GetComponent<MeshRenderer>().material.color = new Color(107f / 255f, 218f / 255f, 255f / 255f, opacity);
                }
            }
            else if (rendering_Notes[num3].note.type == "Glissando")
            {
                float num5 = 10f * (float)Screen.width / (float)Screen.height * 3f / 28f / 12f;
                Mesh mesh = rendering_Notes[num3].obj2[0].GetComponent<MeshFilter>().mesh;
                List<Vector3> vertices = new List<Vector3>();
                List<Vector2> uv = new List<Vector2>();
                List<int> triangles = new List<int>();
                float value1 = get_y(rendering_Notes[num3].note.timing);
                float x1 = (float)rendering_Notes[num3].note.x_pos;
                float x2 = (float)rendering_Notes[num3].note.x_pos2;
                Coordinate coordinate1 = get_Coordinates(get_x((float)rendering_Notes[num3].note.x_pos + 0.5f * (float)rendering_Notes[num3].note.width - 1f), value1);
                rendering_Notes[num3].obj.transform.localScale = new Vector3(num5 * coordinate1.multiplier * (float)rendering_Notes[num3].note.width / 3f, num5 * coordinate1.multiplier, 1f);
                rendering_Notes[num3].obj.transform.position = canvas_to_unity(new Vector2(coordinate1.x, coordinate1.y), -1f - rendering_Notes[num3].note.timing / 1E+07f);
                rendering_Notes[num3].obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, coordinate1.opacity);
                float value2 = get_y(rendering_Notes[num3].note.timing2);
                if (rendering_Notes[num3].score > 0f)
                {
                    x1 = x1 + (Screen.height - value1) / (value2 - value1) * (x2 - x1);
                    value1 = Screen.height;
                    
                }
                if (value1 >= 0f && value2 < 0f)
                {
                    x2 = x2 + (0f - value2) / (value1 - value2) * (x1 - x2);
                    value2 = 0f;
                    
                }
                for (int j = 0; j < tenuto_N; j++)
                {
                    float xc = get_x(x1 + 0.05f * (float)rendering_Notes[num3].note.width - 1f) * (1f-(float)j / (tenuto_N - 1)) + get_x(x2 + 0.05f * (float)rendering_Notes[num3].note.width - 1f) * (float)j / (tenuto_N - 1);
                    float yc = value1 + (value2 - value1) * j / (tenuto_N - 1);
                    Coordinate coord = get_Coordinates(xc, yc);
                    Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                    vertices.Add(canvas_to_unity(new Vector2(coord.x, coord.y), -0.5f));
                    uv.Add(vec);
                    if (j < tenuto_N - 1)
                    {
                        triangles.Add(j);
                        triangles.Add(2 * tenuto_N - 1 - j);
                        triangles.Add(j + 1);
                    }
                }
                for (int j = tenuto_N - 1; j >= 0; j--)
                {
                    float xc = get_x(x1 + 0.95f * (float)rendering_Notes[num3].note.width - 1f) * (1f - (float)j / (tenuto_N - 1)) + get_x(x2 + 0.95f * (float)rendering_Notes[num3].note.width - 1f) * (float)j / (tenuto_N - 1);
                    float yc = value1 + (value2 - value1) * j / (tenuto_N - 1);
                    Coordinate coord = get_Coordinates(xc, yc);
                    Vector2 vec = canvas_to_unity(new Vector2(coord.x, coord.y));
                    vertices.Add(canvas_to_unity(new Vector2(coord.x, coord.y), -0.5f));
                    uv.Add(vec);
                    if (j > 0)
                    {
                        triangles.Add(2 * tenuto_N - j);
                        triangles.Add(2 * tenuto_N - 1 - j);
                        triangles.Add(j);
                    }
                }
                mesh.vertices = vertices.ToArray();
                mesh.uv = uv.ToArray();
                mesh.triangles = triangles.ToArray();
                Coordinate coordinate_op = get_Coordinates(get_x((float)rendering_Notes[num3].note.x_pos + 0.95f * (float)rendering_Notes[num3].note.width - 1f), value1);
                float opacity = 0.8f * coordinate_op.opacity;
                if (value1 < 0f)
                    opacity *= 0f;
                else if (value2 < 0f)
                    value2 = 0f;
                if (rendering_Notes[num3].note.hand)
                {
                    rendering_Notes[num3].obj2[0].GetComponent<MeshRenderer>().material.color = new Color(255f / 255f, 83f / 255f, 83f / 255f, opacity);
                }
                else
                {
                    rendering_Notes[num3].obj2[0].GetComponent<MeshRenderer>().material.color = new Color(107f / 255f, 218f / 255f, 255f / 255f, opacity);
                }
            }
        }
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			SceneManager.LoadSceneAsync("Menu");
		}
        bool[] push = new bool[28];
            for (int n = 0; n < 28; n++)
            {
                if (midi_mode)
                {
                    push[n] = (!pushing[n] && MidiMaster.GetKey(midi_keys[n]) > 0f);
                }
                else
                {
                    push[n] = Input.GetKeyDown(keys[n]);
                }
            }
            for (int num20 = 0; num20 < 28; num20++)
            {
                pushing[num20] = (Input.GetKey(keys[num20]) || MidiMaster.GetKey(midi_keys[num20]) > 0f);
                push_effects[num20].SetActive(pushing[num20]);
            }
        for (int num21 = 0; num21 < 28; num21++)
        {
            if (!push[num21]||locked[num21])
            {
                continue;
            }
            int num22 = -1;
            float num23 = 0f;
            for (int num24 = 0; num24 < rendering_Notes.Count; num24++)
            {
                if (rendering_Notes[num24].score < 0f && num21 >= rendering_Notes[num24].note.x_pos - 1 && num21 < rendering_Notes[num24].note.x_pos + rendering_Notes[num24].note.width - 1)
                {
                    float num25 = (rendering_Notes[num24].note.timing - offset - timing_offset) / bs;
                    if (num25 < 0f - good_time)
                    {
                        num25 = 0f - num25;
                    }
                    if (num22 < 0 || num25 < num23)
                    {
                        num22 = num24;
                        num23 = num25;
                    }
                }
            }
            if (num22 < 0 || !(Math.Abs((rendering_Notes[num22].note.timing - offset - timing_offset) / bs) < good_time))
            {
                continue;
            }
            if (rendering_Notes[num22].note.type == "Note")
            {
                Effect effect2 = new Effect();
                effect2.x_pos = rendering_Notes[num22].note.x_pos;
                effect2.width = rendering_Notes[num22].note.width;
                effect2.timing = offset;
                float num26 = 5f * (float)Screen.width / (float)Screen.height / 28f;
                if (Math.Abs((rendering_Notes[num22].note.timing - offset - timing_offset) / bs) < pjust_time)
                {
                    rendering_Notes[num22].score = 1f;
                    pjust++;
                    effect2.obj = UnityEngine.Object.Instantiate(pjust_effect, canvas_to_unity(new Vector2(get_x((float)effect2.x_pos + effect2.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                    effect2.text = UnityEngine.Object.Instantiate(pjust_text, canvas_to_unity(new Vector2(get_x((float)effect2.x_pos + effect2.width / 2f - 1f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);
                }
                else if (Math.Abs((rendering_Notes[num22].note.timing - offset - timing_offset) / bs) < just_time)
                {
                    rendering_Notes[num22].score = 0.7f;
                    just++;
                    effect2.obj = UnityEngine.Object.Instantiate(just_effect, canvas_to_unity(new Vector2(get_x((float)effect2.x_pos + effect2.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                    effect2.text = UnityEngine.Object.Instantiate(just_text, canvas_to_unity(new Vector2(get_x((float)effect2.x_pos - 1f + effect2.width / 2f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);
                }
                else
                {
                    rendering_Notes[num22].score = 0.5f;
                    good++;
                    effect2.obj = UnityEngine.Object.Instantiate(good_effect, canvas_to_unity(new Vector2(get_x((float)effect2.x_pos + effect2.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                    effect2.text = UnityEngine.Object.Instantiate(good_text, canvas_to_unity(new Vector2(get_x((float)effect2.x_pos - 1f + effect2.width / 2f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);
                }
                rendering_Notes[num22].obj.SetActive(value: false);
                float num27 = 50f * (float)Screen.width / (560f * (float)Screen.height);
                effect2.obj.transform.localScale = new Vector3(num26 * effect2.width * 0.98f, num26, 1f);
                effect2.text.transform.localScale = new Vector3(num27, num27, 1f);
                effect2.obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                effect2.text.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                effect2.score = rendering_Notes[num22].score;
                if (effect2.score < 1f)
                {
                    if ((rendering_Notes[num22].note.timing - offset - timing_offset) / bs > 0f)
                    {
                        effect2.fs = UnityEngine.Object.Instantiate(fast_text, canvas_to_unity(new Vector2(get_x((float)effect2.x_pos - 1f + effect2.width / 2f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 4.5f), -0.2f), Quaternion.identity);
                        fast++;
                    }
                    else
                    {
                        effect2.fs = UnityEngine.Object.Instantiate(slow_text, canvas_to_unity(new Vector2(get_x((float)effect2.x_pos - 1f + effect2.width / 2f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 4.5f), -0.2f), Quaternion.identity);
                        slow++;
                    }
                    effect2.fs.transform.localScale = new Vector3(num27 * 0.4f, num27 * 0.4f, 1f);
                    effect2.fs.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                }
                rendering_Effects.Add(effect2);
                score += rendering_Notes[num22].score;
                for (int num28 = rendering_Notes[num22].note.x_pos - 1; num28 < rendering_Notes[num22].note.x_pos + rendering_Notes[num22].note.width - 1; num28++)
                {
                    push[num28] = false;
                }
            }
            if (rendering_Notes[num22].note.type == "Tenuto")
            {
                float num26 = 5f * (float)Screen.width / (float)Screen.height / 28f;
                if (Math.Abs((rendering_Notes[num22].note.timing - offset - timing_offset) / bs) < pjust_time)
                {
                    rendering_Notes[num22].score = 1f;
                               }
                else if (Math.Abs((rendering_Notes[num22].note.timing - offset - timing_offset) / bs) < just_time)
                {
                    rendering_Notes[num22].score = 0.7f;
                }
                else
                {
                    rendering_Notes[num22].score = 0.5f;
                }
                float num27 = 50f * (float)Screen.width / (560f * (float)Screen.height);
                rendering_Notes[num22].pushing_time = offset;
                lock_check.Add(rendering_Notes[num22]);
                for (int num28 = rendering_Notes[num22].note.x_pos - 1; num28 < rendering_Notes[num22].note.x_pos + rendering_Notes[num22].note.width - 1; num28++)
                {
                    push[num28] = false;
                    locked[num28] = true;
                }
            }
            else if (rendering_Notes[num22].note.type == "Trill")
            {
                float num26 = 5f * (float)Screen.width / (float)Screen.height / 28f;
                if (Math.Abs((rendering_Notes[num22].note.timing - offset - timing_offset) / bs) < pjust_time)
                {
                    rendering_Notes[num22].score = 1f;
                }
                else if (Math.Abs((rendering_Notes[num22].note.timing - offset - timing_offset) / bs) < just_time)
                {
                    rendering_Notes[num22].score = 0.7f;
                }
                else
                {
                    rendering_Notes[num22].score = 0.5f;
                }

                float num27 = 50f * (float)Screen.width / (560f * (float)Screen.height);
                rendering_Notes[num22].pushing_time = offset;
                rendering_Notes[num22].pushing_trill = offset;
                lock_check.Add(rendering_Notes[num22]);
                for (int num28 = rendering_Notes[num22].note.x_pos - 1; num28 < rendering_Notes[num22].note.x_pos + rendering_Notes[num22].note.width - 1; num28++)
                {
                    push[num28] = false;
                    locked[num28] = true;
                }
            }
            else if (rendering_Notes[num22].note.type == "Glissando")
            {
                if (Math.Abs((rendering_Notes[num22].note.timing - offset - timing_offset) / bs)> just_time)
                    continue;

                Effect effect2 = new Effect();
                effect2.x_pos = rendering_Notes[num22].note.x_pos;
                effect2.width = rendering_Notes[num22].note.width;

                float num26 = 5f * (float)Screen.width / (float)Screen.height / 28f;
                if ((rendering_Notes[num22].note.timing - offset - timing_offset) / bs < -pjust_time)
                {
                    rendering_Notes[num22].score = 0.7f;
                    just++;
                    effect2.obj = UnityEngine.Object.Instantiate(just_effect, canvas_to_unity(new Vector2(get_x((float)effect2.x_pos + effect2.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                    effect2.text = UnityEngine.Object.Instantiate(just_text, canvas_to_unity(new Vector2(get_x((float)effect2.x_pos + effect2.width / 2f - 1f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);

                }
                else
                {
                    rendering_Notes[num22].score = 1.0f;
                    pjust++;
                    effect2.obj = UnityEngine.Object.Instantiate(pjust_effect, canvas_to_unity(new Vector2(get_x((float)effect2.x_pos + effect2.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                    effect2.text = UnityEngine.Object.Instantiate(pjust_text, canvas_to_unity(new Vector2(get_x((float)effect2.x_pos + effect2.width / 2f - 1f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);

                }
                float num27 = 50f * (float)Screen.width / (560f * (float)Screen.height);
                effect2.obj.transform.localScale = new Vector3(num26 * effect2.width * 0.98f, num26, 1f);
                effect2.text.transform.localScale = new Vector3(num27, num27, 1f);
                effect2.obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                effect2.text.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                effect2.score = rendering_Notes[num22].score;
                if ((rendering_Notes[num22].note.timing - offset - timing_offset) / bs > 0.0f)
                {
                    effect2.timing = rendering_Notes[num22].note.timing;
                }
                else
                {
                    effect2.timing = offset;
                    rendering_Notes[num22].obj.SetActive(false);
                    score += effect2.score / 2f;
                }
                
                if (effect2.score < 1f)
                {
                    if ((rendering_Notes[num22].note.timing - offset - timing_offset) / bs > 0f)
                    {
                        effect2.fs = UnityEngine.Object.Instantiate(fast_text, canvas_to_unity(new Vector2(get_x((float)effect2.x_pos - 1f + effect2.width / 2f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 4.5f), -0.2f), Quaternion.identity);
                        fast++;
                    }
                    else
                    {
                        effect2.fs = UnityEngine.Object.Instantiate(slow_text, canvas_to_unity(new Vector2(get_x((float)effect2.x_pos - 1f + effect2.width / 2f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 4.5f), -0.2f), Quaternion.identity);
                        slow++;
                    }
                    effect2.fs.transform.localScale = new Vector3(num27 * 0.4f, num27 * 0.4f, 1f);
                    effect2.fs.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                }
                if ((rendering_Notes[num22].note.timing - offset - timing_offset) / bs > 0.0f)
                {
                    lock_check.Add(rendering_Notes[num22]);
                }
                rendering_Effects.Add(effect2);
                for (int num28 = rendering_Notes[num22].note.x_pos - 1; num28 < rendering_Notes[num22].note.x_pos + rendering_Notes[num22].note.width - 1; num28++)
                {
                    push[num28] = false;
                }
            }

        }
        for(int j=lock_check.Count-1;j>=0;j--)
        {
            if(lock_check[j].note.type == "Glissando")
            {
                if(offset < lock_check[j].note.timing2)
                {
                    lock_check[j].obj.SetActive(false);
                    score += lock_check[j].score/ 2f;
                    lock_check.Remove(lock_check[j]);
                }
            }
            else if (lock_check[j].note.type == "Tenuto")
            {
                bool pushing_tenuto = false;
                for (int k = lock_check[j].note.x_pos - 1; k < lock_check[j].note.x_pos + lock_check[j].note.width - 1; k++)
                {
                    if (pushing[k])
                        pushing_tenuto = true;
                }
                if (offset < lock_check[j].note.timing2)
                {
                    Effect effect = new Effect();
                    effect.x_pos = lock_check[j].note.x_pos;
                    effect.timing = lock_check[j].note.timing2;
                    effect.width = lock_check[j].note.width;
                    effect.score = lock_check[j].score;
                    float num26 = 5f * (float)Screen.width / (float)Screen.height / 28f;
                    if (effect.score == 1.0f)
                    {
                        pjust++;
                        score += 4.0f;
                        effect.obj = UnityEngine.Object.Instantiate(pjust_effect, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                        effect.text = UnityEngine.Object.Instantiate(pjust_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);

                    }
                    else if (effect.score == 0.7f)
                    {
                        just++;
                        score += 2.8f;
                        effect.obj = UnityEngine.Object.Instantiate(just_effect, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                        effect.text = UnityEngine.Object.Instantiate(just_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);

                    }
                    else
                    {
                        good++;
                        score += 2.0f;
                        effect.obj = UnityEngine.Object.Instantiate(good_effect, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                        effect.text = UnityEngine.Object.Instantiate(good_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);

                    }
                    for (int num28 = lock_check[j].note.x_pos - 1; num28 < lock_check[j].note.x_pos + lock_check[j].note.width - 1; num28++)
                    {
                        locked[num28] = false;
                    }


                    float num27 = 50f * (float)Screen.width / (560f * (float)Screen.height);
                    effect.obj.transform.localScale = new Vector3(num26 * effect.width * 0.98f, num26, 1f);
                    effect.text.transform.localScale = new Vector3(num27, num27, 1f);
                    effect.obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                    effect.text.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                    if (effect.score < 1f)
                    {
                        if ((lock_check[j].note.timing - lock_check[j].pushing_time - timing_offset) / bs > 0f)
                        {
                            effect.fs = UnityEngine.Object.Instantiate(fast_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos - 1f + effect.width / 2f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 4.5f), -0.2f), Quaternion.identity);
                            fast++;
                        }
                        else
                        {
                            effect.fs = UnityEngine.Object.Instantiate(slow_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos - 1f + effect.width / 2f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 4.5f), -0.2f), Quaternion.identity);
                            slow++;
                        }
                        effect.fs.transform.localScale = new Vector3(num27 * 0.4f, num27 * 0.4f, 1f);
                        effect.fs.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                    }
                    rendering_Effects.Add(effect);
                    Destroy(lock_check[j].obj);

                    Destroy(lock_check[j].obj2[0]);
                    Destroy(lock_check[j].obj4);
                    passed_note++;
                    rendering_Notes.Remove(lock_check[j]);
                    lock_check.Remove(lock_check[j]);


                }
                else if (pushing_tenuto && offset > lock_check[j].note.timing2)
                {
                    continue;
                }
                else if (offset < lock_check[j].note.timing2 - bs * good_time)
                {
                    Effect effect = new Effect();
                    effect.x_pos = lock_check[j].note.x_pos;
                    effect.timing = lock_check[j].note.timing2;
                    effect.width = lock_check[j].note.width;
                    effect.score = lock_check[j].score;
                    float num26 = 5f * (float)Screen.width / (float)Screen.height / 28f;
                    if (effect.score == 1.0f)
                    {
                        pjust++;
                        score += 4.0f;
                        effect.obj = UnityEngine.Object.Instantiate(pjust_effect, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                        effect.text = UnityEngine.Object.Instantiate(pjust_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);

                    }
                    else if (effect.score == 0.7f)
                    {
                        just++;
                        score += 2.8f;
                        effect.obj = UnityEngine.Object.Instantiate(just_effect, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                        effect.text = UnityEngine.Object.Instantiate(just_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);

                    }
                    else
                    {
                        good++;
                        score += 2.0f;
                        effect.obj = UnityEngine.Object.Instantiate(good_effect, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                        effect.text = UnityEngine.Object.Instantiate(good_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);

                    }
                    for (int num28 = lock_check[j].note.x_pos - 1; num28 < lock_check[j].note.x_pos + lock_check[j].note.width - 1; num28++)
                    {
                        locked[num28] = false;
                    }


                    float num27 = 50f * (float)Screen.width / (560f * (float)Screen.height);
                    effect.obj.transform.localScale = new Vector3(num26 * effect.width * 0.98f, num26, 1f);
                    effect.text.transform.localScale = new Vector3(num27, num27, 1f);
                    effect.obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                    effect.text.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                    if (effect.score < 1f)
                    {
                        if ((lock_check[j].note.timing - lock_check[j].pushing_time - timing_offset) / bs > 0f)
                        {
                            effect.fs = UnityEngine.Object.Instantiate(fast_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos - 1f + effect.width / 2f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 4.5f), -0.2f), Quaternion.identity);
                            fast++;
                        }
                        else
                        {
                            effect.fs = UnityEngine.Object.Instantiate(slow_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos - 1f + effect.width / 2f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 4.5f), -0.2f), Quaternion.identity);
                            slow++;
                        }
                        effect.fs.transform.localScale = new Vector3(num27 * 0.4f, num27 * 0.4f, 1f);
                        effect.fs.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                    }
                    rendering_Effects.Add(effect);
                    Destroy(lock_check[j].obj);

                    Destroy(lock_check[j].obj2[0]);
                    Destroy(lock_check[j].obj4);
                    passed_note++;
                    rendering_Notes.Remove(lock_check[j]);
                    lock_check.Remove(lock_check[j]);


                }
                else
                {
                    float num26 = 5f * (float)Screen.width / (float)Screen.height / 28f;
                    float num27 = 50f * (float)Screen.width / (560f * (float)Screen.height);
                    Effect effect = new Effect();
                    effect.x_pos = lock_check[j].note.x_pos;
                    effect.timing = offset;
                    effect.score = 0.25f;
                    effect.width = lock_check[j].note.width;
                    score += 1.0f;
                    good++;
                    effect.obj = UnityEngine.Object.Instantiate(good_effect, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                    effect.text = UnityEngine.Object.Instantiate(good_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);
                    effect.fs = UnityEngine.Object.Instantiate(slow_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos - 1f + effect.width / 2f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 4.5f), -0.2f), Quaternion.identity);
                    slow++;
                    effect.obj.transform.localScale = new Vector3(num26 * effect.width * 0.98f, num26, 1f);
                    effect.text.transform.localScale = new Vector3(num27, num27, 1f);
                    effect.obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                    effect.text.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);

                    effect.fs.transform.localScale = new Vector3(num27 * 0.4f, num27 * 0.4f, 1f);
                    effect.fs.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);

                    for (int num28 = lock_check[j].note.x_pos - 1; num28 < lock_check[j].note.x_pos + lock_check[j].note.width - 1; num28++)
                    {
                        locked[num28] = false;
                    }
                    rendering_Effects.Add(effect);
                    Destroy(lock_check[j].obj);

                    Destroy(lock_check[j].obj2[0]);
                    Destroy(lock_check[j].obj4);
                    passed_note++;
                    rendering_Notes.Remove(lock_check[j]);
                    lock_check.Remove(lock_check[j]);

                }
            }
            else if (lock_check[j].note.type == "Trill")
            {

                bool pushed_trill = false; ;
                for (int k = lock_check[j].note.x_pos - 1; k < lock_check[j].note.x_pos + lock_check[j].note.width - 1; k++)
                {
                    if (push[k])
                        pushed_trill = true;
                }
                if(pushed_trill)
                {
                    lock_check[j].pushing_trill = offset;
                }
                if (offset < lock_check[j].note.timing2)
                {
                    Effect effect = new Effect();
                    effect.x_pos = lock_check[j].note.x_pos;
                    effect.timing = lock_check[j].note.timing2;
                    effect.width = lock_check[j].note.width;
                    effect.score = lock_check[j].score;
                    float num26 = 5f * (float)Screen.width / (float)Screen.height / 28f;
                    if (effect.score == 1.0f)
                    {
                        pjust++;
                        score += 5.0f;
                        effect.obj = UnityEngine.Object.Instantiate(pjust_effect, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                        effect.text = UnityEngine.Object.Instantiate(pjust_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);

                    }
                    else if (effect.score == 0.7f)
                    {
                        just++;
                        score += 3.5f;
                        effect.obj = UnityEngine.Object.Instantiate(just_effect, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                        effect.text = UnityEngine.Object.Instantiate(just_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);

                    }
                    else
                    {
                        good++;
                        score += 2.5f;
                        effect.obj = UnityEngine.Object.Instantiate(good_effect, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                        effect.text = UnityEngine.Object.Instantiate(good_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);

                    }
                    for (int num28 = lock_check[j].note.x_pos - 1; num28 < lock_check[j].note.x_pos + lock_check[j].note.width - 1; num28++)
                    {
                        locked[num28] = false;
                    }


                    float num27 = 50f * (float)Screen.width / (560f * (float)Screen.height);
                    effect.obj.transform.localScale = new Vector3(num26 * effect.width * 0.98f, num26, 1f);
                    effect.text.transform.localScale = new Vector3(num27, num27, 1f);
                    effect.obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                    effect.text.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                    if (effect.score < 1f)
                    {
                        if ((lock_check[j].note.timing - lock_check[j].pushing_time - timing_offset) / bs > 0f)
                        {
                            effect.fs = UnityEngine.Object.Instantiate(fast_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos - 1f + effect.width / 2f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 4.5f), -0.2f), Quaternion.identity);
                            fast++;
                        }
                        else
                        {
                            effect.fs = UnityEngine.Object.Instantiate(slow_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos - 1f + effect.width / 2f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 4.5f), -0.2f), Quaternion.identity);
                            slow++;
                        }
                        effect.fs.transform.localScale = new Vector3(num27 * 0.4f, num27 * 0.4f, 1f);
                        effect.fs.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                    }
                    rendering_Effects.Add(effect);
                    Destroy(lock_check[j].obj);

                    Destroy(lock_check[j].obj2[0]);
                    Destroy(lock_check[j].obj4);
                    passed_note++;
                    rendering_Notes.Remove(lock_check[j]);
                    lock_check.Remove(lock_check[j]);


                }
                else if ( (offset - lock_check[j].pushing_trill) / bs < trill_tolerance && offset > lock_check[j].note.timing2)
                {
                    continue;
                }
                else if (offset < lock_check[j].note.timing2 - bs * good_time)
                {

                    Effect effect = new Effect();
                    effect.x_pos = lock_check[j].note.x_pos;
                    effect.timing = lock_check[j].note.timing2;
                    effect.width = lock_check[j].note.width;
                    effect.score = lock_check[j].score;
                    float num26 = 5f * (float)Screen.width / (float)Screen.height / 28f;
                    if (effect.score == 1.0f)
                    {
                        pjust++;
                        score += 4.0f;
                        effect.obj = UnityEngine.Object.Instantiate(pjust_effect, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                        effect.text = UnityEngine.Object.Instantiate(pjust_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);

                    }
                    else if (effect.score == 0.7f)
                    {
                        just++;
                        score += 2.8f;
                        effect.obj = UnityEngine.Object.Instantiate(just_effect, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                        effect.text = UnityEngine.Object.Instantiate(just_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);

                    }
                    else
                    {
                        good++;
                        score += 2.0f;
                        effect.obj = UnityEngine.Object.Instantiate(good_effect, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                        effect.text = UnityEngine.Object.Instantiate(good_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);

                    }
                    for (int num28 = lock_check[j].note.x_pos - 1; num28 < lock_check[j].note.x_pos + lock_check[j].note.width - 1; num28++)
                    {
                        locked[num28] = false;
                    }


                    float num27 = 50f * (float)Screen.width / (560f * (float)Screen.height);
                    effect.obj.transform.localScale = new Vector3(num26 * effect.width * 0.98f, num26, 1f);
                    effect.text.transform.localScale = new Vector3(num27, num27, 1f);
                    effect.obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                    effect.text.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                    if (effect.score < 1f)
                    {
                        if ((lock_check[j].note.timing - lock_check[j].pushing_time - timing_offset) / bs > 0f)
                        {
                            effect.fs = UnityEngine.Object.Instantiate(fast_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos - 1f + effect.width / 2f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 4.5f), -0.2f), Quaternion.identity);
                            fast++;
                        }
                        else
                        {
                            effect.fs = UnityEngine.Object.Instantiate(slow_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos - 1f + effect.width / 2f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 4.5f), -0.2f), Quaternion.identity);
                            slow++;
                        }
                        effect.fs.transform.localScale = new Vector3(num27 * 0.4f, num27 * 0.4f, 1f);
                        effect.fs.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                    }
                    rendering_Effects.Add(effect);
                    Destroy(lock_check[j].obj);

                    Destroy(lock_check[j].obj2[0]);
                    Destroy(lock_check[j].obj4);
                    passed_note++;
                    rendering_Notes.Remove(lock_check[j]);
                    lock_check.Remove(lock_check[j]);


                }
                else
                {
                    float num26 = 5f * (float)Screen.width / (float)Screen.height / 28f;
                    float num27 = 50f * (float)Screen.width / (560f * (float)Screen.height);
                    Effect effect = new Effect();
                    effect.x_pos = lock_check[j].note.x_pos;
                    effect.timing = offset;
                    effect.score = 0.25f;
                    effect.width = lock_check[j].note.width;
                    score += 1.0f;
                    good++;
                    effect.obj = UnityEngine.Object.Instantiate(good_effect, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - 0.5f * num26 * (float)Screen.height), -0.2f), Quaternion.identity);
                    effect.text = UnityEngine.Object.Instantiate(good_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos + effect.width / 2f - 1f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 5f), -0.2f), Quaternion.identity);
                    effect.fs = UnityEngine.Object.Instantiate(slow_text, canvas_to_unity(new Vector2(get_x((float)effect.x_pos - 1f + effect.width / 2f), (float)Screen.height - judge_offset - (float)Screen.width / 28f * 4.5f), -0.2f), Quaternion.identity);
                    slow++;
                    effect.obj.transform.localScale = new Vector3(num26 * effect.width * 0.98f, num26, 1f);
                    effect.text.transform.localScale = new Vector3(num27, num27, 1f);
                    effect.obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
                    effect.text.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);

                    effect.fs.transform.localScale = new Vector3(num27 * 0.4f, num27 * 0.4f, 1f);
                    effect.fs.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);

                    for (int num28 = lock_check[j].note.x_pos - 1; num28 < lock_check[j].note.x_pos + lock_check[j].note.width - 1; num28++)
                    {
                        locked[num28] = false;
                    }
                    rendering_Effects.Add(effect);
                    Destroy(lock_check[j].obj);

                    Destroy(lock_check[j].obj2[0]);
                    Destroy(lock_check[j].obj4);
                    passed_note++;
                    rendering_Notes.Remove(lock_check[j]);
                    lock_check.Remove(lock_check[j]);

                }
            }

        }
        for (int num11 = rendering_Effects.Count - 1; num11 >= 0; num11--)
		{
            if (rendering_Effects[num11].add_combo == false && offset < rendering_Effects[num11].timing)
            {
                combo++;
                if (combo > max_combo)
                {
                    max_combo = combo;
                }
                rendering_Effects[num11].add_combo = true;
            }

            float num12 = (offset - rendering_Effects[num11].timing) / bs;
			if (num12 > effect_time + text_time)
			{
				UnityEngine.Object.Destroy(rendering_Effects[num11].text);
                if (rendering_Effects[num11].score > 0f)
                {
                    UnityEngine.Object.Destroy(rendering_Effects[num11].obj);
                    if (rendering_Effects[num11].score < 1f)

                    {
                        UnityEngine.Object.Destroy(rendering_Effects[num11].fs);
                    }

                }
				rendering_Effects.Remove(rendering_Effects[num11]);
			}
			else
			{
				float a = 0f;
				float num13 = 0f;
				if (num12 < effect_time)
				{
					a = 1f - Math.Abs(1f - 2f * num12 / effect_time);
				}
				num13 = 1f - Math.Abs(1f - 2f * num12 / (text_time + effect_time));
				if (rendering_Effects[num11].score > 0f)
				{
					rendering_Effects[num11].obj.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, a);
                    if(rendering_Effects[num11].score < 1f)
                    {
                        rendering_Effects[num11].fs.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, num13);
                    }
				}
				rendering_Effects[num11].text.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, num13);
			}
		}
		if (combo == 0)
		{
			combo_text.SetActive(value: false);
			combo_title.SetActive(value: false);
		}
		else
		{
			combo_text.SetActive(value: true);
			combo_title.SetActive(value: true);
            combo_text.GetComponent<Text>().text = combo.ToString();
        }
        score_text.GetComponent<Text>().text = ((int)Math.Round(score / total_score * 1000000f)).ToString();
	}
}
