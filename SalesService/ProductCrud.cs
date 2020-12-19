using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

using Utils;

namespace SalesService
{
    public class ProductCrud : DbContext, ICrud<Product, int>
    {
        public DbSet<Product> Products { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=sales.db");

        public bool DoCreate(Product obj)
        {
            if (!IsValid(obj)) return false;
            Add(obj);
            return this.SaveChanges() > 0;
        }

        public Product DoGet(int id)
        {
            return this.Products.Find(id);
        }

        public Product DoGet(Func<Product, bool> filter)
        {
            return this.Products.Where(filter).FirstOrDefault();
        }

        public Product[] DoGetAll(Func<Product, bool> filter = null)
        {
            return filter == null ? this.Products.ToArray() : this.Products.Where(filter).ToArray();
        }

        public bool DoUpdate(Product obj)
        {
            if (!obj.IsValid()) return false;

            var product = DoGet(obj.Id);

            foreach (var property in typeof(Product).GetProperties().Where(p => p.CanWrite))
            {
                property.SetValue(product, property.GetValue(obj, null), null);
            }

            Update(product);
            return this.SaveChanges() > 0;
        }

        public bool DoDelete(int id)
        {
            Remove(id);
            return this.SaveChanges() > 0;
        }

        public bool DoDelete(Product obj)
        {
            Remove(obj);
            return this.SaveChanges() > 0;
        }

        private bool IsValid(Product obj)
        {
            return obj.IsValid() && !this.Products.Any(p => p.Code == obj.Code || p.Name == obj.Name);
        }
    }
}
