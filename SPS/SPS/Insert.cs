﻿using System;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace Neelov.AutocadPlugin
{
	/// <summary>
	/// Класс реализующий работу с планами
	/// </summary>
	class Insert
	{
		private static string blockName;
		

		/// <summary>
		/// Метод для установки блоков на планы
		/// </summary>
		public static void InsertEqvipment()
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			if (doc == null) return;

			Editor ed = doc.Editor;
			Database db = doc.Database;

			Point3d insPoint = new Point3d();		



	while (true)
			{
				using (Transaction tr = db.TransactionManager.StartTransaction())
				{
					try
					{
						PromptStringOptions pso = new PromptStringOptions("\nУкажите имя блока (0 - выход): ");
						pso.DefaultValue = blockName;
						PromptResult pr = ed.GetString(pso);
						if (pr.Status != PromptStatus.OK) return;
						blockName = CorrectBlockName(pr.StringResult);

						//Выходим если имя 0
						if (blockName == "0")
						{
							break;
						}

						BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;

						// Проверяем наличие блока 
						if (!bt.Has(blockName))
						{
							ed.WriteMessage("\nБлока " + blockName + " нет в черетеже!Вставте его с палитры!");
							return;
						}

						// Получаем точку вставки блока
						PromptPointResult ppr = ed.GetPoint("\nУкажите точку вставки блока: ");
						if (ppr.Status != PromptStatus.OK) return;

						insPoint = ppr.Value;

						// Указываем направление смещения 
						PromptStringOptions psoMove = new PromptStringOptions("\nУкажите направление смещения блока: ");
						PromptResult prMove = ed.GetString(psoMove);
						if (prMove.Status != PromptStatus.OK) return;

						switch (prMove.StringResult)
						{

							case "1":
								insPoint = new Point3d(insPoint.X - 300, insPoint.Y - 300, insPoint.Z);																
								break;

							case "2":
								insPoint = new Point3d(insPoint.X, insPoint.Y - 300, insPoint.Z);
								
								break;

							case "3":
								insPoint = new Point3d(insPoint.X + 300, insPoint.Y - 300, insPoint.Z);
								
								break;

							case "4":
								insPoint = new Point3d(insPoint.X - 300, insPoint.Y, insPoint.Z);
								
								break;

							case "6":
								insPoint = new Point3d(insPoint.X + 300, insPoint.Y, insPoint.Z);
								
								break;

							case "7":
								insPoint = new Point3d(insPoint.X - 300, insPoint.Y + 300, insPoint.Z);
								
								break;

							case "8":
								insPoint = new Point3d(insPoint.X, insPoint.Y + 300, insPoint.Z);
								
								break;

							case "9":
								insPoint = new Point3d(insPoint.X + 300, insPoint.Y + 300, insPoint.Z);
								
								break;

							default:
								ed.WriteMessage("\nТочка без изменений");
								break;
						}

						// Вставляем блок
						Common.Block.InsertWithRotation(blockName, insPoint);

						// Получаем последний вставленный блок
						PromptSelectionResult psrLast = ed.SelectLast();
						SelectionSet ss = psrLast.Value;
						//Записываем номер сектора в атрибуты
						foreach (SelectedObject so in ss)
						{
							BlockReference br = tr.GetObject(so.ObjectId, OpenMode.ForWrite) as BlockReference;
							Common.Sectors.AddSectorInBlock(br);
							Common.Attributes.SetAttribute(br, "2", prMove.StringResult);													
						}

						switch (prMove.StringResult)
						{
							case "1":
								insPoint = new Point3d(insPoint.X - 750, insPoint.Y, insPoint.Z);								
								break;

							case "2":
								insPoint = new Point3d(insPoint.X, insPoint.Y - 750, insPoint.Z);								
								break;
								
							case "3":
								insPoint = new Point3d(insPoint.X + 750, insPoint.Y, insPoint.Z);								
								break;

							case "4":
								insPoint = new Point3d(insPoint.X - 750, insPoint.Y, insPoint.Z);								
								break;
								
							case "6":
								insPoint = new Point3d(insPoint.X + 750, insPoint.Y, insPoint.Z);
								break;

							case "7":
								insPoint = new Point3d(insPoint.X - 750, insPoint.Y, insPoint.Z);
								break;

							case "8":
								insPoint = new Point3d(insPoint.X, insPoint.Y + 750, insPoint.Z);								
								break;

							case "9":
								insPoint = new Point3d(insPoint.X + 750, insPoint.Y, insPoint.Z);								
								break;

							default:								
								break;
						}

						// Автоматически вставляем пульт пациента
						if (blockName == "ZU")
						{
							Common.Block.InsertWithRotation("LJ", insPoint);
						}

						else if (blockName == "ZE")
						{
							Common.Block.InsertWithRotation("TP", insPoint);
						}

						else if (blockName == "ZVJ")
						{
							Common.Block.InsertWithRotation("VJ", insPoint);
						}

						else if (blockName == "RJP" || blockName == "RN")
						{
							Common.Block.InsertWithRotation("ZRJ", insPoint);
						}

						else if (blockName == "EL")
						{
							Common.Block.InsertWithRotation("TEZ", insPoint);
						}

						// Получаем последний вставленный блок
						psrLast = ed.SelectLast();
						ss = psrLast.Value;
						//Записываем номер сектора в атрибуты
						foreach (SelectedObject so in ss)
						{
							BlockReference br = tr.GetObject(so.ObjectId, OpenMode.ForWrite) as BlockReference;
							Common.Sectors.AddSectorInBlock(br);
							Common.Attributes.SetAttribute(br, "2", prMove.StringResult);
						}

						tr.Commit();						
					}

					catch (Autodesk.AutoCAD.Runtime.Exception ex)
					{
						ed.WriteMessage("\nИсключение : " + ex.Message + "\nВ строке " + ex.StackTrace);
					}
					finally
					{
						tr.Dispose();
					}
				}
			}


		}

		private static string CorrectBlockName(string blockName)
		{
			string result = "";

			foreach (char ch in blockName.ToUpper())
			{
				if (ch == 'Й') { result += 'Q'; }
				else if (ch == 'Ц') { result += 'W'; }
				else if (ch == 'У') { result += 'E'; }
				else if (ch == 'К') { result += 'R'; }
				else if (ch == 'Е') { result += 'T'; }
				else if (ch == 'Н') { result += 'Y'; }
				else if (ch == 'Г') { result += 'U'; }
				else if (ch == 'Ш') { result += 'I'; }
				else if (ch == 'Щ') { result += 'O'; }
				else if (ch == 'З') { result += 'P'; }

				else if (ch == 'Ф') { result += 'A'; }
				else if (ch == 'Ы') { result += 'S'; }
				else if (ch == 'В') { result += 'D'; }
				else if (ch == 'А') { result += 'F'; }
				else if (ch == 'П') { result += 'G'; }
				else if (ch == 'Р') { result += 'H'; }
				else if (ch == 'О') { result += 'J'; }
				else if (ch == 'Л') { result += 'K'; }

				else if (ch == 'Я') { result += 'Z'; }
				else if (ch == 'Ч') { result += 'X'; }
				else if (ch == 'С') { result += 'C'; }
				else if (ch == 'М') { result += 'V'; }
				else if (ch == 'И') { result += 'B'; }
				else if (ch == 'Т') { result += 'N'; }
				else if (ch == 'Ь') { result += 'M'; }

				else
				{
					result += ch;
				}
			}
				return result.ToUpper();
		}





	}
}
