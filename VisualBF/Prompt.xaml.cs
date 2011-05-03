using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace VisualBF {
    public partial class Prompt : ChildWindow {
        private bool firstFocus = true;

        public string Response { get { return responseTextBox.Text; } }

        public Prompt(string prompt, string presetResponse = null) {
            InitializeComponent();
            promptTextBlock.Text = prompt;
            responseTextBox.Text = presetResponse ?? string.Empty;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
        }

        private void ChildWindow_Loaded(object sender, RoutedEventArgs e) {
            this.GotFocus += new RoutedEventHandler(ChildWindow_GotFocus);
        }

        private void ChildWindow_GotFocus(object sender, RoutedEventArgs e) {
            if (firstFocus) {
                this.GotFocus -= ChildWindow_GotFocus;
                firstFocus = false;
                responseTextBox.Focus();
            }
        }
    }
}

