using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using SignalRChatRoom.Server.InMemoryData;
using SignalRChatRoom.Server.Models;

using SignalRSwaggerGen.Attributes;
using Group = SignalRChatRoom.Server.Models.Group;
using Infrastructure.AFC.Infrastructure.Database;
using Infrastructure.AFC.Infrastructure.Database.Entities;
using DbGroup = Infrastructure.AFC.Infrastructure.Database.Entities.Group;
namespace SignalRChatRoom.Server.Hubs
{
    // Bir sınıfın hub sınıfı olduğunu anlamak ve sorumlulukları yüklemek için hub sınıfından türetilmesi gerekir..
    [SignalRHub]
    public class ChatHub : Hub
    {

        public ApplicationContext _applicationContext;

        ChatsManageRepository _chateManageRepository;

        public ChatHub(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
            //_chateManageRepository = chateManageRepository;
        }

        #region [ADD]
        // Заполняет комнаты в комнате, зарегистрированные в параметре.
        public async Task AddClientToGroupAsync(string groupName)
        {
            // İstekte bulunan (caller) client bilgisi alınıyor..
            Client? client = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            if (client is null)
                client = new Client() { ConnectionId = Context.ConnectionId, UserId = Guid.NewGuid(), Username = "UsernameTemplate" };
            // groupName üzerinden GroupSourcedaki group listesinden ilgili grup bulunuyor..
            bool result = false;

            Group? group = GroupSource.Groups.FirstOrDefault(g => g.GroupName == groupName);
            if (group != null)
                result = group.Clients.Any(c => c.ConnectionId == Context.ConnectionId);

            if (result != true) // İlgili gruba dahil değilse..
            {
                // Group nesnesinde tutulan client listesine caller client ekleniyor..
                group.Clients.Add(client);

                // İlgili client (caller) gruba dahil ediliyor..
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                // İlgili gruba ait tüm clientların listesini döndürür..
                await GetClientsOfGroupAsync(groupName);
            }

            await _chateManageRepository.AddUserToDbAsync(Guid.Parse(groupName), client.Username);

        }

        // Включает комнаты в комнатах, о которых сообщалось в параметре.
        public async Task AddClientToGroupsAsync(IEnumerable<string> groupNames)
        {
            // İstekte bulunan (caller) client bilgisi alınıyor..
            Client client = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);

            foreach (var groupName in groupNames)
            {
                // groupName üzerinden GroupSourcedaki group listesinden ilgili grup bulunuyor..
                Group _group = GroupSource.Groups.FirstOrDefault(g => g.GroupName == groupName);

                // Caller ilgili groupa subscribe mı kontrol ediliyor..
                var result = _group.Clients.Any(c => c.ConnectionId == Context.ConnectionId);
                if (!result) // İlgili gruba dahil değilse..
                {
                    // Group nesnesinde tutulan client listesine caller client ekleniyor..
                    _group.Clients.Add(client);

                    // İlgili client (caller) gruba dahil ediliyor..
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                    // İlgili gruba ait tüm clientların listesini döndürür..
                    await GetClientsOfGroupAsync(groupName);
                }
            }
        }

        // Поскольку любой клиент сделает процесс формирования группы, группа (вызывающая абонент) в первую очередь подписана на группу.
        public async Task AddGroupAsync(string groupName, string username)
        {
            // Grupların içinde hangi clientların olduğunun bilgisi server tarafından tutuluyor. ViewModel vs. kullanmaya gerek yok..
            var addToGroupTask = Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await addToGroupTask;
            Group group = new Group { GroupName = groupName };

            var messageHistory = await GetMessagesHistoryAsync(Guid.Parse(groupName));
            group.MessageHistory = messageHistory;
            // Group altında tutulan listeye client bilgisi ekleniyor..

            Client? client = ClientSource.Clients.FirstOrDefault(connection => connection.ConnectionId == Context.ConnectionId);
            if (client is null)
                client = new Client { ConnectionId = Context.ConnectionId, Username = username, UserId = Guid.NewGuid() };

            group.Clients.Add(client);

            addToGroupTask.Wait();
            // Sistemde hangi grupların olduğu bilgisini depolamak gerekiyor..
            GroupSource.Groups.Add(group);

            // Sisteme bir grup/oda eklendiğini tüm clientlara bildiriyor..
            await Clients.All.SendAsync("groupAdded", groupName);

            GetGroupsAsync().Wait();
        }

