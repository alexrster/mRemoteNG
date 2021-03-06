using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using mRemoteNG.App;
using mRemoteNG.Forms;
using mRemoteNG.Messages;
using mRemoteNG.UI.Window;
using static System.String;

namespace mRemoteNG.Tools
{
    public class MiscTools
	{
		private struct SHFILEINFO
		{
			public IntPtr hIcon; // : icon
			//public int iIcon; // : icondex
			//public int dwAttributes; // : SFGAO_ flags
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
		}
			
		[DllImport("shell32.dll")]
        private static  extern IntPtr SHGetFileInfo(string pszPath, int dwFileAttributes, ref SHFILEINFO psfi, int cbFileInfo, int uFlags);
		private const int SHGFI_ICON = 0x100;
		private const int SHGFI_SMALLICON = 0x1;
		//Private Const SHGFI_LARGEICON = &H0    ' Large icon


		public static Icon GetIconFromFile(string FileName)
		{
		    try
		    {
		        return File.Exists(FileName) == false ? null : Icon.ExtractAssociatedIcon(FileName);
		    }
		    catch (ArgumentException AEx)
		    {
		        Runtime.MessageCollector.AddMessage(MessageClass.WarningMsg,
		            "GetIconFromFile failed (Tools.Misc) - using default icon" + Environment.NewLine + AEx.Message, true);
                return Resources.mRemote_Icon;

		    }
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(MessageClass.WarningMsg, "GetIconFromFile failed (Tools.Misc)" + Environment.NewLine + ex.Message, true);
				return null;
			}
		}
		

		
		

		public static string PasswordDialog(string passwordName = null, bool verify = true)
		{
			PasswordForm passwordForm = new PasswordForm(passwordName, verify);
				
			return passwordForm.ShowDialog() == DialogResult.OK ? passwordForm.Password : "";
		}
		

		public static string CreateConstantID()
		{
			return Guid.NewGuid().ToString();
		}
		

		public static string LeadingZero(string Number)
		{
		    if (Convert.ToInt32(Number) < 10)
			{
				return "0" + Number;
			}
		    return Number;
		}


        public static string DBDate(DateTime Dt)
		{
		    var strDate = Dt.Year + LeadingZero(Convert.ToString(Dt.Month)) + LeadingZero(Convert.ToString(Dt.Day)) + " " + LeadingZero(Convert.ToString(Dt.Hour)) + ":" + LeadingZero(Convert.ToString(Dt.Minute)) + ":" + LeadingZero(Convert.ToString(Dt.Second));
		    return strDate;
		}

        public static string PrepareForDB(string Text)
		{
			return ReplaceBooleanStringsWithNumbers(Text);
		}
        private static string ReplaceBooleanStringsWithNumbers(string Text)
        {
            Text = ReplaceTrueWith1(Text);
            Text = ReplaceFalseWith0(Text);
            return Text;
        }
        private static string ReplaceTrueWith1(string Text)
        {
            return Text.Replace("'True'", "1");
        }
        private static string ReplaceFalseWith0(string Text)
        {
            return Text.Replace("'False'", "0");
        }
		public static string PrepareValueForDB(string Text)
		{
			return Text.Replace("\'", "\'\'");
		}
		

		public static object StringToEnum(Type t, string value)
		{
			return Enum.Parse(t, value);
		}


        public static string GetExceptionMessageRecursive(Exception ex)
        {
            return GetExceptionMessageRecursive(ex, Environment.NewLine);
        }
        private static string GetExceptionMessageRecursive(Exception ex, string separator)
		{
			string message = ex.Message;
			if (ex.InnerException != null)
			{
				string innerMessage = GetExceptionMessageRecursive(ex.InnerException, separator);
				message = Join(separator, message, innerMessage);
			}
			return message;
		}
		

