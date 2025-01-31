namespace SignalRChatRoom.Server.Models
{
    /// <summary>
    /// Сущность клиента веб сокета
    /// </summary>
    public class Client
    {
        /// <summary>
        /// ID текущего соединения для клиента
        /// </summary>
        public string ConnectionId { get; set; }
        
        /// <summary>
        /// ID пользователя в БД
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Имя введённое пользователем
        /// </summary>
        public string Username { get; set; }
    }
}
