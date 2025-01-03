using System;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

[Serializable]
public struct LobbyConfig
{
    public string RoomName;
    public int MaxPlayerAmount;
    public bool IsPrivate;
}

public static class LobbyCustomization
{
    private const int MAX_QUERY_RESULTS = 25;
    public const int MIN_PLAYERS = 2;
    public const int MAX_PLAYERS = 12;

    public static QueryLobbiesOptions GenerateQueryOptions()
    {
        var options = new QueryLobbiesOptions
        {
            Count = MAX_QUERY_RESULTS,
            Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"
                )
            }
        };

        return options;
    }

    public static CreateLobbyOptions GenerateCreateLobbyOptions(LobbyConfig config, string joinCode)
    {
        var options = new CreateLobbyOptions
        {
            IsPrivate = config.IsPrivate,
            Data = new Dictionary<string, DataObject>
            {
                {
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: joinCode
                    )
                }
            }
        };

        return options;
    }
}