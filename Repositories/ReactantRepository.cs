using System.Data.Entity;
using ApiVrEdu.Data;
using ApiVrEdu.Models.Reactions;

namespace ApiVrEdu.Repositories;

public class ReactantRepository
{
    private readonly DataContext _context;

    public ReactantRepository(DataContext context)
    {
        _context = context;
    }

    public List<Reactant> Reactant()
    {
        return _context.Reactants.AsNoTracking().ToList();
    }

    public List<Reactant> Reactant(int[] ids)
    {
        return _context
            .Reactants
            .AsNoTracking()
            .Where(reactant => ids.Contains(reactant.Id))
            .ToList();
    }

    public Reactant? Reactant(int id)
    {
        return _context.Reactants.AsNoTracking().Single(r => r.Id == id);
    }

    public void Reactant(Reactant reactant)
    {
        _context.Remove(reactant);
        _context.SaveChanges();
    }

    public Reactant Reactant(Reactant reactant, bool isAdd)
    {
        if (isAdd) _context.Add(reactant);
        else _context.Update(reactant);

        _context.SaveChanges();
        return reactant;
    }
}