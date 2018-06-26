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
		[CommandMethod("NK-INSERTEQVIPMENT")]
		public void InsertEqvipment()
		{
			Insert.InsertEqvipment();	
		}

		///// <summary>
		///// Комманда для подключения оборудования 
		///// </summary>
		//[CommandMethod("NK-SPS-CONNECT")]
		//public void SPSConnect()
		//{
		//	ConnectSPS.Connect();
		//}

		/// <summary>
		/// Комманда для подключения оборудования 
		/// </summary>
		[CommandMethod("NK-AV03-CONNECT")]
		public void Connect()
		{
			AV03_Connect.Connect();
		}


		///// <summary>
		///// Метод для отрисовки структурной схемы схемы
		///// </summary>
		//[CommandMethod("NK-SPS-DRAWSTRUCTURALSCHEME")]
		//public void DrawStructuralScheme()
		//{
		//	DrawingOfThePersonnelCallSystemZPT.DrawStructuralSchemeZPT();
		//}		


		[CommandMethod("NK-AV03-STRUCTURALSCHEME")]
			public void StructialScheme()
		{
			AV03_DrawScheme.AV03_Scheme();
		}
	}	
}
