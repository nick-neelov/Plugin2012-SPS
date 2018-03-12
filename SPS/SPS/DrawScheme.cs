﻿using System;
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
		private Point3d InsertPointBlock; // ??

		/// <summary>
		/// Конструктор реализующий экземпляр класса Eqvipment
		/// </summary>
		/// <param name="Room">Номер помещения (сектора) </param>
		/// <param name="BlockName">Имя блока</param>
		/// <param name="NumberInSystem">Номер в системе</param>
		/// <param name="NameInSystem">Обозначение в системе</param>
		/// <param name="DistanceTo">Расстояние до предыдущего блока</param>
		/// <param name="CabelType">Тип кабеля</param>
		/// <param name="InsertPointBlock">Точка в ставки блока ?? </param>
		public Eqvipment(string Room, string BlockName, string NumberInSystem, string NameInSystem, string DistanceTo, string CabelType, Point3d InsertPointBlock)
		{
			this.Room = Room;
			this.BlockName = BlockName;
			this.NumberInSystem = NumberInSystem;
			this.NameInSystem = NameInSystem;
			this.DistanceTo = DistanceTo;
			this.CabelType = CabelType;
			this.InsertPointBlock = InsertPointBlock;
		}


		/// <summary>
		/// Свойство для доступу к номеру помещения, где установленно оборудование
		/// </summary>
		public string RoomNumber
		{
			get { return Room; }
			set { }
		}

		/// <summary>
		/// Свойство для доступу к номеру в системе
		/// </summary>
		public string Number
		{
			get { return NumberInSystem; }
			set { }
		}

		/// <summary>
		/// Свойство для доступу к имени в системе
		/// </summary>
		public string Name
		{
			get { return NameInSystem; }
			set { }
		}

		/// <summary>
		/// Свойство для доступа к длине кабеля до предыдущего устройства
		/// </summary>
		public string CabelLenght
		{
			get { return DistanceTo; }
			set { }
		}


		/// <summary>
		/// Свойство для доступу к марке кабеля
		/// </summary>
		public string CabelMark
		{
			get { return CabelType; }
			set { }
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

			// Список оборудования по типам
			List<Eqvipment> listSM = new List<Eqvipment>(); // Список модулей SM
			List<Eqvipment> listIP = new List<Eqvipment>(); // Список IP Оборудования
			List<Eqvipment> listBus = new List<Eqvipment>(); // Список оборудования по шине

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
							Common.Attributes.GetAttributre(br, "16"),
							br.Position
						);

						switch (br.Name)
						{
							// Добавляем в список  SM-оборудование
							case "SM":
								listSM.Add(eq);
								break;

							// Добавляем в список  IP-оборудование
							case "TP":
							case "KJ":
							case "KJD":
							case "SIJD":
							case "SIJ":
							case "SJD":
							case "LJ":
							case "VKJ":
							case "VKJV":
								listIP.Add(eq);
								break;

							// Добавляем в список  Bus-оборудование
							case "SV":
							case "TNV":
							case "TANV":
							case "TANVT":
								listBus.Add(eq);
								break;

							// Выход по дефолту
							default:
								break;

						}
					}
				}

			}
			// Сортируем списки по номеру в системе
			// Для SM
			var sortSM = from sort in listSM
						 orderby sort.Number
						 select sort;

			// Для IP-Оборудования
			var sortIP = from sort in listIP
						 orderby sort.Number
						 select sort;

			// Для Bus-оборудования
			var sortBus = from sort in listBus
						  orderby sort.Number
						  select sort;


			using (Transaction tr = db.TransactionManager.StartTransaction())
			{
				


			}















		}



	}
}
