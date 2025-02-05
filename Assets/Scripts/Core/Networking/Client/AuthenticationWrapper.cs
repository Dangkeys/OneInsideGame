using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public static class AuthenticationWrapper
{
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

    public static async Task<AuthState> DoAuth(int maxRetries = 5)
    {
        if (AuthState == AuthState.Authenticated &&
            AuthenticationService.Instance.IsSignedIn &&
            AuthenticationService.Instance.IsAuthorized)
        {
            return AuthState;
        }

        if (AuthState == AuthState.Authenticated)
        {
            AuthState = AuthState.NotAuthenticated;
        }

        if (AuthState == AuthState.Authenticating)
        {
            Debug.LogWarning("Already authenticating!");
            await Authenticating();
            return AuthState;
        }

        if (await TrySignInWithCachedCredentials())
        {
            return AuthState;
        }


        await SignInAnonymouslyAsync(maxRetries);

        return AuthState;
    }

    private static async Task<bool> TrySignInWithCachedCredentials()
    {
        // Check if we have cached credentials
        if (!AuthenticationService.Instance.SessionTokenExists)
        {
            return false;
        }

        AuthState = AuthState.Authenticating;

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            AuthState = AuthState.Authenticated;
            return true;
        }
        catch (AuthenticationException ex)
        {
            Debug.LogWarning($"Cached sign-in failed (AuthenticationException): {ex.Message}");
            AuthState = AuthState.NotAuthenticated;
            return false;
        }
        catch (RequestFailedException ex)
        {
            Debug.LogWarning($"Cached sign-in failed (RequestFailedException): {ex.Message}");
            AuthState = AuthState.NotAuthenticated;
            return false;
        }
    }

    private static async Task<AuthState> Authenticating()
    {
        while (AuthState == AuthState.Authenticating || AuthState == AuthState.NotAuthenticated)
        {
            await Task.Delay(200);
        }

        return AuthState;
    }

    private static async Task SignInAnonymouslyAsync(int maxRetries)
    {
        AuthState = AuthState.Authenticating;

        int retries = 0;
        while (AuthState == AuthState.Authenticating && retries < maxRetries)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                {
                    AuthState = AuthState.Authenticated;
                    Debug.Log($"New anonymous sign-in successful. Player ID: {AuthenticationService.Instance.PlayerId}");
                    break;
                }
            }
            catch (AuthenticationException authException)
            {
                Debug.LogError($"Authentication failed: {authException}");
                AuthState = AuthState.Error;
            }
            catch (RequestFailedException requestException)
            {
                Debug.LogError($"Request failed: {requestException}");
                AuthState = AuthState.Error;
            }

            retries++;
            await Task.Delay(1000);
        }

        if (AuthState != AuthState.Authenticated)
        {
            Debug.LogWarning($"Player was not signed in successfully after {retries} retries");
            AuthState = AuthState.TimeOut;
        }
    }

    public static void ClearCachedCredentials()
    {
        if (AuthenticationService.Instance != null)
        {
            AuthenticationService.Instance.SignOut(true);
            AuthState = AuthState.NotAuthenticated;
            Debug.Log("Cached credentials cleared");
        }
    }
}

