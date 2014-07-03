using LabWorkflowManager.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Prism.Events;
using _4tecture.UI.Common.Events;

namespace LabWorkflowManager.Views
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : MetroWindow
    {
        private IEventAggregator eventAggregator;
        public Shell(ShellViewModel viewmodel, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            this.eventAggregator = eventAggregator;
            this.DataContext = viewmodel;
            this.Closing += Shell_Closing;
        }

        void Shell_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.eventAggregator.GetEvent<ApplicationClosingInterceptorEvent>().Publish(e);
        }
    }
}
