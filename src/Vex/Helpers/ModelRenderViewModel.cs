using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using System;
using System.ComponentModel; // INotifyPropertyChanged
using System.Runtime.CompilerServices;
using Media3D = System.Windows.Media.Media3D;
using Point3D = System.Windows.Media.Media3D.Point3D;
using TranslateTransform3D = System.Windows.Media.Media3D.TranslateTransform3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

namespace Vex
{
    public class ModelRenderViewModel : INotifyPropertyChanged, IDisposable
    {
        public EffectsManager EffectsManager { get; }

        public PerspectiveCamera Camera { get; set; }

        public LineGeometry3D Grid { get; private set; }
        public Media3D.Transform3D GridTransform { get; private set; }

        private string statusText;
        public string StatusText
        {
            get { return statusText; }
            set
            {
                if (statusText != value)
                {
                    statusText = value;
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        private GroupModel3D modelGroup;
        public GroupModel3D ModelGroup
        {
            get { return modelGroup; }
            set
            {
                if (modelGroup != value)
                {
                    modelGroup = value;
                    OnPropertyChanged(nameof(ModelGroup));
                }
            }
        }

        public ModelRenderViewModel()
        {
            EffectsManager = new DefaultEffectsManager();
            Camera = new PerspectiveCamera()
            {
                Position = new Point3D(100, 100, 100),
                LookDirection = new Vector3D(-100, -100, -100),
                UpDirection = new Vector3D(0, 0, 1),
                FieldOfView = 65,
                NearPlaneDistance = 0.5,
                FarPlaneDistance = 10000
            };

            var lb = new LineBuilder();
            lb.AddGrid(BoxFaces.Bottom, 40, 40, 80, 80);
            Grid = lb.ToLineGeometry3D();
            GridTransform = new TranslateTransform3D(-40, -40, 0);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string info = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        protected bool Set<T>(ref T backingField, T value, [CallerMemberName] string propertyName = "")
        {
            if (object.Equals(backingField, value))
            {
                return false;
            }

            backingField = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        public void Clear()
        {
            if (modelGroup?.Children != null)
            {
                // need to iterate over everything to wipe the arrays
                foreach (var node in ModelGroup?.Children)
                {
                    if (node is MeshGeometryModel3D mn)
                    {
                        MeshGeometry3D mesh = mn?.Geometry as MeshGeometry3D;
                        mn.Instances = null;
                        mesh?.ClearAllGeometryData();
                        var q = mesh as IDisposable;
                        Disposer.RemoveAndDispose(ref q);
                        mn.Material = null;
                    }
                    node?.Dispose();
                    var n = node as IDisposable;
                    Disposer.RemoveAndDispose(ref n);
                }
                ModelGroup?.Children.Clear();
            }
            GC.Collect();
        }


        private bool _disposed = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here
                    Clear();
                }
                else
                {
                    if (ModelGroup != null)
                    {
                        var modelGroup = ModelGroup as IDisposable;
                        Disposer.RemoveAndDispose(ref modelGroup);
                    }
                    if (EffectsManager != null)
                    {
                        var effectManager = EffectsManager as IDisposable;
                        Disposer.RemoveAndDispose(ref effectManager);
                    }
                    _disposed = true;
                }
            }
        }

        ~ModelRenderViewModel()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
