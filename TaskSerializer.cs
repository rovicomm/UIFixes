﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace UIFixes
{
    public class TaskSerializer<T> : MonoBehaviour
    {
        private Func<T, Task> func;
        private Queue<T> items;
        private Task currentTask;
        private TaskCompletionSource totalTask;

        public Task Initialize(IEnumerable<T> items, Func<T, Task> func)
        {
            this.items = new(items);
            this.func = func;

            currentTask = Task.CompletedTask;
            totalTask = new TaskCompletionSource();

            Update();

            return totalTask.Task;
        }

        public void Update()
        {
            if (!currentTask.IsCompleted)
            {
                return;
            }

            if (items.Any())
            {
                currentTask = func(items.Dequeue());
            }
            else
            {
                totalTask.Complete();
                func = null;
                Destroy(this);
            }
        }
    }
}
