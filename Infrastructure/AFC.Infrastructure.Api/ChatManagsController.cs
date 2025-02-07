using System.IdentityModel.Tokens.Jwt;

using Infrastructure.AFC.Infrastructure.Api.Request.Chat;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Infrastructure.AFC.Infrastructure.Database;

using Group = Infrastructure.AFC.Infrastructure.Database.Entities.Group;
using User = Infrastructure.AFC.Infrastructure.Database.Entities.User;
using AFC.Infrastructure.Api.Request.Chat;
using Infrastructure.AFC.Infrastructure.Database.Entities;

namespace AFC.Infrastructure.Api
{
    //[Authorize]
    public class ChatManagsController : ControllerBase
    {
        private ChatsManageRepository _chateManageRepository;

        public ChatManagsController(ChatsManageRepository chateManageRepository)
        {
            _chateManageRepository = chateManageRepository;
        }

        [HttpPost]
        [Route("Create")]
        //[SwaggerResponse( StatusCodes.Status200OK, "GET 200 aboba", typeof( List<abobaModel> ) )]
        //[SwaggerResponse( StatusCodes.Status400BadRequest, "GET 400 aboba", typeof( List<abobaModel> ) )]
        public IActionResult Create([FromBody] CreateChatRequest request)
        {
            var chat = new Group()
            {
                Name = request.Name,
                Owner = request.Owner,
                Members = new List<User> { request.Owner }
            };
            bool result = _chateManageRepository.AddToGroups(chat);

            return result ? Ok() : BadRequest();
        }

        [HttpPost]
        [Route("Delete")]
        //[SwaggerResponse( StatusCodes.Status200OK, "GET 200 aboba", typeof( List<abobaModel> ) )]
        //[SwaggerResponse( StatusCodes.Status400BadRequest, "GET 400 aboba", typeof( List<abobaModel> ) )]
        public async Task<IActionResult> Delete([FromBody] DeleteChatRequest request)
        {
            bool result = await _chateManageRepository.DeleteById(request.Id);
            
            return result ? Ok() : BadRequest();
        }
        
        [HttpGet]
        [Route("GetChats")]
        //[SwaggerResponse( StatusCodes.Status200OK, "GET 200 aboba", typeof( List<abobaModel> ) )]
        //[SwaggerResponse( StatusCodes.Status400BadRequest, "GET 400 aboba", typeof( List<abobaModel> ) )]
        public async Task<IActionResult> GetChats([FromBody] GetChatsByUserIdRequest request)
        {
            Guid userId = GetUserIdFromToken();
            List<Group> groups = _chateManageRepository.GetRange(request.Count, userId).ToList();

            return Ok(groups);
        }

        [HttpGet]
        [Route("GetMessages")]
        //[SwaggerResponse( StatusCodes.Status200OK, "GET 200 aboba", typeof( List<abobaModel> ) )]
        //[SwaggerResponse( StatusCodes.Status400BadRequest, "GET 400 aboba", typeof( List<abobaModel> ) )]
        public async Task<IActionResult> GetMessagesSortByDecAsync([FromBody] GetChatMessagesByGroupIdRequest request)
        {
            List<Message> groups = await _chateManageRepository.GetAllChatMessagesSortByDecAsync(request.GroupId);

            return Ok(groups);
        }

        [HttpPost]
        [Route("UpdateChat")]
        //[SwaggerResponse( StatusCodes.Status200OK, "GET 200 aboba", typeof( List<abobaModel> ) )]
        //[SwaggerResponse( StatusCodes.Status400BadRequest, "GET 400 aboba", typeof( List<abobaModel> ) )]
        public async Task<IActionResult> Update([FromBody] UpdateChatRequest request) //это не правильно но пока что ренейм будет а редактирование пользователей в чате вынести в отдельный метод хмхмхмх
        {
            Group group = new Group()
            {
                Name = request.Name,

            };
            Guid userId = GetUserIdFromToken();
            bool result = await _chateManageRepository.UpdateById(group, userId);

            return result ? Ok() : BadRequest();
        }

        #region [Private methods]
        private Guid GetUserIdFromToken()
        {
            Request.Headers.TryGetValue("Authorization", out var userToken);
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(userToken.ToString().Remove(0, 7));
            Guid userId = Guid.Parse(jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "UserId")!.Value);
            
            return userId;
        }
        #endregion
    }
}