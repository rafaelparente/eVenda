using System;
using System.Collections.Generic;

namespace Utils
{
    public interface ICrud<T, I>
    {
        bool Create(T obj);

        T Get(I id);

        T Get(Func<Product, bool> filter);

        IReadOnlyCollection<T> GetAll(Func<Product, bool> filter);

        bool Update(T obj);

        bool Delete(I id);

        bool Delete(T obj);
    }
}
