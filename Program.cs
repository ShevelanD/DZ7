using System;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;

public class F
{
    public int i1;
    public int i2;
    public int i3;
    public int i4;
    public int i5;
}

public class SerializationBenchmark
{
    public static void Main(string[] args)
    {
        int iterations = 1000;

        F obj = new F { i1 = 1, i2 = 2, i3 = 3, i4 = 4, i5 = 5 };

        // Рефлексия
        (long reflectionSerializationTime, long reflectionDeserializationTime) =
            BenchmarkReflection(obj, iterations);

        // Newtonsoft.Json
        (long jsonSerializationTime, long jsonDeserializationTime) =
            BenchmarkNewtonsoftJson(obj, iterations);

        // Формирование результата для отправки
        string result = $@"
Сериализуемый класс: class F {{ int i1, i2, i3, i4, i5;}}
код сериализации-десериализации: 
количество замеров: {iterations} итераций
мой рефлекшен:
Время на сериализацию = {reflectionSerializationTime} мс
Время на десериализацию = {reflectionDeserializationTime} мс
стандартный механизм (NewtonsoftJson):
Время на сериализацию = {jsonSerializationTime} мс
Время на десериализацию = {jsonDeserializationTime} мс";

        Console.WriteLine(result);

        // Здесь можно добавить код для отправки результата в чат
        // Например, через какой-либо API или библиотеку

        Console.ReadKey();
    }

    static (long, long) BenchmarkReflection(F obj, int iterations)
    {
        Stopwatch swSerialization = Stopwatch.StartNew();
        string serializedData = "";

        for (int i = 0; i < iterations; i++)
        {
            serializedData = SerializeReflection(obj);
        }

        swSerialization.Stop();
        long serializationTime = swSerialization.ElapsedMilliseconds;

        Stopwatch swDeserialization = Stopwatch.StartNew();
        F deserializedObj;

        for (int i = 0; i < iterations; i++)
        {
            deserializedObj = DeserializeReflection(serializedData);
        }

        swDeserialization.Stop();
        long deserializationTime = swDeserialization.ElapsedMilliseconds;

        return (serializationTime, deserializationTime);
    }

    static (long, long) BenchmarkNewtonsoftJson(F obj, int iterations)
    {
        Stopwatch swSerialization = Stopwatch.StartNew();
        string serializedData = "";

        for (int i = 0; i < iterations; i++)
        {
            serializedData = JsonConvert.SerializeObject(obj);
        }

        swSerialization.Stop();
        long serializationTime = swSerialization.ElapsedMilliseconds;

        Stopwatch swDeserialization = Stopwatch.StartNew();
        F deserializedObj;

        for (int i = 0; i < iterations; i++)
        {
            deserializedObj = JsonConvert.DeserializeObject<F>(serializedData);
        }

        swDeserialization.Stop();
        long deserializationTime = swDeserialization.ElapsedMilliseconds;

        return (serializationTime, deserializationTime);
    }

    static string SerializeReflection(F obj)
    {
        Type type = typeof(F);
        string result = "";

        foreach (FieldInfo field in type.GetFields())
        {
            result += $"{field.Name}:{field.GetValue(obj)};";
        }

        return result;
    }

    static F DeserializeReflection(string data)
    {
        F obj = new F();
        Type type = typeof(F);

        string[] parts = data.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (string part in parts)
        {
            string[] keyValue = part.Split(':');
            string fieldName = keyValue[0];
            string fieldValue = keyValue[1];

            FieldInfo field = type.GetField(fieldName);
            if (field != null)
            {
                field.SetValue(obj, Convert.ChangeType(fieldValue, field.FieldType));
            }
        }

        return obj;
    }
}