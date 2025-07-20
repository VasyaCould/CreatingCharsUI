using System;   // DCU DigitizationCharsUi
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;
using System.Transactions;
// File.WriteAllText(path, text);


namespace engine
{
    public static class MainGameClass
    {
        public static PixArray pic = new();
        private static PixArray extraPic = new();
        public static string? pathToSaveChars = "";
        public static string? curCharName = "unknown";
        public static Vector2int charBorderStart = new Vector2int(99, 99);
        public static Vector2int charBorderEnd = new Vector2int(251, 251);
        public static PixArray settingsImg = new();
        private static bool settingsNext = false;
        private static bool changeSymbolNameNext = false;
        public static string pathPic;
        // public static bool useTransitionTo = false;
        // public static bool useTransitionFrom = false;
        private static void printDoc()
        {
            Console.WriteLine("\n****************************************************************\nЧтобы изменить настройки или сменить изображение, нажмите q\nЧтобы рисовать, удерживайте левую кнопку мыши\nЧтобы двигать изображение, удерживайте правую кнопку мыши\nЧтобы рисовать переход к текущему символу, удерживайте CTRL\nЧтобы рисовать переход к следующему символу удерживайте SHIFT\nНажмите стрелку в право, чтобы закончить символ и перейти к след. символу без смены названия\nНажмите стрелку в верх, чтобы сменить название символа\nЧтобы стереть букву, нажмите delete\n****************************************************************\n\nНажмите ENTER");

        }
        public static void Start()
        {
            settingsImg = new("SettingsImg.png");

            try { Console.WriteLine(File.ReadAllText("iconSmall.png.txt")); }
            catch { Console.WriteLine("typix"); }
            Console.WriteLine("\nDCU\nВведите полный путь к изображению (PNG, JPEG, JPG, GIF, BMP, WebP)");
            while (true)
            {
                try
                {
                    pathPic = Console.ReadLine();
                    pic = new PixArray(pathPic);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error: " + ex + "\nПовторите попытку");
                }
            }
            mapScale.x = pic.Width;
            mapScale.y = pic.Height;
            Console.WriteLine("Введите полный путь для сохранения символов");
            while (true)
            {
                try
                {
                    pathToSaveChars = Console.ReadLine();
                    File.WriteAllText($@"{pathToSaveChars}\test.txt", "test");
                    File.Delete($@"{pathToSaveChars}\test.txt");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error: " + ex + "\nПовторите попытку");
                }
            }
            Console.WriteLine("Введите название символа (символ будет вызван по этому имени в тексте)");
            while (true)
            {
                try
                {
                    curCharName = Console.ReadLine();
                    File.WriteAllText($@"{pathToSaveChars}\{curCharName}.txt", "test");
                    File.Delete($@"{pathToSaveChars}\{curCharName}.txt");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error: " + ex + "\nПовторите попытку");
                }
            }
            // Console.WriteLine("К этому символу нужнен переход от предыдущего? 0-Да 1-Нет");
            // while (true)
            // {
            //     string? answ = Console.ReadLine();
            //     if (answ == "0") { useTransitionTo = true; break;}
            //     else if (answ == "1") { useTransitionTo = false; break;}

            //     else Console.WriteLine("Повторите попытку");
            // }
            // Console.WriteLine("От этого символа существует переход к следующему? 0-Да 1-Нет");
            // while (true)
            // {
            //     string? answ = Console.ReadLine();
            //     if (answ == "0") { useTransitionFrom = true; break;}
            //     else if (answ == "1") { useTransitionFrom = false; break;}

            //     else Console.WriteLine("Повторите попытку");
            // }
            printDoc();
            Console.ReadLine();
            Console.WriteLine("Окно открыто");

            OutputWindow.fullScreen = false;
            OutputWindow.windowName = "typix";
        }
        private static Vector2int mapPos = new();
        private static bool firstClickR = true;
        private static bool firstClickL = true;
        private static Vector2int lastMousePosL = new();//в the legend of zelda хочется все фоткать а нипример в м одессе нет и если делать игру то только такую
        private static Vector2int lastMousePosR = new();
        public static string coordsToWriteToFile = "";
        private static Vector2int mapScale = new();
        public static string fileStringCoords = $"{charBorderStart.x + 1}\n{charBorderStart.y + 1}\n{charBorderEnd.x - 1}\n{charBorderEnd.y - 1}\n";
        private static string lastLine = "";
        private static bool savedFile = false;
        public static int fileNumber;
        public static bool row = true;
        public static void Update()
        {
            if (settingsNext)
            {
                settingsNext = false;
                Settings();
            }
            else if (changeSymbolNameNext)
            {
                OutputWindow.img.Clear();
                OutputWindow.img.MergeRGB(settingsImg);
                changeSymbolNameNext = false;
                while (true)
                {
                    Console.WriteLine("Введите название символа");
                    try
                    {
                        curCharName = Console.ReadLine();
                        File.WriteAllText($@"{pathToSaveChars}\{curCharName}.txt", "test");
                        File.Delete($@"{pathToSaveChars}\{curCharName}.txt");
                        row = true;
                        Console.WriteLine("Окно открыто");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("error: " + ex + "\nПовторите попытку");
                    }
                }
            }

            if (Input.IsPressed(Keys.Q))
            {
                settingsNext = true;
                OutputWindow.img.Clear();
                OutputWindow.img.MergeRGB(settingsImg);
                return;
            }
            if (Input.IsPressed(Keys.MouseR))
            {
                if (firstClickR)
                {
                    lastMousePosR = new(Input.GetMousePosRel().x, Input.GetMousePosRel().y);
                    firstClickR = false;
                }
                mapPos = new(Input.GetMousePosRel().x - lastMousePosR.x + mapPos.x, Input.GetMousePosRel().y - lastMousePosR.y + mapPos.y);
                lastMousePosR = new(Input.GetMousePosRel().x, Input.GetMousePosRel().y);
            }
            else firstClickR = true;

            // pic.show(mapPos, fill: true);

            if (Input.IsPressed(Keys.MouseL))
            {
                if (firstClickL)
                {
                    lastMousePosL = new(Input.GetMousePosRel().x - mapPos.x, Input.GetMousePosRel().y - mapPos.y);
                    if (Input.GetMousePosRel().x < charBorderStart.x) lastMousePosL.x = charBorderStart.x - mapPos.x + 1;
                    else if (Input.GetMousePosRel().x > charBorderEnd.x) lastMousePosL.x = charBorderEnd.x - mapPos.x - 1;
                    if (Input.GetMousePosRel().y < charBorderStart.y) lastMousePosL.y = charBorderStart.y - mapPos.y + 1;
                    else if (Input.GetMousePosRel().y > charBorderEnd.y) lastMousePosL.y = charBorderEnd.y - mapPos.y - 1;
                    firstClickL = false;
                }
                Color color = new(255, 0, 0);
                if (Input.IsPressed(Keys.CONTROL)) color = new(0, 0, 255);
                else if (Input.IsPressed(Keys.SHIFT)) color = new(0, 255, 0);

                // Vector2int temp = Input.GetMousePosRel();


                Vector2int coords = new(Input.GetMousePosRel().x - mapPos.x, Input.GetMousePosRel().y - mapPos.y);


                if (Input.GetMousePosRel().x < charBorderStart.x) coords.x = charBorderStart.x - mapPos.x + 1;
                else if (Input.GetMousePosRel().x > charBorderEnd.x) coords.x = charBorderEnd.x - mapPos.x - 1;
                if (Input.GetMousePosRel().y < charBorderStart.y) coords.y = charBorderStart.y - mapPos.y + 1;
                else if (Input.GetMousePosRel().y > charBorderEnd.y) coords.y = charBorderEnd.y - mapPos.y - 1;



                pic.DrawLine(lastMousePosL, coords, color);
                string curLine = Convert.ToString(coords.x + mapPos.x - charBorderStart.x - 1) + " " + Convert.ToString(coords.y + mapPos.y - charBorderStart.y - 1) + (!row ? "U" : "") + (Input.IsPressed(Keys.CONTROL) ? "T" : Input.IsPressed(Keys.SHIFT) ? "F" : "") + "\n";
                if (lastLine != curLine) fileStringCoords += curLine;
                lastLine = curLine;
                if (savedFile) savedFile = false;
                //OutputWindow.img.SetPixel(Input.GetMousePosRel().x, Input.GetMousePosRel().y, new Color(255, 0, 0)); }//

                // lastMousePosL = temp;

                // OutputWindow.img.DrawLine(new Vector2int(500, 500), new Vector2int(300, 300), new Color(255, 0, 0));
                row = true;
            }
            else
            {
                firstClickL = true;
                row = false;
            }
            // lastMousePosL = Input.GetMousePosRel();
            if (Input.IsPressed(Keys.RIGHT))
            {
                if (!savedFile)
                {
                    if (!Directory.Exists($@"{pathToSaveChars}\DictTypixDCU")) Directory.CreateDirectory($@"{pathToSaveChars}\DictTypixDCU");
                    if (!Directory.Exists($@"{pathToSaveChars}\DictTypixDCU\{curCharName}")) Directory.CreateDirectory($@"{pathToSaveChars}\DictTypixDCU\{curCharName}");
                    File.WriteAllText($@"{pathToSaveChars}\DictTypixDCU\{curCharName}\{fileNumber}.txt", fileStringCoords);
                    Console.WriteLine($@"Файл {fileNumber}.txt создан в директории {pathToSaveChars}\DictTypixDCU\{curCharName}");
                    fileStringCoords = $"{charBorderStart.x + 1}\n{charBorderStart.y + 1}\n{charBorderEnd.x - 1}\n{charBorderEnd.y - 1}\n";
                    savedFile = true;
                    fileNumber++;
                    pic = new(pathPic);
                }
            }
            if (Input.IsPressed(Keys.UP))
            {
                OutputWindow.img.Clear();
                OutputWindow.img.MergeRGB(settingsImg);
                changeSymbolNameNext = true;
                fileNumber = 0;
                row = true;
                return;
            }
            if (Input.IsPressed(Keys.DELETE))
            {
                pic = new(pathPic);
                fileStringCoords = $"{charBorderStart.x + 1}\n{charBorderStart.y + 1}\n{charBorderEnd.x - 1}\n{charBorderEnd.y - 1}\n";
                row = true;
            }

            int scalingRatio = 5;
            if (Input.MouseWheel.up)
            {
                charBorderEnd.x += scalingRatio;
                charBorderEnd.y += scalingRatio;
                // int newMapScale = (int)(mapScale.x * scalingRatio);
                // mapPos.x -= (newMapScale - mapScale.x) / 2;
                // mapScale.x = newMapScale;

                // newMapScale = (int)(mapScale.y * scalingRatio);
                // mapPos.y -= (newMapScale - mapScale.y) / 2;
                // mapScale.y = newMapScale;
            }
            else if (Input.MouseWheel.down)
            {
                charBorderEnd.x -= scalingRatio;
                charBorderEnd.y -= scalingRatio;
                // int newMapScale = (int)(mapScale.x / scalingRatio);
                // mapPos.x += (mapScale.x - newMapScale) / 2;
                // mapScale.x = newMapScale;

                // newMapScale = (int)(mapScale.y / scalingRatio);
                // mapPos.y += (mapScale.y - newMapScale) / 2;
                // mapScale.y = newMapScale;
            }

            OutputWindow.img.Clear();
            OutputWindow.img.MergeRGB(pic.ScaleImageNearest(mapScale.x, mapScale.y), mapPos);
            // pic.MergeRGBA(extraPic);// gpt про simd там он про ассемблер говорит и про процессоры
            OutputWindow.img.DrawLine(new(charBorderStart.x, charBorderStart.y), new(charBorderEnd.x, charBorderStart.y), new Color(255, 0, 255));
            OutputWindow.img.DrawLine(new Vector2int(charBorderEnd.x, charBorderStart.y), new(charBorderEnd.x, charBorderEnd.y), new Color(255, 0, 255));
            OutputWindow.img.DrawLine(new(charBorderEnd.x, charBorderEnd.y), new Vector2int(charBorderStart.x, charBorderEnd.y), new Color(255, 0, 255));
            OutputWindow.img.DrawLine(new(charBorderStart.x, charBorderStart.y), new Vector2int(charBorderStart.x, charBorderEnd.y), new Color(255, 0, 255));

        }
        public static void Settings()
        {

            while (true)
            {
                Console.WriteLine("0-Вернуться\n1-Сменить полный путь к изображению (PNG, JPEG, JPG, GIF, BMP, WebP)\n2-Сменить полный путь для сохранения символов\n3-Сменить название символа\n");// 4-Изменить переходы");
                switch (Console.ReadLine())
                {
                    case "0":
                        printDoc();
                        Console.ReadLine();
                        Console.WriteLine("Окно окрыто");
                        return;
                    case "1":
                        Console.WriteLine("\nВведите полный путь к изображению (PNG, JPEG, JPG, GIF, BMP, WebP)");
                        while (true)
                        {
                            try
                            {
                                pathPic = Console.ReadLine();
                                pic = new PixArray(pathPic);
                                break;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("error: " + ex + "\nПовторите попытку");
                            }
                        }
                        break;
                    case "2":
                        while (true)
                        {
                            try
                            {
                                Console.WriteLine("Введите полный путь для сохранения символов");
                                pathToSaveChars = Console.ReadLine();
                                File.WriteAllText($@"{pathToSaveChars}\test.txt", "test");
                                File.Delete($@"{pathToSaveChars}\test.txt");
                                break;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("error: " + ex + "\nПовторите попытку\n");
                            }
                        }
                        break;
                    case "3":
                        while (true)
                        {
                            Console.WriteLine("Введите название символа (символ будет вызван по этому имени в тексте)");
                            try
                            {
                                curCharName = Console.ReadLine();
                                File.WriteAllText($@"{pathToSaveChars}\{curCharName}.txt", "test");
                                File.Delete($@"{pathToSaveChars}\{curCharName}.txt");
                                break;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("error: " + ex + "\nПовторите попытку");
                            }
                        }
                        break;
                        // case "4":
                        //     Console.WriteLine("К этому символу нужнен переход от предыдущего? 0-Да 1-Нет");
                        //     while (true)
                        //     {
                        //         string? answ = Console.ReadLine();
                        //         if (answ == "0") { useTransitionTo = true; break;}
                        //         else if (answ == "1") { useTransitionTo = false; break;}

                        //         else Console.WriteLine("Повторите попытку");
                        //     }
                        //     Console.WriteLine("От этого символа существует переход к следующему? 0-Да 1-Нет");
                        //     while (true)
                        //     {
                        //         string? answ = Console.ReadLine();
                        //         if (answ == "0") { useTransitionFrom = true; break;}
                        //         else if (answ == "1") { useTransitionFrom = false; break;}

                        //         else Console.WriteLine("Повторите попытку");
                        //     }
                        //     break;
                }
            }
        }
    }
}