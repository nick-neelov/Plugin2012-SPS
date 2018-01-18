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

    public class Program
    {
		[CommandMethod("NK-SPS-INSERT-EQVIPMENT")]
		public void WorkWithPlan()
		{
			while(true)
			{
				InsertEqvipment.AddEqvipmentOnPlan();
			}
			
		}

		[CommandMethod("SPS-Plugin")]
		public void CKCPlugin()
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			if (doc == null)
				return;

			Editor ed = doc.Editor;
			Database db = doc.Database;

			try
			{
				string flagExit = "";

				while (flagExit != "Выход")
				{
					// Главное меню после запуска программы
					PromptKeywordOptions pkoMainMenu = new PromptKeywordOptions("\nВыберите режим работы: ");
					pkoMainMenu.Keywords.Add("Планы");
					pkoMainMenu.Keywords.Add("Схемы");
					pkoMainMenu.Keywords.Add("Выход");
					pkoMainMenu.Keywords.Default = "Планы";

					PromptResult prMainMenu = ed.GetKeywords(pkoMainMenu);
					if (prMainMenu.Status != PromptStatus.OK)
						return;

					switch (prMainMenu.StringResult)
					{
						// Подменю работы с планами
						case "Планы":

							PromptKeywordOptions pkoPlanMenu = new PromptKeywordOptions("\nВыберите режим работы с планами (оборудованием) : ");
							pkoPlanMenu.Keywords.Add("Размещение");
							pkoPlanMenu.Keywords.Add("Подключение");
							pkoPlanMenu.Keywords.Add("Главное меню");
							pkoPlanMenu.Keywords.Default = "Размещение";

							PromptResult prPlanMenu = ed.GetKeywords(pkoPlanMenu);
							if (prPlanMenu.Status != PromptStatus.OK)
								return;

							switch (prPlanMenu.StringResult)
							{
								// Размещение оборудования СКС на планах
								case "Размещение":
									InsertEqvipment.AddEqvipmentOnPlan();
									break;

								// Отрисовка кабелей от шкафа до розеток
								case "Подключение":
									ed.WriteMessage("\nФункции пока нет");
									break;

								// Возврат в меню
								case "Главное":
									break;

								default:
									ed.WriteMessage("Ничего не выбрано...");
									break;
							}
							break;

						// Подменю работы со схемами
						case "Схемы":
							PromptKeywordOptions pkoSchemeMenu = new PromptKeywordOptions("\nВыберите вид схемы: ");
							pkoSchemeMenu.Keywords.Add("Структурная");
							//pkoSchemeMenu.Keywords.Add("Размещения");
							//pkoSchemeMenu.Keywords.Add("Коммутационная");
							pkoSchemeMenu.Keywords.Add("Главное меню");
							pkoSchemeMenu.Keywords.Default = "Структурная";

							PromptResult prSchemeMenu = ed.GetKeywords(pkoSchemeMenu);
							if (prSchemeMenu.Status != PromptStatus.OK)
								return;

							switch (prSchemeMenu.StringResult)
							{
								case "Структурная":
									// метод отросовки структурных схем
									break;

								case "Главное меню":
									// Возврат в меню
									break;

								default:
									// Выход из 
									ed.WriteMessage("Не выбран выд схемы...");
									break;
							}
							break;

						// Выходим из плагина
						case "Выход":
							flagExit = "Выход";
							break;

						// Выходим из плагина, если что-то пошло не так
						default:
							ed.WriteMessage("\nНе выбран режим...");
							break;

					}



				}


			}

			catch (Autodesk.AutoCAD.Runtime.Exception ex)
			{
				ed.WriteMessage("\nИсключение " + ex.Message + "\nВ строке " + ex.StackTrace);
			}






		}






	}


}
