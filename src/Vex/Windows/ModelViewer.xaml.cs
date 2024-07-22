using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.Wpf.SharpDX;
using PhilLibX.Media3D;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Vex.Library;
using Vex.Library.Utility;

namespace Vex
{
    /// <summary>
    /// Interaction logic for ModelViewer.xaml
    /// </summary>
    public partial class ModelViewer : Window
    {
        /// <summary>
        /// Random Int (For material loader)
        /// </summary>
        private static readonly Random RandomInt = new();

        public ProgressView Progress = null;

        /// <summary>
        /// Gets the ViewModel
        /// </summary>
        public ModelRenderViewModel ViewModel { get; } = new ModelRenderViewModel();

        public ModelViewer()
        {
            DataContext = ViewModel;
            InitializeComponent();
            Progress = ProgressView;
            Viewport.ModelUpDirection = new System.Windows.Media.Media3D.Vector3D(0, 0, 1);
        }

        public void LoadModel(Model model, VexInstance instance)
        {
            var Mesh = CreateModel(model, instance);
            ViewModel.ModelGroup = Mesh;
        }

        public void LoadImage(System.Windows.Media.ImageSource image)
        {
            Image.Source = image;
        }

        protected override void OnClosed(EventArgs e)
        {
            Dispose();
            base.OnClosed(e);
        }

        public void Clear()
        {
            ViewModel.Clear();
        }

        public void Dispose()
        {
            ViewModel.Dispose();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ClearBindings(Image);
            ClearBindings(Status);
            ClearBindings(Model);
            Viewport.InputBindings.Clear();
            Viewport.Camera = null;
            Viewport.Items.Clear();
            Viewport.EffectsManager.Dispose();
            Viewport.DataContext = null;
            Image.Source = null;
        }

        public static GroupModel3D CreateModel(Model model, VexInstance instance)
        {
            var ModelGroup = new GroupModel3D();
            foreach (var Mesh in model.Meshes)
            {
                var geometry = new MeshGeometry3D
                {
                    Positions = new Vector3Collection(Mesh.Positions.Select(p => new Vector3(p.X, p.Y, p.Z))),
                    TriangleIndices = new IntCollection(Mesh.Faces.SelectMany(f => new[] { f.Item1, f.Item2, f.Item3 })),
                    Normals = new Vector3Collection(Mesh.Normals.Select(n => new Vector3(n.X, n.Y, n.Z))),
                    TextureCoordinates = new Vector2Collection(Mesh.UVLayers.Select(uv => new Vector2(uv.X, uv.Y))),
                };
                var material = CreateMaterial(Mesh.Materials, instance);
                var MG = new MeshGeometryModel3D
                {
                    Geometry = geometry,
                    Material = material,
                };
                ModelGroup.Children.Add(MG);
            }
            return ModelGroup;
        }

        private static PhongMaterial CreateMaterial(List<PhilLibX.Media3D.Material> materials, VexInstance instance)
        {
            var material = new PhongMaterial
            {
                DiffuseColor = SharpDX.Color.White,
                ReflectiveColor = SharpDX.Color.Black,
                EmissiveColor = SharpDX.Color.Black
            };

            foreach (var mat in materials)
            {
                if (mat.Textures.TryGetValue("DiffuseMap", out var imgHash))
                {
                    var image = instance.VoidSupport.GetEntryFromName(imgHash.FilePath);
                    if(image == null)
                    {
                        material.DiffuseColor = new Color4(
                        (byte)RandomInt.Next(128, 255) / 255f,
                        (byte)RandomInt.Next(128, 255) / 255f,
                        (byte)RandomInt.Next(128, 255) / 255f,
                        1);
                        continue;
                    }
                    var bImage = instance.VoidSupport.GetBImageFromAsset(image, instance);
                    var ms = ImageHelper.ConvertImageToStream(bImage);
                    material.DiffuseMap = new TextureModel(ms, true);
                }
                else
                {
                    material.DiffuseColor = new Color4(
                        (byte)RandomInt.Next(128, 255) / 255f,
                        (byte)RandomInt.Next(128, 255) / 255f,
                        (byte)RandomInt.Next(128, 255) / 255f,
                        1);
                }
/*                if (mat.Textures.TryGetValue("SpecularMap", out var specHash))
                {
                    var image = instance.VoidSupport.GetEntryFromName(imgHash.Name);
                    var bImage = instance.VoidSupport.GetBImageFromAsset(image, instance);
                    var ms = ImageHelper.ConvertImageToStream(bImage);
                    material.DiffuseMap = new TextureModel(ms, true);
                    material.SpecularColorMap = new TextureModel(ms, true);
                    material.SpecularShininess = 10f;
                }
                else
                {*/
                    material.SpecularColor = SharpDX.Color.Black;
                    material.SpecularShininess = 20f;
                //}
            }

            return material;
        }

        private static void ClearBindings(DependencyObject element)
        {
            if (element != null)
            {
                BindingOperations.ClearAllBindings(element);

                // Clear bindings for child elements if it is a Panel (e.g., Grid, StackPanel)
                if (element is Panel panel)
                {
                    foreach (UIElement child in panel.Children)
                    {
                        ClearBindings(child);
                    }
                }

                // Clear bindings for other types of container elements (e.g., ContentControl, ItemsControl)
                if (element is ContentControl contentControl && contentControl.Content is DependencyObject content)
                {
                    ClearBindings(content);
                }

                if (element is ItemsControl itemsControl)
                {
                    foreach (var item in itemsControl.Items)
                    {
                        if (item is DependencyObject dependencyItem)
                        {
                            ClearBindings(dependencyItem);
                        }
                    }
                }
            }
        }
    }
}
