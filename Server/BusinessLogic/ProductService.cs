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

            return $"Name: {product.name} | Description: {product.description} | Stock: {product.stock} | Price: {product.price} | Image Path: {product.imagePath}";
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