//using NPOI.SS.UserModel;
//using NPOI.XSSF.UserModel;
//using System.Drawing.Text;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.InputSystem.iOS;

//[CreateAssetMenu(fileName = "BodypartsSyncronizer", menuName = "Databases/BodypartsSyncronizer")]
//public class BodyPartsSyncronizer : ScriptableObject
//{
//    [SerializeField] private string _complexObjectsTablePath;
//    [SerializeField] private string _simpleObjectsTablePath;
//    [ContextMenu("Sync")]
//    public void Sync()
//    {
//        var complexObjectsTable = ExcelUtills.ReadWorkbook(_complexObjectsTablePath) as XSSFWorkbook;
//        var simpleObjectsTable = ExcelUtills.ReadWorkbook(_simpleObjectsTablePath) as XSSFWorkbook;
//        var complexShet = complexObjectsTable.GetSheetAt(0);
//        var simpleSheet = simpleObjectsTable.GetSheetAt(0);

//        ICell[] complexNamesCells = new ICell[complexShet.PhysicalNumberOfRows - 2];
//        var bodypartsColumnIndex = complexShet.GetRow(0).Cells.First(c => c.StringCellValue == "bodyParts").ColumnIndex;
//        var firstRow = GetFirstRow();
//        var currentRowNum = firstRow;
//        var bodyPartDefaultStats = simpleSheet.GetRow(8);
//        for (int i = 2; i < complexShet.PhysicalNumberOfRows; i++)
//        {
//            var row = complexShet.GetRow(i);
//            var complexObjectName = row.GetCell(0).StringCellValue;
//            var bodyPartsCell = row.GetCell(bodypartsColumnIndex);

//            if (bodyPartsCell != null)
//            {
//                var bodyParts = bodyPartsCell.StringCellValue.Split(',');

//                for (int p = 0; p < bodyParts.Length; p++)
//                {
//                    var bodyPartName = bodyParts[p];

//                    currentRowNum++;

//                    var newRow = simpleSheet.CreateRow(currentRowNum);
//                    newRow.Height = 600;
//                    newRow.CreateCell(0).SetCellValue(complexObjectName + bodyPartName);
//                    newRow.CreateCell(simpleSheet.GetRow(0).Cells.Find(c => c.StringCellValue == "realNameRU").ColumnIndex).SetCellValue("Часть тела");
//                    newRow.CreateCell(simpleSheet.GetRow(0).Cells.Find(c => c.StringCellValue == "realNameEN").ColumnIndex).SetCellValue("Body part");
//                    newRow.CreateCell(simpleSheet.GetRow(0).Cells.Find(c => c.StringCellValue == "descriptionRU").ColumnIndex).SetCellValue("Чья-то часть тела");
//                    newRow.CreateCell(simpleSheet.GetRow(0).Cells.Find(c => c.StringCellValue == "descriptionEN").ColumnIndex).SetCellValue("Someones body part");
//                }
//            }
//        }
//        ExcelUtills.WriteWorkBook(simpleObjectsTable, _simpleObjectsTablePath);

//        int GetFirstRow()
//        {
//            for (int i = 0; i < simpleSheet.PhysicalNumberOfRows; i++)
//            {
//                var row = simpleSheet.GetRow(i);
//                if (row.GetCell(0).CellComment != null && row.GetCell(0).CellComment.String.String == "TableEnd")
//                {
//                    return i;
//                }
//            }
//            return simpleSheet.PhysicalNumberOfRows -1;
//        }

//    }
//}