namespace AFC.Infrastructure.Api.Request.User
{
    /// <summary>
    /// Запись, жранящая Имя и пароль введённые пользователем
    /// </summary>
    /// <param name="Name">Имя от пользователя</param>
    /// <param name="Password">Пароль от пользователя</param>
    public record RegistrationRequest(string Name, string Password);
}
