using System;
using Unity.Netcode;

[Serializable]
public class UserDataDto : INetworkSerializable, IEquatable<UserDataDto>
{
    public string AuthId;
    public string Name;

    public string CrewAbilityId;

    public string ImposterAbilityId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref AuthId);
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref CrewAbilityId);
        serializer.SerializeValue(ref ImposterAbilityId);
    }

    public bool Equals(UserDataDto other)
    {
        if (other == null)
            return false;
            
        return AuthId == other.AuthId && 
               Name == other.Name &&
               CrewAbilityId == other.CrewAbilityId &&
               ImposterAbilityId == other.ImposterAbilityId;
    }

    public override bool Equals(object obj)
    {
        if (obj is UserDataDto other)
            return Equals(other);
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(AuthId, Name, CrewAbilityId, ImposterAbilityId);
    }
}