namespace Chatbot.Models;
using System;
using System.Collections.Generic;
public class ChatSession {
    public Guid SessionId { get; set; } = Guid.NewGuid();
    public List<ChatMessage> Messages { get; set; } = new();
    public SessionState State { get; set; } = new();
    public void Reset() {
        SessionId = Guid.NewGuid();
        Messages.Clear();
        State = new SessionState();
    }

    public void Send(string userMessage) {
        Messages.Add(new ChatMessage { Text = userMessage, IsUser = true });
    }
}