using System;
using System.Collections.Generic;

namespace ETModel
{

    public class ObjectPool
    {
        private readonly Dictionary<Type, Queue<Object>> dictionary = new Dictionary<Type, Queue<Object>>();

        public Object Fetch(Type type)
        {
	        Queue<Object> queue;
            if (!this.dictionary.TryGetValue(type, out queue))
            {
                queue = new Queue<Object>();
                this.dictionary.Add(type, queue);
            }
	        Object obj;
			if (queue.Count > 0)
            {
				obj = queue.Dequeue();
            }
			else
			{
				obj = (Object)Activator.CreateInstance(type);	
			}
	        obj.IsDisposed = false;
            return obj;
        }

        public T Fetch<T>() where T: Object
		{
            T t = (T) this.Fetch(typeof(T));
			return t;
		}
        
        public void Recycle(Object obj)
        {
            Type type = obj.GetType();
	        Queue<Object> queue;
            if (!this.dictionary.TryGetValue(type, out queue))
            {
                queue = new Queue<Object>();
				this.dictionary.Add(type, queue);
            }
	        obj.IsDisposed = true;
            queue.Enqueue(obj);
        }

	    public void Close()
	    {
			this.dictionary.Clear();
	    }
    }
}