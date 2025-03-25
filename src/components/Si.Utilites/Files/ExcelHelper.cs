using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.Data;

namespace Si.Utilites.Files;

public static class ExcelHelper
{
    /// <summary>
    /// 读取Excel文件到DataTable
    /// </summary>
    /// <param name="filePath">Excel文件路径</param>
    /// <param name="sheetName">工作表名称（可选）</param>
    /// <param name="hasHeader">是否包含表头</param>
    /// <returns>DataTable对象</returns>
    public static DataTable ReadToDataTable(string filePath, string? sheetName = null, bool hasHeader = true)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var workbook = GetWorkbook(fs, filePath);
        var sheet = string.IsNullOrEmpty(sheetName) ? workbook.GetSheetAt(0) : workbook.GetSheet(sheetName);
        
        if (sheet == null)
        {
            throw new ArgumentException($"未找到工作表: {sheetName}");
        }

        var dt = new DataTable();
        var firstRow = sheet.GetRow(0);
        var cellCount = firstRow.LastCellNum;

        // 添加列
        for (var i = 0; i < cellCount; i++)
        {
            var cell = firstRow.GetCell(i);
            var columnName = hasHeader ? cell?.StringCellValue ?? $"Column{i + 1}" : $"Column{i + 1}";
            dt.Columns.Add(columnName);
        }

        // 添加数据
        var startRow = hasHeader ? 1 : 0;
        for (var i = startRow; i <= sheet.LastRowNum; i++)
        {
            var row = sheet.GetRow(i);
            if (row == null) continue;

            var dr = dt.NewRow();
            for (var j = 0; j < cellCount; j++)
            {
                var cell = row.GetCell(j);
                dr[j] = GetCellValue(cell);
            }
            dt.Rows.Add(dr);
        }

        return dt;
    }

    /// <summary>
    /// 将DataTable写入Excel文件
    /// </summary>
    /// <param name="dt">DataTable对象</param>
    /// <param name="filePath">Excel文件保存路径</param>
    /// <param name="sheetName">工作表名称（可选）</param>
    public static void WriteFromDataTable(DataTable dt, string filePath, string? sheetName = null)
    {
        var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet(sheetName ?? "Sheet1");

        // 创建表头
        var headerRow = sheet.CreateRow(0);
        for (var i = 0; i < dt.Columns.Count; i++)
        {
            var cell = headerRow.CreateCell(i);
            cell.SetCellValue(dt.Columns[i].ColumnName);
        }

        // 写入数据
        for (var i = 0; i < dt.Rows.Count; i++)
        {
            var row = sheet.CreateRow(i + 1);
            for (var j = 0; j < dt.Columns.Count; j++)
            {
                var cell = row.CreateCell(j);
                cell.SetCellValue(dt.Rows[i][j]?.ToString() ?? string.Empty);
            }
        }

        // 自动调整列宽
        for (var i = 0; i < dt.Columns.Count; i++)
        {
            sheet.AutoSizeColumn(i);
        }

        // 保存文件
        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        workbook.Write(fs);
        workbook.Close();
    }

    /// <summary>
    /// 读取Excel文件到对象列表
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="filePath">Excel文件路径</param>
    /// <param name="sheetName">工作表名称（可选）</param>
    /// <param name="hasHeader">是否包含表头</param>
    /// <returns>对象列表</returns>
    public static List<T> ReadToList<T>(string filePath, string? sheetName = null, bool hasHeader = true) where T : class, new()
    {
        var dt = ReadToDataTable(filePath, sheetName, hasHeader);
        return ConvertDataTableToList<T>(dt);
    }

    /// <summary>
    /// 将对象列表写入Excel文件
    /// </summary>
    /// <typeparam name="T">源类型</typeparam>
    /// <param name="list">对象列表</param>
    /// <param name="filePath">Excel文件保存路径</param>
    /// <param name="sheetName">工作表名称（可选）</param>
    public static void WriteFromList<T>(List<T> list, string filePath, string? sheetName = null) where T : class
    {
        var dt = ConvertListToDataTable(list);
        WriteFromDataTable(dt, filePath, sheetName);
    }

    /// <summary>
    /// 获取单元格的值
    /// </summary>
    private static string GetCellValue(ICell? cell)
    {
        if (cell == null) return string.Empty;

        return cell.CellType switch
        {
            CellType.Numeric => cell.NumericCellValue.ToString(),
            CellType.String => cell.StringCellValue,
            CellType.Boolean => cell.BooleanCellValue.ToString(),
            CellType.Formula => cell.CellFormula,
            CellType.Blank => string.Empty,
            _ => string.Empty
        };
    }

    /// <summary>
    /// 根据文件扩展名获取Workbook
    /// </summary>
    private static IWorkbook GetWorkbook(Stream fs, string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        return extension switch
        {
            ".xlsx" => new XSSFWorkbook(fs),
            ".xls" => new HSSFWorkbook(fs),
            _ => throw new ArgumentException("不支持的文件格式")
        };
    }

    /// <summary>
    /// 将DataTable转换为对象列表
    /// </summary>
    private static List<T> ConvertDataTableToList<T>(DataTable dt) where T : class, new()
    {
        var list = new List<T>();
        var properties = typeof(T).GetProperties();

        foreach (DataRow row in dt.Rows)
        {
            var item = new T();
            foreach (var prop in properties)
            {
                if (dt.Columns.Contains(prop.Name))
                {
                    var value = row[prop.Name];
                    if (value != DBNull.Value)
                    {
                        prop.SetValue(item, Convert.ChangeType(value, prop.PropertyType));
                    }
                }
            }
            list.Add(item);
        }

        return list;
    }

    /// <summary>
    /// 将对象列表转换为DataTable
    /// </summary>
    private static DataTable ConvertListToDataTable<T>(List<T> list) where T : class
    {
        var dt = new DataTable();
        var properties = typeof(T).GetProperties();

        // 添加列
        foreach (var prop in properties)
        {
            dt.Columns.Add(prop.Name);
        }

        // 添加数据
        foreach (var item in list)
        {
            var row = dt.NewRow();
            foreach (var prop in properties)
            {
                row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
            }
            dt.Rows.Add(row);
        }

        return dt;
    }
} 