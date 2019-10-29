using System;
namespace LongTaskApiExample.Data
{
    public class Message
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public DateTime UpdateTime { get; set; }

        public string Status { get; set; } // Enqueue, Processing, Completed, Canceled,
        public float ProcessRate { get; set; }

        public int TotalCount { get; set; }
        public int ProcessCount { get; set; }

    }


    public class CancelJob
    {
        public string Id { get; set; }
    }
    
}
