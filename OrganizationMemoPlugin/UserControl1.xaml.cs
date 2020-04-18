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

namespace OrganizationMemoPlugin
{
    /// <summary>
    /// UserControl1.xaml の相互作用ロジック
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        private TextBlock fleetBlock;

        public UserControl1()
        {
            InitializeComponent();
            FirstFleetBlock.Tag = FirstFleetBox;
            FirstFleetBox.Tag = FirstFleetBlock;
        }

        private void EditOpen(TextBlock block, TextBox box)
        {
            box.Text = block.Text;
            block.Visibility = Visibility.Collapsed;
            box.Visibility = Visibility.Visible;
            Dispatcher.InvokeAsync(() => box.Focus());
        }

        private void EditClose(TextBlock block, TextBox box)
        {
            block.Visibility = Visibility.Visible;
            box.Visibility = Visibility.Collapsed;
        }

        private void ChangeText(TextBlock block, String text)
        {
            var view = DataContext as OrganizationViewModel;
            view.ChangeFleetName(text, null);
            var be = FirstFleetRun.GetBindingExpression(Run.TextProperty);
            be.UpdateTarget();
            FleetList.Items.Refresh();
        }

        private void ContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var content = sender as ContentControl;
            fleetBlock = content.Content as TextBlock;
            var fleetBox = fleetBlock.Tag as TextBox;

            EditOpen(fleetBlock, fleetBox);
        }

        private void FirstFleetBox_KeyDown(object sender, KeyEventArgs e)
        {
            var box = sender as TextBox;
            var block = box.Tag as TextBlock;

            if(e.Key == Key.Escape) { EditClose(block, box); }
            if (e.Key == Key.Enter)
            {
                ChangeText(block, box.Text);
                EditClose(block, box);
            }            
        }

        private void FirstFleetBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var box = sender as TextBox;
            var block = box.Tag as TextBlock;

            EditClose(block, box);
        }

        private void CallMethodButton_Click(object sender, RoutedEventArgs e)
        {
            FleetInfo.Visibility = Visibility.Collapsed;
            FleetOrder.Visibility = Visibility.Visible;
        }

        private void CallMethodButton_Click_1(object sender, RoutedEventArgs e)
        {
            FleetInfo.Visibility = Visibility.Visible;
            FleetOrder.Visibility = Visibility.Collapsed;
        }
    }
}
