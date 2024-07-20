using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Vex
{
    /// <summary>
    /// Interaction logic for Progress.xaml
    /// </summary>
    public partial class ProgressView : UserControl
    {
        private Queue<string> _progressStages;
        private int TotalStageCount;
        private bool bUseFullBar = false;

        public ProgressView()
        {
            InitializeComponent();
        }

        public void Hide()
        {
            Visibility = Visibility.Hidden;
        }

        public void Show()
        {
            Visibility = Visibility.Visible;
        }

        private void UpdateProgress()
        {
            ProgressBar.Value = GetProgressPercentage();
            ProgressText.Text = GetCurrentStageName();
        }

        public void SetProgressStages(List<string> progressStages, bool bUseFullBar = false)
        {
            this.bUseFullBar = bUseFullBar;
            Dispatcher.Invoke(() =>
            {
                TotalStageCount = progressStages.Count;
                _progressStages = new Queue<string>();
                foreach (var progressStage in progressStages)
                {
                    _progressStages.Enqueue(progressStage);
                }

                UpdateProgress();
                Show();
            });
        }

        public void CompleteStage()
        {
            Dispatcher.Invoke(() =>
            {
                if (_progressStages.Count == 0)
                {
                    Hide();
                    return;
                }
                string removed = _progressStages.Dequeue();
                UpdateProgress();
                if (_progressStages.Count == 0)
                {
                    Hide();
                }
            });
        }

        public string GetCurrentStageName()
        {
            if (_progressStages.Count > 0)
            {
                var stage = _progressStages.Peek();
                return stage;
            }
            return "Loading";
        }

        public int GetProgressPercentage()
        {
            // We want to artificially make it more meaningful, so we pad by 15% on each side
            if (bUseFullBar)
                return 100 - 100 * _progressStages.Count / TotalStageCount;
            else
                return 95 - 90 * _progressStages.Count / TotalStageCount;
        }
    }
}
