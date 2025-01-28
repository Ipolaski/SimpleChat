using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using aboba.Infrastructure.Api.Request.User;
using AFC.database.chat;
using AFC.Infrastructure.chat.Database;
using Microsoft.AspNetCore.Mvc;

namespace aboba.Infrastructure.Api
{
	[ApiController]
	public class UserController : ControllerBase
	{
		private UserRepository _userRepository;
		public UserController( UserRepository userRepository ) 
		{
			_userRepository = userRepository;
		}	
		[HttpPost]
		[Route( "Registration" )]
		//[SwaggerResponse( StatusCodes.Status200OK, "GET 200 aboba", typeof( List<abobaModel> ) )]
		//[SwaggerResponse( StatusCodes.Status400BadRequest, "GET 400 aboba", typeof( List<abobaModel> ) )]
		public async Task<IActionResult> Registration ( [FromBody] RegistrationRequest request )
		{
			if( Valid ( request ) )
			{
				if ( !UserExist( request.Name ) )
				{
					var user = new User() 
					{					
						Name = request.Name,
						Password = StringHashingSHA128(request.Password),
						IsAdmin = true,
					};
					_userRepository.Add( user );
					return Ok();
				}
			}
			return BadRequest();
		}
		[HttpPost]
		[Route( "Login" )]
		//[SwaggerResponse( StatusCodes.Status200OK, "GET 200 aboba", typeof( List<abobaModel> ) )]
		//[SwaggerResponse( StatusCodes.Status400BadRequest, "GET 400 aboba", typeof( List<abobaModel> ) )]
		public async Task<IActionResult> Login( [FromBody] LoginRequest request )
		{			
			if ( CheckLogin( request ) )
			{				
				return Ok();
			}
			
			return BadRequest();
		}

		private bool Valid( RegistrationRequest request )
		{
			var user = new User() 
			{
				Name = request.Name,
				Password = request.Password,
				IsAdmin = true,
			};
			//todo подумать над валидацией пользователя
			//bool result = Validator.TryValidateValue( user, new ValidationContext( this, null, null ),null, null);
			return true;// return result;
		}
		private bool UserExist( string name )
		{
			User user = _userRepository.GetByName( name );
			if ( user == null )
			{
				return false; 
			}
			else 
			{
				return true;
			}
		}
		private bool CheckLogin( LoginRequest request )
		{
			User user = _userRepository.GetByName( request.Name );
			if ( user == null )
			{ 
				return false;
			}
			if ( user.Password != StringHashingSHA128(request.Password) )
			{ 
				return false;
			}
			return true;
		}
		private string StringHashingSHA128(string str)
		{
			using ( SHA1 sha1 = SHA1.Create() )
			{
				// Преобразуем строку в байты
				byte[] inputBytes = Encoding.UTF8.GetBytes( str );
				// Вычисляем хэш
				byte[] hashBytes = sha1.ComputeHash( inputBytes );

				string result = Encoding.UTF8.GetString( hashBytes );
				return result;
			}
		}
	}
}