﻿using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XamarinBackgroundKit.Abstractions;
using XamarinBackgroundKit.Controls;
using XamarinBackgroundKit.Extensions;
using Color = Xamarin.Forms.Color;

namespace XamarinBackgroundKit.Android.Renderers
{
    public class GradientStrokeDrawable : PaintDrawable
    {
        private readonly Context _context;
        
        private Paint _strokePaint;
        private int[] _strokeColors;
        private float[] _strokePositions;
        private float[] _strokeColorPositions;

        public GradientStrokeDrawable(Context context, IMaterialVisualElement background = null)
        {
            Initialize();

            _context = context;

            if (background == null) return;
            
            SetColor(background.Color);
            SetCornerRadius(background.CornerRadius);
            SetGradient(background.Gradients, background.Angle);
            SetBorderGradient(background.BorderGradients, background.BorderAngle);
            SetStroke(background.BorderWidth, background.BorderColor);
            SetDashedBorder(background.DashWidth, background.DashGap);
        }

        private void Initialize()
        {
            Shape = new RectShape();
            
            _strokePaint = new Paint
            {
                Dither = true,
                AntiAlias = true
            };
            
            _strokePaint.SetStyle(Paint.Style.Stroke);
        }

        public void SetColor(Color color)
        {
            if (color == Color.Default) return;

            Paint.Color = color.ToAndroid();
            
            InvalidateSelf();
        }

        public void SetStroke(double strokeWidth, Color strokeColor)
        {
            _strokePaint.StrokeWidth = (int) _context.ToPixels(strokeWidth);

            if (_strokeColors == null)
            {
                _strokePaint.Color = strokeColor.ToAndroid();
            }
            
            InvalidateSelf();
        }

        public void SetDashedBorder(double dashWidth, double dashGap)
        {
            if (dashWidth <= 0 || dashGap <= 0)
            {
                _strokePaint.SetPathEffect(null);
            }
            else
            {
                _strokePaint.SetPathEffect(new DashPathEffect(new float[]
                {
                    (int) _context.ToPixels(dashWidth), 
                    (int) _context.ToPixels(dashGap)
                }, 0));
            }
            
            InvalidateSelf();
        }

        public void SetBorderGradient(IList<GradientStop> gradients, float angle)
        {
            if (gradients == null || gradients.Count == 0)
            {
                _strokeColors = null;
                _strokePositions = null;
                _strokeColorPositions = null;

                InvalidateSelf();
                return;
            }

            var positions = angle.ToStartEndPoint();

            for (var i = 0; i < positions.Length; i++)
            {
                if (!(positions[i] > 1)) continue;
                positions[i] = 1;
            }

            _strokePositions = positions;
            _strokeColors = gradients.Select(x => (int) x.Color.ToAndroid()).ToArray();
            _strokeColorPositions = gradients.Select(x => x.Offset).ToArray();

            _strokePaint.Color = Color.White.ToAndroid();

            InvalidateSelf();
        }

        public void SetGradient(IList<GradientStop> gradients, float angle)
        {
            if (gradients == null || gradients.Count == 0) return;
            
            var positions = angle.ToStartEndPoint();

            for (var i = 0; i < positions.Length; i++)
            {
                if (!(positions[i] > 1)) continue;
                positions[i] = 1;
            }

            SetShaderFactory(new GradientShaderFactory
            {
                Positions = positions,
                ColorPositions = gradients.Select(x => x.Offset).ToArray(),
                Colors = gradients.Select(x => (int)x.Color.ToAndroid()).ToArray(),
            });
        }

        public void SetCornerRadius(CornerRadius cornerRadius)
        {
            if (cornerRadius == new CornerRadius(0d)) return;

            var isUniform = cornerRadius.IsAllRadius() && !cornerRadius.IsEmpty();

            var uniformCornerRadius = _context.ToPixels(cornerRadius.TopLeft);
            var cornerRadii = cornerRadius.ToRadii(_context.Resources.DisplayMetrics.Density);

            if (isUniform) base.SetCornerRadius(uniformCornerRadius);
            else SetCornerRadii(cornerRadii);
        }
        
        protected override void OnDraw(Shape shape, Canvas canvas, Paint paint)
        {
            base.OnDraw(shape, canvas, paint);

            if (_strokePaint == null) return;

            if (_strokeColors != null && _strokePositions != null && _strokeColorPositions != null)
            {
                _strokePaint.SetShader(new LinearGradient(
                    canvas.Width * _strokePositions[0],
                    canvas.Height * _strokePositions[1],
                    canvas.Width * _strokePositions[2],
                    canvas.Height * _strokePositions[3],
                    _strokeColors,
                    _strokeColorPositions,
                    Shader.TileMode.Clamp));
            }

            shape.Draw(canvas, _strokePaint);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_strokePaint != null)
                {
                    _strokePaint.Dispose();
                    _strokePaint = null;
                }

                _strokeColors = null;
                _strokePositions = null;
                _strokeColorPositions = null;
            }

            base.Dispose(disposing);
        }

        private class GradientShaderFactory : ShaderFactory
        {
            public int[] Colors { private get; set; }
            public float[] Positions { private get; set; }
            public float[] ColorPositions { private get; set; }

            public override Shader Resize(int width, int height)
            {
                return new LinearGradient(
                    width * Positions[0],
                    height * Positions[1],
                    width * Positions[2],
                    height * Positions[3],
                    Colors,
                    ColorPositions,
                    Shader.TileMode.Clamp);
            }
        }

        public class Builder
        {
            private readonly Context _context;
            private IMaterialVisualElement _materialVisualElement;

            public Builder(Context context)
            {
                _context = context;
            }

            public Builder SetMaterialElement(IMaterialVisualElement materialVisualElement)
            {
                _materialVisualElement = materialVisualElement;
                return this;
            }

            public GradientStrokeDrawable Build()
            {
                return new GradientStrokeDrawable(_context, _materialVisualElement);
            }
        }
    }
}