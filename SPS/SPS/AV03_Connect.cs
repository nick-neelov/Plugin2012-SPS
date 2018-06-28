
using System;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;


namespace Neelov.AutocadPlugin
{
	/// <summary>
	/// Класс реализующий подключени оборудования
	/// </summary>
	class AV03_Connect
	{		
		public static void Connect()
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
			//string fPosition ="";
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
			//string sPosition = "";
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
							if (fNumberInSystem == "")
							{
								fNumberInSystem = ed.GetString("\nУ блока " + fName + " нет обозначения (номера) в системе. Укажите его: ").StringResult;

								// Вставляем текст с обозначение блока
								Point3d text1Point = Methods.FirstTextPosition(sMove, pFirstBlock);
								Point3d text2Point = Methods.NextTextPosition(sMove, text1Point);
								double textAngle = Methods.TextRotation(sMove);

								// Вставляем текст с обозначением блока
								Methods.CreateText(fName + fNumberInSystem, text1Point, textAngle);
								// Вставляем текст с высотой установки
								Methods.CreateText("h = " + fHeight, text2Point, textAngle);

							}

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

				catch (Exception ex)
				{
					ed.WriteMessage("\nИсключение: " + ex.Message + "\nВ строке: " + ex.StackTrace);
				}

				finally
				{
					trFirst.Dispose();
				}
			}


			// Выбираем второй блок			
			PromptSelectionOptions psoSecondBlock = new PromptSelectionOptions();

			string strEq = "";

			if (fName == "NAP")
			{
				strEq = "\nМожно подключить: " + "\nВ магистальную линию: PD";
			}
			else if (fName == "PD")
			{
				strEq = "\nМожно подключить: " + "\nВ магистальную линию: PD" + "\nВ абонентскую линию: SJ, SVO, ZVJ, ZRJ";
			}
			else if (fName == "ZVJ")
				strEq = "\nМожно подключить: " + "\nВ абонентскую линию: VJ";
			else if (fName == "SJ")
				strEq = "\nМожно подключить: " + "\nВ абонентскую линию: TEZ";
			else if (fName == "TEZ")
				strEq = "\nМожно подключить: " + "\nВ абонентскую линию: EL";
			else if (fName == "ZRJ")
				strEq = "\nМожно подключить: " + "\nВ абонентскую линию: RJ, RJP";



			psoSecondBlock.MessageForAdding = ("\nВыберите оборудование которое подключаем: " + strEq);
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

				catch (Exception ex)
				{
					ed.WriteMessage("\nИсключение: " + ex.Message + "\nВ строке: " + ex.StackTrace);
				}

