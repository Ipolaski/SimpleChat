using Infrastructure.AFC.Infrastructure.Database.Entities;

using Microsoft.EntityFrameworkCore;

using File = Infrastructure.AFC.Infrastructure.Database.Entities.File;


namespace Infrastructure.AFC.Infrastructure.Database
{
    public class ChatsManageRepository
    {

        public ApplicationContext _applicationContext;

        public ChatsManageRepository(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        /// <summary>
        /// Запрашивает историю сообщений, без медиа файлов, для заданной беседы, отсортированную по возрастанию, от новых сообщений к старым.
        /// </summary>
        /// <param name="groupId">Guid беседы</param>
        /// <returns>Список сообщений</returns>
        public async Task<List<Message>> GetAllChatMessagesSortByDecAsync(Guid groupId)
        {
            List<Message> groupMessages = await _applicationContext.Messagess.Where(message => message.Group.Id == groupId).OrderByDescending(table => table.DateTime).ToListAsync();

            return groupMessages;
        }

        /// <summary>
        /// Проверяет, есть ли юзер в БД
        /// </summary>
        /// <param name="username"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async Task<bool> IsUserExistsAsync(string username, Guid groupId)
        {
            Group tempGroup = await _applicationContext.Groups.FirstOrDefaultAsync(group => group.Id == groupId);
            User? user = await _applicationContext.Users.FirstOrDefaultAsync(user => user.Name == username && user.BeInGroups.Contains(tempGroup));

            return user != null;
        }

        public async Task<bool> DeleteById(Guid id)
        {
            int result = 0;
            Group group = await GetGroupByIdAsync(id);
            if (group != null)
            {
                _applicationContext.Groups.Remove(group);
                result = await _applicationContext.SaveChangesAsync();
            }

            return result != 0;
        }

        public bool AddToGroups(Group group)
        {
            _applicationContext.Groups.Add(group);
            int result = _applicationContext.SaveChanges();

            return result != 0;
        }

        /// <summary>
        /// Добавляет сущность группы в БД
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public async Task<bool> AddOrUpdateGroupToDbAsync(Group group)
        {
            int result = 0;
            group.OwnerId = Guid.Parse("b1a934a6-65fb-46a5-a4b2-36c20677f339");
            Group? tempGroup = await _applicationContext.Groups.FirstOrDefaultAsync(tableGroup => tableGroup.Id == group.Id);
            if (tempGroup is null)
            {
                await _applicationContext.Groups.AddAsync(group);
            }
            else
            {
                _applicationContext.Groups.Update(group);
            }
            try
            {
                result = _applicationContext.SaveChanges();
            }
            catch (Exception ex)
            {
                ex = null;
            }

            return result != 0;
        }

        /// <summary>
        /// Записывает сообщение в БД
        /// </summary>
        /// <param name="groupId">ID чата</param>
        /// <param name="message">Текст от пользователя</param>
        /// <param name="username">Имя, которое указал пользователь</param>
        /// <returns>true если сообщение было записано в БД</returns>
        public async Task<bool> SaveMessageToDbAsync(Guid groupId, string message, string username)
        {
            bool result = false;

            var tempGroup = await GetGroupByIdAsync(groupId);
            if (tempGroup != null)
            {
                var newMessgae = new Message()
                {
                    Id = Guid.NewGuid(),
                    Group = tempGroup,
                    Username = username,
                    Text = message,
                    DateTime = DateTime.UtcNow
                };
                await _applicationContext.Messagess.AddAsync(newMessgae);
                int temp = await _applicationContext.SaveChangesAsync();
                result = temp != 0;
            }

            return result;
        }

        /// <summary>
        /// Записывает путь к файлу в БД
        /// </summary>
        /// <param name="groupId">ID чата</param>
        /// <param name="pathToFile">Путь к файлу на сервере</param>
        /// <param name="username">Пользователь, который отправил</param>
        /// <returns>true если запись успешна</returns>
        public async Task<bool> SaveFileToDbAsync( Guid groupId, string pathToFile, string username )
        {
            bool result = false;

            File tempFile = new File
            {
                Id = Guid.NewGuid( ),
                FilePath = pathToFile,
                Date = DateOnly.FromDateTime( DateTime.UtcNow ),
            };

            var tempGroup = await GetGroupByIdAsync( groupId );
            if ( tempGroup != null )
            {
                var newMessgae = new Message()
                {
                    Id = Guid.NewGuid(),
                    Group = tempGroup,
                    Username = username,
                    File = tempFile,
                    DateTime = DateTime.UtcNow
                };

                //await _applicationContext.Files.AddAsync( tempFile );
                await _applicationContext.Messagess.AddAsync( newMessgae );
                int temp = await _applicationContext.SaveChangesAsync();
                result = temp != 0;
            }

            return result;
        }

        public async Task<bool> UpdateById(Group newGroup, Guid userId)
        {
            Group group = await GetGroupByIdAsync(newGroup.Id);
            if (group.Id != userId)
            {
                return false;
            }
            group.Name = newGroup.Name;
            _applicationContext.Update(group);
            int result = await _applicationContext.SaveChangesAsync();

            return result != 0;
        }
        #region [GET]
        /// <summary>
        /// Ищет в БД группу по заданному GUID
        /// </summary>
        /// <param name="id">GUID чата</param>
        /// <returns>Чат в виде сущности группы из БД</returns>
        public async Task<Group?> GetGroupByIdAsync(Guid id)
        {
            Group? group = await _applicationContext.Groups.FirstOrDefaultAsync(group => group.Id == id);

            return group;
        }

        /// <summary>
        /// Ищет в БД группу по заданному GUID
        /// </summary>
        /// <param name="id">GUID чата в виде строки</param>
        /// <returns>Чат в виде сущности группы из БД</returns>
        public async Task<Group?> GetGroupByIdAsync(string id)
        {

            Group? group = await GetGroupByIdAsync(Guid.Parse(id));

            return group;
        }

        public IQueryable<Group> GetRange(int count, Guid userId)
        {
            IQueryable<Group> groups = _applicationContext.Groups.Where(g => g.Owner.Id == userId).Take(count);

            return groups;
        }

        public async Task<User?> GetUserAsync(string username, Guid groupId)
        {
            User? user = null;
            Group? tempGroup = await _applicationContext.Groups.FirstOrDefaultAsync(group => group.Id == groupId);
            if (tempGroup != null)
            {
                user = await _applicationContext.Users.FirstOrDefaultAsync(user => user.Name == username && user.BeInGroups.Contains(tempGroup));
            }
            
            return user;
        }
        #endregion
        public async Task SaveAsync()
        {
            await _applicationContext.SaveChangesAsync();
        }

        public async Task AddUserToDbAsync(Guid groupId, string username)
        {

            User user = new User() { Id = Guid.NewGuid(), Name = username, IsAdmin = false };
            await _applicationContext.Users.AddAsync(user);
            await _applicationContext.SaveChangesAsync();

            Group group = await GetGroupByIdAsync(groupId);
            if (group != null)
            {
                group.Members.Add(user);
                _applicationContext.Groups.Update(group);
                await _applicationContext.SaveChangesAsync();
            }

        }

        /// <summary>
        /// Добавляет пользователя в таблицу пользователей и устанавливает связь с таблицей чатов.
        /// </summary>
        /// <param name="groupId">ID чата</param>
        /// <param name="userId">ID пользователя</param>
        /// <param name="username">Имя выбранное пользователем</param>
        /// <returns></returns>
        public async Task AddUserToDbAsync(Guid groupId, Guid userId, string username)
        {
            Group group = await GetGroupByIdAsync(groupId);
            var newUser = new User() { Id = userId, Name = username, IsAdmin = false, Password = string.Empty };

            await _applicationContext.Users.AddAsync(newUser);
            await _applicationContext.SaveChangesAsync();

            group!.Members.Add(newUser);
            _applicationContext.Groups.Update(group);
            await _applicationContext.SaveChangesAsync();


        }

    }
}

