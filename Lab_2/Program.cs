using System.Text;

struct GeneticData
{
    public string protein; 
    public string organism; 
    public string amino_acids; 
}

class Program
{
    static List<GeneticData> geneticDataList = new List<GeneticData>();

    static void Main(string[] args)
    {
        ReadGeneticData("sequences.0.txt"); 
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
                        amino_acids = RLDecoding(parts[2].Trim())
                    };
                    geneticDataList.Add(data);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file {filename}: {ex.Message}");
        }
    }

    static void ProcessCommands(string commandsFile, string outputFile)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                writer.WriteLine("Ivan Ivanov");
                writer.WriteLine("Genetic Searching");
                writer.WriteLine("-----------------------------------------------------------------------------------------");

                string[] commands = File.ReadAllLines(commandsFile);
                int commandNumber = 1;

                foreach (string commandLine in commands)
                {
                    if (string.IsNullOrWhiteSpace(commandLine)) continue;

                    string[] parts = commandLine.Split('\t');
                    if (parts.Length == 0) continue;

                    string operation = parts[0].Trim();
                    
                    // Вывод команды перед выполнением операции
                    writer.WriteLine($"Command: {operation.ToUpper()}");
                    writer.WriteLine($" {commandNumber:D3} {GetOperationDescription(operation, parts)} ");

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

                    writer.WriteLine("-----------------------------------------------------------------------------------------");
                    commandNumber++;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing commands: {ex.Message}");
        }
    }

    static string GetOperationDescription(string operation, string[] parts)
    {
        switch (operation.ToLower())
        {
            case "search":
                return parts.Length >= 2 ? $"{parts[1].Trim()} organism protein" : "search";
            case "diff":
                return parts.Length >= 3 ? $"{parts[1].Trim()} {parts[2].Trim()} amino-acids difference:" : "diff";
            case "mode":
                return parts.Length >= 2 ? $"{parts[1].Trim()} amino-acid occurs:" : "mode";
            default:
                return operation;
        }
    }

    static void SearchOperation(StreamWriter writer, string sequence)
    {
        List<GeneticData> foundResults = new List<GeneticData>();
        
        foreach (GeneticData data in geneticDataList)
        {
            if (data.amino_acids.Contains(sequence))
            {
                foundResults.Add(data);
            }
        }

        if (foundResults.Count == 0)
        {
            writer.WriteLine(" NOT FOUND");
        }
        else
        {
            foreach (GeneticData data in foundResults)
            {
                writer.WriteLine($" {data.organism} ({data.protein}) ");
            }
        }
    }

    static void DiffOperation(StreamWriter writer, string protein1, string protein2)
    {
        GeneticData? data1 = null;
        GeneticData? data2 = null;

        // Find proteins
        foreach (GeneticData data in geneticDataList)
        {
            if (data.protein.Equals(protein1, StringComparison.OrdinalIgnoreCase))
                data1 = data;
            if (data.protein.Equals(protein2, StringComparison.OrdinalIgnoreCase))
                data2 = data;
        }

        // Check if proteins exist
        if (data1 == null || data2 == null)
        {
            if (data1 == null && data2 == null)
                writer.WriteLine($" MISSING: {protein1} {protein2} ");
            else if (data1 == null)
                writer.WriteLine($" MISSING: {protein1} ");
            else
                writer.WriteLine($" MISSING: {protein2} ");
            return;
        }

        // Compare sequences
        int differences = CalculateDifferences(data1.Value.amino_acids, data2.Value.amino_acids);
        writer.WriteLine($" {differences} ");
    }

    static void ModeOperation(StreamWriter writer, string protein)
    {
        GeneticData? targetData = null;

        // Find protein
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
            writer.WriteLine($" MISSING: {protein} ");
            return;
        }

        // Count amino acid frequency
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

        // Find most frequent amino acid
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

        writer.WriteLine($" {mostFrequent} {maxCount} ");
    }

    static int CalculateDifferences(string seq1, string seq2)
    {
        int differences = 0;
        int minLength = Math.Min(seq1.Length, seq2.Length);
        int maxLength = Math.Max(seq1.Length, seq2.Length);

        // Compare common length
        for (int i = 0; i < minLength; i++)
        {
            if (seq1[i] != seq2[i])
                differences++;
        }

        // Account for length difference
        differences += (maxLength - minLength);

        return differences;
    }

    static string RLDecoding(string encoded)
    {
        StringBuilder decoded = new StringBuilder();
        int i = 0;

        while (i < encoded.Length)
        {
            if (char.IsDigit(encoded[i]))
            {
                // Extract repetition count
                int count = encoded[i] - '0';
                i++;
                
                // Add repeated character
                if (i < encoded.Length)
                {
                    decoded.Append(encoded[i], count);
                    i++;
                }
            }
            else
            {
                // Single character
                decoded.Append(encoded[i]);
                i++;
            }
        }

        return decoded.ToString();
    }
}