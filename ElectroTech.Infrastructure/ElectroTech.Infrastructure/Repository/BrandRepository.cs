using ElectroTech.Infrastructure.DbContexts;
using Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Infrastructure.Repository
{
    public class BrandRepository : IBrandRepository
    {
        private readonly ApplicationDbContext _context;

        public BrandRepository(ApplicationDbContext context)
            => _context = context;

        public async Task<List<Brand>> GetAllAsync()
        {
            return await _context.Brands
                .OrderBy(b => b.DisplayOrder)
                .ThenBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<Brand?> GetByIdAsync(int id)
        {
            return await _context.Brands
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Brand?> GetByNameAsync(string name)
        {
            return await _context.Brands
                .FirstOrDefaultAsync(b => b.Name.ToLower() == name.Trim().ToLower());
        }

        public async Task<Brand> AddAsync(Brand brand)
        {
            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task UpdateAsync(Brand brand)
        {
            _context.Brands.Update(brand);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Brand brand)
        {
            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasProductsAsync(int brandId)
        {
            return await _context.Products.AnyAsync(p => p.BrandId == brandId);
        }
    }
}
