using Infrastructure.AFC.Infrastructure.Database.Entities;

namespace SignalRChatRoom.Server.Models
{
    /// <summary>
    /// Група веб сокета
    /// </summary>
    public class Group
    {
        /// <summary>
        /// ID группы
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Список всех участников чата
        /// </summary>
        public List<Client> Clients { get; } = new List<Client>();

        /// <summary>
        /// Хранит историю сообщений до тех пор, пока хоть один из участников чата в веб сокете
        /// </summary>
        public List<Message> MessageHistory { get; set; }
    }
}
