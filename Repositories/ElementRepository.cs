using ApiVrEdu.Data;
using ApiVrEdu.Models.Elements;
using Microsoft.EntityFrameworkCore;

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
        _context.ElementTypes.Add(elementType);
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
        _context.ElementTypes.Remove(elementType);
        _context.SaveChanges();
    }

    // Element Group
    public ElementGroup Group(ElementGroup group)
    {
        _context.ElementGroups.Add(group);
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
        _context.SaveChanges();

        return group;
    }

    public void DeleteGroup(ElementGroup group)
    {
        _context.ElementGroups.Remove(group);
        _context.SaveChanges();
    }

    // Element
    public Element Element(Element element)
    {
        _context.Elements.Add(element);
        _context.SaveChanges();
        return element;
    }

    public List<Element> Element()
    {
        return _context.Elements.AsNoTracking().Include(element => element.User).ToList();
    }

    public Element? Element(int id)
    {
        return _context.Elements.Find(id);
    }

    public List<Element> Element(List<int> ids)
    {
        return _context.Elements.Where(e => ids.Contains(e.Id)).ToList();
    }

    public Element UpdateElement(Element element)
    {
        _context.Elements.Update(element);
        _context.SaveChanges();
        return element;
    }

    public Element DeleteElement(Element element)
    {
        _context.Elements.Remove(element);
        _context.SaveChanges();
        return element;
    }

    public List<ElementChildren> Children(List<ElementChildren> childrenList)
    {
        childrenList.ForEach(c => _context.ElementChildren.Add(c));
        _context.SaveChanges();
        return childrenList;
    }
}