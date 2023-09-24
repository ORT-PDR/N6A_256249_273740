using Models;
using System;
using System.Collections.Generic;

namespace Server
{
    public class TestData
    {
        public List<Product> GetProducts()
        {
            var products = new List<Product>
            {
                new Product
                {
                    id = Guid.NewGuid(),
                    name = "Soccer Ball",
                    description = "Official Balon D'or for matches BOBO",
                    stock = 100,
                    price = 29.99,
                    imagePath = "soccer_ball.jpg",
                    creator = "Lionel Messi",
                    reviews = new List<Review>
                    {
                        new Review
                        {
                            Id = Guid.NewGuid(),
                            user = new User
                            {
                                id = Guid.NewGuid(),
                                username = "Cristiano Ronaldo",
                                password = "password123"
                            },
                            comment = "Great quality and durability",
                            score = 5
                        },
                        new Review
                        {
                            Id = Guid.NewGuid(),
                            user = new User
                            {
                                id = Guid.NewGuid(),
                                username = "Neymar Jr",
                                password = "password456"
                            },
                            comment = "A bit expensive, but worth it",
                            score = 4
                        }
                    }
                },
                new Product
                {
                    id = Guid.NewGuid(),
                    name = "Soccer Jersey",
                    description = "Special edition soccer jersey muito bonito SIUUU",
                    stock = 50,
                    price = 49.99,
                    imagePath = "soccer_jersey.jpg",
                    creator = "Cristiano Ronaldo",
                    reviews = new List<Review>
                    {
                        new Review
                        {
                            Id = Guid.NewGuid(),
                            user = new User
                            {
                                id = Guid.NewGuid(),
                                username = "Kylian Mbappé",
                                password = "password789"
                            },
                            comment = "I love the fabric quality",
                            score = 5
                        },
                        new Review
                        {
                            Id = Guid.NewGuid(),
                            user = new User
                            {
                                id = Guid.NewGuid(),
                                username = "Sergio Ramos",
                                password = "passwordabc"
                            },
                            comment = "Spectacular design",
                            score = 4
                        }
                    }
                },
                new Product
                {
                    id = Guid.NewGuid(),
                    name = "Jerseyy",
                    description = "muito bonito SIUUU",
                    stock = 20,
                    price = 29.99,
                    imagePath = "soccer_jersey.jpg",
                    creator = "Cristiano Ronaldo",
                    reviews = new List<Review>
                    {
                        new Review
                        {
                            Id = Guid.NewGuid(),
                            user = new User
                            {
                                id = Guid.NewGuid(),
                                username = "Kylian Mbappé",
                                password = "password789"
                            },
                            comment = "I love the fabric quality",
                            score = 5
                        },
                        new Review
                        {
                            Id = Guid.NewGuid(),
                            user = new User
                            {
                                id = Guid.NewGuid(),
                                username = "Sergio Ramos",
                                password = "passwordabc"
                            },
                            comment = "Spectacular design",
                            score = 4
                        }
                    }
                },
                new Product
                {
                    id = Guid.NewGuid(),
                    name = "Soccer Cleats",
                    description = "Professional soccer cleats for maximum performance",
                    stock = 30,
                    price = 89.99,
                    imagePath = "soccer_cleats.jpg",
                    creator = "Neymar Jr",
                    reviews = new List<Review>
                    {
                        new Review
                        {
                            Id = Guid.NewGuid(),
                            user = new User
                            {
                                id = Guid.NewGuid(),
                                username = "Lionel Messi",
                                password = "passwordxyz"
                            },
                            comment = "Great comfort and grip",
                            score = 5
                        },
                        new Review
                        {
                            Id = Guid.NewGuid(),
                            user = new User
                            {
                                id = Guid.NewGuid(),
                                username = "Cristiano Ronaldo",
                                password = "password789"
                            },
                            comment = "Expensive but worth every penny",
                            score = 4
                        }
                    }
                }
            };

            return products;
        }

        public List<User> GetUsers()
        {
            var users = new List<User>
            {
                new User
                {
                    id = Guid.NewGuid(),
                    username = "Lionel Messi",
                    password = "password123",
                    purchases = new List<Product>()
                },
                new User
                {
                    id = Guid.NewGuid(),
                    username = "Cristiano Ronaldo",
                    password = "password456",
                    purchases = new List<Product>()
                },
                new User
                {
                    id = Guid.NewGuid(),
                    username = "Neymar Jr",
                    password = "password789",
                    purchases = new List<Product>()
                },
                new User
                {
                    id = Guid.NewGuid(),
                    username = "Kylian Mbappé",
                    password = "passwordabc",
                    purchases = new List<Product>()
                },
                new User
                {
                    id = Guid.NewGuid(),
                    username = "Sergio Ramos",
                    password = "passworddef",
                    purchases = new List<Product>()
                }
            };

            return users;
        }
    }
}
