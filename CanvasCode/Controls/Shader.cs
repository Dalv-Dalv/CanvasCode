// Modified https://avaloniaui.net/blog/avalonia-with-fragment-shaders

using System;
using System.IO;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Skia;
using SkiaSharp;

namespace CanvasCode.Controls;

public class Shader : UserControl {
	private record struct ShaderDrawPayload(
		HandlerCommand HandlerCommand,
		Uri? ShaderCode = default,
		Size? ShaderSize = default,
		Size? Size = default,
		Stretch? Stretch = default,
		StretchDirection? StretchDirection = default,
		bool? IsAnimated = default,
		double? FrameRate = default);

	private enum HandlerCommand {
		Start,
		Stop,
		Update,
		Dispose,
		UpdateAnimation
	}

	private const float DefaultShaderLength = 512;

	private CompositionCustomVisual? _customVisual;


	public static readonly StyledProperty<Stretch> StretchProperty =
		AvaloniaProperty.Register<Shader, Stretch>(nameof(Stretch), Stretch.Uniform);

	public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
		AvaloniaProperty.Register<Shader, StretchDirection>(
			nameof(StretchDirection),
			StretchDirection.Both);

	public static readonly StyledProperty<Uri?> ShaderUriProperty =
		AvaloniaProperty.Register<Shader, Uri?>(nameof(ShaderUri));

	public static readonly StyledProperty<double> ShaderWidthProperty =
		AvaloniaProperty.Register<Shader, double>(
			nameof(ShaderWidth),
			DefaultShaderLength);

	public static readonly StyledProperty<double> ShaderHeightProperty =
		AvaloniaProperty.Register<Shader, double>(
			nameof(ShaderHeight),
			DefaultShaderLength);
	
	public static readonly StyledProperty<bool> IsAnimatedProperty =
		AvaloniaProperty.Register<Shader, bool>(nameof(IsAnimated));

	public static readonly StyledProperty<double> AnimationFrameRateProperty =
		AvaloniaProperty.Register<Shader, double>(nameof(AnimationFrameRate));

	public Stretch Stretch {
		get => GetValue(StretchProperty);
		set => SetValue(StretchProperty, value);
	}

	public StretchDirection StretchDirection {
		get => GetValue(StretchDirectionProperty);
		set => SetValue(StretchDirectionProperty, value);
	}

	public Uri? ShaderUri {
		get => GetValue(ShaderUriProperty);
		set => SetValue(ShaderUriProperty, value);
	}

	public double ShaderWidth {
		get => GetValue(ShaderWidthProperty);
		set => SetValue(ShaderWidthProperty, value);
	}

	public double ShaderHeight {
		get => GetValue(ShaderHeightProperty);
		set => SetValue(ShaderHeightProperty, value);
	}

	public bool IsAnimated {
		get =>  GetValue(IsAnimatedProperty);
		set =>  SetValue(IsAnimatedProperty, value);
	}

	public double AnimationFrameRate {
		get =>  GetValue(AnimationFrameRateProperty);
		set => SetValue(AnimationFrameRateProperty, value);
	}

	static Shader() {
		AffectsRender<Shader>(ShaderUriProperty,
			StretchProperty,
			StretchDirectionProperty,
			ShaderWidthProperty,
			ShaderHeightProperty);

		AffectsMeasure<Shader>(ShaderUriProperty,
			StretchProperty,
			StretchDirectionProperty,
			ShaderWidthProperty,
			ShaderHeightProperty);
		
		IsAnimatedProperty.Changed.AddClassHandler<Shader>((s, e) => s.UpdateAnimation());
		AnimationFrameRateProperty.Changed.AddClassHandler<Shader>((s, e) => s.UpdateAnimation());
	}
	
	private void UpdateAnimation() {
		_customVisual?.SendHandlerMessage(new ShaderDrawPayload(
			HandlerCommand.UpdateAnimation,
			IsAnimated: IsAnimated,
			FrameRate: AnimationFrameRate));
	}

	private Size GetShaderSize() {
		return new Size(ShaderWidth, ShaderHeight);
	}

