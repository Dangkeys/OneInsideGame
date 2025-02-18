using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

public static class PlayerNameGenerator
{
    private static readonly string[] prefixes = { "Player", "Gamer", "Hero", "Champion", "Warrior" };
    private static readonly string[] suffixes = { "Pro", "Elite", "Master", "Star", "Legend" };

    public static async Task<bool> GenerateRandomPlayerName()
    {
        try
        {
            if (AuthenticationService.Instance == null)
            {
                Debug.LogError("Authentication service not initialized");
                return false;
            }

            string randomPrefix = prefixes[UnityEngine.Random.Range(0, prefixes.Length)];
            string randomSuffix = suffixes[UnityEngine.Random.Range(0, suffixes.Length)];
            string uniqueId = AuthenticationService.Instance.PlayerId
                .Replace("-", "")
                .Substring(0, 4)
                .ToUpper();

            string newPlayerName = $"{randomPrefix}{uniqueId}{randomSuffix}";

            await AuthenticationService.Instance.UpdatePlayerNameAsync(newPlayerName);
            Debug.Log($"Successfully updated player name to: {newPlayerName}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to update player name: {ex.Message}");
            return false;
        }
    }

    public static string GenerateName()
    {
        string randomPrefix = prefixes[UnityEngine.Random.Range(0, prefixes.Length)];
        string randomSuffix = suffixes[UnityEngine.Random.Range(0, suffixes.Length)];
        string randomNumber = UnityEngine.Random.Range(100, 999).ToString();

        return $"{randomPrefix}{randomNumber}{randomSuffix}";
    }
}