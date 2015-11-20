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

namespace OpenEdgeHPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            string response;
            string otk;
            string call;

            call = PaymentEngine.generateRequest(PaymentEngine.CallType.otk, PaymentEngine.TranType.CreditSale, amountText.Text);
            response = PaymentEngine.callGateway(call);
            otk = PaymentEngine.parseXML(response, PaymentEngine.CallType.otk);

            string hpf = "https://integrator.t3secure.net/hpf/hpf.aspx?otk=";
            hpf += otk;

            hpfBrowser.Navigate(hpf);

        }
    }
}