	protected override Size MeasureOverride(Size availableSize) {
		var source = ShaderUri;
		var result = new Size();

		if (source != null) result = Stretch.CalculateSize(availableSize, GetShaderSize(), StretchDirection);

		return result;
	}

	protected override Size ArrangeOverride(Size finalSize) {
		var source = ShaderUri;

		if (source == null) return new Size();

		var sourceSize = GetShaderSize();
		var result = Stretch.CalculateSize(finalSize, sourceSize);
		return result;
	}

	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
		base.OnAttachedToVisualTree(e);
		
		if (Design.IsDesignMode) // Avoid crashes in previewer
			return;

		var elemVisual = ElementComposition.GetElementVisual(this);
		var compositor = elemVisual?.Compositor;
		if (compositor is null) return;

		_customVisual = compositor.CreateCustomVisual(new ShaderCompositionCustomVisualHandler());
		ElementComposition.SetElementChildVisual(this, _customVisual);

		LayoutUpdated += OnLayoutUpdated;

		_customVisual.Size = new Vector2((float)Bounds.Size.Width, (float)Bounds.Size.Height);

		_customVisual.SendHandlerMessage(
			new ShaderDrawPayload(
				HandlerCommand.Update,
				null,
				GetShaderSize(),
				Bounds.Size,
				Stretch,
				StretchDirection));

