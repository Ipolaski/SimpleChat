using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using SignalRChatRoom.Server.InMemoryData;
using SignalRChatRoom.Server.Models;

using SignalRSwaggerGen.Attributes;
using Group = SignalRChatRoom.Server.Models.Group;
using Infrastructure.AFC.Infrastructure.Database;
using Infrastructure.AFC.Infrastructure.Database.Entities;
using DbGroup = Infrastructure.AFC.Infrastructure.Database.Entities.Group;
using Microsoft.AspNetCore.Components.Forms;
using System.IO;

namespace SignalRChatRoom.Server.Hubs
{
    // Bir sınıfın hub sınıfı olduğunu anlamak ve sorumlulukları yüklemek için hub sınıfından türetilmesi gerekir..
    [SignalRHub]
    public class ChatHub :Hub
    {

        public ApplicationContext _applicationContext;

        private List<DbGroup> _cachedDbGroups = [];

        ChatsManageRepository _chateManageRepository;

        public ChatHub( ChatsManageRepository chateManageRepository ) : base()
        {
            //_applicationContext = applicationContext;
            _chateManageRepository = chateManageRepository;
        }

        #region [ADD]
        // Заполняет комнаты в комнате, зарегистрированные в параметре.
        public async Task AddClientToGroupAsync( string groupName )
        {
            // İstekte bulunan (caller) newClient bilgisi alınıyor..
            Client? client = ClientSource.Clients.FirstOrDefault( c => c.ConnectionId == Context.ConnectionId );
            if ( client is null )
                client = new Client() { ConnectionId = Context.ConnectionId, UserId = Guid.NewGuid(), Username = "UsernameTemplate" };
            // groupName üzerinden GroupSourcedaki group listesinden ilgili grup bulunuyor..
            bool result = false;

            Group? group = GroupSource.Groups.FirstOrDefault( g => g.GroupName == groupName );
            if ( group != null )
                result = group.Clients.Any( c => c.ConnectionId == Context.ConnectionId );

            if ( result != true ) // İlgili gruba dahil değilse..
            {
                // Group nesnesinde tutulan newClient listesine caller newClient ekleniyor..
                group.Clients.Add( client );

                // İlgili newClient (caller) gruba dahil ediliyor..
                await Groups.AddToGroupAsync( Context.ConnectionId, groupName );

                // İlgili gruba ait tüm clientların listesini döndürür..
                await GetClientsOfGroupAsync( groupName );
            }

            await _chateManageRepository.AddUserToDbAsync( Guid.Parse( groupName ), Guid.NewGuid(), client.Username );

        }

        // Включает комнаты в комнатах, о которых сообщалось в параметре.
        public async Task AddClientToGroupsAsync( IEnumerable<string> groupNames )
        {
            // İstekte bulunan (caller) newClient bilgisi alınıyor..
            Client client = ClientSource.Clients.FirstOrDefault( c => c.ConnectionId == Context.ConnectionId );

            foreach ( var groupName in groupNames )
            {
                // groupName üzerinden GroupSourcedaki group listesinden ilgili grup bulunuyor..
                Group _group = GroupSource.Groups.FirstOrDefault( g => g.GroupName == groupName );

                // Caller ilgili groupa subscribe mı kontrol ediliyor..
                var result = _group.Clients.Any( c => c.ConnectionId == Context.ConnectionId );
                if ( !result ) // İlgili gruba dahil değilse..
                {
                    // Group nesnesinde tutulan newClient listesine caller newClient ekleniyor..
                    _group.Clients.Add( client );

                    // İlgili newClient (caller) gruba dahil ediliyor..
                    await Groups.AddToGroupAsync( Context.ConnectionId, groupName );

                    // İlgili gruba ait tüm clientların listesini döndürür..
                    await GetClientsOfGroupAsync( groupName );
                }
            }
        }

