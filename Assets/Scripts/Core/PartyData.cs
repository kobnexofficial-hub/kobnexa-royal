namespace KobnexaRoyal
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public sealed class PartyMember
    {
        public string Name;
        public bool Ready;
        public bool IsLocal;
    }

    /// <summary>Local party only. No online service: members are local friends
    /// from PrototypeCatalog, ready-up is a local flag, matchmaking is not faked.</summary>
    public sealed class PartyData
    {
        public const int MaxSize = 4;
        private readonly List<PartyMember> members = new List<PartyMember>();

        public PartyData()
        {
            members.Add(new PartyMember { Name = "KobnexaEsteril", Ready = false, IsLocal = true });
        }

        public int Count { get { return members.Count; } }

        public PartyMember Member(int index)
        {
            if (index < 0 || index >= members.Count) return null;
            return members[index];
        }

        public bool Add(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            if (members.Count >= MaxSize) return false;
            for (int i = 0; i < members.Count; i++) if (members[i].Name == name) return false;
            members.Add(new PartyMember { Name = name, Ready = false, IsLocal = false });
            return true;
        }

        public bool Remove(string name)
        {
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].Name == name && !members[i].IsLocal)
                {
                    members.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public void ToggleReady(string name)
        {
            for (int i = 0; i < members.Count; i++)
                if (members[i].Name == name) members[i].Ready = !members[i].Ready;
        }

        public bool AllReady()
        {
            if (members.Count == 0) return false;
            for (int i = 0; i < members.Count; i++) if (!members[i].Ready) return false;
            return true;
        }
    }
}
