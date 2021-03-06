using Contexts;
using SartainStudios.DatabaseInteraction.Consumables;
using SartainStudios.SharedModels.Users;

namespace Consumables;

public interface IUserConsumable : IBaseNonSpecificUserConsumable<UserModel> { }

public class UserConsumable : BaseNonSpecificUserConsumable<UserModel>, IUserConsumable
{
    public UserConsumable(IUserContext context) : base(context) { }
}