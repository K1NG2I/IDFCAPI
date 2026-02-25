using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using IDFCFastTagApi.Services;

namespace IDFCFastTagApi.Controllers
{
    public class FastagController : ApiController
    {
        private readonly IFastagService _service;

        public FastagController()
        {
            _service = new FastagService();
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Push()
        {
            var rawXml = await Request.Content.ReadAsStringAsync();
            var responseXml = _service.ProcessPush(rawXml);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseXml, Encoding.UTF8, "application/xml")
            };

            return response;
        }
    }
}
