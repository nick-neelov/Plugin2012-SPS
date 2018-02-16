
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Internal;




namespace Neelov.AutocadPlugin
{
	/// <summary>
	/// Класс реализующий подключени оборудования
	/// </summary>
	class ConnectEqvipment
	{

		public static void ConnectSPS()
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			if (doc == null) return;

			Editor ed = doc.Editor;
			Database db = doc.Database;

			// Определения блоков
			BlockReference brFirstBlock;
			BlockReference brSecondBlock;

			// Точки вставки блоков
			Point3d pFirstBlock;
			Point3d pSecondBlock;
			

			// Список тегов атрибутов блоков
			List<string> listTag = new List<string>()
			{
				"01", "02", "03", "04", "05", "06", "07", "08", "09", "10",
				"11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
				"21", "22", "23", "24", "25", "26", "27", "28", "29", "30"
			};

			List<string> atFirstBlock = new List<string>();
			List<string> atSecondBlock = new List<string>();


			// Поля для атрибутов блока 1
			string fRoom;
			string fNumberFloor;
			string fPosition;
			string fDistanceTo;
			string fNumberInputs;			
			string fNumberInSystem;
			string fHeight;

			// Поля для атрибутов блока 2
			string sRoom;
			string sPosition;
			string sDistanceTo;
			string sNumberInputs;
			string sNumberFloor;
			string sNumberInSystem;
			string sHeight;


			// Выбираем первый блок, к которому подключаемся		
			PromptSelectionOptions psoFirstBlock = new PromptSelectionOptions();
			psoFirstBlock.MessageForAdding = "\nВыберите оборудование, к которому подключаемся: ";
			PromptSelectionResult psrFirstBlock = ed.GetSelection(psoFirstBlock);

			if (psrFirstBlock.Status != PromptStatus.OK) { return; }	

			SelectionSet firstSS = psrFirstBlock.Value;		

			// Работаем с блоком, к которому подключаемся
			using (Transaction trFirst = db.TransactionManager.StartTransaction())
			{
				try
				{
					foreach (SelectedObject so in firstSS)
					{
						if (so != null)
						{
							brFirstBlock = trFirst.GetObject(so.ObjectId, OpenMode.ForWrite) as BlockReference;

							// Получаем точку вставки блока
							pFirstBlock = brFirstBlock.Position;
														
							// Получаем атрибуты блока 
							fRoom = Common.Attributes.GetAttributre(brFirstBlock, "01");
							fNumberFloor = Common.Attributes.GetAttributre(brFirstBlock, "02");
							fPosition = Common.Attributes.GetAttributre(brFirstBlock, "03");
							fDistanceTo = Common.Attributes.GetAttributre(brFirstBlock, "04");






							// Меням временно слой
							brFirstBlock.Layer = "!СС Метки не печатается";
						}
					}
					trFirst.Commit();
				}
				finally
				{
					trFirst.Dispose();
				}
			}
			
				





			// Выбираем второй блок			
			PromptSelectionOptions psoSecondBlock = new PromptSelectionOptions();

			psoSecondBlock.MessageForAdding = "\nВыберите оборудование которое подключаем: ";			
			PromptSelectionResult psrSecondBlock = ed.GetSelection(psoSecondBlock);

			if (psrSecondBlock.Status != PromptStatus.OK) { return; }
			SelectionSet secondSS = psrSecondBlock.Value;

			// Работаем с блоком, который подключаем
			using (Transaction trSecond = db.TransactionManager.StartTransaction())
			{
				try
				{
					foreach (SelectedObject so in secondSS)
					{
						if (so != null)
						{
							brSecondBlock = trSecond.GetObject(so.ObjectId, OpenMode.ForWrite) as BlockReference;

							
							


							// Устанавливаем слой после подключения
							brSecondBlock.Layer = "!СС Оборудование";
						}
					}

					trSecond.Commit();
				}
				finally
				{
					trSecond.Dispose();
				}
			}







		}
	}
}
