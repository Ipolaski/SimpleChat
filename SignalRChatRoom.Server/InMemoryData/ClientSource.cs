using SignalRChatRoom.Server.Models;

namespace SignalRChatRoom.Server.InMemoryData
{
    /// <summary>
    /// Отвечает за клиенты веб сокета
    /// </summary>
    public class ClientSource
    {
        /// <summary>
        /// Список клиентов веб сокета
        /// </summary>
        public static List<Client> Clients { get; } = new List<Client>(); // Property ReadOnly olarak tanımlandı.
    }
}
