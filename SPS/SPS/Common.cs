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
	class Common
	{
		/// <summary>
		/// Вставка блоков
		/// </summary>
		internal class Block
		{
			internal static void InsertNoRotation(string blockName, Point3d insPoint, double scaleX = 100, double scaleY = 100, double scaleZ = 100)
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
				Attributes.SetAttribute(br, "01", sector);
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
}
