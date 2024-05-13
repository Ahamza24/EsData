using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EsData.Data;
using EsData.Models;
using Microsoft.AspNetCore.Authorization;

namespace EsData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly EsDataContext _context;

        public BrandsController(EsDataContext context)
        {
            _context = context;
        }

        // GET: api/Brands
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Brands>>> GetBrands(string searchExpression = null)
        {
            Expression<Func<Brands, bool>> lambdaExpression = null;

            if (!string.IsNullOrWhiteSpace(searchExpression))
            {
                lambdaExpression = DynamicExpressionParser.ParseLambda<Brands, bool>(new ParsingConfig(), true, searchExpression);
            }

            var queryableBrands =
                this._context
                    .Brands
                    .AsQueryable();

            if (lambdaExpression != null)
            {
                queryableBrands = queryableBrands.Where(lambdaExpression);
            }
            var list = await queryableBrands.ToListAsync();

            return list;
        }

        [HttpGet("GetBrandsWithRelatedData")]
        public async Task<ActionResult<IEnumerable<Brands>>> GetMoviesWithRelatedData(string searchExpression = null)
        {
            Expression<Func<Brands, bool>> lambdaExpression = null;

            if (!string.IsNullOrWhiteSpace(searchExpression))
            {
                lambdaExpression = DynamicExpressionParser.ParseLambda<Brands, bool>(new ParsingConfig(), true, searchExpression);
            }

            var queryableBrands =
                this._context
                    .Brands
                    .Include(x => x.ImageFile)
                    .AsNoTracking()
                    .AsQueryable();

            if (lambdaExpression != null)
            {
                queryableBrands = queryableBrands.Where(lambdaExpression);
            }

            var list = await queryableBrands.ToListAsync();

            return list;
        }


        // GET: api/Brands/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Brands>> GetBrands(int id)
        {
            var brands = await _context.Brands.FindAsync(id);

            if (brands == null)
            {
                return NotFound();
            }

            return brands;
        }

        // PUT: api/Brands/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Worker)]
        public async Task<IActionResult> PutBrands(int id, Brands brands)
        {
            if (id != brands.ID)
            {
                return BadRequest();
            }

            // get the movie as it currently exists in the database, 
            // we'll be updating this with the one posted to the method
            var existingBrand =
                await _context.Brands
                    .SingleOrDefaultAsync();

            if (existingBrand != null)
            {
                // update the existing Movie object
                _context.Entry(existingBrand)
                    .CurrentValues
                    .SetValues(brands);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BrandsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Brands
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = Roles.Admin + "," + Roles.Worker)]
        public async Task<ActionResult<Brands>> PostBrands(Brands brands)
        {
            _context.Brands.Add(brands);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBrands", new { id = brands.ID }, brands);
        }

        // DELETE: api/Brands/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Worker)]
        public async Task<IActionResult> DeleteBrands(int id)
        {
            var brands = await _context.Brands.FindAsync(id);
            if (brands == null)
            {
                return NotFound();
            }

            _context.Brands.Remove(brands);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BrandsExists(int id)
        {
            return _context.Brands.Any(e => e.ID == id);
        }
    }
}
