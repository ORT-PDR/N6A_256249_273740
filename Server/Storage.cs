using System;
using Models;

namespace Server
{
	public class Storage
	{
		public List<User> users;
		public List<Product> products;

		public void AddUser(User user)
		{
			ValidatePassword(user.password);
			ValidateUsername(user.username);
			users.Add(user);
        }

		public void AddProduct(Product product)
		{
			products.Add(product);
		}

        public void AddReview(Product product, Review review)
        {
            if (review.score < 1 || review.score > 10)
            {
				throw new ServerException("Score must be between 1 and 10");
            }
			product.reviews.Add(review);
        }

        private void ValidatePassword(string pass)
		{
			if (pass.Length < 8 && !pass.Any(char.IsUpper))
			{
				throw new ServerException("Password must be at least 8 characters long and must have an UpperCase");
			}
		}

		private void ValidateUsername(string uname)
		{
            if (users.Any(u => u.username == uname))
            {
                throw new ServerException("Username must be unique");
            }
        }
    }
}

