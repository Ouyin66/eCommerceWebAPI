using Microsoft.AspNetCore.SignalR;

namespace eCommerceWebAPI.ModelFromDB
{
    public class NotificationHub : Hub {

        public async Task JoinGroup(string receiptId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, receiptId);
        }

        public async Task LeaveGroup(string receiptId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, receiptId);
        }
    }
}
