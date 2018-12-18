using System;
using UIKit;
using CoreAnimation;
using CoreGraphics;
using Foundation;

namespace OHCheckboxLib
{
    public class OHCheckbox : UIControl
    {
        #region Private props

        //Animation duration for the whole selection transition
        double animationDuration = 0.3;

        //Percentage where the checkmark tail ends
        float finalStrokeEndForCheckmark = 0.85f;

        //Percentage where the checkmark head begins
        float finalStrokeStartForCheckmark = 0.3f;

        //Percentage of the bounce amount of checkmark near animation completion
        float checkmarkBounceAmount = 0.1f;

        //Trail layer. Trail is the circle which appears when the switch is in deselected state.
        CAShapeLayer trailCircle = new CAShapeLayer();

        //Circle layer. Circle appears when the switch is in selected state.
        CAShapeLayer circle = new CAShapeLayer();

        //Checkmark layer. Checkmark appears when the switch is in selected state.
        CAShapeLayer checkmark = new CAShapeLayer();

        //Middle point of the checkmark layer. Calculated each time the sublayers are layout.
        CGPoint checkmarkSplitPoint = CGPoint.Empty;

        float _lineWidth = 2;
        bool _animatesOnTouch = true;
        UIColor _strokeColor = UIColor.Black;
        UIColor _trailStrokeColor = UIColor.Gray;
        bool _isSelected;

        AnimationDelegate animationDelegate;

        #endregion

        #region Events

        /// <summary>
        ///    Called when selection animation started
        ///    Either when selecting or deselecting
        /// </summary>
        public event Action<bool> SelectionAnimationDidStart;

        /// <summary>
        ///    Called when selection animation stopped
        ///    Either when selecting or deselecting
        /// </summary>
        public event Action<bool> SelectionAnimationDidStop;

        #endregion

        #region Public Props

        /// <summary>
        ///   Line width for the circle, trail and checkmark parts of the checkbox.
        /// </summary>
        public float LineWidth
        {
            get
            {
                return _lineWidth;
            }
            set
            {
                _lineWidth = value;
                circle.LineWidth = _lineWidth;
                checkmark.LineWidth = _lineWidth;
                trailCircle.LineWidth = _lineWidth;
            }
        }

        /// <summary>
        ///   Set to false if the selection should not be animated with touch up inside events.
        /// </summary>
        public bool AnimatesOnTouch
        {
            get
            {
                return _animatesOnTouch;
            }
            set
            {
                _animatesOnTouch = value;
            }
        }

        /// <summary>
        ///    Stroke color for circle and checkmark.
        ///    Circle disappears and trail becomes visible when the switch is selected.
        /// </summary>
        public UIColor StrokeColor
        {
            get
            {
                return _strokeColor;
            }
            set
            {
                _strokeColor = value;
                circle.StrokeColor = _strokeColor.CGColor;
                checkmark.StrokeColor = _strokeColor.CGColor;
            }
        }

        /// <summary>
        ///    Stroke color for trail.
        ///    Trail disappears and circle becomes visible when the switch is deselected.
        /// </summary>
        public UIColor TrailStrokeColor
        {
            get
            {
                return _trailStrokeColor;
            }
            set
            {
                _strokeColor = value;
                trailCircle.StrokeColor = _trailStrokeColor.CGColor;
            }
        }

