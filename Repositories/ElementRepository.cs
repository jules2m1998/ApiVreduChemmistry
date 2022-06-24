using ApiVrEdu.Data;
using ApiVrEdu.Models.Elements;

namespace ApiVrEdu.Repositories;

public class ElementRepository
{
    private readonly DataContext _context;

    public ElementRepository(DataContext context)
    {
        _context = context;
    }

    // Element type
    public ElementType Type(ElementType elementType)
    {
        _context.Add(elementType);
        _context.SaveChanges();

        return elementType;
    }

    public ElementType? Type(int id)
    {
        return _context.ElementTypes.Find(id);
    }

    public List<ElementType> Type()
    {
        return _context.ElementTypes.ToList();
    }

    public ElementType UpdateType(ElementType elementType)
    {
        _context.Update(elementType);
        _context.SaveChanges();
        return elementType;
    }

    public void DeleteType(ElementType elementType)
    {
        _context.Remove(elementType);
        _context.SaveChanges();
    }

    // Element Group
    public ElementGroup Group(ElementGroup group)
    {
        _context.Add(group);
        _context.SaveChanges();
        return group;
    }

    public ElementGroup? Group(int id)
    {
        return _context.ElementGroups.Find(id);
    }

    public List<ElementGroup> Group()
    {
        return _context.ElementGroups.ToList();
    }

    public ElementGroup UpdateGroup(ElementGroup group)
    {
        _context.Update(group);
        _context.SaveChanges();

        return group;
    }

    public void DeleteGroup(ElementGroup group)
    {
        _context.Remove(group);
        _context.SaveChanges();
    }

    // Element
    public Element Element(Element element)
    {
        _context.Add(element);
        _context.SaveChanges();
        return element;
    }

    public List<Element> Element()
    {
        return _context.Elements.ToList();
    }

    public Element? Element(int id)
    {
        return _context.Elements.Find(id);
    }

    public Element UpdateElement(Element element)
    {
        _context.Update(element);
        _context.SaveChanges();
        return element;
    }

    public Element DeleteElement(Element element)
    {
        _context.Remove(element);
        _context.SaveChanges();
        return element;
    }
}