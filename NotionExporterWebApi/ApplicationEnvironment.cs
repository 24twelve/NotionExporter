using System.Runtime.Serialization;

namespace NotionExporterWebApi
{
    public enum ApplicationEnvironment
    {
        [EnumMember(Value = "development")]
        Development,

        [EnumMember(Value = "production-linux")]
        ProductionLinux
    }
}