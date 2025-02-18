using Unity.Services.Authentication;

public static class UserWrapper
{
    public static void RandomPlayerName()
    {
        if (AuthenticationService.Instance != null)
        {
            AuthenticationService.Instance.UpdatePlayerNameAsync("Player" + AuthenticationService.Instance.PlayerId.Trim().Substring(0, 5));
        }
    }
}