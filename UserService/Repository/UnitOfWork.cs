using UserMicroservice.Data;

namespace UserMicroservice.Repository;

public class UnitOfWork
{
    private readonly UserdbContext _context;
    public UserRepository userRepository {get; }

    public UnitOfWork (UserdbContext context, UserRepository userRepo)
    {
        _context = context;
        userRepository = userRepo;
    }

    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync();
    }

    private bool _disposed = false;
    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}