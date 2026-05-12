namespace UserMicroservice.Models;

public class UserModel : BaseModel
{
    public Guid Id {get; set;}
    public string Fname {get; set;} = string.Empty;
    public string Lname {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string PasswordHash {get; set;} =null!;
    public bool Gender {get; set;}
    public DateOnly BirthDate {get; set;}
    public string Phone {get; set;} = string.Empty;

    public ICollection<RefreshTokenModel> RefreshToken {get; set;} = new List<RefreshTokenModel>();
}