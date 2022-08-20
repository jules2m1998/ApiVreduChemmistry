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
}