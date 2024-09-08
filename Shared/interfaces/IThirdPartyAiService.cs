using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.interfaces
{
    public interface IThirdPartyAiService
    {
        string GetSuggestion(string prompt);
    }
}
