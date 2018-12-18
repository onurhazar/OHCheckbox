# OHCheckbox
Customizable checkbox written in C# for Xamarin iOS. Can be used instead of UISwitch.

## Requirements
iOS 8.0+
Xcode 8.0+

## How to Use
```C#
var frame = new CGRect(0, 0, 50, 50);
var ohCheckbox = new OHCheckbox(frame)
{
    LineWidth = 2,
    StrokeColor = UIColor.Blue,
    TrailStrokeColor = UIColor.Red.ColorWithAlpha(0.2f),
    AnimatesOnTouch = false
};
View.AddSubview(ohCheckbox);

//Add event handler
ohCheckbox.ValueChanged += HandleCheckboxValueChanged;
ohCheckbox.SelectionAnimationDidStart += OhCheckbox_SelectionAnimationDidStart;
ohCheckbox.SelectionAnimationDidStop += OhCheckbox_SelectionAnimationDidStop;

//Checkbox status check
if (ohCheckbox.IsSelected)
{
    Debug.WriteLine("Checkbox is checked, do something here");
}

//Set checkbox status checked/unchecked, w/ or w/o Animation
ohCheckbox.SetSelected(true, true);
```