		public static Image TakeScreenshot(ConnectionWindow sender)
		{
			try
			{
				int LeftStart = sender.TabController.SelectedTab.PointToScreen(new Point(sender.TabController.SelectedTab.Left)).X; //Me.Left + Splitter.SplitterDistance + 11
				int TopStart = sender.TabController.SelectedTab.PointToScreen(new Point(sender.TabController.SelectedTab.Top)).Y; //Me.Top + Splitter.Top + TabController.Top + TabController.SelectedTab.Top * 2 - 3
				int LeftWidth = sender.TabController.SelectedTab.Width; //Me.Width - (Splitter.SplitterDistance + 16)
				int TopHeight = sender.TabController.SelectedTab.Height; //Me.Height - (Splitter.Top + TabController.Top + TabController.SelectedTab.Top * 2 + 2)
					
				Size currentFormSize = new Size(LeftWidth, TopHeight);
				Bitmap ScreenToBitmap = new Bitmap(LeftWidth, TopHeight);
                Graphics gGraphics = Graphics.FromImage(ScreenToBitmap);
					
				gGraphics.CopyFromScreen(new Point(LeftStart, TopStart), new Point(0, 0), currentFormSize);
					
				return ScreenToBitmap;
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, "Taking Screenshot failed" + Environment.NewLine + ex.Message, true);
			}
				
			return null;
		}
		
		public class EnumTypeConverter : EnumConverter
		{
			private Type _enumType;
				
			public EnumTypeConverter(Type type) : base(type)
			{
				_enumType = type;
			}
				
			public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
			{
				return destType == typeof(string);
			}
				
			public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
			{
			    if (value != null)
			    {
			        FieldInfo fi = _enumType.GetField(Enum.GetName(_enumType, value: value));
			        DescriptionAttribute dna = (DescriptionAttribute) (Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute)));
					
			        return dna != null ? dna.Description : value.ToString();
			    }

			    return null;
			}
				
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
			{
				return srcType == typeof(string);
			}
				
			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				foreach (FieldInfo fi in _enumType.GetFields())
				{
					DescriptionAttribute dna = (DescriptionAttribute) (Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute)));
						
					if ((dna != null) && ((string) value == dna.Description))
					{
						return Enum.Parse(_enumType, fi.Name);
					}
				}
					
				return Enum.Parse(_enumType, (string) value);
			}
		}
		
		public class YesNoTypeConverter : TypeConverter
		{
				
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				if (sourceType == typeof(string))
				{
					return true;
				}
					
				return base.CanConvertFrom(context, sourceType);
			}
				
			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			{
				if (destinationType == typeof(string))
				{
					return true;
				}
					
				return base.CanConvertTo(context, destinationType);
			}
				
			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				if (value is string)
				{
					if (string.Equals(value.ToString(), Language.strYes, StringComparison.CurrentCultureIgnoreCase))
					{
						return true;
					}
						
					if (string.Equals(value.ToString(), Language.strNo, StringComparison.CurrentCultureIgnoreCase))
					{
						return false;
					}
						
					throw (new Exception("Values must be \"Yes\" or \"No\""));
				}
					
				return base.ConvertFrom(context, culture, value);
			}
				
			public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			{
				if (destinationType == typeof(string))
				{
					return ((Convert.ToBoolean(value)) ? Language.strYes : Language.strNo);
				}
					
				return base.ConvertTo(context, culture, value, destinationType);
			}
				
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
				return true;
			}
				
			public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				bool[] bools = {true, false};
					
				StandardValuesCollection svc = new StandardValuesCollection(bools);
					
				return svc;
			}
		}
		
		public class Fullscreen
		{
			public Fullscreen(Form handledForm)
			{
				_handledForm = handledForm;
			}
				
			private Form _handledForm;
			private FormWindowState _savedWindowState;
			private FormBorderStyle _savedBorderStyle;
			private Rectangle _savedBounds;
				
			private bool _value;
            public bool Value
			{
				get
				{
					return _value;
				}
				set
				{
					if (_value == value)
					{
						return ;
					}
					if (!_value)
					{
						EnterFullscreen();
					}
					else
					{
						ExitFullscreen();
					}
					_value = value;
				}
			}
				
			private void EnterFullscreen()
			{
				_savedBorderStyle = _handledForm.FormBorderStyle;
				_savedWindowState = _handledForm.WindowState;
				_savedBounds = _handledForm.Bounds;
					
				_handledForm.FormBorderStyle = FormBorderStyle.None;
				if (_handledForm.WindowState == FormWindowState.Maximized)
				{
					_handledForm.WindowState = FormWindowState.Normal;
				}
				_handledForm.WindowState = FormWindowState.Maximized;
			}
			private void ExitFullscreen()
			{
				_handledForm.FormBorderStyle = _savedBorderStyle;
				_handledForm.WindowState = _savedWindowState;
				_handledForm.Bounds = _savedBounds;
			}
		}
	}
}