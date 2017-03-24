using System;
using System.Collections.Generic;

namespace ChatServer
{
    class Chat_Room
    {
        private Dictionary<Guid, StateObject> chatMembers = new Dictionary<Guid, StateObject>();
        private String name;
        private Guid Id;

        public Chat_Room(String name)
        {
            SetName(name);
            Id = Guid.NewGuid();
        }

        public void SetName(String s)
        {
            this.name = s;
        }

        public String GetName()
        {
            return this.name;
        }

        public Guid GetId()
        {
            return this.Id;
        }

        public void AddMember(StateObject newMember, Guid memberId, String alias)
        {
            newMember.Id = memberId;
            newMember.Alias = alias;
            chatMembers.Add(memberId, newMember);
        }

        public void RemoveMember()
        {

        }

        public Dictionary<Guid, StateObject> GetMembers()
        {
            return chatMembers;
        }
    }
}