        /// <summary>
        ///    Overrides isSelected from UIControl using internal state flag.
        ///    Default value is false.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                SetSelected(_isSelected, false);
            }
        }

        #endregion

        #region Ctor

        public OHCheckbox(CGRect frame)
        {
            Frame = frame;

            Configure();
        }

        #endregion

        #region Override Methods

        public override void LayoutSublayersOfLayer(CALayer layer)
        {
            base.LayoutSublayersOfLayer(layer);

            if (layer == null || layer != this.Layer)
            {
                return;
            }

            var offset = CGPoint.Empty;
            var radius = Math.Min(Bounds.Width, Bounds.Height) / 2 - (_lineWidth / 2);
            offset.X = (nfloat)(Bounds.Width - radius * 2) / 2;
            offset.Y = (nfloat)(Bounds.Height - radius * 2) / 2;

            CATransaction.Begin();
            CATransaction.DisableActions = true;

            // Calculate frame for circle and trail circle
            var circleAndTrailFrame = new CGRect(offset.X, offset.Y, radius * 2, radius * 2);

            var circlePath = UIBezierPath.FromOval(circleAndTrailFrame);
            trailCircle.Path = circlePath.CGPath;

            circle.Transform = CATransform3D.Identity;
            circle.Frame = this.Bounds;
            circle.Path = UIBezierPath.FromOval(circleAndTrailFrame).CGPath;

            // Rotating circle by 212 degrees to be able to manipulate stroke end location.
            circle.Transform = CATransform3D.MakeRotation((float)(212 * Math.PI / 180), 0, 0, 1);

            var origin = new CGPoint(offset.X + radius, offset.Y + radius);

            // Calculate checkmark path
            var checkmarkPath = UIBezierPath.Create();
            var checkmarkStartPoint = CGPoint.Empty;

            // Checkmark will start from circle's stroke end calculated above.
            checkmarkStartPoint.X = (nfloat)(origin.X + radius * ((float)(Math.Cos(212 * Math.PI / 180))));
            checkmarkStartPoint.Y = (nfloat)(origin.Y + radius * ((float)(Math.Sin(212 * Math.PI / 180))));
            checkmarkPath.MoveTo(checkmarkStartPoint);

            checkmarkSplitPoint = new CGPoint(offset.X + radius * 0.9, offset.Y + radius * 1.4);
            checkmarkPath.AddLineTo(checkmarkSplitPoint);

            var checkmarkEndPoint = CGPoint.Empty;
            // Checkmark will end 320 degrees location of the circle layer.
            checkmarkEndPoint.X = (nfloat)(origin.X + radius * (float)(Math.Cos(320 * Math.PI / 180)));
            checkmarkEndPoint.Y = (nfloat)(origin.Y + radius * (float)(Math.Sin(320 * Math.PI / 180)));
            checkmarkPath.AddLineTo(checkmarkEndPoint);

            checkmark.Frame = this.Bounds;
            checkmark.Path = checkmarkPath.CGPath;

            CATransaction.Commit();
        }

        #endregion

        #region Private Methods

        void Configure()
        {
            // Setup layers
            ConfigureShareLayer(trailCircle);
            trailCircle.StrokeColor = _trailStrokeColor.CGColor;

            ConfigureShareLayer(circle);
            circle.StrokeColor = _strokeColor.CGColor;

            ConfigureShareLayer(checkmark);
            checkmark.StrokeColor = _strokeColor.CGColor;

            // Setup initial state
            SetSelected(false, false);

            // Add target for handling touch up inside event as a default manner
            this.AddTarget(HandleTouchUpInside, UIControlEvent.TouchUpInside);
        }

        void HandleTouchUpInside(object sender, EventArgs e)
        {
            SetSelected(!_isSelected, _animatesOnTouch);
            this.SendActionForControlEvents(UIControlEvent.ValueChanged);
        }

        void ConfigureShareLayer(CAShapeLayer shapeLayer)
        {
            shapeLayer.LineJoin = CAShapeLayer.JoinRound;
            shapeLayer.LineCap = CAShapeLayer.CapRound;
            shapeLayer.LineWidth = _lineWidth;
            shapeLayer.FillColor = UIColor.Clear.CGColor;
            this.Layer.AddSublayer(shapeLayer);
        }

        void ResetLayerValues(bool desireSelectedState, bool stateWillBeAnimated)
        {
            CATransaction.Begin();
            CATransaction.DisableActions = true;

            if ((desireSelectedState && stateWillBeAnimated) || (!desireSelectedState && !stateWillBeAnimated))
            {
                // Switch to deselected state
                checkmark.StrokeEnd = 0;
                checkmark.StrokeStart = 0;
                trailCircle.Opacity = 0;
                circle.StrokeStart = 0;
                circle.StrokeEnd = 1;
            }
            else
            {
                // Switch to selected state
                checkmark.StrokeEnd = finalStrokeEndForCheckmark;
                checkmark.StrokeStart = finalStrokeStartForCheckmark;
                trailCircle.Opacity = 1;
                circle.StrokeStart = 0;
                circle.StrokeEnd = 0;
            }

            CATransaction.Commit();
        }

        void AddAnimations(bool desireSelectedState)
        {
            var circleAnimationDuration = animationDuration * 0.5;

            var checkmarkEndDuration = animationDuration * 0.8;
            var checkmarkStartDuration = checkmarkEndDuration - circleAnimationDuration;
            var checkmarkBounceDuration = animationDuration - checkmarkEndDuration;

            var checkmarkAnimationGroup = new CAAnimationGroup();
            checkmarkAnimationGroup.RemovedOnCompletion = false;
            checkmarkAnimationGroup.FillMode = CAFillMode.Forwards;
            checkmarkAnimationGroup.Duration = animationDuration;
            checkmarkAnimationGroup.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear);

            var checkmarkStrokeEndAnimation = CAKeyFrameAnimation.FromKeyPath("strokeEnd");
            checkmarkStrokeEndAnimation.Duration = checkmarkEndDuration + checkmarkBounceDuration;
            checkmarkStrokeEndAnimation.RemovedOnCompletion = false;
            checkmarkStrokeEndAnimation.FillMode = CAFillMode.Forwards;
            checkmarkStrokeEndAnimation.CalculationMode = CAAnimation.AnimationPaced;

            if (desireSelectedState)
            {
                checkmarkStrokeEndAnimation.Values = new NSObject[] {   NSNumber.FromFloat(0),
                                                                        NSNumber.FromFloat(finalStrokeEndForCheckmark + checkmarkBounceAmount),
                                                                        NSNumber.FromFloat(finalStrokeEndForCheckmark) };

                checkmarkStrokeEndAnimation.KeyTimes = new NSNumber[] { NSNumber.FromDouble(0),
                                                                        NSNumber.FromDouble(checkmarkEndDuration),
                                                                        NSNumber.FromDouble(checkmarkEndDuration + checkmarkBounceDuration) };
            }
            else
            {
                checkmarkStrokeEndAnimation.Values = new NSObject[] {   NSNumber.FromFloat(finalStrokeEndForCheckmark),
                                                                        NSNumber.FromFloat(finalStrokeEndForCheckmark + checkmarkBounceAmount),
                                                                        NSNumber.FromFloat(-0.1f) };

                checkmarkStrokeEndAnimation.KeyTimes = new NSNumber[] { NSNumber.FromDouble(0),
                                                                        NSNumber.FromDouble(checkmarkBounceDuration),
                                                                        NSNumber.FromDouble(checkmarkEndDuration + checkmarkBounceDuration) };
            }

            var checkmarkStrokeStartAnimation = CAKeyFrameAnimation.FromKeyPath("strokeStart");
            checkmarkStrokeStartAnimation.Duration = checkmarkStartDuration + checkmarkBounceDuration;
            checkmarkStrokeStartAnimation.RemovedOnCompletion = false;
            checkmarkStrokeStartAnimation.FillMode = CAFillMode.Forwards;
            checkmarkStrokeStartAnimation.CalculationMode = CAAnimation.AnimationPaced;

            if (desireSelectedState)
            {
                checkmarkStrokeStartAnimation.Values = new NSObject[] {     NSNumber.FromFloat(0),
                                                                            NSNumber.FromFloat(finalStrokeStartForCheckmark + checkmarkBounceAmount),
                                                                            NSNumber.FromFloat(finalStrokeStartForCheckmark) };

                checkmarkStrokeStartAnimation.KeyTimes = new NSNumber[] {   NSNumber.FromDouble(0),
                                                                            NSNumber.FromDouble(checkmarkStartDuration),
                                                                            NSNumber.FromDouble(checkmarkStartDuration + checkmarkBounceDuration) };
            }
            else
            {
                checkmarkStrokeStartAnimation.Values = new NSObject[] {     NSNumber.FromFloat(finalStrokeStartForCheckmark),
                                                                            NSNumber.FromFloat(finalStrokeStartForCheckmark + checkmarkBounceAmount),
                                                                            NSNumber.FromFloat(0) };

                checkmarkStrokeStartAnimation.KeyTimes = new NSNumber[] {   NSNumber.FromDouble(0),
                                                                            NSNumber.FromDouble(checkmarkBounceDuration),
                                                                            NSNumber.FromDouble(checkmarkStartDuration + checkmarkBounceDuration) };
            }

            if (desireSelectedState)
            {
                checkmarkStrokeStartAnimation.BeginTime = circleAnimationDuration;
            }

            checkmarkAnimationGroup.Animations = new CAAnimation[] { checkmarkStrokeEndAnimation, checkmarkStrokeStartAnimation };
            checkmark.AddAnimation(checkmarkAnimationGroup, "checkmarkAnimation");

            var circleAnimationGroup = new CAAnimationGroup();
            circleAnimationGroup.RemovedOnCompletion = false;
            circleAnimationGroup.FillMode = CAFillMode.Forwards;
            circleAnimationGroup.Duration = animationDuration;
            circleAnimationGroup.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear);

            var circleStrokeEnd = CABasicAnimation.FromKeyPath("strokeEnd");
            circleStrokeEnd.Duration = circleAnimationDuration;

            if (desireSelectedState)
            {
                circleStrokeEnd.BeginTime = 0;
                circleStrokeEnd.SetFrom(NSNumber.FromFloat(1));
                circleStrokeEnd.SetTo(NSNumber.FromFloat(-0.1f));
            }
            else
            {
                circleStrokeEnd.BeginTime = animationDuration - circleAnimationDuration;
                circleStrokeEnd.SetFrom(NSNumber.FromFloat(0));
                circleStrokeEnd.SetTo(NSNumber.FromFloat(1));
            }

            circleStrokeEnd.RemovedOnCompletion = false;
            circleStrokeEnd.FillMode = CAFillMode.Forwards;

            circleAnimationGroup.Animations = new CAAnimation[] { circleStrokeEnd };

            if (animationDelegate == null)
                animationDelegate = new AnimationDelegate(this);
            circleAnimationGroup.Delegate = animationDelegate;

            circle.AddAnimation(circleAnimationGroup, "circleStrokeEnd");

            var trailCircleColor = CABasicAnimation.FromKeyPath("opacity");
            trailCircleColor.Duration = animationDuration;

            if (desireSelectedState)
            {
                trailCircleColor.SetFrom(NSNumber.FromFloat(0));
                trailCircleColor.SetTo(NSNumber.FromFloat(1));
            }
            else
            {
                trailCircleColor.SetFrom(NSNumber.FromFloat(1));
                trailCircleColor.SetTo(NSNumber.FromFloat(0));
            }
            trailCircleColor.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear);
            trailCircleColor.FillMode = CAFillMode.Forwards;
            trailCircleColor.RemovedOnCompletion = false;
            trailCircle.AddAnimation(trailCircleColor, "trailCircleColor");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Switches between selected and deselected state. Use this method to programmatically change the value of selected state.
        /// </summary>
        /// <param name="isSelected">Whether the switch should be selected or not</param>
        /// <param name="animated">Whether the transition should be animated or not</param>
        public void SetSelected(bool isSelected, bool animated)
        {
            _isSelected = isSelected;

            // Remove all animations before switching to new state
            checkmark.RemoveAllAnimations();
            circle.RemoveAllAnimations();
            trailCircle.RemoveAllAnimations();

            // Reset sublayer values
            ResetLayerValues(_isSelected, animated);

            // Animate to new state
            if (animated)
            {
                AddAnimations(_isSelected);
                this.AccessibilityValue = isSelected ? "checked" : "unchecked";
            }
        }

        public void AnimationDidStart()
        {
            this.SelectionAnimationDidStart(_isSelected);
        }

        public void AnimationDidStop()
        {
            this.SelectionAnimationDidStop(_isSelected);
        }

        #endregion
    }

    public class AnimationDelegate : CAAnimationDelegate
    {
        OHCheckbox control;
        public AnimationDelegate(UIControl ctrl)
        {
            if (ctrl is OHCheckbox)
            {
                control = ctrl as OHCheckbox;
            }
        }

        public override void AnimationStarted(CAAnimation anim)
        {
            control?.AnimationDidStart();
        }

        public override void AnimationStopped(CAAnimation anim, bool finished)
        {
            control?.AnimationDidStop();
        }
    }
}