		Start();
	}

	protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
		base.OnDetachedFromVisualTree(e);
		LayoutUpdated -= OnLayoutUpdated;

		Stop();
		DisposeImpl();
	}

	private void OnLayoutUpdated(object? sender, EventArgs e) {
		if (_customVisual == null) return;

		_customVisual.Size = new Vector2((float)Bounds.Size.Width, (float)Bounds.Size.Height);

		_customVisual.SendHandlerMessage(
			new ShaderDrawPayload(
				HandlerCommand.Update,
				null,
				GetShaderSize(),
				Bounds.Size,
				Stretch,
				StretchDirection));
	}

	public void Start() {
		_customVisual?.SendHandlerMessage(
			new ShaderDrawPayload(
				HandlerCommand.Start,
				ShaderUri,
				GetShaderSize(),
				Bounds.Size,
				Stretch,
				StretchDirection,
				IsAnimated, 
				AnimationFrameRate));

		InvalidateVisual();
	}

	public void Stop() {
		_customVisual?.SendHandlerMessage(new ShaderDrawPayload(HandlerCommand.Stop));
	}

	private void DisposeImpl() {
		_customVisual?.SendHandlerMessage(new ShaderDrawPayload(HandlerCommand.Dispose));
	}

	private class ShaderCompositionCustomVisualHandler : CompositionCustomVisualHandler {
		private bool _running;
		private Stretch? _stretch;
		private StretchDirection? _stretchDirection;
		private Size? _boundsSize;
		private Size? _shaderSize;
		private string? _shaderCode;
		private readonly object _sync = new();
		private SKRuntimeEffectUniforms? _uniforms;
		private SKRuntimeEffect? _effect;
		private bool _isDisposed;

		private bool _isAnimated;
		private TimeSpan _frameInterval;
		private TimeSpan _lastFrameTime;

		public override void OnMessage(object message) {
			if (message is not ShaderDrawPayload msg) return;

			switch (msg) {
				case { HandlerCommand: HandlerCommand.Start, ShaderCode: { } uri, ShaderSize: { } shaderSize, Size: { } size, Stretch: { } st, StretchDirection: { } sd, IsAnimated: { } anim, FrameRate: { } rate }: {
					using var stream = AssetLoader.Open(uri);
					using var txt = new StreamReader(stream);

					_shaderCode = txt.ReadToEnd();

					_effect = SKRuntimeEffect.Create(_shaderCode, out var errorText);
					if (_effect == null) Console.WriteLine($"Shader compilation error: {errorText}");

					_shaderSize = shaderSize;
					_running = true;
					_boundsSize = size;
					_stretch = st;
					_stretchDirection = sd;
					
					_isAnimated = anim;
					_frameInterval = rate > 0 ? TimeSpan.FromSeconds(1.0 / rate) : TimeSpan.Zero;
                    
					if (_isAnimated) {
						RegisterForNextAnimationFrameUpdate();
					} else {
						Invalidate(); // Draw once for static shaders
					}
					break;
				}
				case { HandlerCommand: HandlerCommand.Update, ShaderSize: { } shaderSize, Size: { } size, Stretch: { } st, StretchDirection: { } sd }: {
					_shaderSize = shaderSize;
					_boundsSize = size;
					_stretch = st;
					_stretchDirection = sd;
					Invalidate();
					break;
				}
				case { HandlerCommand: HandlerCommand.UpdateAnimation, IsAnimated: { } anim, FrameRate: { } rate }: {
					_isAnimated = anim;
					_frameInterval = rate > 0 ? TimeSpan.FromSeconds(1.0 / rate) : TimeSpan.Zero;
					// If animation was just turned on, kick off the loop.
					if (_isAnimated) {
						RegisterForNextAnimationFrameUpdate();
					}
					break;
				}
				case { HandlerCommand: HandlerCommand.Stop }: {
					_running = false;
					break;
				}
				case { HandlerCommand: HandlerCommand.Dispose }: {
					DisposeImpl();
					break;
				}
			}
		}

		public override void OnAnimationFrameUpdate() {
			if (!_running || !_isAnimated || _isDisposed)
				return;

			var now = CompositionNow;
            
			// Throttling logic: If not enough time has passed, skip this frame.
			if (_frameInterval > TimeSpan.Zero && (now - _lastFrameTime) < _frameInterval) {
				RegisterForNextAnimationFrameUpdate();
				return;
			}
            
			_lastFrameTime = now;
			Invalidate();
			RegisterForNextAnimationFrameUpdate();
		}

		private void Draw(SKCanvas canvas) {
			if (_isDisposed || _effect is null)
				return;

			canvas.Save();

			var targetWidth = (float)(_shaderSize?.Width ?? DefaultShaderLength);
			var targetHeight = (float)(_shaderSize?.Height ?? DefaultShaderLength);

			_uniforms ??= new SKRuntimeEffectUniforms(_effect);

			if(_isAnimated) _uniforms["iTime"] = (float)CompositionNow.TotalSeconds;
			_uniforms["iResolution"] = new[] { targetWidth, targetHeight };

			using (var paint = new SKPaint())
			using (var shader = _effect.ToShader(false, _uniforms)) {
				paint.Shader = shader;
				canvas.DrawRect(SKRect.Create(targetWidth, targetHeight), paint);
			}

			canvas.Restore();
		}

		public override void OnRender(ImmediateDrawingContext context) {
			lock (_sync) {
				if (_stretch is not { } st
				    || _stretchDirection is not { } sd
				    || _isDisposed)
					return;

				var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
				if (leaseFeature is null) return;

				var rb = GetRenderBounds();
				var size = _boundsSize ?? rb.Size;
				var viewPort = new Rect(rb.Size);
				var sourceSize = _shaderSize!.Value;

				if (sourceSize.Width <= 0 || sourceSize.Height <= 0) return;

				var scale = st.CalculateScaling(rb.Size, sourceSize, sd);
				var scaledSize = sourceSize * scale;
				var destRect = viewPort
				               .CenterRect(new Rect(scaledSize))
				               .Intersect(viewPort);
				var sourceRect = new Rect(sourceSize)
					.CenterRect(new Rect(destRect.Size / scale));

				var bounds = SKRect.Create(new SKPoint(), new SKSize((float)size.Width, (float)size.Height));

				var scaleMatrix = Matrix.CreateScale(
					destRect.Width / sourceRect.Width,
					destRect.Height / sourceRect.Height);

				var translateMatrix = Matrix.CreateTranslation(
					-sourceRect.X + destRect.X - bounds.Top,
					-sourceRect.Y + destRect.Y - bounds.Left);

				using (context.PushClip(destRect))
				using (context.PushPostTransform(translateMatrix * scaleMatrix)) {
					using var lease = leaseFeature.Lease();
					var canvas = lease.SkCanvas;
					Draw(canvas);
				}
			}
		}

		private void DisposeImpl() {
			lock (_sync) {
				if (_isDisposed) return;
				_isDisposed = true;
				_effect?.Dispose();
				_uniforms?.Reset();
				_running = false;
			}
		}
	}
}