using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class DebugSocketClient : MonoBehaviour
{
    private static DebugSocketClient _instance;
    public static DebugSocketClient Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("DebugSocketClient");
                _instance = go.AddComponent<DebugSocketClient>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private UdpClient client;
    private IPEndPoint endPoint;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        try
        {
            client = new UdpClient();
            endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 50051);
            Debug.Log("DebugSocketClient initialized.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize DebugSocketClient: {e.Message}");
            client = null;
        }
    }

    public void SendData(string jsonPayload)
    {
        if (client == null) return;

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(jsonPayload);
            client.Send(data, data.Length, endPoint);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to send data via UDP: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        if (client != null)
        {
            client.Close();
            client = null;
        }
    }
}
