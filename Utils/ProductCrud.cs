using System;
using System.Collections.Generic;

namespace Utils
{
    public class ProductCrud : ICrud<Product, int>
    {
        private readonly List<Product> _database = new List<Product>();

        public bool Create(Product obj)
        {
            if (!IsValid(obj)) return false;
            _database.Add(obj);
            obj.Id = _database.Count - 1;
            return true;
        }

        public Product Get(int id)
        {
            return id >= _database.Count ? null : _database[id];
        }

        public Product Get(Func<Product, bool> filter)
        {
            return _database.Find(p => filter(p));
        }

        public IReadOnlyCollection<Product> GetAll(Func<Product, bool> filter = null)
        {
            if (filter == null)
            {
                return _database.AsReadOnly();
            }
            else
            {
                return _database.FindAll(p => filter(p)).AsReadOnly();
            }
        }

        public bool Update(Product obj)
        {
            if (!obj.IsValid()) return false;
            _database[obj.Id] = obj;
            return true;
        }

        public bool Delete(int id)
        {
            return id < _database.Count && Delete(_database[id]);
        }

        public bool Delete(Product obj)
        {
            return _database.Remove(obj);
        }

        private bool IsValid(Product obj)
        {
            return obj.IsValid() && !_database.Exists(p => p.Code == obj.Code || p.Name == obj.Name);
        }
    }
}
