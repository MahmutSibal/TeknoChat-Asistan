using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TeknofestAsistan.API.Hubs;

/// <summary>Server-push only — clients don't call methods on this hub, they just connect and
/// listen. Every connection is auto-joined to a per-user group (personal notifications) and,
/// for Destek Ekibi/Sistem Yöneticisi, a shared staff group (new-ticket alerts).</summary>
[Authorize]
public class NotificationHub : Hub
{
    public const string StaffGroup = "staff";

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        }

        var role = Context.User?.FindFirstValue(ClaimTypes.Role);
        if (role is "DestekEkibi" or "SistemYoneticisi")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, StaffGroup);
        }

        await base.OnConnectedAsync();
    }

    public static string UserGroup(string userId) => $"user-{userId}";
    public static string UserGroup(int userId) => UserGroup(userId.ToString());
}
