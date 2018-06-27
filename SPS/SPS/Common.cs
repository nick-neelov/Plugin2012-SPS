using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Internal;

namespace Neelov.AutocadPlugin
{
	class Common
	{
		/// <summary>
		/// Вставка блоков
		/// </summary>
		internal class Block
		{
			internal static void InsertWithRotation(string blockName, Point3d insPoint, double rotation = 0, double scaleX = 100, double scaleY = 100, double scaleZ = 100)
			{

				Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
				Database db = Application.DocumentManager.MdiActiveDocument.Database;

				using (Transaction tr = db.TransactionManager.StartTransaction())
				{
					try
					{
						BlockTable bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
						//Получаем ID текущего пространства
						BlockTableRecord btr = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
						//Получаем определние блока для вставки, здесь blkName
						//имя блока, который надо получить,
						//прежде лучше бы проверить на его наличие в файле
						if (!bt.Has(blockName))
						{
							ed.WriteMessage("\nБлока с именем: " + blockName + " нет в чертеже! Добавте его с паслитры!");
							return;
						}

						BlockTableRecord gleaderBtr = bt[blockName].GetObject(OpenMode.ForRead) as BlockTableRecord;
						BlockReference gleader = new BlockReference(insPoint, gleaderBtr.ObjectId);

						// Масштаб блока при вставке
						Matrix3d ucs = ed.CurrentUserCoordinateSystem;
						gleader.TransformBy(ucs);
						gleader.ScaleFactors = new Scale3d(scaleX, scaleY, scaleZ);
						gleader.Rotation = rotation;

						btr.AppendEntity(gleader);
						tr.AddNewlyCreatedDBObject(gleader, true);

						// TODO добавить обработку, если блок аннотитивный
						ObjectContextManager ocm = db.ObjectContextManager;
						ObjectContextCollection occ = ocm.GetContextCollection("ACDB_ANNOTATIONSCALES");
						if (gleaderBtr.Annotative == AnnotativeStates.True)
						{
							ObjectContexts.AddContext(gleader, occ.CurrentContext);
						}

						gleader.SetDatabaseDefaults();

						// Проверяем есть ли атрибуты в блоке
						if (gleaderBtr.HasAttributeDefinitions)
						{
							//Получаепм определения атрибутов
							var attDefs = gleaderBtr.Cast<ObjectId>()
								.Where(n => n.ObjectClass.Name == "AcDbAttributeDefinition")
								.Select(n => (AttributeDefinition)n.GetObject(OpenMode.ForRead));

							foreach (AttributeDefinition ad in attDefs)
							{
								AttributeReference attref = new AttributeReference();
								attref.SetAttributeFromBlock(ad, gleader.BlockTransform);
								gleader.AttributeCollection.AppendAttribute(attref);
								tr.AddNewlyCreatedDBObject(attref, true);
								if (gleaderBtr.Annotative == AnnotativeStates.True)
								{
									ObjectContexts.AddContext(attref, occ.CurrentContext);
								}
							}
						}

						tr.Commit();
					} // end try
					catch (Autodesk.AutoCAD.Runtime.Exception ex)
					{
						ed.WriteMessage("\nИсключение " + ex.Message + "\nСтрока: " + ex.StackTrace);
					}
					finally
					{
						tr.Dispose();
					}
				} //end using       			
			}
		}

		/// <summary>
		/// Добавление имени сектора в блок
		/// </summary>
		internal class Sectors
		{
			string SectorName;
			double[] SectorCoordinates;

			public Sectors()
			{
			}

			/// <summary>
			/// Конструктор 
			/// </summary>
			/// <param name="SectorName">Имя сектора</param>
			/// <param name="SectorCoordinates">Координаты сектора</param>
			public Sectors(string SectorName, double[] SectorCoordinates)
			{
				this.SectorName = SectorName;
				this.SectorCoordinates = SectorCoordinates;
			}

			/// <summary>
			/// Метод возвращающий список секторов (название, координаты)
			/// </summary>
			/// <returns>Список секторов</returns>
			static private List<Sectors> CreateSectorsMassive()
			{
				//Координатные обозначения
				string[] masLetter = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "J", "K" };
				string[] masNumber = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };

