using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace IDFCFastTagApi.Controllers
{
    public class TestController : Controller
    {
        [HttpPost]
        public async Task<ActionResult> Echo()
        {
            Request.InputStream.Position = 0;
            string body;
            using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8))
            {
                body = await reader.ReadToEndAsync();
            }

            var responseText = string.IsNullOrEmpty(body)
                ? "No body received"
                : "Received: " + body;

            return Content(responseText, "text/plain", Encoding.UTF8);
        }
    }
}

