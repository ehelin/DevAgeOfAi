using Shared.Interfaces;

namespace BLL.Ai.Agents
{
    public class BaseAgent : IAgent
    {
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
