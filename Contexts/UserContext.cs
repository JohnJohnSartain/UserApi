using DatabaseInteraction;
using DatabaseInteraction.Models;
using Microsoft.Extensions.Configuration;
using SartainStudios.DataAccess.Interfaces;
using SharedModels;

namespace Contexts;

public interface IUserContext : INonSpecificUserDatabaseAccess<UserModel> { }

public class UserContext : MongoNonUserSpecificDatabaseAccess<UserModel>, IUserContext
{
    public UserContext(IConfiguration configuration) 
    {
        var connectionModel = GetConnectionModel(configuration);

        SetupConnectionAsync(connectionModel);

        Items = MongoDatabase.GetCollection<UserModel>(connectionModel.CollectionName);
    }

    private static ConnectionModel GetConnectionModel(IConfiguration configuration) => new()
    {
        ConnectionString = configuration["ConnectionStrings:UsersDatabaseServer"],
        DatabaseName = configuration["DatabaseNames:Users"],
        CollectionName = configuration[$"CollectionNames:{GetCollectionNameFromContextName()}"]
    };

    private static string GetCollectionNameFromContextName(string nameOfContext = nameof(UserContext)) =>
        nameOfContext.Substring(0, nameOfContext.LastIndexOf("Context"));
}