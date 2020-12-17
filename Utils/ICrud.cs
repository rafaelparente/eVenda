namespace Utils
{
    public interface ICrud<T, I>
    {
        bool Create(T obj);

        T Read(I id);

        bool Update(T obj, I id);

        bool Delete(I id);

        bool Delete(T obj);
    }
}