				finally
				{
					trSecond.Dispose();
				}
			}


			//Производим общие вычисления
			// расстояние между блоками с учетом опуска + запас 20%
			double opuski;
			if (sHeight == "Стол" || sHeight == "Шкаф")
				opuski = Convert.ToDouble(fHeight);
			else if (fHeight == "Стол" || fHeight == "Шкаф")
				opuski = Convert.ToDouble(sHeight);
			else if (fHeight == sHeight)
				opuski = 0;
			else
				opuski = Convert.ToDouble(fHeight) + Convert.ToDouble(sHeight);


			
			double distanceToBlock =Math.Round(((Methods.DictanceBetweenBlocks(pFirstBlock, pSecondBlock) / 1000) + opuski) * 1.2);

			// Вычисления для блока 1
			// Выполняем подключение магистрального оборудования
			if (fName == "NAP" && sName == "PD" || fName == "PD" && sName == "PD")
			{
				// Проверяем есть ли свободные магистральные порты для подключения
				if (Convert.ToInt32(fMagistralFreeInputs) != 0)
				{
					if (fNameInSystem == "")
					{
						fNameInSystem = fName + "." + fNumberInSystem;
					}

					if (fName == "NAP")
					{
						// Добавляем номер в системе во второй блок
						sNumberInSystem = fNumberInSystem + "1";
						string tmp = "";
						// Добавляем имя во второй блок
						foreach (char ch in sNumberInSystem)
						{
							tmp = tmp + "." + ch;
							sNameInSystem = sName + tmp;
						}

					//	sNameInSystem = sName + "." + sNumberInSystem;
					}
					else if (fName == "PD")
					{
						// Добавляем номер в системе во втрой блок
						sNumberInSystem += Convert.ToString(Convert.ToInt32(fNumberInSystem) + 1);

						// Добавляем номер в системе во втрой блок
						string tmp = "";
						foreach (char ch in sNumberInSystem)
						{
							tmp = tmp + "." + ch;
							sNameInSystem = sName + tmp;
						}
						//sNameInSystem = sName + "." + sNumberInSystem;
					}

					// Убераем 1 порт для подключения оборудования			
					fMagistralFreeInputs = Convert.ToString(Convert.ToInt32(fMagistralFreeInputs) - 1);
					sMagistralFreeInputs = Convert.ToString(Convert.ToInt32(sMagistralFreeInputs) - 1);
				}

				else
				{
					ed.WriteMessage("\nОтсутствуют свободные порты для подключения магистральных линий");
					return;
				}
			}
				
			// Подключение абонентского оборудования		
			else
			{
				// Проверяем есть ли есть свободные абонентские порты 
				if (Convert.ToInt32(fAbonentFreeInputs) != 0)
				{
					if (fNameInSystem == "")
					{
						fNameInSystem = fName + "." + fNumberInSystem;
					}

					if (fName == "PD" || fName == "SJ")
					{
						// Добавляем номер в системе во второй блок
						sNumberInSystem = fNumberInSystem + Convert.ToString(5 - Convert.ToInt32(fAbonentFreeInputs));
						string tmp = "";
						// Добавляем имя во второй блок
						foreach (char ch in sNumberInSystem)
						{
							tmp = tmp + "." + ch;
							sNameInSystem = sName + tmp;
						}
					}

					else if (fName == "ZVJ" || fName == "RJP" || fName == "TEZ" || fName == "RJ" || fName == "SJ" || fName == "ZRJ")
					{

						// Добавляем номер в системе во второй блок
						sNumberInSystem = fNumberInSystem + "1";
						string tmp = "";
						// Добавляем имя во второй блок
						foreach (char ch in sNumberInSystem)
						{
							tmp = tmp + "." + ch;
							sNameInSystem = sName + tmp;
						}

						// Добавляем номер в системе во втрой блок
						//sNumberInSystem += Convert.ToString(Convert.ToInt32(fNumberInSystem) + 1);

						//// Добавляем номер в системе во втрой блок
						//string tmp = "";
						//foreach (char ch in sNumberInSystem)
						//{
						//	tmp = tmp + "." + ch;
						//	sNameInSystem = sName + tmp;
						//}
					}

					//else if (fName == "EL")
					//{
					//	// Добавляем номер в системе во второй блок
					//	sNumberInSystem = fNumberInSystem + Convert.ToString(Convert.ToInt32(fAbonentFreeInputs));
					//	string tmp = "";
					//	// Добавляем имя во второй блок
					//	foreach (char ch in sNumberInSystem)
					//	{
					//		tmp = tmp + "." + ch;
					//		sNameInSystem = sName + tmp;
					//	}
					//}

					fAbonentFreeInputs = Convert.ToString(Convert.ToInt32(fAbonentFreeInputs) - 1);				
				}
				else
				{
					ed.WriteMessage("\nОтсутствуют свободные порты для подключения абонентских линий");
					return;
				}
			}

			// Вычисления для блока 2

			sNumberPrevEqvipment = fNumberInSystem; // Обозначение предыдущего оборудования

			sDistanceTo = Convert.ToString(distanceToBlock); // Растояние до предыдущего блока

			sPrevRoom = fRoom; // Номер помещения предыдущего блока

			// Определяем тип кабеля
			// 
			if (fName == "NAP" || fName == "RJP")
			{
				sCabelType = "B";
			}

			else if (fName == "PD" || fName == "SJ")
			{
				sCabelType = "A+B";
			}

			else if (fName == "ZRJ" || fName == "SVO")
			{
				sCabelType = "A";
			}

			else if(fName == "TEZ" || fName == "EL")
			{
				sCabelType = "C";
			}

			else if (fName == "ZVJ")
			{
				sCabelType = "Комп.";
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
							

							// Устанавливаем слой после подключения
							brFirstBlock.Layer = oldLayerBlock;
						}
					}

					trFirst.Commit();
				}

				catch (Exception ex)
				{
					ed.WriteMessage("\nИсключение: " + ex.Message + "\nВ строке: " + ex.StackTrace);
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

							Common.Attributes.SetAttribute(brSecondBlock, "7", sMagistralFreeInputs);
							Common.Attributes.SetAttribute(brSecondBlock, "8", sAbonentFreeInputs);
							Common.Attributes.SetAttribute(brSecondBlock, "11", sNumberInSystem);
							Common.Attributes.SetAttribute(brSecondBlock, "12", sNameInSystem);
							Common.Attributes.SetAttribute(brSecondBlock, "13", sNumberPrevEqvipment);
							Common.Attributes.SetAttribute(brSecondBlock, "14", sPrevRoom);
							Common.Attributes.SetAttribute(brSecondBlock, "15", Convert.ToString(distanceToBlock));
							Common.Attributes.SetAttribute(brSecondBlock, "16", sCabelType);


							Point3d text1Point = Methods.FirstTextPosition(sMove, brSecondBlock.Position);
							Point3d text2Point = Methods.NextTextPosition(sMove, text1Point);
							double textAngle = Methods.TextRotation(sMove);

							// Вставляем текст с обозначением блока
							Methods.CreateText(sNameInSystem, text1Point, textAngle);

							// Вставляем текст с высотой установки
							Methods.CreateText("h = " + sHeight, text2Point, textAngle);



							// Устанавливаем слой после подключения
							brSecondBlock.Layer = "!СС Оборудование";
						}
					}

					trSecond.Commit();
				}

				catch (Exception ex)
				{
					ed.WriteMessage("\nИсключение: " + ex.Message + "\nВ строке: " + ex.StackTrace);
				}

				finally
				{
					trSecond.Dispose();					
				}

			}
		}
	}
}
