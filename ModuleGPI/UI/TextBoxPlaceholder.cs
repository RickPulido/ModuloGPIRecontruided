using System;
using System.Drawing;
using System.Windows.Forms;

namespace ModuleGPI.UI
{
    /// <summary>
    /// Helper para simular PlaceholderText en .NET Framework 4.8
    /// </summary>
    public static class TextBoxPlaceholder
    {
        private const string PlaceholderTag = "_placeholder_";

        /// <summary>
        /// Agrega placeholder text a un TextBox
        /// </summary>
        public static void SetPlaceholder(this TextBox textBox, string placeholder)
        {
            if (textBox == null || string.IsNullOrEmpty(placeholder))
                return;

            // Guardar el placeholder en el Tag
            textBox.Tag = PlaceholderTag + placeholder;
            textBox.ForeColor = Color.Gray;
            textBox.Text = placeholder;

            textBox.Enter += TextBox_Enter;
            textBox.Leave += TextBox_Leave;
        }

        private static void TextBox_Enter(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null || textBox.Tag == null) return;

            string tag = textBox.Tag.ToString();
            if (!tag.StartsWith(PlaceholderTag)) return;

            string placeholder = tag.Substring(PlaceholderTag.Length);

            if (textBox.Text == placeholder)
            {
                textBox.Text = "";
                textBox.ForeColor = SystemColors.WindowText;
            }
        }

        private static void TextBox_Leave(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null || textBox.Tag == null) return;

            string tag = textBox.Tag.ToString();
            if (!tag.StartsWith(PlaceholderTag)) return;

            string placeholder = tag.Substring(PlaceholderTag.Length);

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = placeholder;
                textBox.ForeColor = Color.Gray;
            }
        }

        /// <summary>
        /// Obtiene el valor real del TextBox (sin el placeholder)
        /// </summary>
        public static string GetRealText(this TextBox textBox)
        {
            if (textBox == null) return "";

            if (textBox.Tag != null && textBox.Tag.ToString().StartsWith(PlaceholderTag))
            {
                string placeholder = textBox.Tag.ToString().Substring(PlaceholderTag.Length);
                if (textBox.Text == placeholder)
                    return "";
            }

            return textBox.Text;
        }
    }
}