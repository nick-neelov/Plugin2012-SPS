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
	/// Класс реализующий работу с планами
	/// </summary>
	class WorkWithPlans
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

			try
			{
				PromptStringOptions pso = new PromptStringOptions("\nУкажите имя блока: ");
				pso.DefaultValue = blockName;
				

				PromptResult pr = ed.GetString(pso);
				if (pr.Status != PromptStatus.OK) return;
				blockName = CorrectBlockName(pr.StringResult);

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
					case "2":
						insPoint = new Point3d(insPoint.X, insPoint.Y - 300, insPoint.Z);
						break;

					case "4":
						insPoint = new Point3d(insPoint.X - 300, insPoint.Y, insPoint.Z);
						break;

					case "8":
						insPoint = new Point3d(insPoint.X, insPoint.Y + 300, insPoint.Z);
						break;

					case "6":
						insPoint = new Point3d(insPoint.X + 300, insPoint.Y, insPoint.Z);
						break;

					default:
						ed.WriteMessage("\nТочка без изменений");
						break;
				}
			}
			catch (Autodesk.AutoCAD.Runtime.Exception ex)
			{
				ed.WriteMessage("\nИсключение : " + ex.Message + "\nВ строке " + ex.StackTrace);
			}

			using (Transaction tr = db.TransactionManager.StartTransaction())
			{

				try
				{
					BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;
					// Проверяем наличие блока 
					if (bt.Has(blockName))
					{
						Common.Block.InsertNoRotation(blockName, insPoint);
						

						// Получаем последний вставленный блок
						PromptSelectionResult psrLast = ed.SelectLast();
						SelectionSet ss = psrLast.Value;

						foreach (SelectedObject so in ss)
						{
							BlockReference br = tr.GetObject(so.ObjectId, OpenMode.ForWrite) as BlockReference;
							Common.Sectors.AddSectorInBlock(br);
						}

						tr.Commit();
					}
					else
					{
						ed.WriteMessage("\nТакого блока нет в черетеже! Вставте его с палитры!");
						return;
					}

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


		/// <summary>
		/// Метод для корректировки вводимого имени блока
		/// </summary>
		/// <param name="bn">Имя блока от пользователя</param>
		/// <returns></returns>
		private static string CorrectBlockName(string bn)
		{
			string result = "";

			char[] engLetters = new char[] { 'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'Z', 'X', 'C', 'V', 'B', 'N', 'M' };
			char[] rusLetters = new char[] { 'Й', 'Ц', 'У', 'К', 'Е', 'Н', 'Г', 'Ш', 'Щ', 'З', 'Ф', 'Ы', 'В', 'А', 'П', 'Р', 'О', 'Л', 'Д', 'Я', 'Ч', 'С', 'М', 'И', 'Т', 'Ь' };

			foreach (char ch in bn)
			{
				//Проверям на какой раскладке написано имя блок
				if ((int)ch >= 97 && (int)ch <= 122)
				{
					// Если русскими буквами
					for (int i = 0; i < rusLetters.Count(); i++)
					{
						if (ch == rusLetters[i])
						{
							result = result + engLetters[i];
						}
					}
				}
				else
				{
					result = bn;
				}
			}
			return result;
		}



		// TODO Добавить класс для обработки нажатия клавиш 4, 8, 6, 2 для смещения блока


	}
}
