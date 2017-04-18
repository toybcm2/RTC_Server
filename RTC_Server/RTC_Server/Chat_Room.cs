using System;
using System.Collections.Generic;

namespace ChatServer
{
    class Chat_Room
    {
        private Dictionary<string, StateObject> chatMembers = new Dictionary<string, StateObject>();
        private String name;
        private string Id;

        public Chat_Room(String name, string roomId)
        {
            SetName(name);
            Id = roomId;
        }

        public void SetName(String s)
        {
            this.name = s;
        }

        public String GetName()
        {
            return this.name;
        }

        public string GetId()
        {
            return this.Id;
        }

        public void AddMember(StateObject newMember, string memberId, string alias)
        {
            newMember.ClientId = memberId;
            newMember.Alias = alias;
            chatMembers.Add(memberId, newMember);
        }

        public void RemoveMember()
        {

        }

        public Dictionary<string, StateObject> GetMembers()
        {
            return chatMembers;
        }
    }
}