        // Поскольку любой клиент сделает процесс формирования группы, группа (вызывающая абонент) в первую очередь подписана на группу.
        public async Task AddGroupAsync( string groupName, string username )
        {
            // Grupların içinde hangi clientların olduğunun bilgisi server tarafından tutuluyor. ViewModel vs. kullanmaya gerek yok..
            var addToGroupTask = Groups.AddToGroupAsync( Context.ConnectionId, groupName );
            await addToGroupTask;
            Group group = new Group { GroupName = groupName };

            var messageHistory = await GetDbMessagesHistorySortByDecAsync( Guid.Parse( groupName ) );
            group.MessageHistory = messageHistory;
            // Group altında tutulan listeye newClient bilgisi ekleniyor..

            Client? client = ClientSource.Clients.FirstOrDefault( connection => connection.ConnectionId == Context.ConnectionId );
            if ( client is null )
                client = new Client { ConnectionId = Context.ConnectionId, Username = username, UserId = Guid.NewGuid() };

            group.Clients.Add( client );

            addToGroupTask.Wait();
            // Sistemde hangi grupların olduğu bilgisini depolamak gerekiyor..
            GroupSource.Groups.Add( group );

            // Sisteme bir grup/oda eklendiğini tüm clientlara bildiriyor..
            await Clients.All.SendAsync( "groupAdded", groupName );

            SetCurrentGroupAsync( group ).Wait();
        }

        //todo: метод писался без учёта админ или простой пользователь отправялет данные, просмотреть метод ещё раз и обновить
        /// <summary>
        /// Добавляет пользователя в чат, загружет историю сообщений 
        /// </summary>
        /// <param name="username">Никнейм</param>
        /// <param name="groupName">ID чата</param>
        /// <returns></returns>
        public async Task AddGroupAndUserToAppAndDb( string username, string groupName )
        {
            var dbGroupEntity = new DbGroup();
            DbGroup? cachedDbGroup = null;
            Client newClient = new Client { ConnectionId = Context.ConnectionId, Username = username, UserId = Guid.NewGuid() };
            var dbClient = await _chateManageRepository.GetUserAsync( username, Guid.Parse( groupName ) );
            if ( dbClient != null )
                newClient.UserId = dbClient.Id;
            ClientSource.Clients.Add( newClient );
            // Free memory
            dbClient = null;

            Group? groupFromMemory = GroupSource.Groups.FirstOrDefault( group => group.GroupName == groupName );
            if ( groupFromMemory == null )
            {
                cachedDbGroup = _cachedDbGroups.FirstOrDefault( group => group?.Name == groupName, null );
                if ( cachedDbGroup == null )
                {
                    cachedDbGroup = await _chateManageRepository.GetGroupByIdAsync( groupName );
                    if ( cachedDbGroup is null )
                    {

                        // Todo: такого чата никто не создавал, проверьте номер.
                        await Clients.Groups( groupName ).SendAsync( "WarningToUser", "Такого чата никто не создавал, проверьте номер" );
                    }
                }

                groupFromMemory = new Group { GroupName = groupName };
            }

            if ( groupFromMemory.MessageHistory == null && cachedDbGroup != null )
            {
                List<Message> messageHistory = await GetDbMessagesHistorySortByDecAsync( Guid.Parse( groupName ) );
                if ( messageHistory.Count > 0 )
                {
                    groupFromMemory.MessageHistory = messageHistory;
                }
            }

            bool isClientInGroup = groupFromMemory.Clients.FirstOrDefault(
                                        tempclient => tempclient.UserId == newClient.UserId &&
                                        tempclient.ConnectionId == newClient.ConnectionId, null ) == null;
            if ( isClientInGroup )
            {
                groupFromMemory.Clients.Add( newClient );
                if ( !await IsUserExistsAsync( newClient.Username, groupFromMemory.GroupName ) )
                    await AddUserToDbAsync( newClient.UserId, newClient.Username, groupFromMemory.GroupName );
            }

            var tempgroup = GroupSource.Groups.FirstOrDefault( group => group.GroupName == groupFromMemory.GroupName );

            if ( tempgroup == null || ClientSource.Clients.Any( client => client.ConnectionId == newClient.ConnectionId ) )
            {
                GroupSource.Groups.Add( groupFromMemory );
                await Groups.AddToGroupAsync( Context.ConnectionId, groupName );
                await Clients.Others.SendAsync( "clientJoined", username );
                await SetCurrentGroupAsync( groupFromMemory );
                await SetClientsAsync();
                await SetCurrentGroupClientsAsync( groupFromMemory );
                //await Clients.All.SendAsync("groups", GroupSource.Groups);
                //await GetClientsOfGroupAsync(groupName);

                //Clients.Caller.SendAsync("groups", GroupSource.Groups).Wait();
            }

            if ( !ClientSource.Clients.Any() )

                // todo: удалить из группы и записать в группу? Либо найти способ обновить запись, если она отличается.
                //Group sameGroup = GroupSource.Groups.FirstOrDefault(group => group.GroupName == groupFromMemory.GroupName);
                //if (sameGroup != null)
                //    UpdateItemInList(groupFromMemory, ref sameGroup);
                //else
                //{
                //}

                //_applicationContext.Groups.Add(dbGroupEntity);
                //_applicationContext.SaveChanges();

                if ( cachedDbGroup is not null )
                {
                    dbGroupEntity = new DbGroup()
                    {
                        Id = cachedDbGroup.Id,
                        Name = groupFromMemory.GroupName,
                        Members = (ICollection<User>) groupFromMemory.Clients,
                        OwnerId = cachedDbGroup.OwnerId,
                    };
                }
                else
                {
                    dbGroupEntity = new DbGroup()
                    {
                        Id = Guid.Parse( groupFromMemory.GroupName ),
                        Name = groupFromMemory.GroupName,
                        Members = (ICollection<User>) groupFromMemory.Clients,
                        OwnerId = cachedDbGroup.OwnerId,
                    };
                }

            _chateManageRepository.AddOrUpdateGroupToDbAsync( dbGroupEntity );

            await Clients.Group( groupFromMemory.GroupName ).SendAsync( "LoadMessagesFromDb", groupFromMemory.MessageHistory );


            //void UpdateItemInList(Group newGroupItem, ref Group currentGroup)
            //{
            //    currentGroup = newGroupItem;
            //}

            //await Clients.All.SendAsync("groupAdded2");
        }

