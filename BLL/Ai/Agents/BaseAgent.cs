using Shared.Interfaces;

namespace BLL.Ai.Agents
{
    public class BaseAgent : IAgent
    {
        protected const string DataFile = "data.json";
        protected const string SuggestionsFile = "suggestedHabits.json";

        protected System.Threading.Timer _timer;
        protected bool isRunning = false;

        public virtual void Start()
        {
            throw new NotImplementedException();
        }

        public virtual void Stop()
        {
            throw new NotImplementedException();
        }

        public virtual void AgentLoop()
        {
            throw new NotImplementedException();
        }
    }
}
