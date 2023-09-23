using System;
using System.Collections.Generic;
using Models;

namespace Server.BusinessLogic
{
    public class ProductService
    {
        private readonly Storage storage;

        public ProductService(Storage storage)
        {
            this.storage = storage;
        }

        public void PublishProduct(Product product)
        {
            ValidateProduct(product);
            product.id = Guid.NewGuid();
            product.reviews = new List<Review>();

            storage.AddProduct(product);
        }

        public List<Product> GetProducts(string productNameFilter = null)
        {
            var query = storage.GetAllProducts().AsQueryable();

            if (!string.IsNullOrWhiteSpace(productNameFilter))
            {
                query = query.Where(product => product.name.Contains(productNameFilter, StringComparison.OrdinalIgnoreCase));
            }

            return query.ToList();
        }

        public Product GetProductById(Guid productId)
        {
            return storage.GetProductById(productId);
        }

        public Product GetProductByName(string name)
        {
            return storage.GetProductByName(name);
        }

        public Product UpdateProduct(Product updatedProduct)
        {
            Product existingProduct = storage.GetProductByName(updatedProduct.name);
    
            if (existingProduct == null)
            {
                throw new ServerException("Product not found.");
            }
            
            existingProduct.description = updatedProduct.description;
            existingProduct.stock = updatedProduct.stock;
            existingProduct.price = updatedProduct.price;
            existingProduct.imagePath = updatedProduct.imagePath;
            
            ValidateProduct(existingProduct);

            return existingProduct;
        }

        public void DeleteProduct(string product, string user)
        {
            Product existingProduct = storage.GetUserProductByName(product, user);
            if (existingProduct == null)
            {
                throw new ServerException("Product not found.");
            }
            storage.DeleteProduct(existingProduct.id);
        }

        public void BuyProduct(string product, string user)
        {
            Product existingProduct = storage.GetUserProductByName(product, user);
            if (existingProduct == null)
            {
                throw new ServerException("Product not found.");
            }
            storage.BuyProduct(existingProduct.id);
        }
        
        public List<Product> GetProductsByUser(string userName)
        {
            return storage.GetAllProducts()
                .Where(product => product.creator.Equals(userName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        
        public string ProductToString(Product product)
        {
            if (product == null)
            {
                return "Product is null";
            }

            return $"{product.name}:{product.description}:{product.stock}:{product.price}:{product.imagePath}:{product.creator}";
        }

        private void ValidateProduct(Product product)
        {
            if (product == null)
            {
                throw new ServerException("Product cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(product.name))
            {
                throw new ServerException("Product name cannot be empty or whitespace.");
            }

            if (product.stock < 0)
            {
                throw new ServerException("Stock must be a non-negative number.");
            }

            if (product.price <= 0)
            {
                throw new ServerException("Price must be a positive number.");
            }
        }
    }
}