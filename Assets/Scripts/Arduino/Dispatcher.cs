
using System.Collections.Generic;
using System;

namespace Arduino
{

    public class Dispatcher
    {
        private List<Action> _pending_tasks = new List<Action>();
            
        public Dispatcher()
        {
        }

        public void Dispatch(Action action)
        {
            lock (_pending_tasks)
            {
                _pending_tasks.Add(action);
            }
        }
        
        public void Process()
        {
            List<Action> busy_tasks;
            lock (_pending_tasks)
            {
                busy_tasks = _pending_tasks;
                _pending_tasks = new List<Action>();
            }
            foreach (Action task in busy_tasks)
            {
                task();
            }
        }
    }
    
}