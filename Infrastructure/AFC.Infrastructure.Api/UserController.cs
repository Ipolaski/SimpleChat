using System.Security.Cryptography;
using System.Text;

using AFC.Infrastructure.Api.Request.User;

using Infrastructure.AFC.Infrastructure.Database.Entities;
using Infrastructure.AFC.Infrastructure.Database;

using Microsoft.AspNetCore.Mvc;

namespace AFC.Infrastructure.Api
{
    /// <summary>
    /// Методы для регистрации и входа пользователя
    /// </summary>
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public UserController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        #region [Public methods]
        /// <summary>
        /// Если такой пользователь ещё не существует, то записывает его в БД
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Registration")]
        //[SwaggerResponse( StatusCodes.Status200OK, "GET 200 aboba", typeof( List<abobaModel> ) )]
        //[SwaggerResponse( StatusCodes.Status400BadRequest, "GET 400 aboba", typeof( List<abobaModel> ) )]
        public async Task<IActionResult> Registration([FromBody] RegistrationRequest request)
        {
            User user = null;
            if (Valid(request))
            {
                if (!await UserExist(request.Name))
                {
                    user = new User()
                    {
                        Name = request.Name,
                        Password = StringHashingSHA128(request.Password),
                        IsAdmin = true,
                    };
                    await _userRepository.AddAsync(user);
                }
            }

            return user != null ? Ok(user) : BadRequest();
        }

        /// <summary>
        /// Проверяет, что пользователь есть в БД
        /// </summary>
        /// <param name="request">Запись хранящая Имя и пароль от пользователя</param>
        /// <returns>IActionResult Ok\BadRequest</returns>
        [HttpPost]
        [Route("Login")]
        //[SwaggerResponse( StatusCodes.Status200OK, "GET 200 aboba", typeof( List<abobaModel> ) )]
        //[SwaggerResponse( StatusCodes.Status400BadRequest, "GET 400 aboba", typeof( List<abobaModel> ) )]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (await CheckLogin(request))
            {
                return Ok();
            }

            return BadRequest();
        }
        #endregion

        #region [Private methods]

        //todo подумать над валидацией пользователя
        private bool Valid(RegistrationRequest request)
        {
            bool returnValue = false;

            try
            {
                var user = new User()
                {
                    Name = request.Name,
                    Password = request.Password,
                    IsAdmin = true,
                };

                returnValue = true;
            }
            catch
            {
            }
            //bool result = Validator.TryValidateValue( user, new ValidationContext( this, null, null ),null, null);
            // return result;
            return returnValue;
        }

        private async Task<bool> UserExist(string name)
        {
            User user = await _userRepository.GetByNameAsync(name);

            return user != null;

        }

        /// <summary>
        /// Проверяет, что пользователь есть в базе
        /// </summary>
        /// <param name="request">Запись хранящая Имя и пароль от пользователя</param>
        /// <returns>true - если пользователь существует и его пароль совпадает с хэшем из БД</returns>
        private async Task<bool> CheckLogin(LoginRequest request)
        {
            User user = await _userRepository.GetByNameAsync(request.Name);

            return user != null && user.Password == StringHashingSHA128(request.Password);
        }

        /// <summary>
        /// Преобразует строку в байты и вычисляет хэш
        /// </summary>
        /// <param name="str">Строка для которой нужно вычислить хэш</param>
        /// <returns>Строка хэша в UTF-8 от исходной строки</returns>
        private string StringHashingSHA128(string str)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(str);
                byte[] hashBytes = sha1.ComputeHash(inputBytes);
                string result = Encoding.UTF8.GetString(hashBytes);

                return result;
            }
        }
        #endregion
    }
}