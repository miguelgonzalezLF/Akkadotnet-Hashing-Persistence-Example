using System;
using Akka.Actor;
using IEP.Shared.Commands;
using IEP.Shared.State;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using StructureMap;

namespace IEP.Host.Services
{
    [Route("/FileChange", "POST")]
    public class FileChange: IReturnVoid
    {
        public string FileName { get; set; }
        public string Change { get; set; }
    }

    public class PublisherService: Service
    {
        public void Post(FileChange request)
        {
            var commandExecutor = ObjectFactory.GetNamedInstance<IActorRef>("commands");
            NextColor = 1;
            for (int i = 0; i < 6; i++)
            {
                var color = GetNextConsoleColor();
                for (int j = 0; j < 5; j++)
                {
                    commandExecutor.Tell(new StartJob(new ChangeJob(request.FileName + i, request.Change + j, color)));        
                }
            }
            
        }

        private static int NextColor = 1;
        private static ConsoleColor GetNextConsoleColor()
        {
            var consoleColors = Enum.GetValues(typeof(ConsoleColor));
            return (ConsoleColor)consoleColors.GetValue(NextColor++);
        }
    }
}