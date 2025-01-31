namespace Infrastructure.AFC.Infrastructure.Api.Request.Chat
{
    /// <summary>
    /// Запись хранящая ID чата
    /// </summary>
    /// <param name="Id"></param>
    public record DeleteChatRequest(Guid Id);
}