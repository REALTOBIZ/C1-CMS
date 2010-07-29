using System;
using System.Collections.Generic;
using Composite.ConsoleEventSystem.Foundation;
using Composite.GlobalSettings;


namespace Composite.ConsoleEventSystem
{
    /// <summary>    
    /// </summary>
    /// <exclude />
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
    public static class ConsoleMessageQueueFacade
    {
        private static ConsoleMessageQueue _messageQeueue = new ConsoleMessageQueue(GlobalSettingsFacade.ConsoleMessageQueueItemSecondToLive);

        // Dont add flush / initialize to this class. We do not want console messages to get lost.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consoleMessageQueueItem"></param>
        /// <param name="receiverConsoleId">null or empty string is a broardcast</param>
        public static void Enqueue(IConsoleMessageQueueItem consoleMessageQueueItem, string receiverConsoleId)
        {
            if (consoleMessageQueueItem == null) throw new ArgumentNullException("consoleMessageQueueItem");

            _messageQeueue.Enqueue(consoleMessageQueueItem, receiverConsoleId);
        }



        public static IEnumerable<ConsoleMessageQueueElement> GetQueueElements(int currentConsoleCounter, string consoleId)
        {
            return _messageQeueue.GetQueueElements(currentConsoleCounter, consoleId);
        }


        public static int GetLatestMessageNumber(string consoleId)
        {
            return _messageQeueue.GetLatestMessageNumber(consoleId);
        }


        public static int CurrentChangeNumber
        {
            get
            {
                return _messageQeueue.CurrentQueueItemNumber;
            }
        }


        public static void DoDebugSerializationToFileSystem()
        {
            _messageQeueue.DoDebugSerializationToFileSystem();
        }
    }
}
