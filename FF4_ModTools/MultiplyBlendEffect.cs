using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace FF4_ModTools
{
    class MultiplyBlendEffect : ShaderEffect
    {
        static MultiplyBlendEffect()
        {
            _pixelShader.UriSource = new Uri($"pack://application:,,,/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name};component/Resources/MultiplyBlendEffect.ps");
        }

        private static PixelShader _pixelShader = new PixelShader();

        public MultiplyBlendEffect()
        {
            this.PixelShader = _pixelShader;
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(TextureProperty);
        }
        
        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        public Brush Texture
        {
            get => (Brush)GetValue(TextureProperty);
            set => SetValue(TextureProperty, value);
        }

        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(MultiplyBlendEffect), 0);
        public static readonly DependencyProperty TextureProperty = RegisterPixelShaderSamplerProperty("Blend", typeof(MultiplyBlendEffect), 1);
    }

}
