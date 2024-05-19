using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using System.IO;
using System.Text;

public class SetText : MonoBehaviour
{
    public GameObject musicTitleText; // テキストを変更したいTextMeshProオブジェクトへの参照
    public GameObject composerText;
    public GameObject bpmText;
    public GameObject Charter;
    public GameObject JacketImage;
    public GameObject Level;
    private string composer;
    private string charter;
    private string level;

    // この関数を呼び出してテキストを変更します
    public void ChangeText(string newMusicTitle, string newComposer, string newBPM, string newCharter, string newLevel)
    {
        musicTitleText.GetComponent<TextMeshProUGUI>().text = newMusicTitle;
        composerText.GetComponent<TextMeshProUGUI>().text = newComposer;
        bpmText.GetComponent<TextMeshProUGUI>().text = newBPM;
        Charter.GetComponent<TextMeshProUGUI>().text = newCharter;
        Sprite Jacket = Resources.Load<Sprite>("Jackets/" + newMusicTitle);
        JacketImage.GetComponent<Image>().sprite = Jacket;
        Level.GetComponent<TextMeshProUGUI>().text = newLevel;
    }

    void Start()
    {
        string chart_name = name;
        JObject jsonObj;
        string loadjson = Resources.Load<TextAsset>("Charts/" + chart_name).ToString();
        chart_name = chart_name.Replace("Another/", "").Replace("Master/", "").Replace("Hard/", "").Replace("Easy/", "");
        try
        {
            //StreamReader sr = new StreamReader(name + ".json", Encoding.GetEncoding("UTF-8"));
            //string loadjson = sr.ReadToEnd();
            //sr.Close();

            jsonObj = JObject.Parse(loadjson);

            if (jsonObj["maindata"]["composer"] + "" != "")
            {
                composer = jsonObj["maindata"]["composer"] + "";
            }
            else
            {
                composer = "Unknown";
            }

            if (jsonObj["maindata"]["charter"] + "" != "")
            {
                charter = jsonObj["maindata"]["charter"] + "";
            }
            else
            {
                charter = "Unknown";
            }

            if (jsonObj["maindata"]["level"] + "" != "")
            {
                level = jsonObj["maindata"]["level"] + "";
            }
            else
            {
                level = "Unknown";
            }
        }
        catch
        {
            //StreamReader sr = new StreamReader(name + ".json", Encoding.GetEncoding("Shift_JIS"));
            //string loadjson = sr.ReadToEnd();
            //sr.Close();

            jsonObj = JObject.Parse(loadjson);

            if (jsonObj["maindata"]["composer"] + "" != "")
            {
                composer = jsonObj["maindata"]["composer"] + "";
            }
            else
            {
                composer = "Unknown";
            }

            if (jsonObj["maindata"]["charter"] + "" != "")
            {
                charter = jsonObj["maindata"]["charter"] + "";
            }
            else
            {
                charter = "Unknown";
            }

            if (jsonObj["maindata"]["level"] + "" != "")
            {
                level = jsonObj["maindata"]["level"] + "";
            }
            else
            {
                level = "Unknown";
            }
        }

        ChangeText(chart_name, composer, "BPM: " + jsonObj["maindata"]["bpm"], "譜面制作: " + charter, "Lv. " + level);
        ////Debug.Log("name:" + name + "\ncomposer: " + composer + "\nBPM: " + jsonObj["maindata"]["bpm"]);
    }
}