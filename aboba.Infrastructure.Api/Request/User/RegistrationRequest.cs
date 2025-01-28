using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aboba.Infrastructure.Api.Request.User
{
	public record RegistrationRequest( string Name, string Password );
}
