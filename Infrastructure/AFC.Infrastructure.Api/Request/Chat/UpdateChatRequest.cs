namespace Infrastructure.AFC.Infrastructure.Api.Request.Chat
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="chatId"></param>
    public record UpdateChatRequest(string Name, Guid chatId);
}