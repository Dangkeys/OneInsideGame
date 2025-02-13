using Unity.Services.Authentication;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public static void RandomPlayerName()
    {
        if (AuthenticationService.Instance != null)
        {
            AuthenticationService.Instance.UpdatePlayerNameAsync("Player" + AuthenticationService.Instance.PlayerId.Trim().Substring(0, 5));
        }
    }
}
