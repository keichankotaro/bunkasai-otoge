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

    // http�N���C�A���g
    private static HttpClient client = new HttpClient();
    //���R�[�h�쐬
    private async Task Post(string ActivationCode)
    {
        //Post��URL
        string url = "https://keichankotaro.com/%E6%96%87%E5%8C%96%E7%A5%AD%E9%9F%B3%E3%82%B2%E3%83%BC/api/auth/";
        //Json�f�[�^
        string parameters = "{\"code\": \""+ActivationCode+"\"}";
        //Post�ŕt�^����p�����[�^
        var content = new StringContent(parameters, Encoding.UTF8, "application/json");


        //Post�ʐM
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
            AuthStatus.text = "�A�N�e�B�x�[�V�����ɐ������܂����B�Q�[�����ċN�����܂��B";
        }
        else
        {
            AuthStatus.color = Color.red;
            AuthStatus.text = "�A�N�e�B�x�[�V�����Ɏ��s���܂����B���������m�F���Ă��������B";
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
        // �A�N�e�B�x�[�V����ID���͎��̎����ϊ�����
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

        // �I���E�I�t���C�����[�h�̕ύX��
        if (ButtonOfflineMode.isOn && !ButtonOnlineMode.isOn)
        {
            // �I�t���C�����[�h�I����
            ModeDetail.text = "�����E���ʃf�[�^��[�����ɂ��ׂĕۑ����܂��B";
        }
        else if (ButtonOnlineMode.isOn && !ButtonOfflineMode.isOn)
        {
            // �I�����C�����[�h�I����
            ModeDetail.text = "�v���C���ɖ���C���^�[�l�b�g���特���E���ʃf�[�^���_�E�����[�h���܂��B";
        }
    }
}
