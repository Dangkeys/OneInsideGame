using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;

public class CreateLobbyAllocationResponseDto
    {
        public CreateLobbyAllocationResponseDto(Lobby lobby, Allocation allocation)
        {
            Lobby = lobby;
            Allocation = allocation;
        }
        public Lobby Lobby { get; set; }
        public Allocation Allocation { get; set; }
    }

    public class JoinLobbyAllocationResponseDto
    {
        public JoinLobbyAllocationResponseDto(Lobby lobby, JoinAllocation joinAllocation)
        {
            Lobby = lobby;
            JoinAllocation = joinAllocation;
        }
        public Lobby Lobby { get; set; }
        public JoinAllocation JoinAllocation { get; set; }
    }