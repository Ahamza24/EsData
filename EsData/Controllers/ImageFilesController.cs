using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EsData.Models;
using EsData.Data;

namespace EsData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageFilesController : ControllerBase
    {
        private readonly EsDataContext _context;

        public ImageFilesController(EsDataContext context)
        {
            _context = context;
        }

        // GET: api/ImageFiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImageFile>>> GetImageFile()
        {
            return await _context.ImageFile.ToListAsync();
        }

        // GET: api/ImageFiles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ImageFile>> GetImageFile(int id)
        {
            var imageFile = await _context.ImageFile.FindAsync(id);

            if (imageFile == null)
            {
                return NotFound();
            }

            return imageFile;
        }

        // PUT: api/ImageFiles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutImageFile(int id, ImageFile imageFile)
        {
            if (id != imageFile.Id)
            {
                return BadRequest();
            }

            _context.Entry(imageFile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImageFileExists(id))
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

        // POST: api/ImageFiles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ImageFile>> PostImageFile(ImageFile imageFile)
        {
            _context.ImageFile.Add(imageFile);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetImageFile", new { id = imageFile.Id }, imageFile);
        }

        // DELETE: api/ImageFiles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImageFile(int id)
        {
            var imageFile = await _context.ImageFile.FindAsync(id);
            if (imageFile == null)
            {
                return NotFound();
            }

            _context.ImageFile.Remove(imageFile);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ImageFileExists(int id)
        {
            return _context.ImageFile.Any(e => e.Id == id);
        }
    }
}
