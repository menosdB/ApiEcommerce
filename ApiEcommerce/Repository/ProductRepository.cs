using System;
using ApiEcommerce.Models;
using ApiEcommerce.Repository.IRepository;

namespace ApiEcommerce.Repository;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _db;

    public ProductRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public bool BuyProduct(string name, int quantity)
    {
        if (string.IsNullOrEmpty(name) || quantity <= 0)
            return false;

        var product = _db.Products.FirstOrDefault(p =>
            p.Name.ToLower().Trim() == name.ToLower().Trim()
        );

        if (product == null || product.Stock < quantity)
            return false;

        product.Stock -= quantity;

        _db.SaveChanges();
        return true;
    }

    public bool CreateProduct(Product product)
    {
        if (product == null)
            return false;

        product.CreatedAt = DateTime.Now;
        product.UpdatedAt = DateTime.Now;

        _db.Products.Add(product);
        return Save();
    }

    public bool DeleteProduct(Product product)
    {
        if (product == null)
            return false;

        _db.Products.Remove(product);
        return Save();
    }

    public Product? GetProduct(int id)
    {
        if (id <= 0)
            return null;

        return _db.Products.FirstOrDefault(p => p.ProductId == id);
    }

    public ICollection<Product> GetProducts()
    {
        return _db.Products.OrderBy(p => p.Name).ToList();
    }

    public ICollection<Product> GetProductsForCategory(int categoryId)
    {
        if (categoryId <= 0)
            return new List<Product>();

        return _db.Products.Where(p => p.CategoryId == categoryId).ToList();
    }

    public bool ProductExists(int id)
    {
        if (id <= 0)
            return false;

        return _db.Products.Any(p => p.ProductId == id);
    }

    public bool ProductExists(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return _db.Products.Any(p => p.Name.ToLower().Trim() == name.ToLower().Trim());
    }

    public bool Save()
    {
        return _db.SaveChanges() >= 0;
    }

    public ICollection<Product> SearchProduct(string name)
    {
        IQueryable<Product> query = _db.Products;

        if (!string.IsNullOrEmpty(name))
            query = query.Where(p => p.Name.ToLower().Contains(name.ToLower()));

        return query.ToList();
    }

    public bool UpdateProduct(Product product)
    {
        if (product == null)
            return false;

        product.UpdatedAt = DateTime.Now;

        _db.Products.Update(product);
        return Save();
    }
}