        //todo: метод писался без учёта админ или простой пользователь отправялет данные, просмотреть метод ещё раз и обновить
        /// <summary>
        /// Добавляет пользователя в чат, загружет историю сообщений 
        /// </summary>
        /// <param name="username">Никнейм</param>
        /// <param name="groupName">ID чата</param>
        /// <returns></returns>
        public async Task AddGroupAndUserToAppAndDb(string username, string groupName)
        {
            Client client = new Client { ConnectionId = Context.ConnectionId, Username = username, UserId = Guid.NewGuid() };
            ClientSource.Clients.Add(client);
            Clients.Others.SendAsync("clientJoined", username);
            //await GetClientsAsync();
            Clients.Caller.SendAsync("groups", GroupSource.Groups);

            Group? tempGroup = GroupSource.Groups.FirstOrDefault(group => group.GroupName == groupName);
            if (tempGroup is null)
            {
                tempGroup = new Group { GroupName = groupName };
            }
            else if (tempGroup.MessageHistory is null)
            {
                List<Message> messageHistory = await GetMessagesHistoryAsync(Guid.Parse(groupName));
                tempGroup.MessageHistory = messageHistory;
            }
            tempGroup.Clients.Add(client);
            GroupSource.Groups.Add(tempGroup);
            Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            //await GetGroupsAsync();
            var dbGroupEntity = new DbGroup()
            {
                Id = Guid.Parse(tempGroup.GroupName),
                Name = tempGroup.GroupName,
                Members = (ICollection<User>)tempGroup.Clients
            };

            _applicationContext.Groups.Add(dbGroupEntity);
            _applicationContext.SaveChanges();

            //_chateManageRepository.AddGroupToDbAsync().Wait();
        }


        #endregion

        #region [GET]
        public async Task GetClientsAsync()
        {
            // Sisteme dahil olan clientı kendisi de dahil olmak üzere tüm clientlara bildirir..
            await Clients.All.SendAsync("clients", ClientSource.Clients);
        }

        /// <summary>
        /// Возвращает список всех клиентов соответствующей группы.
        /// </summary>
        /// <param name="groupName">ID группы</param>
        /// <returns></returns>
        public async Task GetClientsOfGroupAsync(string groupName)
        {
            Group group = GroupSource.Groups.FirstOrDefault(g => g.GroupName.Equals(groupName));

            //await Clients.Caller.SendAsync("clientsOfGroup", group.Clients);
            await Clients.Groups(groupName).SendAsync("clientsOfGroup", group.Clients, group.GroupName);
        }

        /// <summary>
        /// todo: выяснить что делает этот метод
        /// </summary>
        /// <returns></returns>
        public async Task GetGroupsAsync()
        {
            // Уведомляет всех клиентов, включая клиента, который создает комнату/группу, добавленную в систему.
            await Clients.All.SendAsync("groups", GroupSource.Groups);
        }

        /// <summary>
        /// Получает историю сообщений для чата
        /// </summary>
        /// <param name="groupNumber">Id группы для получения истории сообщений</param>
        /// <returns></returns>
        [Authorize]
        public async Task<List<Message>> GetMessagesHistoryAsync(Guid groupNumber)
        {
            List<Message> messageHistory = await _chateManageRepository.GetAllChatMessagesAsync(groupNumber);

            return messageHistory;
        }

        public async Task GetUsernameAsync(string username)
        {

            Client client = new Client { ConnectionId = Context.ConnectionId, Username = username, UserId = Guid.NewGuid() };

            // Callerı (Sisteme dahil olan kullanıcıyı) mevcuttaki tüm clientların tutulduğu listeye ekler.
            ClientSource.Clients.Add(client);

            // Sisteme bir clientın dahil olduğunu caller (dahil olan client) dışındaki tüm clientlara bildiriyor..
            await Clients.Others.SendAsync("clientJoined", username);

            // Yeni kullanıcının da eklendiği güncel listeyi tüm clientlara bildirir..
            await GetClientsAsync();

            // Sisteme eklenmiş oda/grup listesi sisteme giriş yapan kullanıcıya (caller) bildirilir..
            await Clients.Caller.SendAsync("groups", GroupSource.Groups);
        }

