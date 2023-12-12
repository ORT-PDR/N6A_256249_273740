using AdministrationServer;

namespace GrpcMainServer.Server.Models;

public class User
{
    public Guid id;
    public string username;
    public string password;
    public ProductList purchases;
}