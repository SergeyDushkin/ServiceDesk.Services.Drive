using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using servicedesk.Services.Drive.Dal;
using ImageSharp;
using ImageSharp.Formats;

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
        public async Task<IActionResult> Download(Guid id, string size)
        {
            var record = await context.Files.Include(r => r.Content).SingleOrDefaultAsync(r => r.Id == id);

            if (record == null)
            {
                return NotFound();
            }

            var content = record.Content.Content;

            if (record.ContentType == "image/jpeg")
            {
                switch (size)
                {
                    case "XXXS":
                        content = ResizeImage(content, 50);
                        break;
                    case "XXS":
                        content = ResizeImage(content, 75);
                        break;
                    case "XS":
                        content = ResizeImage(content, 100);
                        break;
                    case "S":
                        content = ResizeImage(content, 150);
                        break;
                    case "M":
                        content = ResizeImage(content, 300);
                        break;
                    case "L":
                        content = ResizeImage(content, 500);
                        break;
                    case "XL":
                        content = ResizeImage(content, 800);
                        break;
                }
            }

            return File(content, record.ContentType, record.Name);
        }

        private byte[] ResizeImage(byte[] bytes, int size)
        {
            Configuration.Default.AddImageFormat(new JpegFormat());

            var options = new ImageSharp.Processing.ResizeOptions {
                Mode = ImageSharp.Processing.ResizeMode.Max,
                Size = new Size(size, size)
            };

            using(var stream = new MemoryStream())
            using (var image = new Image(bytes))
            {
                image.Resize(options).Save(stream);
                return stream.ToArray();
            }
        }
        
        [HttpPost] // Authorize
        public async Task<IActionResult> Post(string resource, Guid referenceId, ICollection<IFormFile> file)
        {
            foreach(var f in file)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await f.CopyToAsync(memoryStream);

                    var create = new Domain.File
                    {
                        Resource = resource,
                        ReferenceId = referenceId,
                        Name  = f.FileName,
                        FileType  = f.ContentType,
                        ContentType = f.ContentType,
                        Size = f.Length,
                        Content = new Domain.FileContent(memoryStream.ToArray())
                    };
                    
                    context.Add(create);
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