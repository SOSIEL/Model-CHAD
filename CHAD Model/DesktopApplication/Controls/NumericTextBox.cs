using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DesktopApplication.Controls
{
    public enum InputType
    {
        Default,
        Integer,
        Decimal
    }

    public class ChadTextBox : TextBox
    {
        #region Static Fields and Constants

        public static readonly DependencyProperty InputTypeProperty =
            DependencyProperty.Register("InputType", typeof(InputType), typeof(ChadTextBox),
                new PropertyMetadata(InputType.Default));

        #endregion

        #region Properties, Indexers

        public InputType InputType
        {
            get => (InputType) GetValue(InputTypeProperty);
            set => SetValue(InputTypeProperty, value);
        }

        #endregion

        #region All other members

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            var fullText = Text.Insert(SelectionStart, e.Text);

            switch (InputType)
            {
                case InputType.Integer:
                    e.Handled = !int.TryParse(fullText, out _);
                    break;
                case InputType.Decimal:
                    e.Handled = !decimal.TryParse(fullText, out var value);
                    var integer = Math.Truncate(value);
                    var fraction = value - integer;
                    if (fraction.ToString(CultureInfo.InvariantCulture).Length > 4)
                        e.Handled = true;
                    break;
            }
        }

        #endregion
    }
}