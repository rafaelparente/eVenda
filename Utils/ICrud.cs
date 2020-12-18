using System.Collections.Generic;

namespace Utils
{
    public interface ICrud<T, I>
    {
        bool Create(T obj);

        T Get(I id);

        IReadOnlyCollection<T> GetAll();

        bool Update(T obj, I id);

        bool Delete(I id);

        bool Delete(T obj);
    }
}
