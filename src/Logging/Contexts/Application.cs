using Appalachia.Utility.Logging.Contexts.Base;

namespace Appalachia.Utility.Logging.Contexts
{
    public class Application : AppaLogContext<Application>
    {
        protected override AppaLogFormats.LogFormat GetPrefixFormat()
        {
            return formats.contexts.application;
        }
    }
}