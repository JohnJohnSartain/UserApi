using Contexts;
using SartainStudios.Common.Consumables;
using SharedModels;

namespace Consumables;

public interface IUserConsumable : IBaseNonSpecificUserConsumable<UserModel> { }

public class UserConsumable : BaseNonSpecificUserConsumable<UserModel>, IUserConsumable
{
    public UserConsumable(IUserContext context) : base(context) { }
}