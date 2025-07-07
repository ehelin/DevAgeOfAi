using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class HistorianState
    {
        public List<HabitPattern> HabitHistory { get; set; } = new();
    }
}
