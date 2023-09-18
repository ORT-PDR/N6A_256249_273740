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

            storage.AddProduct(product);
        }

        public List<Product> GetAllProducts()
        {
            return storage.GetAllProducts();
        }

        public Product GetProductById(Guid productId)
        {
            return storage.GetProductById(productId);
        }

        public Product UpdateProduct(Product updatedProduct)
        {
            updatedProduct.name = "Updated Product";
            ValidateProduct(updatedProduct);

            Product existingProduct = storage.GetProductById(updatedProduct.id);
            if (existingProduct == null)
            {
                throw new ServerException("Product not found.");
            }
            
            existingProduct.description = updatedProduct.description;
            existingProduct.stock = updatedProduct.stock;
            existingProduct.price = updatedProduct.price;
            existingProduct.imagePath = updatedProduct.imagePath;

            return existingProduct;
        }

        public void DeleteProduct(Guid productId)
        {
            Product existingProduct = storage.GetProductById(productId);
            if (existingProduct == null)
            {
                throw new ServerException("Product not found.");
            }
            
            storage.DeleteProduct(productId);
        }
        
        public List<Product> GetUserProductsByName(string userName, string productName)
        {
            return storage.GetAllProducts()
                .Where(product => product.creator.username.Equals(userName, StringComparison.OrdinalIgnoreCase)
                                  && product.name.Equals(productName, StringComparison.OrdinalIgnoreCase))
                .ToList();
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
            
            if (!string.IsNullOrWhiteSpace(product.imagePath) && !File.Exists(product.imagePath))
            {
                throw new ServerException("Image path is invalid.");
            }
        }
    }
}