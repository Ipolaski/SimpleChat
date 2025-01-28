using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFC.Infrastructure.chat.Database;

namespace AFC.database.chat
{
	public class UserRepository
	{
		public ApplicationContext _applicationContext;
		public UserRepository( ApplicationContext applicationContext )
		{
			_applicationContext = applicationContext;
		}

		public bool DeleteById( Guid id )
		{
			User user = GetById(id);
			if ( user != null )
			{
				_applicationContext.Users.Remove( user );
				int count = _applicationContext.SaveChanges();
				return true;
			}
			return false;
		}

		public bool Add( User user )
		{
			_applicationContext.Users.Add( user );
			int count = _applicationContext.SaveChanges();
			if ( count != 0 )
			{
				return true;
			}
			else 
			{
				return false;
			}
		}

		public bool UpdateById( User newUser )
		{
			User user = GetById( newUser.Id );
			user.Name = newUser.Name;
			user.Password = newUser.Password;
			user.IsAdmin = newUser.IsAdmin;
			_applicationContext.Update( user );
			int count = _applicationContext.SaveChanges();
			if ( count != 0 )
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public User GetById( Guid id )
		{
			User user = _applicationContext.Users.Where( u => u.Id == id ).FirstOrDefault();
			return user;
		}

		public User GetByName( string name )
		{
			User user = _applicationContext.Users.Where( u => u.Name == name).FirstOrDefault();
			return user;
		}

		public void Save()
		{
			_applicationContext.SaveChanges();
		}
	}
}
