namespace Infrastructure.AFC.Infrastructure.Api.Request.Chat
{
    /// <summary>
    /// Запись хранящая ID пользователя и количество чатов
    /// </summary>
    /// <param name="UserId"></param>
    /// <param name="Count"></param>
    public record GetChatsByUserIdRequest(Guid UserId, int Count);
}