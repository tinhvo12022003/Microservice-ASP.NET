namespace UserMicroservice.Models;

public class UserViewModel
{
    public string Fname {get; set;} = string.Empty;
    public string Lname {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public bool Gender {get; set;}
    public DateOnly BirthDate {get; set;}
    public string Phone {get; set;} = string.Empty;
}