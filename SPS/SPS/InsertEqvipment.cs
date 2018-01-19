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

namespace Neelov.AutocadPlugin
{
	class InsertEqvipment
	{
		private static string sideMove;
		private static string blockName;

		internal static void AddEqvipmentOnPlan()
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Editor ed = doc.Editor;
			Database db = doc.Database;

			try
			{
				while (true)
				{
					PromptStringOptions pkoBlockName = new PromptStringOptions("\nУкажите имя блока: ");
					pkoBlockName.DefaultValue = blockName;
					PromptResult prBlockName = ed.GetString(pkoBlockName);

					if (prBlockName.Status != PromptStatus.OK)
						return;

					blockName = CorrectBlockName(prBlockName);

					//// Преверяем наличие блока в черетеже
					BlockTable bt = db.TransactionManager.StartTransaction().GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

					if (bt.Has(blockName))
					{
						// Точка вставки блока
						PromptPointOptions ppoInsertPoint = new PromptPointOptions("\nУкажите точку вставки блока : ");
						PromptPointResult pprInsertPoint = ed.GetPoint(ppoInsertPoint);
						if (pprInsertPoint.Status != PromptStatus.OK)
							return;
						Point3d pointInsert = pprInsertPoint.Value;

						//Смещение блока относительно точки вставки
						PromptStringOptions psoMove = new PromptStringOptions("\nВ какую сторону сместить блок: ");
						psoMove.DefaultValue = sideMove;
						PromptResult prMove = ed.GetString(psoMove);
						if (prMove.Status != PromptStatus.OK)
							return;
						sideMove = prMove.StringResult;

						switch (sideMove)
						{
							//Смещаем вверх на 300
							case "8":
								pointInsert = new Point3d(pointInsert.X, pointInsert.Y + 300, pointInsert.Z);
								break;

							//Смещаем вниз на 300
							case "2":
								pointInsert = new Point3d(pointInsert.X, pointInsert.Y - 300, pointInsert.Z);
								break;

							//Смещаем влево на 300
							case "4":
								pointInsert = new Point3d(pointInsert.X - 300, pointInsert.Y, pointInsert.Z);
								break;

							//Смещаем вправо на 300
							case "6":
								pointInsert = new Point3d(pointInsert.X + 300, pointInsert.Y, pointInsert.Z);
								break;

							default:
								ed.WriteMessage("\nТочка без изменений...");
								break;
						}

						// Вставляем блоки и записываем в него необходимые атрибуты
						using (Transaction tr = db.TransactionManager.StartTransaction())
						{
							try
							{
								// Вставляем блок
								Common.Block.InsertNoRotation(blockName, pointInsert);

								//// Получаем последний вставленный блок
								//PromptSelectionResult psrLast = ed.SelectLast();
								//SelectionSet ss = psrLast.Value;

								//foreach (SelectedObject so in ss)
								//{
								//	BlockReference br = tr.GetObject(so.ObjectId, OpenMode.ForWrite) as BlockReference;
								//	Sectors.AddSectorInBlock(br);
								//}
								ed.WriteMessage("\nБлок {0} вставлен в чертеж", blockName);
								tr.Commit();

							}

							catch (Autodesk.AutoCAD.Runtime.Exception ex)
							{
								ed.WriteMessage("\nИсключение " + ex.Message + "\nВ строке " + ex.StackTrace);
							}
							finally
							{
								tr.Dispose();
							}
						}

					}
					else
					{
						ed.WriteMessage("\nТакого блока нет в чертеже! Добавте его в черетеж с панели элементов.");
						//return;	
					}
				}
			}

			catch (Autodesk.AutoCAD.Runtime.Exception ex)
			{
				ed.WriteMessage("\nИсключение " + ex.Message + "\nВ строке " + ex.StackTrace);
			}
		}

		/// <summary>
		/// Метод корректировки имени блока при вводе
		/// </summary>
		/// <param name="pr">Имя блока в русской или латинской раскладке</param>
		/// <returns>Коректное имя блока в латинской раскладке</returns>
		static string CorrectBlockName(PromptResult pr)
		{

			string result = "";
			char[] rusLetters = new char[] { 'Й', 'Ц', 'У', 'К', 'Е', 'Н', 'Г', 'Ш', 'Щ', 'З', 'Ф', 'Ы', 'В', 'А', 'П', 'Р', 'О', 'Л', 'Д', 'Я', 'Ч', 'С', 'М', 'И', 'Т', 'Ь' };
			char[] engLettres = new char[] { 'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'Z', 'X', 'C', 'V', 'B', 'N', 'M' };


				foreach (char ch in pr.StringResult.ToUpper())
				{
					for (int i = 0; i < rusLetters.Count(); i++)
					{
						if (ch == rusLetters[i])
						{
							result = result + ch;
						}
					else
					{
						result = pr.StringResult.ToUpper();
					}
					}
				}
		

			return result;
		}




	}
}

