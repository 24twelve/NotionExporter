using Microsoft.AspNetCore.Mvc;

namespace NotionExporterWebApi.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class PingController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Ok";
        }
    }
}