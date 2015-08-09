using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLinq.Util
{
    internal static class TaskExtensions
    {
        public static T AssumeCompleted<T>(this Task<T> task)
        {
            if (task.Exception != null) throw task.Exception;
            if (task.IsCanceled) throw new TaskCanceledException(task);
            if (task.Status != TaskStatus.RanToCompletion) throw new InvalidOperationException();
            return task.Result;
        }
        
        public static void AssumeCompleted(this Task task)
        {
            if (task.Exception != null) throw task.Exception;
            if (task.IsCanceled) throw new TaskCanceledException(task);
            if (task.Status != TaskStatus.RanToCompletion) throw new InvalidOperationException();
        }
    }
}
