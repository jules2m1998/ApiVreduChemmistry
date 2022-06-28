using ApiVrEdu.Data;
using ApiVrEdu.Models.Textures;

namespace ApiVrEdu.Repositories;

public class TextureRepository
{
    private readonly DataContext _context;

    public TextureRepository(DataContext context)
    {
        _context = context;
    }

    public Texture Create(Texture texture)
    {
        _context.Textures.Add(texture);
        _context.SaveChanges();
        return texture;
    }

    public Texture Update(Texture texture)
    {
        _context.SaveChanges();
        return texture;
    }

    public void Delete(Texture texture)
    {
        _context.SaveChanges();
    }

    public Texture? Get(int id)
    {
        return _context.Textures.Find(id);
    }

    public List<Texture> Get()
    {
        return _context.Textures.ToList();
    }

    public TextureGroup? GetOneGroup(int id)
    {
        return _context.TextureGroups.Find(id);
    }

    public List<TextureGroup> GetAllGroups()
    {
        return _context.TextureGroups.ToList();
    }

    public TextureGroup? CreateGroup(TextureGroup group)
    {
        _context.TextureGroups.Add(group);
        _context.SaveChanges();
        return _context.TextureGroups.Find(group.Id);
    }

    public TextureGroup UpdateGroup(TextureGroup group)
    {
        _context.SaveChanges();
        return group;
    }

    public void DeleteGroup(TextureGroup group)
    {
        _context.TextureGroups.Remove(group);
        _context.SaveChanges();
    }
}