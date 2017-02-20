using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using servicedesk.Services.Drive.Dal;

namespace servicedesk.Services.Drive.Controllers
{
    [Route("[controller]")]
    public class FilesController : Controller
    {
        private readonly DriveDbContext context;

        public FilesController(DriveDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string resource, Guid referenceId)
        {
            var query = await context.Files.Where(r => r.Resource == resource && r.ReferenceId == referenceId).ToListAsync();
            return Ok(query);
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await context.Files.SingleOrDefaultAsync(r => r.Id == id);

            if (record == null)
            {
                return NotFound();
            }

            return Ok(record);
        }

        [Route("{id}/download")]
        [HttpGet]
        public async Task<IActionResult> Download(Guid id)
        {
            var record = await context.Files.Include(r => r.Content).SingleOrDefaultAsync(r => r.Id == id);

            if (record == null)
            {
                return NotFound();
            }

            return File(record.Content.Content, record.ContentType, record.Name);
        }
        
        [HttpPost] // Authorize
        public async Task<IActionResult> Post(string resource, Guid referenceId, ICollection<IFormFile> files)
        {
            foreach(var f in files)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await f.CopyToAsync(memoryStream);

                    var file = new Domain.File
                    {
                        Resource = resource,
                        ReferenceId = referenceId,
                        Name  = f.FileName,
                        FileType  = f.ContentType,
                        ContentType = f.ContentType,
                        Size = f.Length,
                        Content = new Domain.FileContent(memoryStream.ToArray())
                    };
                    
                    context.Add(file);
                    await context.SaveChangesAsync();
                }
            }
            
            return Accepted();
        }

        [Route("{id}")]
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await context.Files.SingleOrDefaultAsync(r => r.Id == id);

            if (deleted == null)
            {
                return NotFound();
            }

            context.Remove(deleted);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}