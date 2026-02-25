using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using IDFCFastTagApi.Services;
using RSuite.Infrastructure.Core.Context;
using RSuite.UserInterface.Web.Mvc.AppCode.Common.Factory;

namespace IDFCFastTagApi.Controllers
{
    public class FastagController : ApiController
    {
        private readonly IFastagService _service;

        public FastagController()
        {
            _service = EntityFactory.GetInstance<IFastagService>();
        }
        private void SetLoginContextService()
        {

            ILoginContextService _loginContextService = EntityFactory.GetInstance<ILoginContextService>();
            _loginContextService.SetLoginContext(1, "riddhi", 1, 26, 1);
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Push()
        {
            SetLoginContextService();
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
