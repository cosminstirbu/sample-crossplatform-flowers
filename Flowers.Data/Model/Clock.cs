using System;
namespace Flowers.Model
{
    public interface IClock 
    {
        DateTime Now();
    }

    public class Clock : IClock
    {
        public DateTime Now()
        {
            return DateTime.Now;
        }
    }
}
