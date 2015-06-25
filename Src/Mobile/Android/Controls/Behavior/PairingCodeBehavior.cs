using System;
using apcurium.MK.Common.Extensions;
using Android.Views;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Behavior
{
    public static class PairingCodeBehavior
    {
        private const int PairingCode1MaxLength = 3;
        private const int PairingCode2MaxLength = 4;

        public static void ApplyTo(EditText pairingCodeEditText1, EditText pairingCodeEditText2)
        {
            pairingCodeEditText1.KeyPress += (sender, args) =>
            {
                // We need to ignore the KeyUp event as well as any events if we have lower then 3 characters in the left textbox.
                if (args.Event.Action == KeyEventActions.Up || pairingCodeEditText1.Text.Length < PairingCode1MaxLength)
                {
                    args.Handled = false;
                    return;
                }

                var keyPressed = GetNumberOrDefault(args.KeyCode);

                var isCaretAtEnd = pairingCodeEditText1.SelectionStart == pairingCodeEditText1.Text.Length;

                // We check if the caret is at the end of the first textbox And we have typed a number And the second textbox is not at max length.
                if (pairingCodeEditText2.Text.Length < PairingCode2MaxLength && keyPressed.HasValue() && isCaretAtEnd)
                {
                    pairingCodeEditText2.Text = keyPressed + pairingCodeEditText2.Text;
                    pairingCodeEditText2.RequestFocus();
                    pairingCodeEditText2.SetSelection(1);
                    return;
                }

                args.Handled = false;
            };

            pairingCodeEditText2.KeyPress += (sender, args) =>
            {
                // We need to ignore the KeyUp event and only do the modifications on KeyDown.
                if (args.Event.Action == KeyEventActions.Up)
                {
                    args.Handled = false;
                    return;
                }

                if (pairingCodeEditText2.Text.Length == 0 && args.KeyCode == Keycode.Del)
                {
                    pairingCodeEditText1.Text = pairingCodeEditText1.Text.Substring(0, pairingCodeEditText1.Text.Length - 1);
                    pairingCodeEditText1.RequestFocus();
                    pairingCodeEditText1.SetSelection(pairingCodeEditText1.Text.Length);

                    return;
                }

                args.Handled = false;
            };
        }

        private static string GetNumberOrDefault(Keycode code)
        {
            switch (code)
            {
                case Keycode.Num0:
                case Keycode.Numpad0:
                    return "0";
                case Keycode.Num1:
                case Keycode.Numpad1:
                    return "1";
                case Keycode.Num2:
                case Keycode.Numpad2:
                    return "2";
                case Keycode.Num3:
                case Keycode.Numpad3:
                    return "3";
                case Keycode.Num4:
                case Keycode.Numpad4:
                    return "4";
                case Keycode.Num5:
                case Keycode.Numpad5:
                    return "5";
                case Keycode.Num6:
                case Keycode.Numpad6:
                    return "6";
                case Keycode.Num7:
                case Keycode.Numpad7:
                    return "7";
                case Keycode.Num8:
                case Keycode.Numpad8:
                    return "8";
                case Keycode.Num9:
                case Keycode.Numpad9:
                    return "9";
                default:
                    return null;
            }
        }
    }
}