        public async Task GetUsernameWithGroupAsync(string username, string groupId)
        {
            Client client = GroupSource.Groups.FirstOrDefault(group => group.GroupName == groupId)
                                       .Clients.FirstOrDefault(client => client.Username == username);

            if (client == null)
            {
                client = new Client { ConnectionId = Context.ConnectionId, UserId = Guid.NewGuid(), Username = username };
                _chateManageRepository.AddUserToDbAsync(Guid.Parse(groupId), client.UserId, client.Username);
            }

            // Callerı (Sisteme dahil olan kullanıcıyı) mevcuttaki tüm clientların tutulduğu listeye ekler.
            ClientSource.Clients.Add(client);

            // Sisteme bir clientın dahil olduğunu caller (dahil olan client) dışındaki tüm clientlara bildiriyor..
            await Clients.OthersInGroup(groupId).SendAsync("clientJoined", username);

            // Уведомляет текущий список, в котором новый пользователь добавляется всем клиентам.
            await GetClientsAsync();

            // Sisteme eklenmiş oda/grup listesi sisteme giriş yapan kullanıcıya (caller) bildirilir..
            // отключено т.к. чел может быть только в одной группе
            //await Clients.Caller.SendAsync("groups", GroupSource.Groups);
        }
        #endregion

        #region [SEND]
        // Вызывающий абонент будет вызван, если клиента отправит сообщение ..
        //public async Task SendMessageAsync(string message, Client client)
        //{
        //    // Caller bilgisi alınıyor..
        //    Client senderClient = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);

        //    // Clientta tanımlı receiveMessage fonksiyonunu tetikler.Mesaj, senderClient ve receiverClient değerleri döndürülür..receiveMessage fonk..4 değer bekliyor..
        //    // 3.dönüş değeri bir Client beklemekte, grup mesajlaşmalarında bir clienta gönderilmediği için bu değer null olarak döndürülüyor..
        //    // 4.dönüş değeri ise string beklemekte, bu da karşılıklı mesajlaşma durumunda null döndürülüyor..
        //    await Clients.Client(client.ConnectionId).SendAsync("receiveMessage", message, senderClient, client, null);
        //}

        /// <summary>
        /// Отправляет сообщение соответствующей группе
        /// </summary>
        /// <param name="groupName">Имя группы</param>
        /// <param name="message">Сообщение, которое нужно доставить</param>
        /// <param name="username">Ник отправителя</param>
        /// <returns></returns>
        //[Authorize]
        public async Task SendMessageToGroupAsync(string groupName, string message, string username)
        {
            Group? tempGroup = GroupSource.Groups.FirstOrDefault(group => group.GroupName == groupName);
            Client? tempClient = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            await Clients.Groups(groupName).SendAsync("receiveMessage2", message, tempClient.Username, null, groupName, tempGroup);

            //Guid groupId = Guid.Parse(groupName);
            //await _chateManageRepository.SaveMessageToDbAsync(groupId, message, username);
        }
        #endregion

        #region [OVERRIDE]
        // todo: сделать поиск группы к которой подключается пользователь

        //public override async Task OnConnectedAsync()
        //{
        //    //await Clients.All.SendAsync("clientJoined", Context.ConnectionId);
        //}

        // Sistemden varolan bir bağlantı koptuğu zaman bu fonksiyon tetiklenecek..
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Client client = ClientSource.Clients.First(c => c.ConnectionId == Context.ConnectionId);
            await Clients.All.SendAsync("clientLeaved", client != null ? client.Username : null);

            // Callerı (Sisteme dahil olan kullanıcıyı) mevcuttaki tüm clientların tutulduğu listeden siler..
            ClientSource.Clients.Remove(client);

            // Kullanıcının çıkarıldığı güncel listeyi tüm clientlara bildirir..
            await GetClientsAsync();

            // Client subscribe olduğu gruplardan siliniyor.. 
            foreach (var group in GroupSource.Groups)
            {
                // Caller ilgili groupa subscribe mı kontrol ediliyor..
                var result = group.Clients.Any(c => c.ConnectionId == Context.ConnectionId);
                if (result) // İlgili gruba dahilse..
                {
                    // Group nesnesinde tutulan client listesinden caller client siliniyor..
                    group.Clients.Remove(client);

                    string groupName = group.GroupName.ToString();
                    // İlgili client (caller) gruptan siliniyor..
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

                    // İlgili gruba ait tüm clientların listesini döndürür..
                    await GetClientsOfGroupAsync(group.GroupName);
                }
            }
        }
        #endregion

    }
}
