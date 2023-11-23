using Models;

namespace AdministrationServer
{
    public class Storage
    {
        private static Storage instance = null;
        private static readonly object lockObject = new object();

        public List<Product> products = new List<Product>();
		
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
        
        public void AddProduct(Product product)
        {
            lock (lockObject)
            {
                ValidateProduct(product);
                products.Add(product);
            }
        }

        public void AddReview(Product product, Review review)
        {
            if (review.score < 1 || review.score > 5)
            {
                throw new ServerException("Score must be between 1 and 5");
            }

            lock (lockObject)
            {
                product.reviews.Add(review);
            }
        }
        
        private void ValidateProduct(Product p)
        {
            if (products.Any(u => u.name == p.name && u.creator == p.creator))
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
        
        public void DeleteProduct(Guid id)
        {
            lock (lockObject)
            {
                Product productToRemove = products.FirstOrDefault(p => p.id == id);

                if (productToRemove != null)
                {
                    if (!string.IsNullOrWhiteSpace(productToRemove.imagePath))
                    {
                        File.Delete(productToRemove.imagePath);
                    }
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
