using UserMicroservice.Data;
using UserMicroservice.Models;

namespace UserMicroservice.Repository;


public class UserRepository : GenericRepository<UserModel>
{
    public UserRepository(UserdbContext context) : base (context)
    {
    }
}
