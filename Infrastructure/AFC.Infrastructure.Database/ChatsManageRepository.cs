using Infrastructure.AFC.Infrastructure.Database.Entities;

using Microsoft.EntityFrameworkCore;

using OpenTelemetry.Trace;

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
        public async Task<List<Message>> GetAllChatMessagesAsync(Guid groupId)
        {
            List<Message> groupMessages = await _applicationContext.Messagess.Where(message => message.Group.Id == groupId).OrderByDescending(table => table.DateTime).ToListAsync();

            return groupMessages;
        }

        public async Task<bool> DeleteById(Guid id)
        {
            int result = 0;
            Group group = await GetByIdAsync(id);
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
        public async Task<bool> AddGroupToDbAsync(Group group)
        {
            Group? tempGroup = await _applicationContext.Groups.FirstOrDefaultAsync(tableGroup => tableGroup.Id == group.Id);
            if (tempGroup is null)
            {
                await _applicationContext.Groups.AddAsync(group);
            }
            else
            {
                _applicationContext.Groups.Update(group);
            }
            int result = await _applicationContext.SaveChangesAsync();

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

            var tempGroup = await GetByIdAsync(groupId);
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

        public async Task<bool> UpdateById(Group newGroup, Guid userId)
        {
            Group group = await GetByIdAsync(newGroup.Id);
            if (group.Id != userId)
            {
                return false;
            }
            group.Name = newGroup.Name;
            _applicationContext.Update(group);
            int result = await _applicationContext.SaveChangesAsync();

            return result != 0;
        }

        public async Task<Group> GetByIdAsync(Guid id)
        {
            Group group = await _applicationContext.Groups.FirstOrDefaultAsync(group => group.Id == id);

            return group;
        }
        public IQueryable<Group> GetRange(int count, Guid userId)
        {
            IQueryable<Group> groups = _applicationContext.Groups.Where(g => g.Owner.Id == userId).Take(count);

            return groups;
        }

        public async Task SaveAsync()
        {
            await _applicationContext.SaveChangesAsync();
        }

        public async Task AddUserToDbAsync(Guid groupId, string username)
        {

            User user = new User() { Id = Guid.NewGuid(), Name = username, IsAdmin = false };
            await _applicationContext.Users.AddAsync(user);
            await _applicationContext.SaveChangesAsync();

            Group group = await GetByIdAsync(groupId);
            if (group != null)
            {
                group.Members.Add(user);
                _applicationContext.Groups.Update(group);
                await _applicationContext.SaveChangesAsync();
            }

        }

        public async Task AddUserToDbAsync(Guid groupId, Guid userId, string username)
        {
            User oldUser = await _applicationContext.Users.FirstOrDefaultAsync(user => user.Id == userId);
            User newUser = new User() { Id = userId, Name = username, IsAdmin = false };
            if (oldUser == null)
            {
                await _applicationContext.Users.AddAsync(newUser);
            }
            else if (!Equals(oldUser, newUser) && oldUser.Id == newUser.Id)
            {
                _applicationContext.Users.Update(newUser);
            }

            await _applicationContext.SaveChangesAsync();

            Group group = await GetByIdAsync(groupId);
            if (group != null)
            {
                group.Members.Add(newUser);
                _applicationContext.Groups.Update(group);
                await _applicationContext.SaveChangesAsync();
            }
        }

    }
}

