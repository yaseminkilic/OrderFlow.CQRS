using Microsoft.EntityFrameworkCore;
using OrderFlow.CQRS.Domain.Entities;
using OrderFlow.CQRS.Domain.Interfaces;
using OrderFlow.CQRS.Infrastructure.Data;

namespace OrderFlow.CQRS.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> Test(string name, CancellationToken cancellationToken = default)
    {
        if (!_context.Products.Any())
        {
            _context.Products.AddRange(
                new Product { Id = Guid.NewGuid(), Name = "Laptop", Price = 1500 },
                new Product { Id = Guid.NewGuid(), Name = "Mouse", Price = 50 },
                new Product { Id = Guid.NewGuid(), Name = "Keyboard", Price = 100 }
            );
            await _context.SaveChangesAsync(cancellationToken);
        }

        if (!_context.Orders.Any())
        {
            var product = _context.Products.First();
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-001"
            };
            _context.Orders.Add(order);

            _context.OrderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Quantity = 2,
                UnitPrice = product.Price
            });
            await _context.SaveChangesAsync(cancellationToken);
        }
        return true;
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync(new object[] { id }, cancellationToken);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
