using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Text.RegularExpressions;

namespace ColorSquaresFromText
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            try
            {
                string filePath = @"D:\CPO-lab\Lab_1\Aeroport.txt";

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File {filePath} не найден.");
                    return;
                }

                string text = File.ReadAllText(filePath, System.Text.Encoding.UTF8);

                
                var result = GetColorsFromText(text);
                var coloredWords = result.Item1;
                var colors = result.Item2;

                if (colors.Count == 0)
                {
                    Console.WriteLine("Не найдено подходящих слов для создания цветов.");
                    return;
                }

                
                Console.WriteLine("Найдены цветные слова:");
                foreach (var word in coloredWords)
                {
                    Console.WriteLine(word);
                }

                int squareSize = 20;
                int columns = 20;

                string outputPath = CreateColorImage(colors, squareSize, columns);

                Console.WriteLine($"\nИзображение создано: {outputPath}");
                Console.WriteLine($"Обработано {colors.Count} цветных слов");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static (List<string>, List<Color>) GetColorsFromText(string text)
        {
            var colors = new List<Color>();
            var coloredWords = new List<string>();

            var colorMap = new Dictionary<string, Color>
            {
                {"красн", Color.Red},
                {"син", Color.Blue},
                {"зелен", Color.Green},
                {"желт", Color.Yellow},
                {"оранж", Color.Orange},
                {"фиолет", Color.Purple},
                {"розов", Color.Pink},
                {"черн", Color.Black},
                {"бел", Color.White},
                {"сер", Color.Gray},
                {"голуб", Color.LightBlue},
                {"коричн", Color.Brown},
                {"бирюз", Color.Turquoise},
                {"золот", Color.Gold},
                {"серебр", Color.Silver},
                {"лазур", Color.Azure},
                {"вишн", Color.Crimson},
                {"изумруд", Color.Green},
                {"сапфир", Color.DarkBlue},
                {"бронз", Color.RosyBrown},
                {"медн", Color.Chocolate},
                {"малинов", Color.Pink},
                {"оливков", Color.Olive},
                {"пурпур", Color.Magenta},
                {"нефрит", Color.Green},
                {"коралл", Color.Coral}
            };

            var exclusionWords = new HashSet<string>
            {
                "синди", "синтез", "синхронизация", "синдикат", "синтаксис", "анализ",
                "синтетика", "синхрометр", "синхрология", "синграф", "синномия",
                "красноречивый", "красноармеец", "краснодар",
                "зеленушка", "зеленоград", "зеленодольск",
                "желтуха", "желчегонный", "желчью",
                "чернозем", "чернобыль", "черноморский",
                "белгород", "беллетристика", "белладонна",
                "серпухов", "серповидный", "сердечный",
                "голубятня", "голубика", "голубоглазый",
                "коридор", "корифей", "коричневший",
                "золотник", "золотоноша", "золотушный",
                "серебрение", "серебряник", "серебрянка"
            };

            var words = Regex.Matches(text, @"\b[\p{IsCyrillic}]{3,}\b", RegexOptions.IgnoreCase);
            
            foreach (Match wordMatch in words)
            {
                string originalWord = wordMatch.Value;
                string word = originalWord.ToLower();
                
                if (exclusionWords.Contains(word))
                {
                    continue; 
                }
                
                var colorMatch = colorMap.FirstOrDefault(KeyValuePair => word.StartsWith(KeyValuePair.Key));
                
                if (!colorMatch.Equals(default(KeyValuePair<string, Color>)))
                {
                    colors.Add(colorMatch.Value);
                    coloredWords.Add(originalWord);
                }
            }

            return (coloredWords, colors);
        }

        static string CreateColorImage(List<Color> colors, int squareSize, int columns)
        {
            int rows = (int)Math.Ceiling((double)colors.Count / columns);
            int width = columns * squareSize;
            int height = rows * squareSize;

            using (var bitmap = new Bitmap(width, height))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);

                for (int i = 0; i < colors.Count; i++)
                {
                    int row = i / columns;
                    int col = i % columns;
                    
                    int x = col * squareSize;
                    int y = row * squareSize;

                    using (var brush = new SolidBrush(colors[i]))
                    {
                        graphics.FillRectangle(brush, x, y, squareSize, squareSize);
                    }

                    graphics.DrawRectangle(Pens.Black, x, y, squareSize, squareSize);
                }

                string outputPath = @"D:\CPO-lab\Lab_1\color_squares_from_aeroport.png";
                bitmap.Save(outputPath, ImageFormat.Png);
                
                return outputPath;
            }
        }
    }
}