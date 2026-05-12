using UserMicroservice.Data;
using UserMicroservice.Models;
using Microsoft.EntityFrameworkCore;

namespace UserMicroservice.Repository;

/// <summary>
/// Repository for managing refresh tokens.
/// Inherits generic CRUD operations from GenericRepository&lt;RefreshTokenModel&gt;.
/// No extra DbContext field is required; the base class already provides protected _context.
/// </summary>
public class RefreshTokenRepository : GenericRepository<RefreshTokenModel>
{
    public RefreshTokenRepository(UserdbContext context) : base(context)
    {
        
    }

}