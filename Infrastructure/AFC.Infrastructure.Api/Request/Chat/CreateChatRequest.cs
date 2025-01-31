namespace Infrastructure.AFC.Infrastructure.Api.Request.Chat
{
    /// <summary>
    /// Запись хранящая имя чата и его создателя
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="Owner"></param>
	public record CreateChatRequest(string Name, Database.Entities.User Owner);
}