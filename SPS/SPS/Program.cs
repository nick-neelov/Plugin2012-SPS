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
			Insert.InsertEqvipment();	
		}

		/// <summary>
		/// Комманда для подключения оборудования 
		/// </summary>
		[CommandMethod("NK-SPS-CONNECTSPS")]
		public void SPSConnect()
		{
			ConnectZptSPS.ConnectSPS();
		}

		/// <summary>
		/// Метод для отрисовки структурной схемы схемы
		/// </summary>
		[CommandMethod("NK-SPS-DRAWSTRUCTURALSCHEME")]
		public void DrawStructuralScheme()
		{
			DrawScheme.DrawStructuralSchemeZPT();
		}		
	}	
}
