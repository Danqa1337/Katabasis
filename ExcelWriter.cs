//using System.Collections;
//using System.Collections.Generic;
//using NPOI;
//using NPOI.SS.UserModel;
//using System;
//using System.IO;
//using NPOI.XSSF.UserModel;
//using UnityEngine;

//public static class ExcelUtills
//{
//    public static IWorkbook ReadWorkbook(string path)
//    {
//        IWorkbook book;

//        FileStream stream = new FileStream(Application.dataPath + path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

//        book = new XSSFWorkbook(stream);

//        stream.Close();
//        return book;
//    }
//    public static void WriteWorkBook(IWorkbook workbook, string path)
//    {
//        using (FileStream stream = new FileStream(Application.dataPath + path, FileMode.Create, FileAccess.Write))
//        {
//            workbook.Write(stream);
//            stream.Close();
//        }

//    }
//}