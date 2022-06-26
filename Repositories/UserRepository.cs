using System.Data.Entity;
using ApiVrEdu.Data;
using ApiVrEdu.Models;

namespace ApiVrEdu.Repositories;

public class UserRepository
{
    private readonly DataContext _context;

    public UserRepository(DataContext context)
    {
        _context = context;
    }

    public User Create(string username, string hashedPwd, string lastname, string firstname, string email, SexType sex,
        DateTime birthDate, string? image, string phoneNumber)
    {
        var user = new User
        {
            UserName = username,
            PasswordHash = hashedPwd,
            LastName = lastname,
            FirstName = firstname,
            Email = email,
            Sex = sex,
            BirthDate = birthDate,
            Image = image,
            PhoneNumber = phoneNumber
        };
        _context.Users.Add(user);
        _context.SaveChanges();

        return user;
    }

    public User? GetByUserName(string username)
    {
        return _context.Users.Include(user => user.Reactions).Single(u => u.UserName == username);
    }

    public User? GetOne(int id)
    {
        return _context.Users.Include(user => user.Reactions).Single(user => user.Id == id);
    }

    public IEnumerable<User> GetAll()
    {
        return _context.Users.ToList();
    }

    public User Update(User user)
    {
        _context.SaveChanges();
        return user;
    }

    public void Delete(User user)
    {
        _context.Users.Remove(user);
        _context.SaveChanges();
    }
}