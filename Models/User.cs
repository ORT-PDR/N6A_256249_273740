using System;
namespace Models
{
	public class User
	{
		public Guid id;
		public string username;
		public string password;
		public List<Product> purchases;
	}
}

