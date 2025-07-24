using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

public class AuthUI : MonoBehaviour
{
    public TMP_InputField ActivationCode;
    public UnityEngine.UI.Toggle ButtonOnlineMode;
    public UnityEngine.UI.Toggle ButtonOfflineMode;
    public UnityEngine.UI.Button ButtonActivate;
    public TMP_Text ModeDetail;
    public TMP_Text AuthStatus;

    private string LastActivationCode = "";
    private bool LastOnlineModeButton = false;
    private bool LastOfflineModeButton = true;
    private List<int> SplitCounts = new List<int>();
    private List<char> ValiedChars = new List<char>();

    // httpクライアント
    private static HttpClient client = new HttpClient();
    //レコード作成
    private async Task Post(string ActivationCode)
    {
        //Post先URL
        string url = "https://keichankotaro.com/%E6%96%87%E5%8C%96%E7%A5%AD%E9%9F%B3%E3%82%B2%E3%83%BC/api/auth/";
        //Jsonデータ
        string parameters = "{\"code\": \""+ActivationCode+"\"}";
        //Postで付与するパラメータ
        var content = new StringContent(parameters, Encoding.UTF8, "application/json");


        //Post通信
        HttpResponseMessage Response;
        try
        {
            Response = await client.PostAsync(url, content);
            string res = await Response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(res);

        }
        catch
        {
            return;
        }
    }

    private void Activate()
    {
        if (ActivationCode.text.Length == 23)
        {
            AuthStatus.color = Color.green;
            AuthStatus.text = "アクティベーションに成功しました。ゲームを再起動します。";
        }
        else
        {
            AuthStatus.color = Color.red;
            AuthStatus.text = "アクティベーションに失敗しました。文字数を確認してください。";
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SplitCounts.Add(5);
        SplitCounts.Add(11);
        SplitCounts.Add(17);
        ValiedChars.Add('0');
        ValiedChars.Add('1');
        ValiedChars.Add('2');
        ValiedChars.Add('3');
        ValiedChars.Add('4');
        ValiedChars.Add('5');
        ValiedChars.Add('6');
        ValiedChars.Add('7');
        ValiedChars.Add('8');
        ValiedChars.Add('9');
        ValiedChars.Add('A');
        ValiedChars.Add('B');
        ValiedChars.Add('C');
        ValiedChars.Add('D');
        ValiedChars.Add('E');
        ValiedChars.Add('F');
        ValiedChars.Add('a');
        ValiedChars.Add('b');
        ValiedChars.Add('c');
        ValiedChars.Add('d');
        ValiedChars.Add('e');
        ValiedChars.Add('f');

        ButtonActivate.onClick.AddListener(Activate);
    }

    // Update is called once per frame
    void Update()
    {
        // アクティベーションID入力時の自動変換処理
        if (ActivationCode.text != LastActivationCode)
        {
            //Debug.Log(ActivationCode.text);
            if (LastActivationCode.Length > ActivationCode.text.Length)
            {
                ActivationCode.text = "";
                LastActivationCode = "";
            }
            else if (ActivationCode.text.Length > 23)
            {
                ActivationCode.text = LastActivationCode;
            }
            else if (ValiedChars.Contains(ActivationCode.text[ActivationCode.text.Length - 1]))
            {
                if (SplitCounts.Contains(ActivationCode.text.Length))
                {
                    ActivationCode.text += "-";
                    ActivationCode.text = ActivationCode.text.ToUpper();
                    ActivationCode.MoveToEndOfLine(false, false);
                    LastActivationCode = ActivationCode.text;
                }
                else
                {
                    ActivationCode.text = ActivationCode.text.ToUpper();
                    LastActivationCode = ActivationCode.text;
                }
            }
            else
            {
                ActivationCode.text = LastActivationCode;
            }
        }

        // オン・オフラインモードの変更時
        if (ButtonOfflineMode.isOn && !ButtonOnlineMode.isOn)
        {
            // オフラインモード選択時
            ModeDetail.text = "音声・譜面データを端末側にすべて保存します。";
        }
        else if (ButtonOnlineMode.isOn && !ButtonOfflineMode.isOn)
        {
            // オンラインモード選択時
            ModeDetail.text = "プレイ時に毎回インターネットから音声・譜面データをダウンロードします。";
        }
    }
}
