using DatabaseInteraction;
using DatabaseInteraction.Models;
using Microsoft.Extensions.Configuration;
using SartainStudios.DataAccess.Interfaces;
using SharedModels;

namespace Contexts;

public interface IUserContext : INonSpecificUserDatabaseAccess<UserModel> { }

public class UserContext : MongoNonUserSpecificDatabaseAccess<UserModel>, IUserContext
{
    private static readonly ConnectionModel connectionModel = new()
    {
        ConnectionString = "UsersDatabaseServer",
        DatabaseName = "Users",
        CollectionName = nameof(UserContext)
    };

    public UserContext(IConfiguration configuration) : base(configuration, connectionModel) =>
        Items = MongoDatabase.GetCollection<UserModel>(connectionModel.CollectionName);
}