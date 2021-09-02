using Amazon.Textract;
using Amazon.Textract.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DemoTextract.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TextController : ControllerBase
    {
        private readonly IAmazonTextract amazonTextract;

        public TextController(IAmazonTextract amazonTextract)
        {
            this.amazonTextract = amazonTextract;
        }

        [HttpPost]
        public async Task<IActionResult> Text([FromForm] IFormFile CedFrontal)
        {
            //Convercion Imagen
            Document ImageTarget = new Document();
            using (var ms = new MemoryStream())
            {
                CedFrontal.CopyTo(ms);
                ImageTarget.Bytes = new MemoryStream(ms.ToArray());
                // act on the Base64 data
            }

            var response = await amazonTextract.DetectDocumentTextAsync(new DetectDocumentTextRequest(){ 
                        Document = ImageTarget,
            });

            Console.WriteLine("________________");
            foreach (var data in response.Blocks) {

                if (data.BlockType.Value == "LINE" && data.Confidence >= 85)
                {
                    Console.WriteLine(data.Text);
                }
            }
            
            return Ok(response.Blocks);
        }
    }
}
