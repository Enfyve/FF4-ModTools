using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace FF4_ModTools
{
    class AdditiveBlendEffect : ShaderEffect
    {
        static AdditiveBlendEffect()
        {
            _pixelShader.UriSource = new Uri($"pack://application:,,,/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name};component/Resources/AdditiveBlendEffect.ps");
        }

        private static PixelShader _pixelShader = new PixelShader();

        public AdditiveBlendEffect()
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

        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(AdditiveBlendEffect), 0);
        public static readonly DependencyProperty TextureProperty = RegisterPixelShaderSamplerProperty("Blend", typeof(AdditiveBlendEffect), 1);
    }

}
