using System;
using System.Diagnostics;
using CoreGraphics;
using UIKit;
using OHCheckboxLib;

namespace Sample
{
    public partial class ViewController : UIViewController
    {
        protected ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var ohCheckbox = new OHCheckbox(new CGRect((UIScreen.MainScreen.Bounds.Width - 50) / 2, (UIScreen.MainScreen.Bounds.Height - 26) / 2, 50, 50))
            {
                LineWidth = 2,
                StrokeColor = UIColor.Blue,
                TrailStrokeColor = UIColor.Red.ColorWithAlpha(0.2f),
                AnimatesOnTouch = false
            };

            ohCheckbox.ValueChanged += HandleCheckboxValueChanged;
            ohCheckbox.SelectionAnimationDidStart += OhCheckbox_SelectionAnimationDidStart;
            ohCheckbox.SelectionAnimationDidStop += OhCheckbox_SelectionAnimationDidStop;

            View.AddSubview(ohCheckbox);

            //ohCheckbox.SetSelected(true, true);
        }

        void OhCheckbox_SelectionAnimationDidStart(bool isSelected)
        {
            Debug.WriteLine("New State: " + isSelected);
        }

        void OhCheckbox_SelectionAnimationDidStop(bool isSelected)
        {
            Debug.WriteLine("State when animation stopped: " + isSelected);
        }

        private void HandleCheckboxValueChanged(object sender, EventArgs e)
        {
            if (sender is OHCheckbox)
            {
                var checkbox = sender as OHCheckbox;
                Debug.WriteLine("Checkbox is touched. Status: " + checkbox.IsSelected);
            }
        }
    }
}
