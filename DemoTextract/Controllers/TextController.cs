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
            var cont = 0;
            var list = new List<String>();
            Document ImageTarget = new Document();

            //Convercion Imagen a Bytes
            using (var ms = new MemoryStream())
            {
                CedFrontal.CopyTo(ms);
                ImageTarget.Bytes = new MemoryStream(ms.ToArray()); 
            }

            var response = await amazonTextract.DetectDocumentTextAsync(new DetectDocumentTextRequest(){ 
                        Document = ImageTarget,
            });

 
            foreach (var data in response.Blocks) {

                if (data.BlockType.Value == "LINE" && data.Confidence >= 85)
                {
                    cont++;

                    if (cont > 2 && cont != 4) 
                    {
                        if (data.Text != "NOMBRES" && data.Text != "APELLIDOS" && data.Text != "FIRMA" && data.Text != "NUMERO")
                        {
                            list.Add(data.Text);
                        }   
                    }

                    if (cont == 4)
                    {
                        if (data.Text != "NÚMERO" && data.Text != "NUMERO")
                        {
                            string[] split = data.Text.Split(' ');

                            if (split.Length != 1)
                            {
                                list.Add(split[1]);
                            }
                            else 
                            {
                                list.Add(data.Text);
                            }
                        }
                    }
                }
            }

            return Ok(list);
        }
    }
}
