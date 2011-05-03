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
using System.Text;
using System.Windows.Threading;

namespace VisualBF {
    public partial class MainPage : UserControl {

        private static readonly char[] BFOPS = { '>', '<', '+', '-', '.', ',', '[', ']', };
        private string source;
        private int dPtr;
        private int iPtr;
        private List<byte> data;
        private List<BoxControl> dataBoxes, sourceBoxes;
        private Stack<int> loopIndexes;

        DispatcherTimer timer;

        public MainPage() {
            InitializeComponent();
            dPtr = 0;
            iPtr = 0;
            data = Enumerable.Repeat<byte>(0, 100).ToList();
            source = "";
            dataBoxes = new List<BoxControl>();
            sourceBoxes = new List<BoxControl>();
            loopIndexes = new Stack<int>();
            timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(.05) };
            timer.Tick += new EventHandler(timer_Tick);
            InitializeUI();
        }

        private void InitializeUI() {
            for (int i = 0; i < data.Count; i++) {
                var bf = GetNewDataBox(i);
                dataWrapPanel.Children.Add(bf);
                dataBoxes.Add(bf);
            }
            dataBoxes.First().Data.IsActive = true;
        }

        private BoxControl GetNewDataBox(int i) {
            var bf = new BoxControl(i, data[0].ToString(), false, true) { Width = 35, Height = 35 };
            return bf;
        }

        private void AddSourceHyperlinkButton_Click(object sender, RoutedEventArgs e) {
            Prompt p = new Prompt("Enter BF Source.");
            EventHandler f = null;
            f = (s, ea) => {
                p.Closed -= f;
                if (p.DialogResult ?? false)
                    AddSource(p.Response);
            };
            p.Closed += f;
            p.Show();
        }

        private void AddSource(string src) {
            StringBuilder sb = new StringBuilder();
            foreach (char c in src)
                if (BFOPS.Contains(c))
                    sb.Append(c);
            source = sb.ToString();
            for (int i = 0; i < source.Length; i++) {
                var bf = new BoxControl(i, source[i].ToString(), false, false) { Height = 30, Width = 30, ShowIndex = false };
                sourceWrapPanel.Children.Add(bf);
                sourceBoxes.Add(bf);
            }
            sourceBoxes.First().Data.IsActive = true;
            executeHyperlinkButton.Visibility = System.Windows.Visibility.Visible;
        }

        private void ExecuteHyperlinkButton_Click(object sender, RoutedEventArgs e) {
            dPtr = 0;
            iPtr = 0;
            TryExecuteInstruction();
            timer.Start();
        }

        private void ExecuteNonVisiblyHyperlinkButton_Click(object sender, RoutedEventArgs e) {
            //dPtr = 0;
            //iPtr = 0;
            //TryExecuteInstruction();
            //timer.Start();
        }

        void timer_Tick(object sender, EventArgs e) {
            TryExecuteInstruction();
        }

        private void TryExecuteInstruction() {
            if (iPtr >= source.Length) {
                timer.Stop();
                var bf = new BoxControl(iPtr, "END", false, false) { Height = 30, Width = 30, ShowIndex = false };
                sourceWrapPanel.Children.Add(bf);
            }
            else
                ExecuteInstruction();
            UpdateUI();
        }

        private void UpdateUI() {
            for (int i = 0; i < data.Count; i++) {
                if (i >= dataBoxes.Count)
                    dataBoxes.Add(GetNewDataBox(i));
                dataBoxes[i].Data.Value = data[i].ToString();
                dataBoxes[i].Data.IsActive = i == dPtr;
            }
            for (int i = 0; i < source.Length; i++) {
                sourceBoxes[i].Data.IsActive = i == iPtr;
            }
            //UpdateLayout();
        }

        private void ExecuteInstruction() {
            switch (source[iPtr]) {
                case '>':
                    ++dPtr;
                    break;
                case '<':
                    --dPtr;
                    break;
                case '+':
                    ++data[dPtr];
                    break;
                case '-':
                    --data[dPtr];
                    break;
                case '.':
                    outputTextBlock.Text += ((char)data[dPtr]).ToString();
                    break;
                case ',':
                    var input = RequestInput();
                    break;
                case '[':
                    loopIndexes.Push(iPtr);
                    int endLoopIndex = source.FindBalancingBrace(iPtr);
                    if (data[dPtr] == 0)
                        iPtr = endLoopIndex;
                    break;
                case ']':
                    if (data[dPtr] != 0)
                        iPtr = loopIndexes.Peek();
                    else loopIndexes.Pop();
                    break;
            }
            ++iPtr;
        }

        private string RequestInput() {
            throw new NotImplementedException();
        }
    }
}
