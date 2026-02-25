using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using IDFCFastTagApi.Services;
using RSuite.Infrastructure.Core.Context;
using RSuite.UserInterface.Web.Mvc.AppCode.Common.Factory;

namespace IDFCFastTagApi.Controllers
{
    public class FastagController : Controller
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
        public async Task<ActionResult> Push()
        {
            SetLoginContextService();

            Request.InputStream.Position = 0;
            string rawXml;
            using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8))
            {
                rawXml = await reader.ReadToEndAsync();
            }

            var responseXml = _service.ProcessPush(rawXml);

            return Content(responseXml, "application/xml", Encoding.UTF8);
        }
    }
}