				// Сектор
				Sectors sector;

				//Список секторов
				List<Sectors> result = new List<Sectors>();

				// Начальные координаты сектора
				double y1 = 0;
				double y2 = 10000;

				// Рабочие координаты сектора
				double[] workCoordinates = { 0, 0, 10000, 10000 };


				for (int i = 0; i < masLetter.Count(); i++)
				{
					double x1 = 0;
					double x2 = 10000;

					for (int j = 0; j < masNumber.Count(); j++)
					{
						// Имя сектора
						string sectorName = masLetter[i] + masNumber[j];

						// Создаем экземпляр класса сектор					
						sector = new Sectors(sectorName, workCoordinates);


						// Добавляем сектор в результурующий список	
						sector = new Sectors(sectorName, workCoordinates);
						result.Add(sector);

						// Сдивагаем координаты к следующему сектору по горизонтали
						x1 = x1 + 10000;
						x2 = x2 + 10000;

						//if (masNumber[j] == "15")
						//{
						//	x2 = 9999999;
						//}

						workCoordinates = new double[] { x1, y1, x2, y2 };



					}
					//Сдивагаем координаты к следующему сектору по вертикали
					y1 = y1 + 10000;
					y2 = y2 + 10000;
					x1 = 0;
					x2 = 10000;

					workCoordinates = new double[] { x1, y1, x2, y2 };
				}
				//результат
				return result;
			}

