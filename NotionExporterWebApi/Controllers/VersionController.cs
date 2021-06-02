using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace NotionExporterWebApi.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var attr = (AssemblyInformationalVersionAttribute) assembly.GetCustomAttribute(
                typeof(AssemblyInformationalVersionAttribute))!;
            return attr.InformationalVersion;
        }
        
    }
}