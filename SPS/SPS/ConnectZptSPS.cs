
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
	class ConnectZptSPS
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
			Point3d pFirstBlock = new Point3d();
			Point3d pSecondBlock = new Point3d();

			// Старый слой для блока 1
			string oldLayerBlock = "";

			// Поля для атрибутов блока 1
			string fRoom = "";
			string fMove = "";
			string fRotate = "";
			string fName = "";
			string fMagistralFreeInputs = "";
			string fAbonentFreeInputs = "";
			string fHeight = "";
			string fNumberInSystem = "";
			string fNameInSystem = "";
			string fNumberPrevEqvipment = "";
			string fPrevRoom = "";
			string fDistanceTo = "";
			string fCabelType = "";
			
			// Поля для атрибутов блока 2
			string sRoom = "";
			string sMove = "";
			string sRotate = "";
			string sName = "";
			string sMagistralFreeInputs = "";
			string sAbonentFreeInputs = "";
			string sHeight = "";
			string sNumberInSystem = "";
			string sNameInSystem = "";
			string sNumberPrevEqvipment = "";
			string sPrevRoom = "";
			string sDistanceTo = "";
			string sCabelType = "";
			
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
														
							// Получаем атрибуты блока 1
							fRoom = Common.Attributes.GetAttributre(brFirstBlock, "1"); // Номер помещения 
							fMove = Common.Attributes.GetAttributre(brFirstBlock, "2"); // Значение сдвига блока
							fRotate = Common.Attributes.GetAttributre(brFirstBlock, "3"); // Поворот блока
							fName = Common.Attributes.GetAttributre(brFirstBlock, "6"); // Имя блока
							fMagistralFreeInputs = Common.Attributes.GetAttributre(brFirstBlock, "7"); // Количество свободных магистральных линий
							fAbonentFreeInputs = Common.Attributes.GetAttributre(brFirstBlock, "8"); // Количество свободных абонентских линий
							fHeight = Common.Attributes.GetAttributre(brFirstBlock, "10"); // Высота установки оборудования
							fNumberInSystem = Common.Attributes.GetAttributre(brFirstBlock, "11"); // Номер в системе
							fNameInSystem = Common.Attributes.GetAttributre(brFirstBlock, "12"); // Обозначение в  системе
							fNumberPrevEqvipment = Common.Attributes.GetAttributre(brFirstBlock, "13"); // Обозначение оборудования к которому подключено
							fPrevRoom = Common.Attributes.GetAttributre(brFirstBlock, "14"); // Номер помещения оборудования, к которому подключаемся
							fDistanceTo = Common.Attributes.GetAttributre(brFirstBlock, "15"); // Длина до оборудования, к которому подключаемся
							fCabelType = Common.Attributes.GetAttributre(brFirstBlock, "16"); // Марка кабеля

							// Меням временно слой
							oldLayerBlock = brFirstBlock.Layer;
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

							// Получаем точку вставки блока
							pSecondBlock = brSecondBlock.Position;
							
							// Получаем атрибуты блока 2 
							// НУЖНО ЛИ ЭТО ДЕЛАТЬ?!!
							sRoom = Common.Attributes.GetAttributre(brSecondBlock, "1"); // Номер помещения 
							sMove = Common.Attributes.GetAttributre(brSecondBlock, "2"); // Значение сдвига блока
							sRotate = Common.Attributes.GetAttributre(brSecondBlock, "3"); // Поворот блока
							sName = Common.Attributes.GetAttributre(brSecondBlock, "6"); // Имя блока
							sMagistralFreeInputs = Common.Attributes.GetAttributre(brSecondBlock, "7"); // Количество свободных магистральных линий
							sAbonentFreeInputs = Common.Attributes.GetAttributre(brSecondBlock, "8"); // Количество свободных абонентских линий
							sHeight = Common.Attributes.GetAttributre(brSecondBlock, "10"); // Высота установки оборудования
							sNumberInSystem = Common.Attributes.GetAttributre(brSecondBlock, "11"); // Номер в системе
							sNameInSystem = Common.Attributes.GetAttributre(brSecondBlock, "12"); // Обозначение в  системе
							sNumberPrevEqvipment = Common.Attributes.GetAttributre(brSecondBlock, "13"); // Обозначение оборудования к которому подключено *
							sPrevRoom = Common.Attributes.GetAttributre(brSecondBlock, "14"); // Номер помещения оборудования, к которому подключаемся *
							sDistanceTo = Common.Attributes.GetAttributre(brSecondBlock, "15"); // Длина до оборудования, к которому подключаемся *
							sCabelType = Common.Attributes.GetAttributre(brSecondBlock, "16"); // Марка кабеля *

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


			//Производим общие вычисления
			double distanceToBlock = (Methods.DictanceBetweenBlocks(pFirstBlock, pSecondBlock) + 8000)  / 1000 * 1.2; // расстояние между блоками с учетом 4 м запаса

			// Вычисления для блока 1
			// Выполняем подключение магистрального оборудования
			if (sName == "SM" || sName == "RT")
			{
				// Если есть свободные порты для подключения
				if (Convert.ToInt32(fMagistralFreeInputs) != 0)
				{
					if (fNumberInSystem == "")
					{
						fNumberInSystem = ed.GetString("\nУкажите номер оборудования в системе: ").StringResult;
						fNameInSystem = fName + "." + fNameInSystem;

					}
					else
					{						
						fMagistralFreeInputs = Convert.ToString(Convert.ToInt32(fMagistralFreeInputs) - 1);					
					}

				}
				else
				{
					ed.WriteMessage("\nОтсутствуют свободные порты для подключения магистральных линий");
					return;
				}

			}

			// Выполняем подключение абонентского оборудования
			else if (fName == "KJ" || fName == "KJD" || fName == "SIJ" || fName == "TNV" || fName == "TANV" || fName == "TANVT")
			{
				// Если есть свободные порты
				if (Convert.ToInt32(fAbonentFreeInputs) != 0)
				{
					if (fNumberInSystem == "")
					{
						fNumberInSystem = ed.GetString("\nУкажите номер оборудования в системе: ").StringResult;						 
					}
					else
					{
						fNumberInSystem = Convert.ToString(Convert.ToInt32(fNumberInSystem) + 1);
						fMagistralFreeInputs = Convert.ToString(Convert.ToInt32(fMagistralFreeInputs) - 1);
						fNameInSystem = fName + "." + fNameInSystem;
					}
				}
				else
				{
					ed.WriteMessage("\nОтсутствуют свободные порты для подключения абонентских линий");
					return;
				}
			}

			// Вычисления для блока 2

			sNumberPrevEqvipment = fNameInSystem; // Обозначение предыдущего оборудования

			sDistanceTo = Convert.ToString(distanceToBlock); // Растояние до предыдущего блока

			sPrevRoom = fRoom; // Номер помещения предыдущего блока
			
			// Определяем тип кабеля
			if (fName == "RT")
			{
				sCabelType = "A";
			}
			else if (fName == "SM")
			{
				sCabelType = "A+B";
			}
			else if (fName == "KJ" || fName == "KJD" || fName == "SIJ" || fName == "TNV" || fName == "TANV" || fName == "TANVT")
			{
				sCabelType = "C";
			}
			else
			{
				sCabelType = "";
			}

			// Записываем данные в блок 1 и вставляем текст с обозначением оборудования
			using (Transaction trFirst = db.TransactionManager.StartTransaction())
			{
				try
				{
					foreach (SelectedObject so in firstSS)
					{
						if (so != null)
						{
							brFirstBlock = trFirst.GetObject(so.ObjectId, OpenMode.ForWrite) as BlockReference;

							Common.Attributes.SetAttribute(brFirstBlock, "7", fMagistralFreeInputs);
							Common.Attributes.SetAttribute(brFirstBlock, "8", fAbonentFreeInputs);
							Common.Attributes.SetAttribute(brFirstBlock, "11", fNumberInSystem);
							Common.Attributes.SetAttribute(brFirstBlock, "12", fNameInSystem);





							//sRoom = Common.Attributes.GetAttributre(brSecondBlock, "1"); // Номер помещения 
							//sMove = Common.Attributes.GetAttributre(brSecondBlock, "2"); // Значение сдвига блока
							//sRotate = Common.Attributes.GetAttributre(brSecondBlock, "3"); // Поворот блока
							//sName = Common.Attributes.GetAttributre(brSecondBlock, "6"); // Имя блока
							//sMagistralFreeInputs = Common.Attributes.GetAttributre(brSecondBlock, "7"); // Количество свободных магистральных линий
							//sAbonentFreeInputs = Common.Attributes.GetAttributre(brSecondBlock, "8"); // Количество свободных абонентских линий
							//sHeight = Common.Attributes.GetAttributre(brSecondBlock, "10"); // Высота установки оборудования
							//sNumberInSystem = Common.Attributes.GetAttributre(brSecondBlock, "11"); // Номер в системе
							//sNameInSystem = Common.Attributes.GetAttributre(brSecondBlock, "12"); // Обозначение в  системе
							//sNumberPrevEqvipment = Common.Attributes.GetAttributre(brSecondBlock, "13"); // Обозначение оборудования к которому подключено *
							//sPrevRoom = Common.Attributes.GetAttributre(brSecondBlock, "14"); // Номер помещения оборудования, к которому подключаемся *
							//sDistanceTo = Common.Attributes.GetAttributre(brSecondBlock, "15"); // Длина до оборудования, к которому подключаемся *
							//sCabelType = Common.Attributes.GetAttributre(brSecondBlock, "16"); // Марка кабеля *


							// Устанавливаем слой после подключения
							brFirstBlock.Layer = oldLayerBlock;
						}
					}

					trFirst.Commit();
				}
				finally
				{
					trFirst.Dispose();
				}

			}


			// Записываем данные в блок 2 и вставляем текст с обозначением оборудования
			using (Transaction trSecond = db.TransactionManager.StartTransaction())
			{

				try
				{
					foreach (SelectedObject so in secondSS)
					{
						if (so != null)
						{
							brSecondBlock = trSecond.GetObject(so.ObjectId, OpenMode.ForWrite) as BlockReference;

							Common.Attributes.SetAttribute(brSecondBlock, "13", sNumberPrevEqvipment);
							Common.Attributes.SetAttribute(brSecondBlock, "14", sPrevRoom);
							Common.Attributes.SetAttribute(brSecondBlock, "15", Convert.ToString(distanceToBlock));
							Common.Attributes.SetAttribute(brSecondBlock, "16", sCabelType);

							

							
							
							//sRoom = Common.Attributes.GetAttributre(brSecondBlock, "1"); // Номер помещения 
							//sMove = Common.Attributes.GetAttributre(brSecondBlock, "2"); // Значение сдвига блока
							//sRotate = Common.Attributes.GetAttributre(brSecondBlock, "3"); // Поворот блока
							//sName = Common.Attributes.GetAttributre(brSecondBlock, "6"); // Имя блока
							//sMagistralFreeInputs = Common.Attributes.GetAttributre(brSecondBlock, "7"); // Количество свободных магистральных линий
							//sAbonentFreeInputs = Common.Attributes.GetAttributre(brSecondBlock, "8"); // Количество свободных абонентских линий
							//sHeight = Common.Attributes.GetAttributre(brSecondBlock, "10"); // Высота установки оборудования
							//sNumberInSystem = Common.Attributes.GetAttributre(brSecondBlock, "11"); // Номер в системе
							//sNameInSystem = Common.Attributes.GetAttributre(brSecondBlock, "12"); // Обозначение в  системе
							//sNumberPrevEqvipment = Common.Attributes.GetAttributre(brSecondBlock, "13"); // Обозначение оборудования к которому подключено *
							//sPrevRoom = Common.Attributes.GetAttributre(brSecondBlock, "14"); // Номер помещения оборудования, к которому подключаемся *
							//sDistanceTo = Common.Attributes.GetAttributre(brSecondBlock, "15"); // Длина до оборудования, к которому подключаемся *
							//sCabelType = Common.Attributes.GetAttributre(brSecondBlock, "16"); // Марка кабеля *

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
