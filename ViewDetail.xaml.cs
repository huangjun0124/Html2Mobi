using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KindleGenCaller
{
    /// <summary>
    /// Interaction logic for ViewDetail.xaml
    /// </summary>
    public partial class ViewDetail : Window
    {
        public string Text
        {
            set { textBox.Text = value; }
        }

        public ViewDetail()
        {
            InitializeComponent();
        }
    }
}
