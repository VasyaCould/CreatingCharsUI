import os # Динамическая типизация - криндж
from random import randint # dcg Dictionary Convert to Gcode

sizeOfBildPlate = int(input("Введите размер bildPlate в см: "))

offsetRight = int(input("Введите отступ справа в см: "))
offsetLeft = int(input("Введите отступ слева в см: "))
offsetUp = int(input("Введите отступ сверху в см: "))
offsetDown = int(input("Введите отступ снизу в см: "))
moveUp = int(input("Введите расстояние на которое будет подниматься сопло (чтобы не задевать bildplate) в см (предлагаемое - 1): "))

distanceBetweenLines = int(input("Введите расстояние между строк в см: "))

cage = int(input("Введите высоту клетки: ")) if input("Если бумага в клетку, введите 0: ") == "0" else False

if(not cage): symbolHeight = int(input("Введите высоту символов в см: "))
else: symbolHeight = cage

minimumWidthChars = int(input("Введите минимальную ширину символов: ")) if input("Если вы хотите задать минимальную ширину символов, введите 0: ") == "0" else False

pathToDict = input("Введите полный путь до словаря (>disc:\\~~~\\<DictTypixDCU) (символов созданных ранее в typix dcu): ")

resName = input("Введите название создаваемого файла (.gcode не нужно): ")

resPath = input("Введите полный путь для сохранения " + resName + ".gcode: ")



text = input("Введите текст который хотите написать (если текст в файле, нажмите enter): ")

pathToText = ""

if(text == ""):
    pathToText = input("Введите полный путь до файла: ")
    with open(pathToText) as file: text = file.read()

fileStringGcode = ""
with open("gcodeFirstPart.txt") as file:
    resLine = file.read()
resLineTestGcode = ""
currentPosX = 0
currentPosY = 0
countLosses = 0
for i in text:
    try:
        with open(f"{pathToDict}\\DictTypixDCU\\{i}\\0.txt"): None
        
        numOfcharsVars = os.listdir(f"{pathToDict}\\DictTypixDCU\\{i}")[-1].replace(".txt", "")

        with open(f"{pathToDict}\\DictTypixDCU\\{i}\\{randint(0, int(numOfcharsVars))}.txt") as file:

            lines = file.readlines()
            for i2 in range(4, len(lines)):
                x = ""
                y = ""
                secondStarted = False
                for i3 in lines[i2]:
                    if(secondStarted):
                        try:
                            int(i3)
                            str(i3)
                            y += i3
                        except:
                            break
                    if(i3 == " "): secondStarted = True
                    if(not secondStarted): x += i3
                    
            
                resLine += "\nG1 X"# F1000
                resLineTestGcode += "\n" + str(int(x) + currentPosX) + "\n" + str(int(y) + currentPosY)

                resLine += str(int(x) + currentPosX) + " Y" + str(int(y) + currentPosY) 

            currentPosX += int(lines[2]) - int(lines[0])
            #currentPosY = int(lines[3]) - int(lines[1]) no
    except:
        print("В словаре отсутствует " + i)
        countLosses += 1

with open("gcodeSecondPart.txt") as file:
    resLine += file.read()

with open(resPath + "\\" + resName + ".gcode", "w") as file:
    file.write(resLine)
with open(resPath + "\\" + resName + ".testGcode", "w") as file:
    file.write(resLineTestGcode[1:-1])


print("Готово!")
print(f"Отсутствующие файлы в словаре: {countLosses} из {len(text)} ({int(countLosses / len(text) * 100)}%)")
#input()