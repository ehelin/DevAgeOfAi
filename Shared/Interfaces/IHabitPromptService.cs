using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    public interface IHabitPromptService
    {
        string BuildHabitPrompt(string userPrompt, int numberOfHabits = 5);
    }
}
