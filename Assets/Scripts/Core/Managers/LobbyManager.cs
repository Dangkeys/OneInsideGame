using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mono.CSharp.Linq;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public event Action<RequestErrorDto> OnRequestFailed;
    private Lobby currentLoby;
    #region CRUD Methods


    public async Task<CreateLobbyAllocationResponseDto> CreateLobbyAsync(CreateLobbyDto createLobbyDto)
    {
        try
        {
            Allocation allocation = await CreateAllocationAsync();

            string relayJoinCode = await GetRelayJoinCodeAsync(allocation);

            currentLoby = await LobbyService.Instance.CreateLobbyAsync(createLobbyDto.LobbyName, createLobbyDto.MaxPlayers, new CreateLobbyOptions
            {
                Player = new Unity.Services.Lobbies.Models.Player(AuthenticationService.Instance.PlayerId),
                IsPrivate = createLobbyDto.IsPrivate,
                Data = new Dictionary<string, DataObject>
                {
                    {OneInside.Constants.Lobby.KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, createLobbyDto.GameMode.ToString(), DataObject.IndexOptions.S1)},
                    { OneInside.Constants.Lobby.KEY_IMPOSTER_AMOUNT, new DataObject(DataObject.VisibilityOptions.Public, createLobbyDto.ImposterAmount.ToString(), DataObject.IndexOptions.S2) },
                    { OneInside.Constants.Lobby.KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode, DataObject.IndexOptions.S3) },
                }
            });

            StopAllCoroutines();
            StartCoroutine(HeartbeatLobby());

            return new CreateLobbyAllocationResponseDto(currentLoby, allocation);
        }
        catch (RequestFailedException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogError($"Lobby creation failed: {e.Message}");
            return null;
        }
    }
    public async Task<Lobby> UpdateCurrentLobbyAsync(UpdateLobbyDto updateLobbyDto)
    {
        try
        {
            var updateOptions = new UpdateLobbyOptions();
            var dataToUpdate = new Dictionary<string, DataObject>();

            if (updateLobbyDto.LobbyName != null)
            {
                updateOptions.Name = updateLobbyDto.LobbyName;
            }

            if (updateLobbyDto.MaxPlayers.HasValue)
            {
                updateOptions.MaxPlayers = updateLobbyDto.MaxPlayers.Value;
            }

            if (updateLobbyDto.IsPrivate != null)
            {
                updateOptions.IsPrivate = updateLobbyDto.IsPrivate;
            }

            if (updateLobbyDto.HostId != null)
            {
                updateOptions.HostId = updateLobbyDto.HostId;
            }

            if (updateLobbyDto.GameMode.HasValue)
            {
                dataToUpdate.Add(
                    OneInside.Constants.Lobby.KEY_GAME_MODE,
                    new DataObject(DataObject.VisibilityOptions.Public, updateLobbyDto.GameMode.Value.ToString(), DataObject.IndexOptions.S1)
                );
            }

            if (updateLobbyDto.ImposterAmount.HasValue)
            {
                dataToUpdate.Add(
                    OneInside.Constants.Lobby.KEY_IMPOSTER_AMOUNT,
                    new DataObject(DataObject.VisibilityOptions.Public, updateLobbyDto.ImposterAmount.Value.ToString(), DataObject.IndexOptions.S2)
                );
            }

            if (dataToUpdate.Count > 0)
            {
                updateOptions.Data = dataToUpdate;
            }

            currentLoby = await LobbyService.Instance.UpdateLobbyAsync(currentLoby.Id, updateOptions);
            return currentLoby;
        }
        catch (LobbyServiceException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogError($"Lobby update failed: {e.Message}");
            return null;
        }
    }
    public async Task<Lobby> DeleteCurrentLobbyAsync()
    {
        try
        {
            var deletedLobby = currentLoby;
            await LobbyService.Instance.DeleteLobbyAsync(currentLoby.Id);
            currentLoby = null;
            return deletedLobby;
        }
        catch (LobbyServiceException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogError($"Lobby deletion failed: {e.Message}");
            return null;
        }
    }

    public async Task<PagedResultDto<Lobby>> GetAllLobbyAsync(LobbyQueryDto queryDto)
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

            var options = new QueryLobbiesOptions
            {
                SampleResults = false,
                Filters = filters,
                Count = queryDto.PageSize,
                ContinuationToken = queryDto.ContinuationToken,
                Order = new List<QueryOrder>
            {   
                // TODO: (Optional)  add more order options
                new QueryOrder(true, QueryOrder.FieldOptions.Created)
            }
            };

            var response = await LobbyService.Instance.QueryLobbiesAsync(options);
            return new PagedResultDto<Lobby>
            {
                Items = response.Results,
                CurrentPageCount = response.Results.Count,
                PageSize = queryDto.PageSize,
                ContinuationToken = response.ContinuationToken
            };
        }
        catch (LobbyServiceException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogError($"Lobby query failed: {e.Message}");
            return null;
        }
    }

    public async Task<Lobby> GetLobbyByIdAsync(string lobbyId)
    {
        try
        {
            return await LobbyService.Instance.GetLobbyAsync(lobbyId);
        }
        catch (LobbyServiceException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogError($"Lobby query failed: {e.Message}");
            return null;
        }
    }


    #endregion
    #region Player Methods
    public async Task<JoinLobbyAllocationResponseDto> QuickJoin()
    {
        try
        {
            currentLoby = await LobbyService.Instance.QuickJoinLobbyAsync();

            JoinAllocation joinAllocation = await JoinRelayAsync(currentLoby.Data[OneInside.Constants.Lobby.KEY_RELAY_JOIN_CODE].Value);

            return new JoinLobbyAllocationResponseDto(currentLoby, joinAllocation);
        }
        catch (RequestFailedException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogError($"Quick join failed: {e.Message}");
            return null;
        }
    }

    public async Task<JoinLobbyAllocationResponseDto> JoinLobbyAsync(string identifier, bool useCode = false)
    {
        try
        {
            currentLoby = useCode
                ? await LobbyService.Instance.JoinLobbyByCodeAsync(identifier)
                : await LobbyService.Instance.JoinLobbyByIdAsync(identifier);

            JoinAllocation joinAllocation = await JoinRelayAsync(currentLoby.Data[OneInside.Constants.Lobby.KEY_RELAY_JOIN_CODE].Value);

            return new JoinLobbyAllocationResponseDto(currentLoby, joinAllocation);
        }
        catch (RequestFailedException e)
        {
            string context = useCode ? "by code" : "";
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogError($"Join lobby {context} failed: {e.Message}");
            return null;
        }
    }

    public async Task<JoinLobbyAllocationResponseDto> JoinLobbyByCodeAsync(string lobbyCode)
    {
        try
        {
            currentLoby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            JoinAllocation joinAllocation = await JoinRelayAsync(currentLoby.Data[OneInside.Constants.Lobby.KEY_RELAY_JOIN_CODE].Value);

            return new JoinLobbyAllocationResponseDto(currentLoby, joinAllocation);
        }
        catch (RequestFailedException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogError($"Join lobby by code failed: {e.Message}");
            return null;
        }
    }

    public async void LeaveLobbyAsync()
    {
        try
        {
            if (currentLoby.HostId == AuthenticationService.Instance.PlayerId)
            {
                StopAllCoroutines();
            }

            await LobbyService.Instance.RemovePlayerAsync(currentLoby.Id, AuthenticationService.Instance.PlayerId);
            currentLoby = null;
        }
        catch (LobbyServiceException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogError($"Lobby leave failed: {e.Message}");
        }
    }

    public async void KickPlayerAsync(string playerId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(currentLoby.Id, playerId);
        }
        catch (LobbyServiceException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogError($"Player kick failed: {e.Message}");
        }
    }

    #endregion

    #region Lobby Methods

    public async void MigrateHostAsync()
    {
        try
        {
            var newHost = currentLoby.Players.FirstOrDefault(p => p.Id != AuthenticationService.Instance.PlayerId);
            if (newHost == null)
            {
                Debug.LogError("No other player to migrate host to");
                return;
            }

            await LobbyService.Instance.UpdateLobbyAsync(currentLoby.Id, new UpdateLobbyOptions
            {
                HostId = newHost.Id
            });

            StopAllCoroutines();
            StartCoroutine(HeartbeatLobby());
        }
        catch (LobbyServiceException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogError($"Host migration failed: {e.Message}");
        }
    }

    private IEnumerator HeartbeatLobby()
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(OneInside.Constants.Lobby.HEART_BEAT_WAIT_TIME);
        while (true)
        {
            if (currentLoby == null || currentLoby.HostId != AuthenticationService.Instance.PlayerId)
            {
                yield break;
            }

            LobbyService.Instance.SendHeartbeatPingAsync(currentLoby.Id);
            yield return delay;
        }
    }
    #endregion

    #region  Relay Methods
    private async Task<Allocation> CreateAllocationAsync()
    {
        try
        {
            return await RelayService.Instance.CreateAllocationAsync(OneInside.Constants.Player.MAX_PLAYERS - 1);
        }
        catch (RelayServiceException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogError($"Relay allocation failed: {e.Message}");
            return null;
        }
    }

    private async Task<string> GetRelayJoinCodeAsync(Allocation allocation)
    {
        try
        {
            return await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        }
        catch (RelayServiceException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogError($"Relay join code failed: {e.Message}");
            return null;
        }
    }

    private async Task<JoinAllocation> JoinRelayAsync(string joinCode)
    {
        try
        {
            return await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (RelayServiceException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogError($"Relay join failed: {e.Message}");
            return null;
        }
    }

    #endregion
}
