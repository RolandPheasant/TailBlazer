using DynamicData.Binding;
using Microsoft.Expression.Interactivity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TailBlazer.Views.Dialog
{
    //This class is responsible for displaying custom dialog windows
    public class DialogViewModel : AbstractNotifyPropertyChanged, IDisposable
    {
        public Guid Id { get; } = Guid.NewGuid();
        //The text of the dialog window
        public String text { get; set; }
        public ICommand ButtonClickEvent { get; set; }
        public bool Button { get; set; } 

        public DialogViewModel()
        {
            //This can be triggered from the DialogView.xaml
            ButtonClickEvent = new ActionCommand(o =>
            {
                var content = o as string;

                //Here we can test the content of the pushed button
                if (!string.IsNullOrEmpty(content))
                {
                    if (content == "Yes")
                    {
                        Button = true;
                    }
                    else if (content == "No")
                    {
                        Button = false;
                    }
                }
            });
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