        /// <summary>
        /// Если такого пользователя нет в базе, то добавляет его туда
        /// </summary>
        /// <param name="username">имя пльзователя</param>
        /// <param name="groupId">ID чата</param>
        /// <returns></returns>
        public async Task AddUserToDbAsync( Guid UserId, string username, string groupId )
        {
            if ( !await IsUserExistsAsync( username, groupId ) )
            {
                var client = new Client { ConnectionId = Context.ConnectionId, UserId = Guid.NewGuid(), Username = username };
                await _chateManageRepository.AddUserToDbAsync( Guid.Parse( groupId ), client.UserId, client.Username );
            }
        }
        #endregion

        #region [SET]
        /// <summary>
        /// Передаёт всех клиентов на фронт в Home.cs
        /// </summary>
        /// <returns></returns>
        public async Task SetClientsAsync()
        {
            // Sisteme dahil olan clientı kendisi de dahil olmak üzere tüm clientlara bildirir..
            await Clients.All.SendAsync( "clients", ClientSource.Clients );
        }

        public async Task SetCurrentGroupClientsAsync( Group group )
        {
            await Clients.All.SendAsync( "clientsOfGroup", group.Clients, group.GroupName );
        }

        /// <summary>
        /// Передаёт чат на фронт в Home.cs
        /// </summary>
        /// <param name="group">Объект чата</param>
        public async Task SetCurrentGroupAsync( Group group )
        {
            // Уведомляет всех клиентов, включая клиента, который создает комнату/группу, добавленную в систему.
            await Clients.All.SendAsync( "selectGroup", group );
        }
        #endregion

