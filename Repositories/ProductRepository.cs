using System.Data.Entity;
using ApiVrEdu.Data;
using ApiVrEdu.Models.Reactions;

namespace ApiVrEdu.Repositories;

public class ProductRepository
{
    private readonly DataContext _context;

    public ProductRepository(DataContext context)
    {
        _context = context;
    }

    public List<Product> Product()
    {
        return _context.Products.AsNoTracking().ToList();
    }

    public List<Product> Product(int[] ids)
    {
        return _context.Products.AsNoTracking()
            .Where(product => ids.Contains(product.Id))
            .ToList();
    }

    public void Product(Product product)
    {
        _context.Remove(product);
        _context.SaveChanges();
    }

    public Product Product(Product product, bool isAdd)
    {
        if (isAdd) _context.Add(product);
        else _context.Update(product);
        _context.SaveChanges();
        return product;
    }

    public Product? Product(int id)
    {
        return _context.Products.AsNoTracking().Single(p => p.Id == id);
    }
}