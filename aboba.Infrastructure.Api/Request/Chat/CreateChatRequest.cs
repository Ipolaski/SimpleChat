using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFC.Infrastructure.chat.Database;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using AFC.database.chat;
using AFC.Infrastructure.chat.Database;

namespace aboba.Infrastructure.Api.Request.Chat
{
	public record CreateChatRequest(string Name, AFC.Infrastructure.chat.Database.User Owner);
}
