using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace RestaurantAPI.Controllers
{
    [Route("file")]
    [AllowAnonymous]
    public class FileControler : ControllerBase
    {
        [HttpGet]
        [ResponseCache(Duration = 1200, VaryByQueryKeys = ["fileName"])]
        public ActionResult GetFile([FromQuery] string fileName)
        {
            var rootPath = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(rootPath, "PrivateFiles", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var contentProvider = new FileExtensionContentTypeProvider();
            contentProvider.TryGetContentType(fileName, out string? contentType);

            var fileContents = System.IO.File.ReadAllBytes(filePath);
            return File(fileContents, contentType ?? "application/octet-stream", fileName);
        }

        [HttpPost]
        public ActionResult Upload([FromForm] IFormFile file)
        {
            if (file == null || file.Length < 1)
            {
                return BadRequest();
            }

            var rootPath = Directory.GetCurrentDirectory();
            var fileName = file.FileName;
            var filePath = Path.Combine(rootPath, "PrivateFiles", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return Ok();
        }
    }
}