        #region [GET]
        /// <summary>
        /// Возвращает список всех клиентов соответствующей группы.
        /// </summary>
        /// <param name="groupName">ID группы</param>
        /// <returns></returns>
        public async Task GetClientsOfGroupAsync( string groupName )
        {
            Group group = GroupSource.Groups.FirstOrDefault( g => g.GroupName.Equals( groupName ) );

            await Clients.Groups( groupName ).SendAsync( "clientsOfGroup", group.Clients, group.GroupName );
        }

        //public async Task<DbGroup?> GetGroupById( Guid groupId )
        //{
        //    DbGroup? result;
        //    result = await _chateManageRepository.GetGroupByIdAsync( groupId );

        //    return result;
        //}

        public async Task GetFilesToServer( List<IBrowserFile> loadedFiles, string groupId, string username )
        {
            foreach ( var file in loadedFiles )
            {
                var path = Path.Combine( Directory.GetCurrentDirectory(), 
                                         "UserFiles");
                Directory.CreateDirectory( path );
                path = Path.Combine( path, file.Name );
                using FileStream fs = new( path, FileMode.Create );
                await file.OpenReadStream().CopyToAsync( fs );

                await _chateManageRepository.SaveFileToDbAsync( Guid.Parse( groupId ), path, username );
            }
        }

        /// <summary>
        /// Получает отсортированную историю сообщений для чата из БД
        /// </summary>
        /// <param name="groupNumber">Id группы для получения истории сообщений</param>
        /// <returns></returns>
        [Authorize]
        public async Task<List<Message>> GetDbMessagesHistorySortByDecAsync( Guid groupNumber )
        {
            List<Message> messageHistory = await _chateManageRepository.GetAllChatMessagesSortByDecAsync( groupNumber );
            return messageHistory;
        }

        public async Task<bool> IsUserExistsAsync( string username, string groupId )
        {
            return await _chateManageRepository.IsUserExistsAsync( username, Guid.Parse( groupId ) );
        }



        public async Task GetUsernameAsync( string username )
        {

            Client client = new Client { ConnectionId = Context.ConnectionId, Username = username, UserId = Guid.NewGuid() };

            // Callerı (Sisteme dahil olan kullanıcıyı) mevcuttaki tüm clientların tutulduğu listeye ekler.
            ClientSource.Clients.Add( client );

            // Sisteme bir clientın dahil olduğunu caller (dahil olan newClient) dışındaki tüm clientlara bildiriyor..
            await Clients.Others.SendAsync( "clientJoined", username );

            // Yeni kullanıcının da eklendiği güncel listeyi tüm clientlara bildirir..
            await SetClientsAsync();

            // Sisteme eklenmiş oda/grup listesi sisteme giriş yapan kullanıcıya (caller) bildirilir..
            await Clients.Caller.SendAsync( "groups", GroupSource.Groups );
        }

        public async Task GetUsernameWithGroupAsync( string username, string groupId )
        {
            Client client = GroupSource.Groups.FirstOrDefault( group => group.GroupName == groupId )
                                       .Clients.FirstOrDefault( client => client.Username == username );

            if ( client == null )
            {
                client = new Client { ConnectionId = Context.ConnectionId, UserId = Guid.NewGuid(), Username = username };
                _chateManageRepository.AddUserToDbAsync( Guid.Parse( groupId ), client.UserId, client.Username );
            }

            // Callerı (Sisteme dahil olan kullanıcıyı) mevcuttaki tüm clientların tutulduğu listeye ekler.
            ClientSource.Clients.Add( client );

            // Sisteme bir clientın dahil olduğunu caller (dahil olan newClient) dışındaki tüm clientlara bildiriyor..
            await Clients.OthersInGroup( groupId ).SendAsync( "clientJoined", username );

            // Уведомляет текущий список, в котором новый пользователь добавляется всем клиентам.
            await SetClientsAsync();

            // Sisteme eklenmiş oda/grup listesi sisteme giriş yapan kullanıcıya (caller) bildirilir..
            // отключено т.к. чел может быть только в одной группе
            //await Clients.Caller.SendAsync("groups", GroupSource.Groups);
        }
        #endregion

        #region [SEND]
        // Вызывающий абонент будет вызван, если клиента отправит сообщение ..
        //public async Task SendMessageAsync(string message, Client newClient)
        //{
        //    // Caller bilgisi alınıyor..
        //    Client senderClient = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);

