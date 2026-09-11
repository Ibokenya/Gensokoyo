using UnityEngine;

namespace ReplaySystem
{
    /// <summary>
    /// Unity KeyCode → 设置面板显示文本映射表（单行，不换行）。
    /// 方向键用符号，修饰键缩写（L-Shift / R-Ctrl），其他键直接显示枚举名缩写。
    /// </summary>
    public static class KeyCodeDisplayTable
    {
        public static string GetDisplayText(KeyCode code)
        {
            return code switch
            {
                KeyCode.UpArrow      => "↑",
                KeyCode.DownArrow    => "↓",
                KeyCode.LeftArrow    => "←",
                KeyCode.RightArrow   => "→",

                KeyCode.LeftShift    => "L-Shift",
                KeyCode.RightShift   => "R-Shift",
                KeyCode.LeftControl  => "L-Ctrl",
                KeyCode.RightControl => "R-Ctrl",
                KeyCode.LeftAlt      => "L-Alt",
                KeyCode.RightAlt     => "R-Alt",

                KeyCode.Space        => "Space",
                KeyCode.Return       => "Enter",
                KeyCode.KeypadEnter  => "KP-Enter",
                KeyCode.Backspace    => "Backspace",
                KeyCode.Tab          => "Tab",
                KeyCode.Escape       => "Esc",
                KeyCode.Delete       => "Del",
                KeyCode.Insert       => "Ins",
                KeyCode.Home         => "Home",
                KeyCode.End          => "End",
                KeyCode.PageUp       => "PgUp",
                KeyCode.PageDown     => "PgDn",

                KeyCode.LeftBracket  => "[",
                KeyCode.RightBracket => "]",
                KeyCode.Backslash    => "\\",
                KeyCode.Semicolon    => ";",
                KeyCode.Colon        => ":",
                KeyCode.Quote        => "\"",
                KeyCode.Comma        => ",",
                KeyCode.Period       => ".",
                KeyCode.Slash        => "/",
                KeyCode.Ampersand    => "&",
                KeyCode.Asterisk     => "*",
                KeyCode.Plus         => "+",
                KeyCode.Minus        => "-",
                KeyCode.Underscore   => "_",
                KeyCode.Equals       => "=",
                KeyCode.BackQuote    => "`",
                KeyCode.Percent      => "%",

                KeyCode.Alpha0 => "0",
                KeyCode.Alpha1 => "1",
                KeyCode.Alpha2 => "2",
                KeyCode.Alpha3 => "3",
                KeyCode.Alpha4 => "4",
                KeyCode.Alpha5 => "5",
                KeyCode.Alpha6 => "6",
                KeyCode.Alpha7 => "7",
                KeyCode.Alpha8 => "8",
                KeyCode.Alpha9 => "9",

                KeyCode.A => "A", KeyCode.B => "B", KeyCode.C => "C",
                KeyCode.D => "D", KeyCode.E => "E", KeyCode.F => "F",
                KeyCode.G => "G", KeyCode.H => "H", KeyCode.I => "I",
                KeyCode.J => "J", KeyCode.K => "K", KeyCode.L => "L",
                KeyCode.M => "M", KeyCode.N => "N", KeyCode.O => "O",
                KeyCode.P => "P", KeyCode.Q => "Q", KeyCode.R => "R",
                KeyCode.S => "S", KeyCode.T => "T", KeyCode.U => "U",
                KeyCode.V => "V", KeyCode.W => "W", KeyCode.X => "X",
                KeyCode.Y => "Y", KeyCode.Z => "Z",

                KeyCode.F1  => "F1",
                KeyCode.F2  => "F2",
                KeyCode.F3  => "F3",
                KeyCode.F4  => "F4",
                KeyCode.F5  => "F5",
                KeyCode.F6  => "F6",
                KeyCode.F7  => "F7",
                KeyCode.F8  => "F8",
                KeyCode.F9  => "F9",
                KeyCode.F10 => "F10",
                KeyCode.F11 => "F11",
                KeyCode.F12 => "F12",
                KeyCode.F13 => "F13",
                KeyCode.F14 => "F14",
                KeyCode.F15 => "F15",

                KeyCode.Keypad0 => "KP-0",
                KeyCode.Keypad1 => "KP-1",
                KeyCode.Keypad2 => "KP-2",
                KeyCode.Keypad3 => "KP-3",
                KeyCode.Keypad4 => "KP-4",
                KeyCode.Keypad5 => "KP-5",
                KeyCode.Keypad6 => "KP-6",
                KeyCode.Keypad7 => "KP-7",
                KeyCode.Keypad8 => "KP-8",
                KeyCode.Keypad9 => "KP-9",
                KeyCode.KeypadPlus     => "KP-+",
                KeyCode.KeypadMinus    => "KP--",
                KeyCode.KeypadMultiply => "KP-*",
                KeyCode.KeypadDivide   => "KP-/",
                KeyCode.KeypadPeriod   => "KP-.",
                KeyCode.KeypadEquals   => "KP-=",

                KeyCode.RightCommand => "R-Cmd",
                KeyCode.LeftCommand  => "L-Cmd",

                // 兜底：直接返回枚举名
                _ => code.ToString()
            };
        }
    }
}
