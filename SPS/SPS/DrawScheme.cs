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

	class Eqvipment
	{
		private string Room;
		private string BlockName;		
		private string NumberInSystem;
		private string NameInSystem;
		private string DistanceTo;
		private string CabelType;

		/// <summary>
		/// Конструктор реализующий экземпляр класса Eqvipment
		/// </summary>
		/// <param name="Room"></param>
		/// <param name="BlockName"></param>	
		/// <param name="NumberInSystem"></param>
		/// <param name="NameInSystem"></param>
		/// <param name="DistanceTo"></param>
		/// <param name="CabelType"></param>
		public Eqvipment(string Room, string BlockName, string NumberInSystem, string NameInSystem, string DistanceTo, string CabelType)
		{
			this.Room = Room;
			this.BlockName = BlockName;			
			this.NumberInSystem = NumberInSystem;
			this.NameInSystem = NameInSystem;
			this.DistanceTo = DistanceTo;
			this.CabelType = CabelType;
		}

	}


	class DrawScheme
	{
		/// <summary>
		/// Метод для отрисовки структурной схемы палатной сигнализации
		/// </summary>
		public void DrawStructuralSchemeZPT()
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;

			if (doc == null) { return; }

			Editor ed = doc.Editor;
			Database db = doc.Database;

			// Список оборудования
			List<Eqvipment> listEqvipment = new List<Eqvipment>();

			// Фильтр для выбора блоков
			TypedValue[] typeBlock = new TypedValue[]
			{
				new TypedValue((int)DxfCode.Start, "INSERT")
			};

			SelectionFilter filter = new SelectionFilter(typeBlock);


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


			// Перебераем набор блоков, и добавляем их в сиписок, а затем сортируем по номеру в системе
			using (Transaction tr = db.TransactionManager.StartTransaction())
			{

				foreach (SelectedObject so in psrAllBlock.Value)
				{
					BlockReference br = tr.GetObject(so.ObjectId, OpenMode.ForRead) as BlockReference;

					if (br != null)
					{
						// Создаем экземпляр оборудования
						Eqvipment eq = new Eqvipment
						(
							Common.Attributes.GetAttributre(br, "1"),
							Common.Attributes.GetAttributre(br, "6"),						
							Common.Attributes.GetAttributre(br, "11"),
							Common.Attributes.GetAttributre(br, "12"),
							Common.Attributes.GetAttributre(br, "15"),
							Common.Attributes.GetAttributre(br, "16")
						);

						switch(br.Name)
						{
							case "":
								break;

							default:
								break;								

						}



						listEqvipment.Add(eq);
					}



				}

			}











		}



	}
}
