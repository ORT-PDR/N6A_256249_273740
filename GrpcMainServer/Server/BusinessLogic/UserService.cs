using AdministrationServer;
using GrpcMainServer.Server.Models;

namespace GrpcMainServer.Server.BusinessLogic;

public class UserService
{
    private Storage storage;
    private static UserService instance;

    private static readonly object singletonlock = new object();

    public static UserService GetInstance()
    {
        lock (singletonlock)
        {
            if (instance == null)
                instance = new UserService();
            instance.storage = Storage.GetInstance();
        }
        return instance;
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
            password = password,
            purchases = new ProductList()
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

    public User GetUser(string username)
    {
        try
        {
            User usr = storage.users.FirstOrDefault(u => u.username == username);
            return usr;
        }
        catch (ArgumentNullException)
        {
            throw new ServerException("There was an error and the user could not be retrieved.");
        }
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