using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public event Action<RequestErrorDto> OnRequestFailed;
    private Lobby currentLoby;

    public async Task CreateLobby(CreateLobbyDto createLobbyDto)
    {
        try
        {
            currentLoby = await LobbyService.Instance.CreateLobbyAsync(createLobbyDto.LobbyName, createLobbyDto.MaxPlayers, new CreateLobbyOptions
            {
                IsPrivate = createLobbyDto.IsPrivate,
                Data = new Dictionary<string, DataObject>
                {
                    {OneInside.Constants.Lobby.KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, createLobbyDto.GameMode.ToString(), DataObject.IndexOptions.S1)},
                    { OneInside.Constants.Lobby.KEY_IMPOSTER_AMOUNT, new DataObject(DataObject.VisibilityOptions.Public, createLobbyDto.ImposterAmount.ToString(), DataObject.IndexOptions.S2) },
                    { OneInside.Constants.Lobby.KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, createLobbyDto.RelayJoinCode, DataObject.IndexOptions.S3) },
                }
            });
        }
        catch (LobbyServiceException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
        }
    }

    public async Task<Lobby> GetLobby(LobbyQueryDto queryDto)
    {
        try
        {
            var filters = new List<QueryFilter>();

            if (!string.IsNullOrEmpty(queryDto.LobbyName))
            {
                filters.Add(new QueryFilter(
                    field: QueryFilter.FieldOptions.Name,
                    op: QueryFilter.OpOptions.EQ,
                    value: queryDto.LobbyName
                ));
            }

            if (queryDto.GameMode.HasValue)
            {
                filters.Add(new QueryFilter(
                    field: QueryFilter.FieldOptions.S1,
                    op: QueryFilter.OpOptions.EQ,
                    value: queryDto.GameMode.Value.ToString()
                ));
            }

            if (queryDto.MaxPlayers.HasValue)
            {
                filters.Add(new QueryFilter(
                    field: QueryFilter.FieldOptions.MaxPlayers,
                    op: QueryFilter.OpOptions.EQ,
                    value: queryDto.MaxPlayers.Value.ToString()
                ));
            }

            if (queryDto.ImposterAmount.HasValue)
            {
                filters.Add(new QueryFilter(
                    field: QueryFilter.FieldOptions.S2,
                    op: QueryFilter.OpOptions.EQ,
                    value: queryDto.ImposterAmount.Value.ToString()
                ));
            }

            if (!string.IsNullOrEmpty(queryDto.LobbyJoinCode))
            {
                filters.Add(new QueryFilter(
                    field: QueryFilter.FieldOptions.S3,
                    op: QueryFilter.OpOptions.EQ,
                    value: queryDto.LobbyJoinCode
                ));
            }

            if (!string.IsNullOrEmpty(queryDto.IsPrivate))
            {
                filters.Add(new QueryFilter(
                    field: QueryFilter.FieldOptions.HasPassword,
                    op: QueryFilter.OpOptions.EQ,
                    value: queryDto.IsPrivate
                ));
            }

            var options = new QueryLobbiesOptions
            {
                Filters = filters
            };

            var results = await LobbyService.Instance.QueryLobbiesAsync(options);
            return results.Results.FirstOrDefault();
        }
        catch (LobbyServiceException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogError($"Lobby query failed: {e.Message}");
            return null;
        }
    }
}
