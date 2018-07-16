﻿using System;
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
	class V03IP_DrawScheme
	{
		internal static void V03IP_Scheme()
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
			PromptSelectionResult psrAllBlock = ed.SelectWindow(firstPointCW.Value, secondPointCW.Value, filter);
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


						// Базовые коордитаты (точка вставик 1 источника питания)
						double baseX = baseSchemePojnt.X;
						double baseY = baseSchemePojnt.Y;
						double baseZ = 0;

						//точка в ставки блоков на схеме
						Point3d insPoint = new Point3d();

						// Приращения координат для отрисовки разных систем 
						int dY = 0;
						int dX = 0;

						// последня цифра в обозначнии номера в системе
						int lastNumber;

						LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
						string layer = "0";

						string txtDist;
						if (brDistanceTo != "")
							txtDist = brDistanceTo;
						else
							txtDist = "";

						switch (brName)
						{
							// Работа с источниками роутерами
							case "RT":
								if (brNumberInSystem == "1")
								{
									//Находим точку вставки блока источника питания c №1 в системе
									insPoint = new Point3d(baseX, baseY, baseZ);
								}
								else
								{
									//Находим точку вставки блока для все последующих источников питания
									insPoint = new Point3d(baseX, baseY + 6000 * Convert.ToInt32(brNumberInSystem), baseZ);
								}

								// Вставляем тексты с обозначением и номером помещения


								Methods.CreateText(brNameInSystem, new Point3d(insPoint.X - 100, insPoint.Y + 350, 0), 90);
								Methods.CreateText(brRoom, new Point3d(insPoint.X + 300, insPoint.Y + 350, 0), 90);


								// Вставляем блок 
								Common.Block.InsertWithRotation(br.Name, insPoint);
								break;



							// Работа с системными свитчами
							case "SM":
								//Проверяем наличие слоя !СС Кабель питания
								if (lt.Has("!СС Кабель электропитания"))
									layer = "!СС Кабель электропитания";
								else
									layer = "0";

								// последняя цифра номера в системе для размещения на схеме
								lastNumber = Convert.ToInt32(brNumberInSystem.Substring(1, 1));

								// предпоследняя цифра номера в системе для размещения на схеме
								int predLastNummber = Convert.ToInt32(brNumberInSystem.Substring(0, 1));

								// Точка вставки для блоков SM
								if (predLastNummber == 1)
								{
									insPoint = new Point3d(baseX + 12000 * lastNumber, baseY, baseZ);
								}
								else
								{
									insPoint = new Point3d(baseX + 12000 * lastNumber, baseY + 6000 * predLastNummber, baseZ);
								}

								// Рисуем мальстальную линию 
								Methods.CreateLine(insPoint, new Point3d(insPoint.X - 12000, insPoint.Y, insPoint.Z), layer);

								// Находим центр линии 
								Point3d txtPnt = Methods.CenterPointBetweenPoints(insPoint, new Point3d(insPoint.X - 12000, insPoint.Y, insPoint.Z));

								// Вставляем текст с типом кабеля и длиной кабеля
								//Methods.CreateText(brCabelType, new Point3d(txtPnt.X, txtPnt.Y + 100, 0), 0);
								//Methods.CreateText(txtDist.ToString() + " м", new Point3d(txtPnt.X, txtPnt.Y - 250, 0), 0);

								Methods.CreateTextAlignet(brCabelType, new Point3d(txtPnt.X, txtPnt.Y + 100, 0), 0, 1);
								Methods.CreateTextAlignet(txtDist + " м", new Point3d(txtPnt.X, txtPnt.Y - 250, 0), 0, 1);

								// Номер прибора и помещения
								Methods.CreateText(brNameInSystem, new Point3d(insPoint.X + 350, insPoint.Y + 50, 0), 0);
								Methods.CreateText(brRoom, new Point3d(insPoint.X + 350, insPoint.Y - 250, 0), 0);

								// Вставляем блок коробки								
								Common.Block.InsertWithRotation(br.Name, insPoint);
								break;


							// Работа с оборудованием
							case "ZU":
							case "ZE":
							case "KJD":
							case "KJ":
							case "SJD":
							case "SIJ":
							case "SIJD":
							case "VKJ":
							case "VKJV":
								
								//Проверяем наличие слоя !СС КаБель абонентский
								if (lt.Has("!СС КаБель абонентский"))
									layer = "!СС КаБель абонентский";
								else
									layer = "0";
								
					
								// Смещения по Y для последующих блоков
								if (brNumberInSystem.Substring(0, 1) != "1")
								{
									dY = 6000 * Convert.ToInt32(brNumberInSystem.Substring(0, 1));
								}

								// Смещение по X для первого блока
								if (brNumberInSystem.Substring(1, 1) == "1")
								{
									dX = 12000;
								}
								// Смещение по X для последеющих блоков
								else
								{
									dX = 12000 * Convert.ToInt32(brNumberInSystem.Substring(1, 1));
								}

								lastNumber = Convert.ToInt32(brNumberInSystem.Substring(2, 1));

								// Определям точку вставки в соответствии с номером
								if (brNumberInSystem.Substring(2, 1) == "1")
								{
									insPoint = new Point3d(baseX + dX - 3500, baseY + dY + 1800, 0);

									// Рисуем абонeyтскую линию, 1 участок
									Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y - 1500, 0), layer);

									// Рисуем абонeyтскую линию, 2 участок
									Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y - 1500, 0), new Point3d(insPoint.X + 3500, insPoint.Y - 1500, 0), layer);
								}


								else if (brNumberInSystem.Substring(2, 1) == "2")
								{
									insPoint = new Point3d(baseX + dX, baseY + dY + 1800, 0);

									// Рисуем абонeyтскую линию, 1 участок
									Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y - 1800, 0), layer);
								}

								else if (brNumberInSystem.Substring(2, 1) == "3")
								{

									insPoint = new Point3d(baseX + dX + 3500, baseY + dY + 1800, 0);

									//// Рисуем абонeyтскую линию, 1 участок
									Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y - 1500, 0), layer);

									//// Рисуем абонeyтскую линию, 2 участок
									Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y - 1500, 0), new Point3d(insPoint.X - 3500, insPoint.Y - 1500, 0), layer);

								}

								// Определям точку вставки в соответствии с номером
								if (brNumberInSystem.Substring(2, 1) == "4")
								{
									insPoint = new Point3d(baseX + dX - 3500, baseY + dY - 1800, 0);

									// Рисуем абонeyтскую линию, 1 участок
									Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y + 1500, 0), layer);
											
									// Рисуем абонeyтскую линию, 2 участок
									Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y + 1500, 0), new Point3d(insPoint.X + 3500, insPoint.Y + 1500, 0), layer);
								}


								else if (brNumberInSystem.Substring(2, 1) == "5")
								{

									insPoint = new Point3d(baseX + dX, baseY + dY - 1800, 0);

									// Рисуем абонeyтскую линию, 1 участок
									Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y + 1800, 0), layer);

								}

								else if (brNumberInSystem.Substring(2, 1) == "6")
								{
									insPoint = new Point3d(baseX + dX + 3500, baseY + dY - 1800, 0);

									//// Рисуем абонeyтскую линию, 1 участок
									Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y + 1500, 0), layer);

									//// Рисуем абонeyтскую линию, 2 участок
									Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y + 1500, 0), new Point3d(insPoint.X - 3500, insPoint.Y + 1500, 0), layer);
								}


								if (brNumberInSystem.Substring(2, 1) == "1" || brNumberInSystem.Substring(2, 1) == "2" || brNumberInSystem.Substring(2, 1) == "3")
									txtPnt = Methods.CenterPointBetweenPoints(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y - 1500, 0));
								else
									txtPnt = Methods.CenterPointBetweenPoints(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y + 1500, 0));

							
								//// Вставляем текст с типом кабеля и длиной кабеля
								Methods.CreateTextAlignet(brCabelType, new Point3d(txtPnt.X - 50, txtPnt.Y, 0), 90, 1);
								Methods.CreateTextAlignet(brDistanceTo + " м", new Point3d(txtPnt.X + 250, txtPnt.Y, 0), 90, 1);

								//// Вставляем текст с обозначение прибора и номер помещения
								Methods.CreateText(brNameInSystem, new Point3d(insPoint.X + 350, insPoint.Y + 100, 0), 0);
								Methods.CreateText(brRoom, new Point3d(insPoint.X + 350, insPoint.Y - 300, 0), 0);
								
								Common.Block.InsertWithRotation(br.Name, insPoint);
								break;


							case "TNV":
							case "TANV":
							case "SV":
							case "TANVT":
							case "LJ":
							case "TP":

								if (lt.Has("!СС КаБель абонентский"))
									layer = "!СС КаБель абонентский";
								else
									layer = "0";


								// Смещения по Y для блоков
								if (brNumberInSystem.Substring(0, 1) != "1")
								{
									dY = 6000 * Convert.ToInt32(brNumberInSystem.Substring(0, 1));
								}

								// Смещение по X для первого блока
								if (brNumberInSystem.Substring(1, 1) == "1")
								{
									dX = 12000;
								}
								// Смещение по X для последеющих блоков
								else
								{
									dX = 12000 * Convert.ToInt32(brNumberInSystem.Substring(1, 1));
								}

								// Определям точку вставки в соответствии с местом вставки KJD, KJ SIJ и пр.
								if (brNumberInSystem.Substring(2, 1) == "1")
								{
									if (brNumberInSystem.Substring(3, 1) == "1")
									{
										insPoint = new Point3d(baseX + dX - 3500 - 1250, baseY + dY + 1800 + 1800, 0);

										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y - 1800, 0), layer);									

										// Второй участок линии
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y - 1800, 0), new Point3d(insPoint.X + 1250, insPoint.Y - 1800, 0), layer);

									}

									else if (brNumberInSystem.Substring(3, 1) == "2")
									{
										insPoint = new Point3d(baseX + dX - 3500, baseY + dY + 1800 + 1800, 0);

										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y - 1800, 0), layer);
									}

									else if (brNumberInSystem.Substring(3, 1) == "3")
									{
										insPoint = new Point3d(baseX + dX - 3500 + 1250, baseY + dY + 1800 + 1800, 0);

										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y - 1800, 0), layer);									

										////Участок линии 2
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y - 1800, 0), new Point3d(insPoint.X - 1250, insPoint.Y - 1800, 0), layer);
									}
								}


								else if (brNumberInSystem.Substring(2, 1) == "2")
								{
									if (brNumberInSystem.Substring(3, 1) == "1")
									{

										insPoint = new Point3d(baseX + dX - 1250, baseY + dY + 3600, 0);
										
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y - 1800, 0), layer);							

										//// Второй участок линии
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y - 1800, 0), new Point3d(insPoint.X + 1250, insPoint.Y - 1800, 0), layer);

									}

									else if (brNumberInSystem.Substring(3, 1) == "2")
									{
										insPoint = new Point3d(baseX + dX, baseY + dY + 3600, 0);

										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y - 1800, 0), layer);

									}

									else if (brNumberInSystem.Substring(3, 1) == "3")
									{

										insPoint = new Point3d(baseX + dX + 1250, baseY + dY + 3600, 0);
										
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y - 1800, 0), layer);

										////Участок линии 2
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y - 1800, 0), new Point3d(insPoint.X - 1250, insPoint.Y - 1800, 0), layer);
									}
								}


								//
								else if (brNumberInSystem.Substring(2, 1) == "3")
								{
									if (brNumberInSystem.Substring(3, 1) == "1")
									{
										insPoint = new Point3d(baseX + dX + 3500 - 1250, baseY + dY + 3600, 0);
										
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y - 1800, 0), layer);

										// Второй участок линии
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y - 1800, 0), new Point3d(insPoint.X + 1250, insPoint.Y - 1800, 0), layer);
									}

									else if (brNumberInSystem.Substring(3, 1) == "2")
									{
										insPoint = new Point3d(baseX + dX + 3500, baseY + dY + 3600, 0);
										
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y - 1800, 0), layer);

									}

									else if (brNumberInSystem.Substring(3, 1) == "3")
									{
										insPoint = new Point3d(baseX + dX + 3500 + 1250, baseY + dY + 3600, 0);
										
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y - 1800, 0), layer);

										//Участок линии 2
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y - 1800, 0), new Point3d(insPoint.X - 1250, insPoint.Y - 1800, 0), layer);
									}					

								}


								//
								else if (brNumberInSystem.Substring(2, 1) == "4")
								{
									if (brNumberInSystem.Substring(3, 1) == "1")
									{
										insPoint = new Point3d(baseX + dX - 3500 - 1250, baseY + dY - 1800 - 1800, 0);

										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y + 1800, 0), layer);

										// Второй участок линии
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y + 1800, 0), new Point3d(insPoint.X + 1250, insPoint.Y + 1800, 0), layer);

									}

									else if (brNumberInSystem.Substring(3, 1) == "2")
									{
										insPoint = new Point3d(baseX + dX - 3500, baseY + dY - 1800 - 1800, 0);
										
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y + 1800, 0), layer);

									}

									else if (brNumberInSystem.Substring(3, 1) == "3")
									{
										insPoint = new Point3d(baseX + dX - 3500 + 1250, baseY + dY - 1800 - 1800, 0);										

										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y + 1800, 0), layer);

										//Участок линии 2
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y + 1800, 0), new Point3d(insPoint.X - 1250, insPoint.Y + 1800, 0), layer);
									}									
								}



								else if (brNumberInSystem.Substring(2, 1) == "5")
								{
									if (brNumberInSystem.Substring(3, 1) == "1")
									{

										insPoint = new Point3d(baseX + dX - 1250, baseY + dY - 1800 - 1800, 0);

										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y + 1800, 0), layer);


										// Второй участок линии
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y + 1800, 0), new Point3d(insPoint.X + 1250, insPoint.Y + 1800, 0), layer);

									}

									else if (brNumberInSystem.Substring(3, 1) == "2")
									{
										insPoint = new Point3d(baseX + dX, baseY + dY - 1800 - 1800, 0);										

										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y + 1800, 0), layer);
									}

									else if (brNumberInSystem.Substring(3, 1) == "3")
									{

										
										insPoint = new Point3d(baseX + dX + 1250, baseY + dY - 1800 - 1800, 0);
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y + 1800, 0), layer);

										// Находим центр линии 
										txtPnt = Methods.CenterPointBetweenPoints(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y + 1500, 0));

										//Участок линии 2
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y + 1800, 0), new Point3d(insPoint.X - 1250, insPoint.Y + 1800, 0), layer);
									}									
								}


								else if (brNumberInSystem.Substring(2, 1) == "6")
								{
									if (brNumberInSystem.Substring(3, 1) == "1")
									{
										insPoint = new Point3d(baseX + dX + 3500 - 1250, baseY + dY - 1800 - 1800, 0);									

										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y + 1800, 0), layer);


										// Второй участок линии
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y + 1800, 0), new Point3d(insPoint.X + 1250, insPoint.Y + 1800, 0), layer);

									}

									else if (brNumberInSystem.Substring(3, 1) == "2")
									{
										insPoint = new Point3d(baseX + dX + 3500, baseY + dY - 1800 - 1800, 0);
										

										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y + 1800, 0), layer);

									}

									else if (brNumberInSystem.Substring(3, 1) == "3")
									{
										insPoint = new Point3d(baseX + dX + 3500 + 1250, baseY + dY - 1800 - 1800, 0);


										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y + 1800, 0), layer);

										//Участок линии 2
										Methods.CreateLine(new Point3d(insPoint.X, insPoint.Y + 1800, 0), new Point3d(insPoint.X - 1250, insPoint.Y + 1800, 0), layer);
									}									
								}



								if (brNumberInSystem.Substring(2, 1) == "1" || brNumberInSystem.Substring(2, 1) == "2" || brNumberInSystem.Substring(2, 1) == "3")
								{
									Methods.CreateText(brNameInSystem, new Point3d(insPoint.X - 100, insPoint.Y + 350, 0), 90);
									Methods.CreateText(brRoom, new Point3d(insPoint.X + 300, insPoint.Y + 350, 0), 90);

									txtPnt = Methods.CenterPointBetweenPoints(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y - 1800, 0));
								}
								else
								{
						
									Methods.CreateTextAlignet(brNameInSystem, new Point3d(insPoint.X - 100, insPoint.Y - 350, 0), 90, 2);
									Methods.CreateTextAlignet(brRoom, new Point3d(insPoint.X + 300, insPoint.Y - 350, 0), 90, 2);

									txtPnt = Methods.CenterPointBetweenPoints(new Point3d(insPoint.X, insPoint.Y, 0), new Point3d(insPoint.X, insPoint.Y + 1800, 0));
								}


								//// Вставляем текст с типом кабеля и длиной кабеля
								Methods.CreateTextAlignet(brCabelType, new Point3d(txtPnt.X - 50, txtPnt.Y, 0), 90, 1);
								Methods.CreateTextAlignet(brDistanceTo + " м", new Point3d(txtPnt.X + 250, txtPnt.Y, 0), 90, 1);


								Common.Block.InsertWithRotation(br.Name, insPoint);
								break;

							
							// выход
							default:
								break;
						}

						// Записываем атрибуты в вставленный блок
						PromptSelectionResult psr = ed.SelectLast();
						SelectionSet ssLast = psr.Value;

						foreach (SelectedObject s in ssLast)
						{
							BlockReference newBR = tr.GetObject(s.ObjectId, OpenMode.ForWrite) as BlockReference;

							//Записываем атрибуты
							Common.Attributes.SetAttribute(newBR, "1", brRoom);
							Common.Attributes.SetAttribute(newBR, "2", brMove);
							Common.Attributes.SetAttribute(newBR, "3", brRotate);
							Common.Attributes.SetAttribute(newBR, "6", brName);
							Common.Attributes.SetAttribute(newBR, "7", brMagistralFreeInputs);
							Common.Attributes.SetAttribute(newBR, "8", brAbonentFreeInputs);
							Common.Attributes.SetAttribute(newBR, "10", brHeight);
							Common.Attributes.SetAttribute(newBR, "11", brNumberInSystem);
							Common.Attributes.SetAttribute(newBR, "12", brNameInSystem);
							Common.Attributes.SetAttribute(newBR, "13", brNumberPrevEqvipment);
							Common.Attributes.SetAttribute(newBR, "14", brPrevRoom);
							Common.Attributes.SetAttribute(newBR, "15", brDistanceTo);
							Common.Attributes.SetAttribute(newBR, "16", brCabelType);
						}
					}

					tr.Commit();
				}

				catch (Exception ex)
				{
					ed.WriteMessage("\nИсключение в методе AV03_Scheme " + ex.Message + "\n В строке: " + ex.StackTrace);

				}

				finally
				{
					tr.Dispose();
				}


			}


		}

	}
}
