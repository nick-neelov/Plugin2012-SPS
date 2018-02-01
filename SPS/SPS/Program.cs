using System;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

[assembly : CommandClass(typeof(Neelov.AutocadPlugin.Program))]

namespace Neelov.AutocadPlugin
{

	/// <summary>
	/// Реализующий запуск программ (плагинов) для проектирования палатной сигнализации
	/// </summary>
    public class Program
    {	
		/// <summary>
		/// Комманда для вставки оборудования на планы
		/// </summary>
		[CommandMethod("NK-SPS-INSERTEQVIPMENT")]
		public void InsertEqvipment()
		{
			WorkWithPlans.InsertEqvipment();	
		}

		/// <summary>
		/// Комманда для подключения оборудования 
		/// </summary>
		[CommandMethod("NK-SPS-CONNECTEQVEPMENT")]
		public void ConnectEqvipment()
		{

		}

		/// <summary>
		/// Метод для отрисовки структурной схемы схемы
		/// </summary>
		[CommandMethod("NK-SPS-DRAWSTRUCTURALSCHEME")]
		public void DrawStructuralScheme()
		{

		}		
	}
	

	/// <summary>
	/// Класс реализующий фильтрацию сообщений
	/// </summary>
	public class MyMessageFilter : System.Windows.Forms.IMessageFilter
	{
		public const int WM_KEYDOWN = 0x0100;
		public const int WM_KEYUP = 0x0101;

		public bool bCancaled = false;

		public bool PreFilterMessage(ref System.Windows.Forms.Message m)
		{
			if (m.Msg == WM_KEYDOWN || m.Msg == WM_KEYUP)
			{
				// Проверяем нажание ESC
				System.Windows.Forms.Keys kc = (System.Windows.Forms.Keys)(int)m.WParam & System.Windows.Forms.Keys.KeyCode;

				if (kc == System.Windows.Forms.Keys.Escape)
				{
					bCancaled = true;
					return true;
				}							
			}
			return false;
		}
	}



}
