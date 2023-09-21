using System;
using Models;

namespace Server
{
	public class Storage
	{
		private static Storage instance = null;
		private static readonly object lockObject = new object();
		
		public List<User> users;
		public List<Product> products;
		
		public static Storage Instance
		{
			get
			{
				lock (lockObject)
				{
					if (instance == null)
					{
						instance = new Storage();
					}
				}
				return instance;
			}
		}

		public void AddUser(User user)
		{
			lock (lockObject)
			{
				ValidatePassword(user.password);
				ValidateUsername(user.username);
				users.Add(user);
			}
        }

		public void AddProduct(Product product)
		{
			lock (lockObject)
			{
                ValidateProduct(product.name);
                products.Add(product);
			}
		}

        public void AddReview(Product product, Review review)
        {
            if (review.score < 1 || review.score > 10)
            {
				throw new ServerException("Score must be between 1 and 10");
            }

            lock (lockObject)
            {
	            product.reviews.Add(review);
            }
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

        private void ValidateProduct(string name)
        {
            if (products.Any(u => u.name == name))
            {
                throw new ServerException("Product name must be unique");
            }
        }

        public List<Product> GetAllProducts()
		{
			lock (lockObject)
			{
				return products;
			}
		}

		public Product GetProductById(Guid productId)
		{
			lock (lockObject)
			{
				return products.FirstOrDefault(p => p.id == productId);
			}
		}

        public Product GetProductByName(string productName)
        {
            lock (lockObject)
            {
                return products.FirstOrDefault(p => p.name == productName);
            }
        }

        public void DeleteProduct(Guid productId)
		{
			lock (lockObject)
			{
				Product productToRemove = products.FirstOrDefault(p => p.id == productId);

				if (productToRemove != null)
				{
					products.Remove(productToRemove);
				}
				else
				{
					throw new ServerException("Product not found.");
				}
			}
		}
    }
}

