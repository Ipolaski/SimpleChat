namespace AFC.Infrastructure.Api.Request.Chat
{
    /// <summary>
    /// Запись, содержащая Guid беседы для которой нужно загрузить сообщения
    /// </summary>
    /// <param name="GroupId">Guid беседы</param>
    public record GetChatMessagesByGroupIdRequest(Guid GroupId);
}