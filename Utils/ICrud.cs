using System;

namespace Utils
{
    public interface ICrud<T, I>
    {
        bool DoCreate(T obj);

        T DoGet(I id);

        T DoGet(Func<Product, bool> filter);

        T[] DoGetAll(Func<Product, bool> filter);

        bool DoUpdate(T obj);

        bool DoDelete(I id);

        bool DoDelete(T obj);
    }
}
