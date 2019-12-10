using System.Windows;

namespace TreeViewScrollingIssue
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = new WindowViewModel();
    }
  }
}
