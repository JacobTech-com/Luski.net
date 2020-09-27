using System;

namespace Luski.net
{
    public class Exceptions
    {

        [Serializable]
        public class MissingEventException : Exception
        {
            public string EventName;
            public MissingEventException(string Event) : base(Event)
            {
                EventName = Event;
            }
        }


        [Serializable]
        public class NotConnectedException : Exception
        {
            public NotConnectedException(object sender, string message) : base(message)
            {
                Sender = sender;
            }

            public object Sender { get; }
        }
    }
}
