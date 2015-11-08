using System.Windows;
using Dragablz;

namespace TailBlazer.Infrastucture
{
    public class InterTabClient : IInterTabClient
    {
        private readonly IWindowFactory _factory;

        public InterTabClient(IWindowFactory tradeWindowFactory)
        {
            _factory = tradeWindowFactory;
        }

        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            var window = _factory.Create();

            return new NewTabHost<Window>(window, window.InitialTabablzControl);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.CloseWindowOrLayoutBranch;
        }
    }
}
