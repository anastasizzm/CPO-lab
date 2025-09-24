using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

struct GeneticData
{
    public string protein; // название белка
    public string organism; // название организма
    public string amino_acids; // цепочка аминокислот
}

class Program
{
    static List<GeneticData> geneticDataList = new List<GeneticData>();

    static void Main(string[] args)
    {
        // Чтение данных о белках
        ReadGeneticData("sequences.0.txt");
        
        // Обработка команд и запись результатов
        ProcessCommands("commands.0.txt", "genedata.0.txt");
    }

    static void ReadGeneticData(string filename)
    {
        try
        {
            string[] lines = File.ReadAllLines(filename);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                string[] parts = line.Split('\t');
                if (parts.Length >= 3)
                {
                    GeneticData data = new GeneticData
                    {
                        protein = parts[0].Trim(),
                        organism = parts[1].Trim(),
                        amino_acids = RLDecoding(parts[2].Trim()) // раскодируем RLE
                    };
                    geneticDataList.Add(data);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при чтении файла {filename}: {ex.Message}");
        }
    }

    static void ProcessCommands(string commandsFile, string outputFile)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                // Заголовок файла
                writer.WriteLine("Имя студента");
                writer.WriteLine("Генетический поиск");
                writer.WriteLine();

                string[] commands = File.ReadAllLines(commandsFile);
                int commandNumber = 1;

                foreach (string commandLine in commands)
                {
                    if (string.IsNullOrWhiteSpace(commandLine)) continue;

                    string[] parts = commandLine.Split('\t');
                    if (parts.Length == 0) continue;

                    string operation = parts[0].Trim();
                    
                    // Записываем номер операции
                    writer.WriteLine($"{commandNumber:D3}");
                    writer.WriteLine(new string('-', 50));

                    switch (operation.ToLower())
                    {
                        case "search":
                            if (parts.Length >= 2)
                            {
                                string sequence = RLDecoding(parts[1].Trim());
                                SearchOperation(writer, sequence);
                            }
                            break;

                        case "diff":
                            if (parts.Length >= 3)
                            {
                                string protein1 = parts[1].Trim();
                                string protein2 = parts[2].Trim();
                                DiffOperation(writer, protein1, protein2);
                            }
                            break;

                        case "mode":
                            if (parts.Length >= 2)
                            {
                                string protein = parts[1].Trim();
                                ModeOperation(writer, protein);
                            }
                            break;
                    }

                    writer.WriteLine();
                    commandNumber++;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке команд: {ex.Message}");
        }
    }

    // Операция поиска
    static void SearchOperation(StreamWriter writer, string sequence)
    {
        bool found = false;
        
        foreach (GeneticData data in geneticDataList)
        {
            if (data.amino_acids.Contains(sequence))
            {
                writer.WriteLine($"{data.organism} ({data.protein})");
                found = true;
            }
        }

        if (!found)
        {
            writer.WriteLine("NOT FOUND");
        }
    }

    // Операция сравнения
    static void DiffOperation(StreamWriter writer, string protein1, string protein2)
    {
        GeneticData? data1 = null;
        GeneticData? data2 = null;

        // Поиск белков
        foreach (GeneticData data in geneticDataList)
        {
            if (data.protein.Equals(protein1, StringComparison.OrdinalIgnoreCase))
                data1 = data;
            if (data.protein.Equals(protein2, StringComparison.OrdinalIgnoreCase))
                data2 = data;
        }

        // Проверка наличия белков
        if (data1 == null || data2 == null)
        {
            writer.Write("amino-acids difference: MISSING:");
            if (data1 == null) writer.Write($" {protein1}");
            if (data2 == null) writer.Write($" {protein2}");
            writer.WriteLine();
            return;
        }

        // Сравнение последовательностей
        int differences = CalculateDifferences(data1.Value.amino_acids, data2.Value.amino_acids);
        writer.WriteLine($"amino-acids difference: {differences}");
    }

    // Операция поиска наиболее частой аминокислоты
    static void ModeOperation(StreamWriter writer, string protein)
    {
        GeneticData? targetData = null;

        // Поиск белка
        foreach (GeneticData data in geneticDataList)
        {
            if (data.protein.Equals(protein, StringComparison.OrdinalIgnoreCase))
            {
                targetData = data;
                break;
            }
        }

        if (targetData == null)
        {
            writer.WriteLine($"amino-acid occurs: MISSING: {protein}");
            return;
        }

        // Подсчет частоты аминокислот
        Dictionary<char, int> frequency = new Dictionary<char, int>();
        foreach (char aminoAcid in targetData.Value.amino_acids)
        {
            if (char.IsLetter(aminoAcid))
            {
                if (frequency.ContainsKey(aminoAcid))
                    frequency[aminoAcid]++;
                else
                    frequency[aminoAcid] = 1;
            }
        }

        // Поиск наиболее частой аминокислоты
        char mostFrequent = ' ';
        int maxCount = 0;

        foreach (var pair in frequency)
        {
            if (pair.Value > maxCount || (pair.Value == maxCount && pair.Key < mostFrequent))
            {
                mostFrequent = pair.Key;
                maxCount = pair.Value;
            }
        }

        writer.WriteLine($"amino-acid occurs: {mostFrequent} ({maxCount})");
    }

    // Расчет различий между последовательностями
    static int CalculateDifferences(string seq1, string seq2)
    {
        int differences = 0;
        int minLength = Math.Min(seq1.Length, seq2.Length);
        int maxLength = Math.Max(seq1.Length, seq2.Length);

        // Сравнение по общей длине
        for (int i = 0; i < minLength; i++)
        {
            if (seq1[i] != seq2[i])
                differences++;
        }

        // Учет разницы в длине
        differences += (maxLength - minLength);

        return differences;
    }

    // RLE декодирование
    static string RLDecoding(string encoded)
    {
        StringBuilder decoded = new StringBuilder();
        int i = 0;

        while (i < encoded.Length)
        {
            if (char.IsDigit(encoded[i]))
            {
                // Извлекаем число повторений
                int count = encoded[i] - '0';
                i++;
                
                // Добавляем повторяющийся символ
                if (i < encoded.Length)
                {
                    decoded.Append(encoded[i], count);
                    i++;
                }
            }
            else
            {
                // Одиночный символ
                decoded.Append(encoded[i]);
                i++;
            }
        }

        return decoded.ToString();
    }

    // RLE кодирование (может пригодиться для других задач)
    static string RLEncoding(string decoded)
    {
        if (string.IsNullOrEmpty(decoded))
            return string.Empty;

        StringBuilder encoded = new StringBuilder();
        int count = 1;
        char current = decoded[0];

        for (int i = 1; i < decoded.Length; i++)
        {
            if (decoded[i] == current)
            {
                count++;
            }
            else
            {
                // Добавляем в закодированную строку
                if (count > 2)
                {
                    encoded.Append(count);
                    encoded.Append(current);
                }
                else
                {
                    encoded.Append(new string(current, count));
                }
                
                current = decoded[i];
                count = 1;
            }
        }

        // Добавляем последнюю последовательность
        if (count > 2)
        {
            encoded.Append(count);
            encoded.Append(current);
        }
        else
        {
            encoded.Append(new string(current, count));
        }

        return encoded.ToString();
    }
}
