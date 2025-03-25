using CsvHelper;
using CsvHelper.Configuration;
using System.Data;
using System.Globalization;

namespace Si.Utilites.Files;

public static class CsvHelper
{
    /// <summary>
    /// 读取CSV文件到DataTable
    /// </summary>
    /// <param name="filePath">CSV文件路径</param>
    /// <param name="hasHeader">是否包含表头</param>
    /// <param name="delimiter">分隔符</param>
    /// <returns>DataTable对象</returns>
    public static DataTable ReadToDataTable(string filePath, bool hasHeader = true, string delimiter = ",")
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = hasHeader,
            Delimiter = delimiter,
            DetectDelimiter = true,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        using var dr = new CsvDataReader(csv);

        var dt = new DataTable();
        dt.Load(dr);
        return dt;
    }

    /// <summary>
    /// 将DataTable写入CSV文件
    /// </summary>
    /// <param name="dt">DataTable对象</param>
    /// <param name="filePath">CSV文件保存路径</param>
    /// <param name="delimiter">分隔符</param>
    public static void WriteFromDataTable(DataTable dt, string filePath, string delimiter = ",")
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            TrimOptions = TrimOptions.Trim
        };

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, config);

        // 写入表头
        foreach (DataColumn column in dt.Columns)
        {
            csv.WriteField(column.ColumnName);
        }
        csv.NextRecord();

        // 写入数据
        foreach (DataRow row in dt.Rows)
        {
            foreach (var item in row.ItemArray)
            {
                csv.WriteField(item);
            }
            csv.NextRecord();
        }
    }

    /// <summary>
    /// 读取CSV文件到对象列表
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="filePath">CSV文件路径</param>
    /// <param name="hasHeader">是否包含表头</param>
    /// <param name="delimiter">分隔符</param>
    /// <returns>对象列表</returns>
    public static List<T> ReadToList<T>(string filePath, bool hasHeader = true, string delimiter = ",") where T : class
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = hasHeader,
            Delimiter = delimiter,
            DetectDelimiter = true,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<T>().ToList();
    }

    /// <summary>
    /// 将对象列表写入CSV文件
    /// </summary>
    /// <typeparam name="T">源类型</typeparam>
    /// <param name="list">对象列表</param>
    /// <param name="filePath">CSV文件保存路径</param>
    /// <param name="delimiter">分隔符</param>
    public static void WriteFromList<T>(List<T> list, string filePath, string delimiter = ",") where T : class
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            TrimOptions = TrimOptions.Trim
        };

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, config);
        csv.WriteRecords(list);
    }

    /// <summary>
    /// 读取CSV文件到字典列表
    /// </summary>
    /// <param name="filePath">CSV文件路径</param>
    /// <param name="hasHeader">是否包含表头</param>
    /// <param name="delimiter">分隔符</param>
    /// <returns>字典列表</returns>
    public static List<Dictionary<string, string>> ReadToDictionaryList(string filePath, bool hasHeader = true, string delimiter = ",")
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = hasHeader,
            Delimiter = delimiter,
            DetectDelimiter = true,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<dynamic>().Select(row =>
            ((IDictionary<string, object>)row).ToDictionary(x => x.Key, x => x.Value?.ToString() ?? string.Empty)
        ).ToList();
    }

    /// <summary>
    /// 将字典列表写入CSV文件
    /// </summary>
    /// <param name="list">字典列表</param>
    /// <param name="filePath">CSV文件保存路径</param>
    /// <param name="delimiter">分隔符</param>
    public static void WriteFromDictionaryList(List<Dictionary<string, string>> list, string filePath, string delimiter = ",")
    {
        if (list == null || !list.Any())
        {
            throw new ArgumentException("列表不能为空");
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            TrimOptions = TrimOptions.Trim
        };

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, config);

        // 写入表头
        foreach (var key in list[0].Keys)
        {
            csv.WriteField(key);
        }
        csv.NextRecord();

        // 写入数据
        foreach (var dict in list)
        {
            foreach (var value in dict.Values)
            {
                csv.WriteField(value);
            }
            csv.NextRecord();
        }
    }

    /// <summary>
    /// 读取CSV文件到字符串列表
    /// </summary>
    /// <param name="filePath">CSV文件路径</param>
    /// <param name="hasHeader">是否包含表头</param>
    /// <param name="delimiter">分隔符</param>
    /// <returns>字符串列表</returns>
    public static List<string[]> ReadToStringList(string filePath, bool hasHeader = true, string delimiter = ",")
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = hasHeader,
            Delimiter = delimiter,
            DetectDelimiter = true,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<dynamic>().Select(row =>
            ((IDictionary<string, object>)row).Values.Select(x => x?.ToString() ?? string.Empty).ToArray()
        ).ToList();
    }

    /// <summary>
    /// 将字符串列表写入CSV文件
    /// </summary>
    /// <param name="list">字符串列表</param>
    /// <param name="filePath">CSV文件保存路径</param>
    /// <param name="delimiter">分隔符</param>
    public static void WriteFromStringList(List<string[]> list, string filePath, string delimiter = ",")
    {
        if (list == null || !list.Any())
        {
            throw new ArgumentException("列表不能为空");
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            TrimOptions = TrimOptions.Trim
        };

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, config);

        foreach (var row in list)
        {
            foreach (var field in row)
            {
                csv.WriteField(field);
            }
            csv.NextRecord();
        }
    }
}