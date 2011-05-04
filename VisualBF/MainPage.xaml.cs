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
using System.ComponentModel;

namespace VisualBF {
    public partial class MainPage : UserControl {

        private static readonly char[] BFOPS = { '>', '<', '+', '-', '.', ',', '[', ']', };
        private string source;
        private int dPtr;
        private int iPtr;
        private List<byte> data;
        private List<BoxControl> dataBoxes, sourceBoxes;
        private Stack<int> loopIndexes;
        private bool isRunning = false;
        private List<Button> opButtons;

        private double delay { get { return Math.Round(delaySlider.Value, 1); } }

        DispatcherTimer timer;

        public MainPage() {
            InitializeComponent();
            dPtr = 0;
            iPtr = 0;
            data = Enumerable.Repeat<byte>(0, 100).ToList();
            //source = "";
            dataBoxes = new List<BoxControl>();
            sourceBoxes = new List<BoxControl>();
            loopIndexes = new Stack<int>();
            timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(.05) };
            timer.Tick += new EventHandler(timer_Tick);
            opButtons = opStackPanel.Children.OfType<Button>().ToList();
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

        private BoxControl GetNewSourceBox(int i, string val) {
            var bf = new BoxControl(i, val, false, false) { Height = 30, Width = 30, ShowIndex = false };
            return bf;
        }

        private void AddSourceFromClipboard_Click(object sender, RoutedEventArgs e) {
            if (Clipboard.ContainsText()) {
                AddSourceText(Clipboard.GetText());
            }
        }

        private void EnterSourceInPrompt_Click(object sender, RoutedEventArgs e) {
            Prompt p = new Prompt("Enter BF Source.");
            EventHandler f = null;
            f = (s, ea) => {
                p.Closed -= f;
                if (p.DialogResult ?? false)
                    AddSourceText(p.Response);
            };
            p.Closed += f;
            p.Show();
        }

        private void AddSourceText(string src) {
            Queue<char> queue = new Queue<char>();
            foreach (char c in src)
                if (BFOPS.Contains(c))
                    queue.Enqueue(c);
            //source = sb.ToString();
            int i = sourceBoxes.Count;
            //for (; i < source.Length; i++) {
            while (queue.Any()) {
                var bf = GetNewSourceBox(i++, queue.Dequeue().ToString());
                sourceWrapPanel.Children.Add(bf);
                sourceBoxes.Add(bf);
            }
            source = sourceBoxes.Select(x => x.Data.Value).Aggregate((w, n) => w + n);
            UpdateUI();
        }

        private void ClearSource_Click(object sender, RoutedEventArgs e) {
            ClearSource();
        }

        private void ClearSource() {
            timer.Stop();
            isRunning = false;
            source = "";
            sourceBoxes.Clear();
            sourceWrapPanel.Children.Clear();
            Reset();
        }

        private void Reset() {
            iPtr = 0;
            dPtr = 0;
            for (int i = 0; i < data.Count; i++)
                data[i] = 0;
            outputTextBlock.Text = "";
            UpdateUI();
        }

        void timer_Tick(object sender, EventArgs e) {
            TryExecuteInstruction();
        }

        private bool TryExecuteInstruction() {
            if (iPtr >= sourceBoxes.Count) {
                timer.Stop();
                isRunning = false;
                UpdateUI();
                return false;
            }
            ExecuteInstruction();
            if (delay > 0)
                UpdateUI();
            return true;
        }

        private void UpdateUI() {
            for (int i = 0; i < data.Count; i++) {
                if (i >= dataBoxes.Count)
                    dataBoxes.Add(GetNewDataBox(i));
                dataBoxes[i].Data.Value = data[i].ToString();
                dataBoxes[i].Data.IsActive = i == dPtr;
            }
            for (int i = 0; i < sourceBoxes.Count; i++) {
                sourceBoxes[i].Data.IsActive = i == iPtr;
            }
            opButtons.ForEach(x => x.IsEnabled = iPtr >= sourceBoxes.Count);
            sourceContextMenu.IsEnabled = !isRunning;
            playButton.IsEnabled = !isRunning;
            stepButton.IsEnabled = !isRunning;
            delaySlider.IsEnabled = !isRunning;
            inputTextBlock.IsReadOnly = isRunning;
            pauseButton.IsEnabled = isRunning;
        }

        private void ExecuteInstruction() {

            switch (sourceBoxes[iPtr].Data.Value[0]) {
                case '>':
                    ++dPtr;
                    break;
                case '<':
                    --dPtr;
                    if (dPtr < 0) {
                        SetErrorCondition("Can't move pointer left of 0");
                        return;
                    }
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
                    data[dPtr] = GetInput();
                    break;
                case '[':
                    loopIndexes.Push(iPtr);
                    int endLoopIndex = source.FindBalancingBrace(iPtr);
                    if (data[dPtr] == 0) {
                        if (endLoopIndex == -1) {
                            SetErrorCondition("Syntax error: no matching close for start of loop.");
                            return;
                        }
                        else
                            iPtr = endLoopIndex;
                    }
                    break;
                case ']':
                    if (!loopIndexes.Any()) {
                        SetErrorCondition("Syntax error: no matching start for end of loop.");
                        return;
                    }
                    if (data[dPtr] != 0)
                        iPtr = loopIndexes.Peek();
                    else loopIndexes.Pop();
                    break;
            }
            ++iPtr;
        }

        private void SetErrorCondition(string error) {
            MessageBox.Show(error);
            ClearSource();
        }

        private byte GetInput() {
            if (inputTextBlock.Text == string.Empty)
                return 0;
            char c = inputTextBlock.Text[0];
            if (c == (char)13)
                c = (char)10;
            inputTextBlock.Text = inputTextBlock.Text.Substring(1);
            return (byte)c;
        }

        private void OpButton_Click(object sender, RoutedEventArgs e) {
            string op = (sender as Button).Tag as string;
            AddSourceText(op);
            TryExecuteInstruction();
        }

        private void delaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (delayTextBlock != null) {
                delayTextBlock.Text = delay + " s";
                timer.Interval = TimeSpan.FromSeconds(delay);
            }
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e) {
            isRunning = false;
            timer.Stop();
            UpdateUI();
        }

        private void playButton_Click(object sender, RoutedEventArgs e) {
            if (iPtr >= sourceBoxes.Count)
                Reset();
            isRunning = true;
            if (delay > 0) {
                timer.Start();
                TryExecuteInstruction();
            }
            else {
                while (TryExecuteInstruction()) { UpdateUI(); }
            }
        }

        private void stepButton_Click(object sender, RoutedEventArgs e) {
            if (iPtr >= sourceBoxes.Count)
                Reset();
            TryExecuteInstruction();
        }

        private void About_Click(object sender, RoutedEventArgs e) {
            var about = new About();
            about.Show();
        }
    }
}
