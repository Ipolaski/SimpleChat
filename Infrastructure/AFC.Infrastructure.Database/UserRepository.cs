using Microsoft.EntityFrameworkCore;
using Infrastructure.AFC.Infrastructure.Database.Entities;

namespace Infrastructure.AFC.Infrastructure.Database
{
    /// <summary>
    /// Класс для работы с таблицей User в БД 
    /// </summary>
    public class UserRepository
    {
        /// <summary>
        /// Контекст базы данных
        /// </summary>
        public ApplicationContext _applicationContext;

        public UserRepository(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        /// <summary>
        /// Ищет пользователя по ID и удаляет из БД. Возвращает
        /// </summary>
        /// <param name="id">Номер пользователя в БД</param>
        /// <returns>true - в случае успешного нахождения и удаления</returns>
		public async Task<bool> DeleteById(Guid id)
        {
            bool returnValue = false;

            User user = await GetByIdAsync(id);
            if (user != null)
            {
                _applicationContext.Users.Remove(user);
                int count = await _applicationContext.SaveChangesAsync();
                returnValue = true;
            }

            return returnValue;
        }

        /// <summary>
        /// Добавление пользователя в БД
        /// </summary>
        /// <param name="user">Объект пользователя</param>
        /// <returns>true - если количество записанных строк > 0</returns>
		public async Task<bool> AddAsync(User user)
        {
            await _applicationContext.Users.AddAsync(user);
            int count = await _applicationContext.SaveChangesAsync();

            return count != 0;
        }

        /// <summary>
        /// Обновлениие существующей записи в БД
        /// </summary>
        /// <param name="newUser">Данные, которые нужно внести</param>
        /// <returns>true - если количество обновлённых строк > 0</returns>
		public async Task<bool> UpdateById(User newUser)
        {
            User user = await GetByIdAsync(newUser.Id);
            if (user != null)
            {
                user.Name = newUser.Name;
                user.Password = newUser.Password;
                user.IsAdmin = newUser.IsAdmin;
                _applicationContext.Update(user);
            }
            int count = await _applicationContext.SaveChangesAsync();

            return count != 0;
        }

        /// <summary>
        /// Получение объекта пользователя из БД
        /// </summary>
        /// <param name="id">льзователя в БД</param>
        /// <returns>Объект пользователя из БД</returns>
        public async Task<User> GetByIdAsync(Guid id)
        {
#pragma warning disable CS8600 // Преобразование литерала, допускающего значение NULL или возможного значения NULL в тип, не допускающий значение NULL.
            User user = await _applicationContext.Users.FirstOrDefaultAsync(u => u.Id == id);
#pragma warning restore CS8600 // Преобразование литерала, допускающего значение NULL или возможного значения NULL в тип, не допускающий значение NULL.

#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
            return user;
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
        }

        /// <summary>
        /// Поиск пользователя по Имени
        /// </summary>
        /// <param name="name">Имя пользователя</param>
        /// <returns>Объект пользователя найденый в БД</returns>
        public async Task<User> GetByNameAsync(string name)
        {
#pragma warning disable CS8600 // Преобразование литерала, допускающего значение NULL или возможного значения NULL в тип, не допускающий значение NULL.
            User user = await _applicationContext.Users.FirstOrDefaultAsync(u => u.Name == name);
#pragma warning restore CS8600 // Преобразование литерала, допускающего значение NULL или возможного значения NULL в тип, не допускающий значение NULL.

#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
            return user;
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
        }

        /// <summary>
        /// Выполняет SaveChanges
        /// </summary>
        public async Task SaveAsync()
        {
            await _applicationContext.SaveChangesAsync();
        }
    }
}
