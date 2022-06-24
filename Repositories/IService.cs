namespace ApiVrEdu.Repositories;

public interface IService<T>
{
    public T Create(T model);
    public T? GetOne(int id);
    public IEnumerable<T> GetAll();
    public T Update(T model, int userId);
    public void Delete(int id, int usrId);
}