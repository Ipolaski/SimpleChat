using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFC.Infrastructure.chat.Database;

namespace AFC.database.chat
{
	public class ChatsManageRepository
	{

		public ApplicationContext _applicationContext;
		public ChatsManageRepository( ApplicationContext applicationContext )
		{
			_applicationContext = applicationContext;
		}

		public bool DeleteById( Guid id )
		{
			Group group = GetById( id );
			if ( group != null )
			{
				_applicationContext.Groups.Remove( group );
				int count = _applicationContext.SaveChanges();
				return true;
			}
			return false;
		}

		public bool Add( Group group )
		{
			_applicationContext.Groups.Add( group );
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

		public bool UpdateById( Group newGroup,Guid userId )
		{
			Group group = GetById( newGroup.Id );
            if (group.Id != userId )
            {
				return false;
			}
            group.Name = newGroup.Name;
			_applicationContext.Update( group );
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

		public Group GetById( Guid id )
		{
			Group group = _applicationContext.Groups.Where( u => u.Id == id ).FirstOrDefault();
			return group;
		}
		public IQueryable<Group> GetRange( int count , Guid userId )
		{
			IQueryable<Group> groups = _applicationContext.Groups.Where(g=> g.Owner.Id == userId ).Take( count );			
			return groups;
		}

		public void Save()
		{
			_applicationContext.SaveChanges();
		}
	}
}
