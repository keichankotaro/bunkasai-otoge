
using UnityEngine;

public class MusicSelectManager : MonoBehaviour
{
    void Start()
    {
        // Ensure APIManager instance exists
        if (APIManager.Instance == null)
        {
            gameObject.AddComponent<APIManager>();
        }

        // Fetch all high scores once when the scene loads
        if (APIManager.Instance != null && APIManager.Instance.IsLoggedIn())
        {
            StartCoroutine(APIManager.Instance.FetchHighScores());
        }
    }
}
