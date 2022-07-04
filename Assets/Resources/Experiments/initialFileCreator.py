group = 6
groupMove = [["drag","TP","joy"],["TP","drag","joy"],["joy","TP","drag"],
            ["drag","joy","TP"],["TP","joy","drag"],["joy","drag","TP"]]
cardList = [12,28,40,2,20,15,0,50,36,9,22,47]
#cardList = [22, 37, 11, 51, 47, 8, 34, 29, 59, 43, 6, 10, 31, 25, 14, 19, 
#            40, 56, 24, 39, 2, 17, 54, 49, 26, 32, 4, 0, 58, 42, 15, 18, 
 #           23, 38, 45, 50, 12, 9, 36, 30, 53, 57, 13, 7, 35, 28, 48, 52]
lenght = (int) (len(cardList)/3)
training = ""

file = open("initialTrialFile.txt", "w")
file.write("Group;Participant;CollabEnvironememn;trialNb;training;MoveMode;CardToTag;\n");

for g in range(1, group+1):
    for n in range(0, lenght):
        if n < 6:
            training = "1"
        else:
            training = "0"
        if n%2 == 0:
            file.write("g0" + str(g) + ";p01;C;" + str(n) + ";" + training + ";" + groupMove[g-1][0] + ";" + str(cardList[n]) + ";\n")
            file.write("g0" + str(g) + ";p02;C;" + str(n) + ";" + training + ";" + "sync" + ";" + str(cardList[n]) + ";\n")
        else:
            file.write("g0" + str(g) + ";p01;C;" + str(n) + ";" + training + ";" + "sync" + ";" + str(cardList[n]) + ";\n")
            file.write("g0" + str(g) + ";p02;C;" + str(n) + ";" + training + ";" + groupMove[g-1][0] + ";" + str(cardList[n]) + ";\n")
    file.write("#pause;\n")
    for n in range(lenght, 2*lenght):
        if n-lenght < 6:
            training = "1"
        else:
            training = "0"
        if n%2 == 0:
            file.write("g0" + str(g) + ";p01;C;" + str(n) + ";" + training + ";" + groupMove[g-1][1] + ";" + str(cardList[n]) + ";\n")
            file.write("g0" + str(g) + ";p02;C;" + str(n) + ";" + training + ";" + "sync" + ";" + str(cardList[n]) + ";\n")
        else:
            file.write("g0" + str(g) + ";p01;C;" + str(n) + ";" + training + ";" + "sync" + ";" + str(cardList[n]) + ";\n")
            file.write("g0" + str(g) + ";p02;C;" + str(n) + ";" + training + ";" + groupMove[g-1][1] + ";" + str(cardList[n]) + ";\n")
    file.write("#pause;\n")
    for n in range(2*lenght, 3*lenght):
        if n-2*lenght < 6:
            training = "1"
        else:
            training = "0"
        if n%2 == 0:
            file.write("g0" + str(g) + ";p01;C;" + str(n) + ";" + training + ";" + groupMove[g-1][2] + ";" + str(cardList[n]) + ";\n")
            file.write("g0" + str(g) + ";p02;C;" + str(n) + ";" + training + ";" + "sync" + ";" + str(cardList[n]) + ";\n")
        else:
            file.write("g0" + str(g) + ";p01;C;" + str(n) + ";" + training + ";" + "sync" + ";" + str(cardList[n]) + ";\n")
            file.write("g0" + str(g) + ";p02;C;" + str(n) + ";" + training + ";" + groupMove[g-1][2] + ";" + str(cardList[n]) + ";\n")
    if g != group:
        file.write("#pause;\n")
    else:
        file.write("#pause;")

file.close()
