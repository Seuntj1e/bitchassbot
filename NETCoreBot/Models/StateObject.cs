using Microsoft.AspNetCore.SignalR;

namespace BitchAssBot.Models
{
    public class StateObject
    {
        public string ConnectionId { get; set; }
        public int PreviousTick { get; set; }
        public int CurrentTick { get; set; }
    }
}