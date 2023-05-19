using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ShareBucket.DataAccessLayer.Data;
using ShareBucket.DataAccessLayer.Models.Entities;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;

namespace UserMicroService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MetadataController : Controller
    {
        private DataContext _context;

        public MetadataController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.MemoryAreas
                );
        }
        [HttpPost]
        public IActionResult AddOne()
        {
            // add new metadata of mock
            MemoryArea memoryArea = new MemoryArea();
            memoryArea.CreationDate = DateTime.Now;
            memoryArea.EncryptionKey = new byte[64];
            //memoryArea.EncryptionKey = new byte[] {
            //    0x2b, 0x7e, 0x15, 0x16, 0x28, 0xae, 0xd2, 0xa6,
            //    0xab, 0xf7, 0x15, 0x88, 0x09, 0xcf, 0x4f, 0x3c,
            //    0x7c, 0xbf, 0x3f, 0x4c, 0xef, 0x8c, 0xd2, 0x2f,
            //    0x45, 0x7f, 0x1f, 0x7e, 0x67, 0x76, 0x88, 0x6f
            //};
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(memoryArea.EncryptionKey);
            }

            _context.MemoryAreas.Add(memoryArea);

            _context.SaveChanges();


            // Empty Encryption array to avoid it having password saved in memory, even if GC is not immediatly called
            //Array.Clear(memoryArea.EncryptionKey, 0, memoryArea.EncryptionKey.Length);
            //memoryArea.EncryptionKey = null;

            return Ok(memoryArea);
        }
    }
}
