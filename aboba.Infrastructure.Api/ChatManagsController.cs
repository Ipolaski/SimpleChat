using System.IdentityModel.Tokens.Jwt;
using aboba.Infrastructure.Api.Request.Chat;
using AFC.database.chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Group = AFC.Infrastructure.chat.Database.Group;
using User = AFC.Infrastructure.chat.Database.User;

namespace aboba.Infrastructure.Api
{
	[Authorize]
	public class ChatManagsController : ControllerBase
	{
		private ChatsManageRepository _chateManageRepository;
		public ChatManagsController( ChatsManageRepository chateManageRepository )
		{
			_chateManageRepository = chateManageRepository;
		}
		[HttpPost]
		[Route( "Create" )]
		//[SwaggerResponse( StatusCodes.Status200OK, "GET 200 aboba", typeof( List<abobaModel> ) )]
		//[SwaggerResponse( StatusCodes.Status400BadRequest, "GET 400 aboba", typeof( List<abobaModel> ) )]
		public async Task<IActionResult> Create( [FromBody] CreateChatRequest request )
		{
			var chat = new AFC.Infrastructure.chat.Database.Group()
			{
				Name = request.Name,
				Owner = request.Owner,
				Members = new List<AFC.Infrastructure.chat.Database.User> { request.Owner }
			};
			bool result = _chateManageRepository.Add( chat );
			if ( result )
			{
				return Ok();
			}
			else
			{ 
			return BadRequest();
			}
		}
		[HttpPost]
		[Route( "Delete" )]
		//[SwaggerResponse( StatusCodes.Status200OK, "GET 200 aboba", typeof( List<abobaModel> ) )]
		//[SwaggerResponse( StatusCodes.Status400BadRequest, "GET 400 aboba", typeof( List<abobaModel> ) )]
		public async Task<IActionResult> Delete( [FromBody] DeleteChatRequest request )
		{			
			bool result = _chateManageRepository.DeleteById( request.Id );
			if ( result )
			{
				return Ok();
			}
			else
			{
				return BadRequest();
			}
		}
		[HttpGet]
		[Route( "GetChats" )]
		//[SwaggerResponse( StatusCodes.Status200OK, "GET 200 aboba", typeof( List<abobaModel> ) )]
		//[SwaggerResponse( StatusCodes.Status400BadRequest, "GET 400 aboba", typeof( List<abobaModel> ) )]
		public async Task<IActionResult> GetChats( [FromBody] GetChatsByUserIdRequest request )
		{
			Guid userId = GetUserIdFromToken();
			List<Group> groups = _chateManageRepository.GetRange( request.Count, userId ).ToList();
			
			return Ok( groups );
			
		}
		[HttpPost]
		[Route( "UpdateChat" )]
		//[SwaggerResponse( StatusCodes.Status200OK, "GET 200 aboba", typeof( List<abobaModel> ) )]
		//[SwaggerResponse( StatusCodes.Status400BadRequest, "GET 400 aboba", typeof( List<abobaModel> ) )]
		public async Task<IActionResult> Update( [FromBody] UpdateChatRequest request ) //это не правильно но пока что ренейм будет а редактирование пользователей в чате вынести в отдельный метод хмхмхмх
		{
			Group group = new Group() 
			{
				Name = request.Name,

			};
			Guid userId = GetUserIdFromToken();
			bool result = _chateManageRepository.UpdateById( group, userId );
			if ( result )
			{
				return Ok();
			}
			else
			{
				return BadRequest();
			}
		}

		private Guid GetUserIdFromToken()
		{
			Request.Headers.TryGetValue( "Authorization", out var userToken );
			var handler = new JwtSecurityTokenHandler();
			var jwtSecurityToken = handler.ReadJwtToken( userToken.ToString().Remove( 0, 7 ) );
			var _userId = Guid.Parse( jwtSecurityToken.Claims.Where( claim => claim.Type == "UserId" ).FirstOrDefault().Value );
			return _userId;
		}
	}
}
