namespace AFC.Infrastructure.Api.Request.User
{
    /// <summary>
    /// Запись хранящая Имя и пароль от пользователя
    /// </summary>
    /// <param name="Name">Имя от пользователя</param>
    /// <param name="Password">Пароль от пользователя</param>
    public record LoginRequest(string Name, string Password);
}