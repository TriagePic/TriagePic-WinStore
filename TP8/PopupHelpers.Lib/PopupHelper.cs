using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace SocialEbola.Lib.PopupHelpers
{
	public class PopupHelper
	{
		public static PopupSettings DefaultPopupSettings = PopupSettings.CenterWideDialog;

		private Popup m_popup;
		private UserControl m_control;
		private Grid m_container;
		private Border m_overlay;
		private Rect m_inputPaneRect;
		private InputPane m_popupInputPane;
		private Storyboard m_storyboard;
		private IPopupControl m_comm;
		private TaskCompletionSource<bool> m_showCompleteTaskSource;
		public PopupHelper(UserControl control)
		{
			m_control = control;
			m_comm = m_control as IPopupControl;
		}

		public virtual PopupSettings Settings
		{
			get { return DefaultPopupSettings; }
		}

		protected UserControl Control
		{
			get
			{
				return m_control;
			}
		}

		protected void CallControl(Action<IPopupControl> pred)
		{
			if (m_comm != null)
			{
				pred(m_comm);
			}
		}


		protected virtual PopupAnimation GetAnimation()
		{
			return Settings.Animation;
		}

		protected virtual TimeSpan GetAnimationDuration()
		{
			return Settings.AnimationDuration;
		}

		protected virtual double GetOverlayOpacity()
		{
			return Settings.OverlayOpacity;
		}

		protected virtual double GetOverlayToControlAnimationRatio()
		{
			return Settings.OverlayToControlAnimationRatio;
		}

        public object DataCotext { get; set; }

		private Storyboard CreateBaseAnimation()
		{
			Storyboard board = new Storyboard();
			return board;
		}

		public Task CloseAsync()
		{
			return CloseAsync(CloseAction.CloseCalled);
		}

		protected async Task CloseAsync(CloseAction action)
		{
			await SetAnimationAsync(false);
			m_popup.IsOpen = false;
			CallControl(x => x.Closed(action));
			m_showCompleteTaskSource.SetResult(true);
		}

		protected void AddFlipAnimation(Storyboard board, FrameworkElement element, bool open, TimeSpan duration)
		{
			PlaneProjection projection = element.Projection as PlaneProjection;
			if (projection == null)
			{
				element.Projection = projection = new PlaneProjection();
			}

			projection.CenterOfRotationY = 0.5;

			var flipAnim = new DoubleAnimationUsingKeyFrames();

			double start = open ? -90 : 0;
			double end = open ? 0 : 90;
			TimeSpan startTime;
			TimeSpan endTime;

			CalculateDurations(open, duration, ref startTime, ref endTime);

			Storyboard.SetTarget(flipAnim, projection);
			Storyboard.SetTargetProperty(flipAnim, "RotationX");

			flipAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = TimeSpan.Zero, Value = start });
			flipAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = startTime, Value = start });
			flipAnim.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = endTime, EasingFunction = GetEasingFunction(open), Value = end });

			board.Children.Add(flipAnim);
		}

		private void CalculateDurations(bool open, TimeSpan duration, ref TimeSpan startTime, ref TimeSpan endTime)
		{
			startTime = GetAnimationDuration() - duration;
			endTime = GetAnimationDuration();
			if (!open)
			{
				startTime = TimeSpan.Zero;
				endTime = duration;
			}
		}

		private static void Swap(ref TimeSpan startTime, ref TimeSpan endTime)
		{
			TimeSpan temp = endTime;
			endTime = startTime;
			startTime = temp;
		}

		protected EasingFunctionBase GetEasingFunction(bool open)
		{
			return new QuadraticEase() { EasingMode = open ? EasingMode.EaseOut : EasingMode.EaseIn };
		}

		private void AddFadeAnimation(Storyboard board, Border m_overlay, bool open, double fadeTo, TimeSpan duration)
		{
			var fadeAnim = new DoubleAnimationUsingKeyFrames();

			double start = open ? 0 : fadeTo;
			double end = open ? fadeTo : 0;

			Storyboard.SetTarget(fadeAnim, m_overlay);
			Storyboard.SetTargetProperty(fadeAnim, "Opacity");

			TimeSpan startTime;
			TimeSpan endTime;

			CalculateDurations(open, duration, ref startTime, ref endTime);

			fadeAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = TimeSpan.Zero, Value = start });
			fadeAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = startTime, Value = start });
			fadeAnim.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = endTime, Value = end, EasingFunction = GetEasingFunction(open), });

			board.Children.Add(fadeAnim);
		}


		public async Task ShowAsync()
		{
			if (m_popup != null)
			{
				throw new InvalidOperationException();
			}
			m_popup = new Popup();
			m_popup.VerticalAlignment = VerticalAlignment.Stretch;
			m_popup.HorizontalAlignment = HorizontalAlignment.Stretch;
			Window.Current.SizeChanged += Current_SizeChanged;

			m_container = new Grid();
			m_container.HorizontalAlignment = HorizontalAlignment.Stretch;
			m_container.VerticalAlignment = VerticalAlignment.Stretch;

			m_overlay = new Border();
			m_overlay.HorizontalAlignment = HorizontalAlignment.Stretch;
			m_overlay.VerticalAlignment = VerticalAlignment.Stretch;
			m_overlay.Background = new SolidColorBrush(Colors.Black);
			m_overlay.Opacity = GetOverlayOpacity();
			if (Settings.OverlayDismissal)
			{
				m_overlay.Tapped += Overlay_Tapped;
				m_overlay.IsTapEnabled = true;
			}

			m_container.Children.Add(m_overlay);

			m_container.Children.Add(m_control);

			m_popup.Child = m_container;
			m_popupInputPane = InputPane.GetForCurrentView();
			m_popupInputPane.Showing += pane_Showing;
			m_popupInputPane.Hiding += pane_Showing;

			AdjustSize();

			CallControl(x => x.SetParent(this));



			m_popup.IsOpen = true;

			await SetAnimationAsync(true);
            if (DataCotext != null)
            {
                m_control.DataContext = DataCotext;
            }

            CallControl(x => x.Opened());

			await WaitForClosureAsync();
		}

		void Overlay_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
		{
			m_overlay.Tapped -= Overlay_Tapped;

			if (m_popup.IsOpen)
			{
				var dummy = CloseAsync(CloseAction.OverlayDismissal);
			}
		}

		protected virtual Task WaitForClosureAsync()
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			m_showCompleteTaskSource = tcs;
			return tcs.Task;
		}

		private Task SetAnimationAsync(bool open)
		{
			var board = CreateAnimation(open);
			SetStoryboard(board);
			return m_storyboard.BeginAsync();
		}

		private void SetStoryboard(Storyboard board)
		{
			if (m_storyboard != null)
			{
				m_storyboard.Stop();
				m_storyboard = null;
			}

			m_storyboard = board;
		}

		private static TimeSpan TimeSpanMul(TimeSpan span, double mul)
		{
			return TimeSpan.FromMilliseconds(span.TotalMilliseconds * mul);

		}
		protected virtual Storyboard CreateAnimation(bool open)
		{
			var board = CreateBaseAnimation();
			if (GetAnimation().IsFlagOn(PopupAnimation.ControlFlip))
			{
				AddFlipAnimation(board, m_control, open, TimeSpanMul(GetAnimationDuration(), GetOverlayToControlAnimationRatio()));
			}
			else if (GetAnimation().IsFlagOn(PopupAnimation.ControlFlyoutRight))
			{
				AddFlyoutAnimation(board, m_control, open, TimeSpanMul(GetAnimationDuration(), GetOverlayToControlAnimationRatio()));
			}

			if (GetAnimation().IsFlagOn(PopupAnimation.OverlayFade))
			{
				AddFadeAnimation(board, m_overlay, open, GetOverlayOpacity(), GetAnimationDuration());
			}

			return board;
		}

		private void AddFlyoutAnimation(Storyboard board, FrameworkElement element, bool open, TimeSpan duration)
		{
			CompositeTransform transform = element.RenderTransform as CompositeTransform;
			if (transform == null)
			{
				element.RenderTransform = transform = new CompositeTransform();
			}

			var flyoutAnim = new DoubleAnimationUsingKeyFrames();

			if (Double.IsNaN(element.Width) || (element.Width == 0))
			{
				throw new InvalidOperationException("When animating as flyout, control width must be set");
			}

			double start = open ? element.Width: 0;
			double end = open ? 0 : element.Width;
			TimeSpan startTime;
			TimeSpan endTime;

			CalculateDurations(open, duration, ref startTime, ref endTime);

			Storyboard.SetTarget(flyoutAnim, transform);
			Storyboard.SetTargetProperty(flyoutAnim, "TranslateX");

			flyoutAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = TimeSpan.Zero, Value = start });
			flyoutAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = startTime, Value = start });
			flyoutAnim.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = endTime, EasingFunction = GetEasingFunction(open), Value = end });

			board.Children.Add(flyoutAnim);
		}

		void pane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
		{
			m_inputPaneRect = args.OccludedRect;
			AdjustSize();
		}

		private void Cleanup()
		{
			Debug.Assert(m_popup != null);
			Window.Current.SizeChanged -= Current_SizeChanged;
			m_popupInputPane.Showing -= pane_Showing;
			m_popupInputPane.Hiding -= pane_Showing;
			m_popup = null;
		}

		void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
		{
			AdjustSize();
		}

		private void AdjustSize()
		{
			Rect rect = Window.Current.CoreWindow.Bounds;
			if (m_inputPaneRect.Top == 0)
			{
				rect.Y = m_inputPaneRect.Bottom;
			}

			rect.Height -= m_inputPaneRect.Height;

			m_popup.HorizontalOffset = rect.Left;
			m_popup.VerticalOffset = rect.Top;
			m_container.Width = rect.Width;
			m_container.Height = rect.Height;
		}

	}

	public class PopupHelperWithResult<R> : PopupHelper
	{
		public R Result { get; set; }

		public PopupHelperWithResult(UserControl control)
			: base(control)
		{

		}
		public new async Task<R> ShowAsync()
		{
			await base.ShowAsync();
			return Result;
		}
	}

	public class PopupHelperWithResult<R, C> : PopupHelperWithResult<R> where C : UserControl, new()
	{
		public PopupHelperWithResult()
			: base(new C())
		{
		}
	}

	public class PopupHelper<C> : PopupHelper where C : UserControl, new()
	{
		public PopupHelper()
			: base(new C())
		{
		}
	}


}
