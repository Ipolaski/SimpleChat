using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aboba.Infrastructure.Api.Request.Chat
{
	public record GetChatsByUserIdRequest(Guid UserId, int Count);
}
