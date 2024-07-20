using Microsoft.Win32;
using Vex.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Vex.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page, IDisposable
    {
        /// <summary>
        /// Active Instance
        /// </summary>
        private readonly VexInstance Instance = new();

        /// <summary>
        /// Gets or Sets the Log
        /// </summary>
        private StreamWriter LogStream { get; set; }

        /// <summary>
        /// Dedicated Log object
        /// </summary>
        private readonly object _logLock = new();

        /// <summary>
        /// Gets the ViewModel
        /// </summary>
        public MainViewModel ViewModel { get; } = new MainViewModel();

        /// <summary>
        /// Gets or Sets whether or not to end the current thread
        /// </summary>
        private readonly bool EndThread = false;

        /// <summary>
        /// The model viewer
        /// </summary>
        private ModelViewer ModelViewer;
        private bool disposedValue;

        public MainPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
            ViewModel.DimmerVisibility = Visibility.Hidden;
            Instance.Settings = VexSettings.Load("Settings.scfg");

            InitializeLogging();

/*            if (Instance.Settings.AutoUpdates)
            {
                try
                {
                    var TempPath = Path.GetTempPath();
                    if (TempPath == string.Empty)
                        return;

                    TryCleanupTemp(TempPath);
                    var UpdaterPath = Path.Combine(AppContext.BaseDirectory, "VexUpdater.exe");
                    var NewPath = Path.Combine(TempPath, $"VexUpdater_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds():x}.exe");
                    File.Copy(UpdaterPath, NewPath);
                    Process.Start(NewPath, $"echo000 Vex Vex Vex.exe {AppContext.BaseDirectory} true");
                }
                catch (Exception)
                {
                    // Log(ex.Message, "ERROR");
                }
            }*/
        }

        /// <summary>
        /// Sets the dimmer
        /// </summary>
        private void SetDimmer(Visibility visibility) => Dispatcher.BeginInvoke(new Action(() => Dimmer.Visibility = visibility));

        /// <summary>
        /// Initializes the logger
        /// </summary>
        private void InitializeLogging()
        {
            try
            {
                LogStream = new StreamWriter("Log.txt", true);
            }
            catch
            {
                LogStream?.Dispose();
                LogStream = null;
            }
        }

        /// <summary>
        /// Tries to cleanup the temp directory
        /// </summary>
        /// <param name="dir">Temp path</param>
        private static void TryCleanupTemp(string dir)
        {
            try
            {
                foreach (var filePath in Directory.GetFiles(dir, "VexUpdater*.exe"))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during file deletion
                Debug.WriteLine($"Error deleting files: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports the asset on double click
        /// </summary>
        private void AssetListMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((FrameworkElement)e.OriginalSource).DataContext is Asset asset)
            {
                ExportAssets([asset]);
            }
        }

        /// <summary>
        /// Change the preview when the selection changes
        /// </summary>
        private void AssetListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (App.IsWindowOpen<ModelViewer>())
            {
                BuildPreview();
            }
        }

        /// <summary>
        /// Exports an asset
        /// </summary>
        private void ExportAsset(Asset asset)
        {
            try
            {
                asset.Save(Instance);
                Log($"Exported {asset.Name}", "INFO");
                asset.Status = AssetStatus.Exported;
            }
            catch (Exception exception)
            {
                // Anything else we should log it
                Log($"An unhandled exception while exporting {asset.Name}:\n\n{exception}", "ERROR");
                asset.Status = AssetStatus.Error;
            }
        }

        /// <summary>
        /// Exports the list of assets
        /// </summary>
        public void ExportAssets(List<Asset> assets)
        {
            Instance.Settings = VexSettings.Load("Settings.scfg");
            var progressWindow = new ProgressWindow()
            {
                Owner = Window.GetWindow(this),
            };
            Dispatcher.BeginInvoke(new Action(() => progressWindow.ShowDialog()));
            progressWindow.SetProgressCount(assets.Count);
            Task.Run(() =>
            {
                SetDimmer(Visibility.Visible);

                Parallel.ForEach(assets, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (asset, loop) =>
                {
                    asset.Status = AssetStatus.Processing;
                    ExportAsset(asset);

                    if (progressWindow.IncrementProgress() || EndThread)
                        loop.Break();
                });

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    GC.Collect();
                    progressWindow.Complete();
                    SetDimmer(Visibility.Hidden);
                }));
            });
        }

        /// <summary>
        /// Writes to the log in debug
        /// </summary>
        private void Log(string value, string messageType)
        {
            if (LogStream != null)
            {
                lock (_logLock)
                {
                    LogStream?.WriteLine("{0} [ {1} ] {2}", DateTime.Now.ToString("dd-MM-yyyy - HH:mm:ss"), messageType.PadRight(12), value);
                    LogStream?.Flush();
                }
            }
        }

        /// <summary>
        /// Opens file dialog to open a package
        /// </summary>
        private async void OpenFileClick(object sender, RoutedEventArgs e)
        {
            TaskLabel.Content = "Loading File...";
            ViewModel.AssetButtonsEnabled = false;

            if (App.IsWindowOpen<ModelViewer>())
            {
                ModelViewer.ViewModel.ModelGroup = null;
                ModelViewer.Image.Source = null;
                SetIdleStatus();
            }

            Instance.Clear();
            ViewModel.Assets.ClearAllItems();

            // Call the helper method
            await ExecuteLoadingProcess(() => Task.Run(LoadFile));
        }

        /// <summary>
        /// Loads a file
        /// </summary>
        private void LoadFile()
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "All files (*.*)|*.*;|Index File (*.index)|*.index;",
                Multiselect = false,
                Title = "Select a game file to load"
            };

            if ((bool)openFileDialog.ShowDialog())
            {
                Instance.BeginGameFileMode(openFileDialog.FileName);
                ViewModel.Assets.AddRange(Instance.Assets);
            }
        }

        /// <summary>
        /// Executes the loading process
        /// </summary>
        /// <param name="loadProcess">Async function to load</param>
        /// <returns></returns>
        private async Task ExecuteLoadingProcess(Func<Task> loadProcess)
        {
            try
            {
                await loadProcess();
                ProgressComplete(null);
            }
            catch (Exception ex)
            {
                ProgressComplete(ex);
            }
        }

        /// <summary>
        /// Handles on progress complete
        /// </summary>
        private void ProgressComplete(Exception e)
        {
            GC.Collect();
            ViewModel.AssetButtonsEnabled = true;

            if (e == null)
            {
                if (Instance.Assets != null)
                {
                    ViewModel.Assets.SendNotify();
                    TaskLabel.Content = $"{Instance.Assets.Count} assets loaded";
                }
                else
                {
                    TaskLabel.Content = "0 assets loaded";
                    Instance.Clear();
                }
            }
            else if (e is Exception exception)
            {
                Log($"An unhandled exception has occurred, take this to my creator:\n\n{exception}", "ERROR");
                MessageBox.Show($"An unhandled exception has occurred, take this to my creator:\n\n{exception}", "Vex", MessageBoxButton.OK, MessageBoxImage.Error);
                TaskLabel.Content = "0 assets loaded";
                Instance.Clear();
            }
        }

        /// <summary>
        /// Exports all loaded assets
        /// </summary>
        private void ExportAllClick(object sender, RoutedEventArgs e)
        {
            var assets = AssetList.Items.Cast<Asset>().ToList();

            if (assets.Count == 0)
            {
                SetDimmer(Visibility.Visible);
                MessageBox.Show("There are no assets listed to export.", "Vex | Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SetDimmer(Visibility.Hidden);
            }
            else
            {
                SetDimmer(Visibility.Visible);
                ExportAssets(assets);
                SetDimmer(Visibility.Hidden);
            }
        }

        /// <summary>
        /// Exports selected assets
        /// </summary> 
        private void ExportSelectedClick(object sender, RoutedEventArgs e)
        {
            var assets = AssetList.SelectedItems.Cast<Asset>().ToList();

            if (assets.Count == 0)
            {
                SetDimmer(Visibility.Visible);
                MessageBox.Show("There are no assets listed to export.", "Vex | Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SetDimmer(Visibility.Hidden);
                return;
            }
            ExportAssets(assets);
        }

        private void ClearAllAssets(object sender, RoutedEventArgs e)
        {
            TaskLabel.Content = "0 assets loaded";
            Instance.Clear();
            ViewModel.Assets.ClearAllItems();
            if (App.IsWindowOpen<ModelViewer>())
            {
                ModelViewer.Image.Source = null;
                ModelViewer.ViewModel.ModelGroup = null;
                SetIdleStatus();
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public void OpenPreviewWindow()
        {
            if (!App.IsWindowOpen<ModelViewer>())
            {
                ModelViewer = new ModelViewer()
                {
                    Owner = Window.GetWindow(this),
                    Topmost = false,
                };
                ModelViewer.Show();
                BuildPreview();
            }
        }

        private async void BuildPreview()
        {
            var selectedItems = AssetList.SelectedItems;
            if (selectedItems.Count == 0)
            {
                SetIdleStatus();
                return;
            }

            if (selectedItems[0] is Asset asset)
            {
                ResetModelViewer();
                try
                {
                    switch (asset)
                    {
                        case ModelAsset modelAsset:
                            await LoadModelAsset(modelAsset);
                            break;
                        case ImageAsset:
                        case MaterialAsset:
                            await LoadImageAsset(asset);
                            break;
                    }
                }
                catch (Exception exception)
                {
                    Log($"An unhandled exception while loading {asset.Name}:\n\n{exception}", "ERROR");
                    ModelViewer.Viewport.Visibility = Visibility.Visible;
                    ModelViewer.Image.Visibility = Visibility.Hidden;
                    ModelViewer.Status.Visibility = Visibility.Hidden;
                    SetErrorStatus(exception);
                }
            }
        }

        private async Task LoadModelAsset(ModelAsset modelAsset)
        {
            List<string> mapStages = [$"Loading model: {modelAsset.Name}", "Loading Textures", "Preparing renderer"];
            ModelViewer.Progress.SetProgressStages(mapStages);

            var model = await Task.Run(() => modelAsset.BuildPreview(Instance));
            ModelViewer.Progress.CompleteStage();
            var imgs = Instance.Settings.LoadImagesModel ? await Task.Run(() => modelAsset.BuildPreviewImages(Instance)) : null;
            ModelViewer.Progress.CompleteStage();

            Dispatcher.Invoke(() =>
            {
                ModelViewer.LoadModel(model, imgs);
                ModelViewer.ViewModel.StatusText = $"Status     : Loaded {modelAsset.DisplayName}";
                ModelViewer.Viewport.SubTitle = $"Bones      : {model.Skeleton.Bones.Count}\n" +
                                                $"Vertices   : {model.GetVertexCount()}\n" +
                                                $"Faces      : {model.GetFaceCount()}\n" +
                                                $"Materials  : {model.Materials.Count}\n";
                ModelViewer.Viewport.Visibility = Visibility.Visible;
                ModelViewer.Image.Visibility = Visibility.Hidden;
                ModelViewer.Status.Visibility = Visibility.Hidden;
                ModelViewer.Progress.CompleteStage();
                ModelViewer.Viewport.ZoomExtents(0);
            });
        }

        private async Task LoadImageAsset(Asset asset)
        {
            List<string> mapStages = [$"Loading Image: {asset.Name}", "Rendering Image"];
            ModelViewer.Progress.SetProgressStages(mapStages);

            var imageSource = await Task.Run(() => asset.BuildPreviewTexture(Instance));
            ModelViewer.Progress.CompleteStage();

            Dispatcher.Invoke(() =>
            {
                ModelViewer.LoadImage(imageSource);
                ModelViewer.ViewModel.StatusText = $"Status     : Loaded {asset.DisplayName}".Replace("_", "__");
                ModelViewer.Image.Visibility = Visibility.Visible;
                ModelViewer.Status.Visibility = Visibility.Visible;
                ModelViewer.Viewport.Visibility = Visibility.Hidden;
                ModelViewer.Progress.CompleteStage();
                ViewModel.DimmerVisibility = Visibility.Hidden;
            });
        }

        private void SetErrorStatus(Exception exception)
        {
            Dispatcher.Invoke(() =>
            {
                ModelViewer.ViewModel.StatusText = $"Status     : Error - {exception.Message.Replace("\\", "\\\\")}";
                ModelViewer.Viewport.SubTitle = "";
                ViewModel.DimmerVisibility = Visibility.Hidden;
                ModelViewer.ProgressView.Visibility = Visibility.Hidden;
            });
        }

        private void SetIdleStatus()
        {
            ModelViewer.ViewModel.StatusText = "Status     : Idle";
            ModelViewer.Viewport.SubTitle = "";
            ModelViewer.Viewport.Visibility = Visibility.Visible;
            ModelViewer.Image.Visibility = Visibility.Hidden;
            ModelViewer.Status.Visibility = Visibility.Hidden;
        }

        private void ResetModelViewer()
        {
            ModelViewer.Viewport.Visibility = Visibility.Hidden;
            ModelViewer.Image.Visibility = Visibility.Hidden;
            ModelViewer.Status.Visibility = Visibility.Hidden;
            ModelViewer.Image.Source = null;
            ModelViewer.ViewModel.ModelGroup = null;
        }

        private void OpenSettingsWindow(object sender, RoutedEventArgs e)
        {
            var ns = (Application.Current.MainWindow as MainWindow).MainContentFrame.NavigationService;
            ns.Navigate(new SettingsPage(Instance, ReloadSettings));
        }

        public void ReloadSettings()
        {
            Instance.Settings = VexSettings.Load("Settings.scfg");
        }

        private void OpenAboutWindow(object sender, RoutedEventArgs e)
        {
            new AboutWindow()
            {
                Owner = Window.GetWindow(this),
            }.ShowDialog();
        }

        //Do this just to get rid of the warning
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Instance.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
