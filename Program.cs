using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

public class F
{
    public int i1, i2, i3, i4, i5;

    public F Get() => new F() { i1 = 1, i2 = 2, i3 = 3, i4 = 4, i5 = 5 };
}

public class Program
{
    private const int Iterations = 10000;

    public static void Main(string[] args)
    {
        F obj = new F().Get();

        // 1. Кастомная сериализация свойств/полей
        TimeSpan customSerializationTime = MeasureTime(() =>
        {
            for (int i = 0; i < Iterations; i++)
            {
                CustomSerialize(obj);
            }
        });

        string serializedString = CustomSerialize(obj);
        Console.WriteLine($"Custom Serialization Result: {serializedString}");
        Console.WriteLine($"Custom Serialization Time ({Iterations} iterations): {customSerializationTime}");

        // 2. Замер времени вывода в консоль
        TimeSpan consoleOutputTime = MeasureTime(() => Console.WriteLine($"Console Output (from Custom Serialization): {serializedString}"));
        Console.WriteLine($"Console Output Time: {consoleOutputTime}");

        // 3. JSON сериализация
        TimeSpan jsonSerializationTime = MeasureTime(() =>
        {
            for (int i = 0; i < Iterations; i++)
            {
                JsonSerialize(obj);
            }
        });

        string jsonString = JsonSerialize(obj);
        Console.WriteLine($"JSON Serialization Result: {jsonString}");
        Console.WriteLine($"JSON Serialization Time ({Iterations} iterations): {jsonSerializationTime}");

        // 4. Десериализация из строки (INI-подобный формат)
        TimeSpan customDeserializationTime = MeasureTime(() =>
        {
            for (int i = 0; i < Iterations; i++)
            {
                CustomDeserialize(serializedString);
            }
        });

        F deserializedObj = CustomDeserialize(serializedString);
        Console.WriteLine($"Custom Deserialization Result: i1={deserializedObj.i1}, i2={deserializedObj.i2}, i3={deserializedObj.i3}, i4={deserializedObj.i4}, i5={deserializedObj.i5}");
        Console.WriteLine($"Custom Deserialization Time ({Iterations} iterations): {customDeserializationTime}");


        Console.ReadKey();
    }

    // Кастомная сериализация
    public static string CustomSerialize(F obj)
    {
        StringBuilder sb = new StringBuilder();
        Type type = typeof(F);
        FieldInfo[] fields = type.GetFields();

        foreach (FieldInfo field in fields)
        {
            sb.Append($"{field.Name}={field.GetValue(obj)};");
        }

        return sb.ToString();
    }

    // Кастомная десериализация (INI-подобный формат)
    public static F CustomDeserialize(string serializedString)
    {
        F obj = new F();
        string[] parts = serializedString.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (string part in parts)
        {
            string[] keyValue = part.Split('=');
            if (keyValue.Length == 2)
            {
                string fieldName = keyValue[0];
                string fieldValue = keyValue[1];

                FieldInfo field = typeof(F).GetField(fieldName);
                if (field != null && field.FieldType == typeof(int))
                {
                    if (int.TryParse(fieldValue, out int parsedValue))
                    {
                        field.SetValue(obj, parsedValue);
                    }
                }
            }
        }

        return obj;
    }

    // JSON сериализация
    public static string JsonSerialize(F obj)
    {
        return JsonSerializer.Serialize(obj);
    }

    // Метод для замера времени
    public static TimeSpan MeasureTime(Action action)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }
}