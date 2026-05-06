using UserMicroservice.Data;
using UserMicroservice.Models;

namespace UserMicroservice.Repository;


public class UserRepository : GenericRepository<UserModel>
{
    private readonly UserdbContext _context;

    public UserRepository(UserdbContext context) : base (context)
    {
        _context = context;
    }

}