        //    // Clientta tanımlı receiveMessage fonksiyonunu tetikler.Mesaj, senderClient ve receiverClient değerleri döndürülür..receiveMessage fonk..4 değer bekliyor..
        //    // 3.dönüş değeri bir Client beklemekte, grup mesajlaşmalarında bir clienta gönderilmediği için bu değer null olarak döndürülüyor..
        //    // 4.dönüş değeri ise string beklemekte, bu da karşılıklı mesajlaşma durumunda null döndürülüyor..
        //    await Clients.Client(newClient.ConnectionId).SendAsync("receiveMessage", message, senderClient, newClient, null);
        //}

        /// <summary>
        /// Отправляет сообщение соответствующей группе
        /// </summary>
        /// <param name="groupName">Имя группы</param>
        /// <param name="message">Сообщение, которое нужно доставить</param>
        /// <param name="username">Ник отправителя</param>
        /// <returns></returns>
        //[Authorize]
        //todo: выяснить нужно ли здесь получать сообщения из БД
        public async Task SendMessageToGroupAsync( string groupName, string message, string username )
        {
            Group? tempGroup = GroupSource.Groups.FirstOrDefault( group => group.GroupName == groupName );
            if ( tempGroup == null )
            {
                var dbGroup = _cachedDbGroups.FirstOrDefault( group => group.Name == groupName );
                if ( dbGroup != null )
                {
                    tempGroup.GroupName = dbGroup.Name;
                    tempGroup.MessageHistory = await GetDbMessagesHistorySortByDecAsync( Guid.Parse( dbGroup.Name ) );
                }
            }
            Client? tempClient = ClientSource.Clients.FirstOrDefault( c => c.ConnectionId == Context.ConnectionId );
            await Clients.Groups( groupName ).SendAsync( "receiveMessage2", message, tempClient.Username, tempClient, groupName, tempGroup );

            Guid groupId = Guid.Parse( groupName );
            if ( !await _chateManageRepository.SaveMessageToDbAsync( groupId, message, username ) )
                await Clients.Groups( groupName ).SendAsync( "WarningToUser", "Сообщение отправлено, но возможно не записалось в историю" );
        }
        #endregion

        #region [OVERRIDE]
        // todo: сделать поиск группы к которой подключается пользователь

        //public override async Task OnConnectedAsync()
        //{
        //    //await Clients.All.SendAsync("clientJoined", Context.ConnectionId);
        //}

        // Sistemden varolan bir bağlantı koptuğu zaman bu fonksiyon tetiklenecek..
        public override async Task OnDisconnectedAsync( Exception? exception )
        {
            Client client = ClientSource.Clients.First( c => c.ConnectionId == Context.ConnectionId );
            await Clients.All.SendAsync( "clientLeaved", client != null ? client.Username : null );

            // Callerı (Sisteme dahil olan kullanıcıyı) mevcuttaki tüm clientların tutulduğu listeden siler..
            ClientSource.Clients.Remove( client );

            // Kullanıcının çıkarıldığı güncel listeyi tüm clientlara bildirir..
            await SetClientsAsync();

            // Client subscribe olduğu gruplardan siliniyor.. 
            foreach ( var group in GroupSource.Groups )
            {
                // Caller ilgili groupa subscribe mı kontrol ediliyor..
                var result = group.Clients.Any( c => c.ConnectionId == Context.ConnectionId );
                if ( result ) // İlgili gruba dahilse..
                {
                    // Group nesnesinde tutulan newClient listesinden caller newClient siliniyor..
                    group.Clients.Remove( client );

                    string groupName = group.GroupName.ToString();
                    // İlgili newClient (caller) gruptan siliniyor..
                    await Groups.RemoveFromGroupAsync( Context.ConnectionId, groupName );

                    // İlgili gruba ait tüm clientların listesini döndürür..
                    await GetClientsOfGroupAsync( group.GroupName );
                }
            }
        }
        #endregion
    }
}