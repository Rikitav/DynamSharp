using ICSharpCode.AvalonEdit;
using System;
using System.ComponentModel;
using System.Windows;

namespace SharpIdeMini.ApplicationInterface.Controls
{
    public class AvalonCodeEditor : TextEditor, INotifyPropertyChanged
    {
        /// <summary>
        /// A bindable Text property
        /// </summary>
        public new string Text
        {
            get => (string)GetValue(TextProperty);
            set
            {
                SetValue(TextProperty, value);
                RaisePropertyChanged("Text");
            }
        }

        protected static void OnDependencyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            AvalonCodeEditor target = (AvalonCodeEditor)obj;
            if (target.Document != null)
            {
                int caretOffset = target.CaretOffset;
                string newValue = (string)args.NewValue ?? string.Empty;

                target.Document.Text = newValue;
                target.CaretOffset = Math.Min(caretOffset, newValue.ToString().Length);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (Document != null)
                Text = Document.Text;

            base.OnTextChanged(e);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged(string property)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(AvalonCodeEditor),
            new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDependencyPropertyChanged));
    }
}