			static public void AddSectorInBlock(BlockReference br)
			{
				// Получаем список секторов
				List<Sectors> listSectors = CreateSectorsMassive();
				string sector = "";

				//Координаты точки вставки блока
				double brXPoint = br.Position.X;
				double brYPoint = br.Position.Y;

				foreach (Sectors se in listSectors)
				{
					double dy = 0;
					// Координаты сектора
					double[] coordinates = new double[4];
					coordinates = se.SectorCoordinates;

					// Вычисляем номер сектора, куда вставлен блок
					for (int j = 0; j < 14; j++)
					{
						if (brXPoint >= coordinates[0] && brYPoint >= coordinates[1] + dy && brXPoint <= coordinates[2] && brYPoint <= coordinates[3] + dy)
						{
							sector = se.SectorName;
						}
						// Переход на следующий этаж
						dy = dy + 200000;
					}
				}
				// Добавляем в блок номер сектора
				Attributes.SetAttribute(br, "1", sector);
			}

		}

		/// <summary>
		/// Класс для работы с атрибутами блоков
		/// </summary>
		internal class Attributes
		{
			/// <summary>
			/// Метод возвращает значение атрибутa блока по имени 
			/// </summary>
			/// <param name="br">Вхождение блока</param>
			/// <param name="tag">Имя атрибута</param>
			/// <returns>Значение атрибута</returns>
			static public string GetAttributre(BlockReference br, string tag)
			{
				string result = "";
				using (Transaction tr = Application.DocumentManager.MdiActiveDocument
					.Database.TransactionManager.StartTransaction())
				{
					foreach (ObjectId id in br.AttributeCollection)
					{
						Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
						if (ent is AttributeReference)
						{
							AttributeReference ar = ent as AttributeReference;
							if (ar.Tag == tag)
							{
								result = ar.TextString;
							}
						}
					}
					tr.Commit();
				}
				return result;
			}


			/// <summary>
			/// Установка значений атрибутов
			/// </summary>
			/// <param name="br">Вхождение блока</param>
			/// <param name="tag">Имя атрибута</param>
			/// <param name="txt">Значение атрибута</param>
			static public void SetAttribute(BlockReference br, string tag, string txt)
			{
				using (Transaction tr = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
				{
					foreach (ObjectId id in br.AttributeCollection)
					{
						Entity ent = tr.GetObject(id, OpenMode.ForWrite) as Entity;
						if (ent is AttributeReference)
						{
							AttributeReference ar = ent as AttributeReference;
							if (ar.Tag == tag)
							{
								ar.TextString = txt;
							}
						}
					}
					tr.Commit();
				}
			}
		}
	}


	internal static class Methods
	{


		/// <summary>
		/// Метод для вычисления в какую сторону сдвить текст 
		/// </summary>
		/// <param name="moveSide">Направление смещения блока</param>
		/// <param name="blockPosition">Координаты блока</param>
		/// <returns></returns>
		static internal Point3d FirstTextPosition(string moveSide, Point3d blockPosition)
		{
			Point3d result = new Point3d();

			switch (moveSide)
			{
				case "2":
					result = new Point3d(blockPosition.X + 50, blockPosition.Y - 350, blockPosition.Z);
					break;

				case "4":
					result = new Point3d(blockPosition.X - 1500, blockPosition.Y + 50, blockPosition.Z);
					break;

				case "6":
					result = new Point3d(blockPosition.X + 350, blockPosition.Y + 50, blockPosition.Z);
					break;

				case "8":
					result = new Point3d(blockPosition.X - 50, blockPosition.Y + 350, blockPosition.Z);
					break;

				default:
					result = new Point3d(blockPosition.X + 350, blockPosition.Y + 50, blockPosition.Z);
					break;
			}
			return result;
		}

		static internal Point3d NextTextPosition(string moveSide, Point3d firstText)
		{
			Point3d result = new Point3d();
			
			switch (moveSide)
			{
				case "2":
					result = new Point3d(firstText.X - 250, firstText.Y, firstText.Z);
					break;

				case "4":
				case "6":
					result = new Point3d(firstText.X, firstText.Y - 250, firstText.Z);
					break;

				case "8":
					result = new Point3d(firstText.X + 250, firstText.Y, firstText.Z);
					break;

				default:
					result = new Point3d(firstText.X, firstText.Y - 250, firstText.Z);
					break;
			}
			return result;
		}
		
		/// <summary>
		/// Метод для вычисления поворота текста в радианах
		/// </summary>
		/// <param name="moveSide">Направление смещения блока</param>
		/// <returns></returns>
		static internal double TextRotation(string moveSide)
		{
			double result = 0;

			if (moveSide == "8")
				result = 90;
			else if (moveSide == "2")
				result = 270;

			return ConvertDegToRad(result);
		}
		
		static public double ConvertDegToRad(double deg)
		{
			return deg * Math.PI / 180;
		}

		static public double ConvertRadToDeg(double rad)
		{
			return rad * 180 / Math.PI;
		}

		static public double DictanceBetweenBlocks(Point3d pnt1, Point3d pnt2)
		{
			return Math.Sqrt(Math.Pow(pnt2.X - pnt1.X, 2) + Math.Pow(pnt2.Y - pnt2.Y, 2));
		}

		/// <summary>
		/// Метод для определения индекса минимального элемента массива
		/// </summary>
		/// <param name="mass"> Массив целых чисел</param>
		/// <returns>Индекс минимального элемента в массиве</returns>
		static int GetMimIndexElemMas(int[] mass)
		{
			int index = 0;
			int minElem = mass[0];

			for (int i = 0; i < mass.GetLength(0); i++)
			{
				if (minElem > mass[i])
				{
					minElem = mass[i];
					index = i;
				}
			}
			return index;
		}

		/// <summary>
		/// Получение координат точки, находящейся между двумя точками (центра)
		/// </summary>
		/// <param name="pnt1"> Точка 1</param>
		/// <param name="pnt2"> Точка 2</param>
		/// <returns>Координаты точки (центра)</returns>
		static public Point3d CenterPointBetweenPoints(Point3d pnt1, Point3d pnt2)
		{
			return new Point3d((pnt1.X + pnt2.X) / 2, (pnt1.Y + pnt2.Y) / 2, (pnt1.Z + pnt2.Z) / 2);
		}

		/// <summary>
		/// Метод для получения координат вставки текста с учетом минимального количества пересечений с другими блоками и текстом
		/// </summary>
		/// <param name="br"> Ссылка на вхождение блока</param>
		/// <param name="portText"> Текстовая строка, котороя будет вставленна</param>
		/// <returns> Массив [Координаты точки вставки текста] [Номер положения текста], </returns>
		static public object[] GetPointToInsert(BlockReference br, string portText)
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Database db = doc.Database;
			Editor ed = doc.Editor;

			// Угол вставки текста
			double ang = 0;

			// Результат вычислений
			object[] result = new object[2];

			// Промежуточная точка вставки  
			Point3d preInsPoint = new Point3d();

			// Массив для хранения количества пересечений 
			int[] masIntersection = new int[4];

			// Массив для хранения точек вставки текста
			Point3d[] masPointInsert = new Point3d[4];

			using (Transaction tr = db.TransactionManager.StartTransaction())
			{
				try
				{
					// Получаем точки габаритов контейнера объекта
					Extents3d extBlk = br.GeometryExtentsBestFit();
					// Нижний левфй угол
					Point3d botLeftPoint = extBlk.MinPoint;
					// Верхний правый угол
					Point3d topRightPoint = extBlk.MaxPoint;
					//Центр блока
					Point3d cenPoint = CenterPointBetweenPoints(botLeftPoint, topRightPoint);

					// Определяем точки секущей рамки для выбора объектов вокруг блока
					Point3d fPointCrossWindow = new Point3d(botLeftPoint.X - 3000.0, botLeftPoint.Y - 3000.0, botLeftPoint.Z);
					Point3d sPointCrossWindow = new Point3d(topRightPoint.X + 3000.0, topRightPoint.Y + 3000.0, topRightPoint.Z);

					// Выбор объектов вокруг блока                    
					SelectionSet ssObj = ed.SelectWindow(fPointCrossWindow, sPointCrossWindow).Value;

					for (int i = 0; i <= 3; i++)
					{
						// Количество пресечений
						int count = 0;
						// Первое место вставки текста
						if (i == 0)
						{
							preInsPoint = new Point3d(cenPoint.X + 350.0, cenPoint.Y + 50, cenPoint.Z);
							ang = 0;
						}
						// Второе место вставки текста
						if (i == 1)
						{
							preInsPoint = new Point3d(cenPoint.X - 50.0, cenPoint.Y - 350, cenPoint.Z);
							ang = 270;
						}
						// Третье место вставки текста
						if (i == 2)
						{
							preInsPoint = new Point3d(cenPoint.X - 1100, cenPoint.Y + 50, cenPoint.Z);
							ang = 0;
						}
						// Четвертое место вставки текста
						if (i == 3)
						{
							preInsPoint = new Point3d(cenPoint.X - 50.0, cenPoint.Y + 350, cenPoint.Z);
							ang = 90.0;
						}

						// ПРроверяем пересечение текста с другими объектами
						using (DBText text = new DBText())
						{
							text.TextString = portText;
							text.Height = 200;
							text.Position = preInsPoint;
							text.Rotation = ConvertDegToRad(ang);

							foreach (SelectedObject soEnt in ssObj)
							{
								// Ссылка на обхекты вокруг блока
								Entity entObj = tr.GetObject(soEnt.ObjectId, OpenMode.ForRead) as Entity;
								Point3dCollection pColl = new Point3dCollection();
								Plane pln = new Plane();

								text.IntersectWith(entObj, Intersect.OnBothOperands, pln, pColl, IntPtr.Zero, IntPtr.Zero);
								// Количество пересечений
								count = count + pColl.Count;
							}
						}
						//// Записываем в массив количество пресечений
						masIntersection[i] = count;

						//// Записываем в массив координаты точки вставки текста
						masPointInsert[i] = preInsPoint;
					}
					// Точка вставки текста с минимальным количеством пересечений
					result[0] = masPointInsert[GetMimIndexElemMas(masIntersection)];

					// Номер позициии для вставки текста
					result[1] = GetMimIndexElemMas(masIntersection);
				}

				catch (System.Exception ex)
				{
					ed.WriteMessage("\n" + ex.Message);
				}

				finally
				{
					tr.Dispose();
				}
			}
			return result;
		}

		public static void CreateText(string txt, Point3d pos, double angle)
		{
			// Устанавливаем текущий документ и базу данных
			Document acDoc = Application.DocumentManager.MdiActiveDocument;
			Database acCurDb = acDoc.Database;

			// Начинаем транзакцию
			using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
			{
				// Открываем таблицу Блока для чтения
				BlockTable acBlkTbl;
				acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

				// Открываем запись таблицы Блока пространство Модели (Model space) для записи
				BlockTableRecord acBlkTblRec;
				acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

				// Создаем однострочный текстовый объект
				DBText acText = new DBText();
				acText.SetDatabaseDefaults();

				acText.Position = pos;
				acText.Height = 200;
				acText.TextString = txt;
				acText.Rotation = ConvertDegToRad(angle);

				acBlkTblRec.AppendEntity(acText);
				acTrans.AddNewlyCreatedDBObject(acText, true);

				// Сохраняем изменения и закрываем транзакцию
				acTrans.Commit();
			}
		}


		//public static void CreateTextAlignet(string txt, Point3d pos, double angle, int mode)
		//{
		//	// Get the current document and database
		//	Document acDoc = Application.DocumentManager.MdiActiveDocument;
		//	Database acCurDb = acDoc.Database;

		//	// Start a transaction
		//	using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
		//	{
		//		// Open the Block table for read
		//		BlockTable acBlkTbl;
		//		acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
		//									 OpenMode.ForRead) as BlockTable;

		//		// Open the Block table record Model space for write
		//		BlockTableRecord acBlkTblRec;
		//		acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
		//										OpenMode.ForWrite) as BlockTableRecord;

		//		string[] textString = new string[3];
		//		textString[0] = "Left";
		//		textString[1] = "Center";
		//		textString[2] = "Right";

		//		int[] textAlign = new int[3];
		//		textAlign[0] = (int)TextHorizontalMode.TextLeft;
		//		textAlign[1] = (int)TextHorizontalMode.TextCenter;
		//		textAlign[2] = (int)TextHorizontalMode.TextRight;

		//		Point3d acPtIns = new Point3d(3, 3, 0);
		//		Point3d acPtAlign = new Point3d(3, 3, 0);

		//	//	int nCnt = 0;

		//		foreach (string strVal in textString)
		//		{
		//			// Create a single-line text object
		//			DBText acText = new DBText();
		//			acText.SetDatabaseDefaults();
		//			acText.Position = acPtIns;
		//			acText.Height = 0.5;
		//			acText.TextString = strVal;

		//			// Set the alignment for the text
		//			acText.HorizontalMode = (TextHorizontalMode)textAlign[mode];

		//			if (acText.HorizontalMode != TextHorizontalMode.TextLeft)
		//			{
		//				acText.AlignmentPoint = acPtAlign;
		//			}

		//			acBlkTblRec.AppendEntity(acText);
		//			acTrans.AddNewlyCreatedDBObject(acText, true);

		//			// Create a point over the alignment point of the text
		//			DBPoint acPoint = new DBPoint(acPtAlign);
		//			acPoint.SetDatabaseDefaults();
		//			acPoint.ColorIndex = 1;

		//			acBlkTblRec.AppendEntity(acPoint);
		//			acTrans.AddNewlyCreatedDBObject(acPoint, true);

		//			// Adjust the insertion and alignment points
		//			acPtIns = pos;
		//			acPtAlign = acPtIns;

					
		//		}

		//		// Set the point style to crosshair
		//		Application.SetSystemVariable("PDMODE", 2);

		//		// Save the changes and dispose of the transaction
		//		acTrans.Commit();
		//	}
		//	}


		/// <summary>
		/// Метод для рисования линии
		/// </summary>
		/// <param name="pnt1">Точка 1</param>
		/// <param name="pnt2">Точка 2</param>
		/// <param name="layer">Имя слоя</param>
		public static void CreateLine(Point3d pnt1, Point3d pnt2, string layer)
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Database db = doc.Database;

			using (Transaction tr = db.TransactionManager.StartTransaction())
			{
				BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
				BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

				Line line = new Line(pnt1, pnt2);
				line.SetDatabaseDefaults();

				
				line.Layer = layer;

				btr.AppendEntity(line);
				tr.AddNewlyCreatedDBObject(line, true);

				tr.Commit();
			}
		}

	}










}
