using System.Data.Entity;
using ApiVrEdu.Data;
using ApiVrEdu.Dto.Reactions;
using ApiVrEdu.Models.Reactions;

namespace ApiVrEdu.Repositories;

public class ReactionRepository
{
    private readonly DataContext _context;
    private readonly ElementRepository _elementRepository;

    public ReactionRepository(DataContext context, ElementRepository elementRepository)
    {
        _context = context;
        _elementRepository = elementRepository;
    }

    public List<Reaction> Reaction()
    {
        return _context.Reactions.ToList();
    }

    public Reaction? Reaction(int id)
    {
        return _context
            .Reactions.Include(reaction => reaction.User).AsNoTracking().Single(r => r.Id == id);
    }

    public Reaction Reaction(Reaction reaction, List<ReactantDto> reactantDtos, List<ProductDto> productDtos)
    {
        try
        {
            using var transaction = _context.Database.BeginTransaction();
            _context.Add(reaction);
            _context.SaveChanges();
            reactantDtos.ForEach(dto =>
            {
                var elt = _elementRepository.Element(dto.ElementId);
                if (elt == null) throw new Exception("Element de l'un des reactifs inexistant");
                var reactant = new Reactant
                {
                    Element = elt,
                    Quantity = dto.Quantity,
                    Reaction = reaction
                };
                _context.Add(reactant);
                _context.SaveChanges();
            });

            productDtos.ForEach(dto =>
            {
                var elt = _elementRepository.Element(dto.ElementId);
                if (elt == null) throw new Exception("Element de l'un des produits inexistant");
                var product = new Product
                {
                    Element = elt,
                    Quantity = dto.Quantity,
                    Reaction = reaction
                };
                _context.Add(product);
                _context.SaveChanges();
            });

            transaction.Commit();
            return reaction;
        }
        catch (Exception e)
        {
            throw new Exception("Une erreure s'est produite lors de la creation de la reaction !");
        }
    }

    public void Reaction(Reaction reaction)
    {
        _context.Remove(reaction);
        _context.SaveChanges();
    }
}