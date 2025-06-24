using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    public interface IGoalMemoryService
    {
        Task<List<string>> GetTrackedGoalsAsync();
        Task AddGoalAsync(string goal);
    }
}
