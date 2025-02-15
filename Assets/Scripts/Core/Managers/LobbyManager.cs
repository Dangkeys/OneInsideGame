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
    public Lobby CurrentLobby {get; private set;}
    #region CRUD Methods


    public async Task<CreateLobbyAllocationResponseDto> CreateLobbyAsync(CreateLobbyDto createLobbyDto)
    {
        try
        {
            Allocation allocation = await CreateAllocationAsync();

            string relayJoinCode = await GetRelayJoinCodeAsync(allocation);

            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(createLobbyDto.LobbyName, createLobbyDto.MaxPlayers, new CreateLobbyOptions
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

            return new CreateLobbyAllocationResponseDto(CurrentLobby, allocation);
        }
        catch (RequestFailedException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogWarning($"Lobby creation failed: {e.Message}");
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

            CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, updateOptions);
            return CurrentLobby;
        }
        catch (LobbyServiceException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogWarning($"Lobby update failed: {e.Message}");
            return null;
        }
    }
    public async Task<Lobby> DeleteCurrentLobbyAsync()
    {
        try
        {
            var deletedLobby = CurrentLobby;
            await LobbyService.Instance.DeleteLobbyAsync(CurrentLobby.Id);
            CurrentLobby = null;
            return deletedLobby;
        }
        catch (LobbyServiceException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogWarning($"Lobby deletion failed: {e.Message}");
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
                    op: QueryFilter.OpOptions.CONTAINS,
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
            Debug.LogWarning($"Lobby query failed: {e.Message}");
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
            Debug.LogWarning($"Lobby query failed: {e.Message}");
            return null;
        }
    }


    #endregion
    #region Player Methods
    public async Task<JoinLobbyAllocationResponseDto> QuickJoinAsync()
    {
        try
        {
            CurrentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            JoinAllocation joinAllocation = await JoinRelayAsync(CurrentLobby.Data[OneInside.Constants.Lobby.KEY_RELAY_JOIN_CODE].Value);

            return new JoinLobbyAllocationResponseDto(CurrentLobby, joinAllocation);
        }
        catch (RequestFailedException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogWarning($"Quick join failed: {e.Message}");
            return null;
        }
    }

    public async Task<JoinLobbyAllocationResponseDto> JoinLobbyByCodeAsync(string lobbyCode)
    {
        try
        {
            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            JoinAllocation joinAllocation = await JoinRelayAsync(CurrentLobby.Data[OneInside.Constants.Lobby.KEY_RELAY_JOIN_CODE].Value);

            return new JoinLobbyAllocationResponseDto(CurrentLobby, joinAllocation);
        }
        catch (RequestFailedException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogWarning($"Join lobby by code failed: {e.Message}");
            return null;
        }
    }

    public async Task<JoinLobbyAllocationResponseDto> JoinLobbyByIdAsync(string lobbyId)
    {
        try
        {
            CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            JoinAllocation joinAllocation = await JoinRelayAsync(CurrentLobby.Data[OneInside.Constants.Lobby.KEY_RELAY_JOIN_CODE].Value);

            return new JoinLobbyAllocationResponseDto(CurrentLobby, joinAllocation);
        }
        catch (RequestFailedException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogWarning($"Join lobby by id failed: {e.Message}");
            return null;
        }
    }

    public async Task LeaveLobbyAsync()
    {
        try
        {
            if (CurrentLobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                StopAllCoroutines();
            }

            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId);
            CurrentLobby = null;
        }
        catch (LobbyServiceException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogWarning($"Lobby leave failed: {e.Message}");
        }
    }

    public async Task KickPlayerAsync(string playerId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);
        }
        catch (LobbyServiceException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogWarning($"Player kick failed: {e.Message}");
        }
    }

    #endregion

    #region Lobby Methods

    public async void MigrateHostAsync()
    {
        try
        {
            var newHost = CurrentLobby.Players.FirstOrDefault(p => p.Id != AuthenticationService.Instance.PlayerId);
            if (newHost == null)
            {
                Debug.LogWarning("No other player to migrate host to");
                return;
            }

            await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions
            {
                HostId = newHost.Id
            });

            StopAllCoroutines();
            StartCoroutine(HeartbeatLobby());
        }
        catch (LobbyServiceException e)
        {
            OnRequestFailed?.Invoke(new RequestErrorDto(e.ErrorCode, e.Message));
            Debug.LogWarning($"Host migration failed: {e.Message}");
        }
    }

    private IEnumerator HeartbeatLobby()
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(OneInside.Constants.Lobby.HEART_BEAT_WAIT_TIME);
        while (true)
        {
            if (CurrentLobby == null || CurrentLobby.HostId != AuthenticationService.Instance.PlayerId)
            {
                yield break;
            }
            LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
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
            Debug.LogWarning($"Relay allocation failed: {e.Message}");
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
            Debug.LogWarning($"Relay join code failed: {e.Message}");
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
            Debug.LogWarning($"Relay join failed: {e.Message}");
            return null;
        }
    }

    #endregion
}
