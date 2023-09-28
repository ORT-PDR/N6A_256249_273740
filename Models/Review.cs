using System;
namespace Models
{
	public class Review
	{
		public Guid Id;
		public User user;
		public string comment;
		public int score;
	}
}

