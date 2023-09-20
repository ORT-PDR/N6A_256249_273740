using System;
using System.Linq;
using System.Collections.Generic;
using Models;

namespace Server.BusinessLogic
{
    public class UserService
    {
        private readonly Storage storage;

        public UserService(Storage storage)
        {
            this.storage = storage;
        }

        public void CreateUser(string username, string password)
        {
            if (UserExists(username))
            {
                throw new ServerException("Username already exists.");
            }
            
            ValidatePassword(password);
            
            var newUser = new User
            {
                id = Guid.NewGuid(),
                username = username,
                password = password
            };
            
            storage.AddUser(newUser);
        }

        public bool AuthenticateUser(string username, string password)
        {
            var user = storage.users.FirstOrDefault(u => u.username == username);
            
            if (user == null)
            {
                return false;
            }
            
            if (user.password == password)
            {
                return true;
            }

            return false; 
        }

        private void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                throw new ServerException("Password must be at least 8 characters long.");
            }
        }

        private bool UserExists(string username)
        {
            return storage.users.Any(u => u.username == username);
        }
    }
}
