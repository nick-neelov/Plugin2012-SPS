using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;



namespace Neelov.AutocadPlugin
{
	class AV03_DrawScheme
	{
		internal static void AV03_Scheme()
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			if (doc == null) { return; }

			Editor ed = doc.Editor;
			Database db = doc.Database;

			// Поля для атрибутов блока
			string brRoom = "";
			string brMove = "";
			string brRotate = "";			
			string brName = "";
			string brMagistralFreeInputs = "";
			string brAbonentFreeInputs = "";
			string brHeight = "";
			string brNumberInSystem = "";
			string brNameInSystem = "";
			string brNumberPrevEqvipment = "";
			string brPrevRoom = "";
			string brDistanceTo = "";
			string brCabelType = "";

			// Фильтр для выбора блоков
			TypedValue[] typeBlock = new TypedValue[]
			{
				new TypedValue((int)DxfCode.Start, "INSERT")
			};			
			SelectionFilter filter = new SelectionFilter(typeBlock);




			//Выбираем блоки рамкой
			// Первая точка рамки
			PromptPointResult firstPointCW = ed.GetPoint("\nУкажите первую точку рамки для выбора: ");
			if (firstPointCW.Status != PromptStatus.OK) { return; }
						// Вторая точка рамки
			PromptCornerOptions pCornOpt = new PromptCornerOptions("\nУкажите другой угол: ", firstPointCW.Value);
			PromptPointResult secondPointCW = ed.GetCorner(pCornOpt);
			if (secondPointCW.Status != PromptStatus.OK) { return; }
						// Выбираем блоки внитри рамки
			PromptSelectionResult psrAllBlock = ed.SelectWindow(firstPointCW.Value, secondPointCW.Value);
			if (psrAllBlock.Status != PromptStatus.OK) { return; }

			//Если все ОК, создаем набор
			SelectionSet ss = psrAllBlock.Value;


			Point3d baseSchemePojnt = ed.GetPoint("\nУкажите точку вставки схемы: ").Value;
			
			using (Transaction tr = db.TransactionManager.StartTransaction())
			{
				try
				{
					//Перебераем блоки
					foreach (SelectedObject so in ss)
					{
						BlockReference br = tr.GetObject(so.ObjectId, OpenMode.ForRead) as BlockReference;

						//Получаем атрибуты блока
						brRoom = Common.Attributes.GetAttributre(br, "1"); // Номер помещения 
						brMove = Common.Attributes.GetAttributre(br, "2"); // Значение сдвига блока
						brRotate = Common.Attributes.GetAttributre(br, "3"); // Поворот блока
						brName = Common.Attributes.GetAttributre(br, "6"); // Имя блока
						brMagistralFreeInputs = Common.Attributes.GetAttributre(br, "7"); // Количество свободных магистральных линий
						brAbonentFreeInputs = Common.Attributes.GetAttributre(br, "8"); // Количество свободных абонентских линий
						brHeight = Common.Attributes.GetAttributre(br, "10"); // Высота установки оборудования
						brNumberInSystem = Common.Attributes.GetAttributre(br, "11"); // Номер в системе
						brNameInSystem = Common.Attributes.GetAttributre(br, "12"); // Обозначение в  системе
						brNumberPrevEqvipment = Common.Attributes.GetAttributre(br, "13"); // Обозначение оборудования к которому подключено *
						brPrevRoom = Common.Attributes.GetAttributre(br, "14"); // Номер помещения оборудования, к которому подключаемся *
						brDistanceTo = Common.Attributes.GetAttributre(br, "15"); // Длина до оборудования, к которому подключаемся *
						brCabelType = Common.Attributes.GetAttributre(br, "16"); // Марка кабеля *



						double baseX = baseSchemePojnt.X;
						double baseY = baseSchemePojnt.Y;
						double baseZ = 0;

						Point3d insPoint = new Point3d();

						switch (brName)
						{
							// Работа с источниками питания начало схемы
							case "NAP":
								if (brNumberInSystem == "1")
								{
									//Находим точку вставки блока для 1 источника питания в системе
									insPoint = new Point3d(baseX, baseY, baseZ);
								}
								else
								{
									//Находим точку вставки блока для все последующих
									insPoint = new Point3d(baseX + 5000 * Convert.ToInt32(brNumberInSystem), baseY, baseZ);									
								}


								// Вставляем блок 
								Common.Block.InsertWithRotation("NAP", insPoint);

								// Рисуем линиюма гистрального кабеля до следующего блока							
								//Methods.CreateLine(new Point3d(baseX, baseY, baseZ), new Point3d(baseX + 8000, baseY, baseZ), "0");
								break;

							// Работа с коробками
							case "PD":
								if (brNumberInSystem == "11")
								{

								}

								// последняя цифра номера в системе для размещения на схеме
								int posNumber = Convert.ToInt32(brNumberInSystem.Substring(brNumberInSystem.Length - 1));

								// Точка вставки для блоков коробок PD
								insPoint = new Point3d(baseX + 8000 * posNumber, baseY, baseZ);
								Common.Block.InsertWithRotation("PD", insPoint);
								// Магистральную линию
								Methods.CreateLine(insPoint, new Point3d(insPoint.X + 8000, insPoint.Y, insPoint.Z), "0");
								break;








							default:
								break;
						}

					}
					tr.Commit();
				}

				catch(Exception ex)
				{
					ed.WriteMessage("\nИсключение в методе AV03_Scheme " + ex.Message + "\n В строке: " + ex.StackTrace);

				}

				finally
				{
					tr.Dispose();
				}
				

			}


		}

		/// <summary>
		/// Получаем точку вставки блока 1 уровня (NAP)
		/// </summary>
		/// <returns></returns>
		private static Point3d GetLevel1Point()
		{
			Point3d result = new Point3d();
			
			return result;
		}


		private static Point3d PointPDBlock(string number, Point3d pnt)
		{
			Point3d result = new Point3d();

			double x = pnt.X;
			double y = pnt.Y;
			double z = pnt.Z;


			foreach (char ch in number)
			{
				x = x + 10000 * ch;
				y = y + 8000 * ch;
				
			}

			return result = new Point3d(x, y, z);

		}




	}
}
