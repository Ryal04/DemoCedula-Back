using Amazon.Textract;
using Amazon.Textract.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DemoTextract_Back.Controllers
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
        public async Task<IActionResult> Text([FromForm] IFormFile CedDorsal)
        {
            var cont = 0;
            var list = new List<String>();
            Document ImageTarget = new Document();

            //Convercion Imagen a Bytes
            using (var ms = new MemoryStream())
            {
                CedDorsal.CopyTo(ms);
                ImageTarget.Bytes = new MemoryStream(ms.ToArray());
            }

            var response = await amazonTextract.DetectDocumentTextAsync(new DetectDocumentTextRequest()
            {
                Document = ImageTarget,
            });

            foreach (var data in response.Blocks)
            {

                if (data.BlockType.Value == "LINE" && data.Confidence >= 85)
                {
                    cont++;

                    if (data.Text != "FECHA DE NACIMIENTO" && cont == 1)
                    {
                        list.Add(data.Text);
                    }

                    if (data.Text != "LUGAR DE NACIMIENTO"
                    && data.Text != "SEXO"
                    && data.Text != "G.S. RH"
                    && data.Text != "ESTATURA"
                    && cont > 1 && cont < 13)
                    {
                        list.Add(data.Text);
                    }
                }
            }

            //Split
            string[] fechanacimiento = list[0].Split(' ');
            if (fechanacimiento.Length > 1)
            {
                list[0] = fechanacimiento[3];
            }

            //Split
            string[] DepartamentoNacimiento = list[2].Split('(', ')');
            if (DepartamentoNacimiento.Length > 1)
            {
                list[2] = DepartamentoNacimiento[1];
            }

            //Split
            string[] fechaylugarexp = list[6].Split(' ');
            if (fechaylugarexp.Length > 1)
            {
                list[6] = fechaylugarexp[1];

                if (list.Count > 7)
                {
                    list[7] = fechaylugarexp[0];
                }
                else
                {
                    list.Add(fechaylugarexp[0]);
                }
            }

            return Ok(list);
        }
    }
}

